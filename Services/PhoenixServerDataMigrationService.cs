using System.Diagnostics;
using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

internal static class PhoenixServerDataMigrationService
{
    private const string PhoenixServerServiceName = "PhoenixServer";
    private const string ServerDataBackupName = "server-data";
    private const string ConfigBackupName = "config.json";
    private const string WorkspaceBackupName = "workspace";
    private const string JournalFileName = "migration.json";

    internal static PhoenixServerMigrationBackup? Backup(
        AppConfig config,
        string planId,
        PhoenixServerDataMigrationPaths paths,
        IPhoenixServerServiceController serviceController,
        Action<string, string> log,
        string? migrationRoot = null,
        bool useProtectedStorage = false)
    {
        _ = config;
        var hasServerData = Directory.Exists(paths.LegacyServerDataDirectory);
        var hasConfig = File.Exists(paths.LegacyConfigPath);
        var hasWorkspace = Directory.Exists(paths.LegacyWorkspaceDirectory);

        if (!hasServerData && !hasConfig && !hasWorkspace)
        {
            log("未发现需要迁移的旧版 PhoenixServer 数据。", "INFO");
            return null;
        }

        var backupRoot = migrationRoot ?? PhoenixServerMigrationStorage.GetDefaultRoot();
        var backupDirectory = GetBackupDirectory(backupRoot, planId);
        if (Directory.Exists(backupDirectory))
            throw new InvalidOperationException($"安装任务已有 PhoenixServer 数据备份: {backupDirectory}");

        log("检测到旧版 PhoenixServer 数据，正在停止 PhoenixServer 服务。", "INFO");
        var serviceWasRunning = serviceController.StopAndWait(PhoenixServerServiceName);
        log("已确认 PhoenixServer 服务停止或未安装。", "INFO");

        try
        {
            PrepareBackupDirectory(backupDirectory, migrationRoot is null || useProtectedStorage);

            if (hasServerData)
            {
                CopyDirectory(paths.LegacyServerDataDirectory, Path.Combine(backupDirectory, ServerDataBackupName));
                log("已备份旧版 server-data。", "INFO");
            }

            if (hasConfig)
            {
                CopyFile(paths.LegacyConfigPath, Path.Combine(backupDirectory, ConfigBackupName));
                log("已备份旧版 config.json。", "INFO");
            }

            if (hasWorkspace)
            {
                CopyDirectory(paths.LegacyWorkspaceDirectory, Path.Combine(backupDirectory, WorkspaceBackupName));
                log("已备份旧版 PhoenixWorkspace。", "INFO");
            }

            var backup = new PhoenixServerMigrationBackup(
                backupDirectory,
                hasServerData,
                hasConfig,
                hasWorkspace,
                serviceWasRunning);
            WriteJournal(backup, planId, PhoenixServerMigrationPhase.BackupCompleted);
            return backup;
        }
        catch (Exception ex)
        {
            RestartLegacyServiceAfterBackupFailure(serviceController, serviceWasRunning, ex);
            throw;
        }
    }

    internal static void Restore(
        PhoenixServerMigrationBackup backup,
        PhoenixServerDataMigrationPaths paths,
        Action<string, string> log,
        bool useProductionDestination = false)
    {
        var replacementTransaction = useProductionDestination
            ? new PhoenixServerMigrationReplacementTransaction()
            : null;

        try
        {
            if (useProductionDestination)
                PhoenixServerProductionDataRootSecurity.EnsureTrustedAndSecure(paths);

            if (backup.HasServerData)
            {
                CopyDirectory(
                    Path.Combine(backup.BackupDirectory, ServerDataBackupName),
                    paths.ServerDataDirectory,
                    useProductionDestination,
                    replacementTransaction);
                log("已恢复 server-data 到规范数据目录。", "INFO");
            }

            if (backup.HasConfig)
            {
                CopyFile(
                    Path.Combine(backup.BackupDirectory, ConfigBackupName),
                    paths.ConfigPath,
                    useProductionDestination,
                    replacementTransaction);
                log("已恢复 config.json 到规范数据目录。", "INFO");
            }

            if (backup.HasWorkspace)
            {
                CopyDirectory(
                    Path.Combine(backup.BackupDirectory, WorkspaceBackupName),
                    paths.WorkspaceDirectory,
                    useProductionDestination,
                    replacementTransaction);
                log("已恢复 PhoenixWorkspace 到 workspace 目录。", "INFO");
            }

            if (replacementTransaction is not null)
            {
                UpdateJournalPhase(backup, PhoenixServerMigrationPhase.CleanupPending);
                try
                {
                    replacementTransaction.Commit();
                }
                catch (Exception cleanupException)
                {
                    log($"PhoenixServer 旧目标清理失败，已保留隔离副本等待下次启动清理: {cleanupException.Message}", "WARNING");
                    return;
                }
            }

            UpdateJournalPhase(backup, PhoenixServerMigrationPhase.RestoreCompleted);
        }
        catch (Exception ex)
        {
            try
            {
                replacementTransaction?.Rollback();
            }
            catch (Exception rollbackException)
            {
                throw new InvalidOperationException(
                    $"恢复 PhoenixServer 数据失败，且无法回滚新安装目录，备份保留在: {backup.BackupDirectory}",
                    new AggregateException(ex, rollbackException));
            }

            throw new InvalidOperationException($"恢复 PhoenixServer 数据失败，备份保留在: {backup.BackupDirectory}", ex);
        }
    }

    internal static void RestoreWithServiceHandling(
        PhoenixServerMigrationBackup backup,
        PhoenixServerDataMigrationPaths paths,
        IPhoenixServerServiceController serviceController,
        Action<string, string> log,
        bool useProductionDestination,
        bool allowMissingServiceAfterFailedInstaller = false)
    {
        Exception? restoreException = null;
        try
        {
            _ = serviceController.StopAndWait(PhoenixServerServiceName);
            Restore(backup, paths, log, useProductionDestination);
        }
        catch (Exception ex)
        {
            restoreException = ex;
        }

        Exception? restartException = null;
        if (backup.ServiceWasRunning)
        {
            try
            {
                serviceController.StartAndWait(PhoenixServerServiceName);
            }
            catch (InvalidOperationException) when (allowMissingServiceAfterFailedInstaller)
            {
                log("Server 安装失败后未检测到可重启的 PhoenixServer 服务。", "WARNING");
            }
            catch (Exception ex)
            {
                restartException = ex;
            }
        }

        if (restoreException is not null && restartException is not null)
        {
            throw new InvalidOperationException(
                "恢复 PhoenixServer 数据失败，且无法恢复 PhoenixServer 服务状态。",
                new AggregateException(restoreException, restartException));
        }

        if (restoreException is not null)
            throw restoreException;

        if (restartException is not null)
        {
            throw new InvalidOperationException(
                "PhoenixServer 数据已恢复，但无法恢复服务运行状态。",
                restartException);
        }
    }

    internal static void MarkInstallerStarting(PhoenixServerMigrationBackup backup) =>
        UpdateJournalPhase(backup, PhoenixServerMigrationPhase.InstallerStarting);

    internal static void MarkInstallerStarted(
        PhoenixServerMigrationBackup backup,
        int processId,
        DateTimeOffset processStartedAt) =>
        UpdateJournalPhase(
            backup,
            PhoenixServerMigrationPhase.InstallerStarting,
            processId,
            processStartedAt);

    internal static void MarkInstallerCompleted(PhoenixServerMigrationBackup backup) =>
        UpdateJournalPhase(backup, PhoenixServerMigrationPhase.InstallerCompleted);

    internal static void RestorePendingMigrations(
        PhoenixServerDataMigrationPaths paths,
        Action<string, string> log,
        Action<string, string>? markPlanFailed = null)
    {
        var migrationRoot = PhoenixServerMigrationStorage.GetDefaultRoot();
        using var migrationLock = PhoenixServerMigrationLock.TryAcquire(migrationRoot);
        if (migrationLock is null)
        {
            log("PhoenixServer 数据迁移正在执行，跳过并发恢复。", "WARNING");
            return;
        }

        RestorePendingMigrations(
            paths,
            log,
            new PhoenixServerServiceController(),
            migrationRoot,
            useProductionDestination: true,
            requireInstallerConfirmation: true,
            markPlanFailed);
    }

    internal static void RestorePendingMigrations(
        PhoenixServerDataMigrationPaths paths,
        Action<string, string> log,
        IPhoenixServerServiceController serviceController,
        string migrationRoot,
        bool useProductionDestination,
        bool requireInstallerConfirmation = false,
        Action<string, string>? markPlanFailed = null)
    {
        if (!Directory.Exists(migrationRoot))
            return;

        foreach (var backupDirectory in Directory.EnumerateDirectories(migrationRoot))
        {
            if (useProductionDestination)
                PhoenixServerMigrationStorage.EnsureSecureDirectory(backupDirectory);

            if (!Guid.TryParseExact(Path.GetFileName(backupDirectory), "N", out var planId))
                continue;

            PhoenixServerMigrationJournal? journal;
            try
            {
                journal = ReadJournal(backupDirectory);
            }
            catch (Exception ex)
            {
                log($"PhoenixServer 迁移记录无法读取，已保留备份等待人工恢复: {backupDirectory} - {ex.Message}", "ERROR");
                continue;
            }

            if (journal is null ||
                !string.Equals(journal.PlanId, planId.ToString("N"), StringComparison.Ordinal) ||
                journal.Phase is not (
                    PhoenixServerMigrationPhase.BackupCompleted or
                    PhoenixServerMigrationPhase.InstallerStarting or
                    PhoenixServerMigrationPhase.InstallerCompleted or
                    PhoenixServerMigrationPhase.CleanupPending))
            {
                continue;
            }

            var backup = new PhoenixServerMigrationBackup(
                backupDirectory,
                journal.HasServerData,
                journal.HasConfig,
                journal.HasWorkspace,
                journal.ServiceWasRunning);
            if (!backup.HasServerData && !backup.HasConfig && !backup.HasWorkspace)
                continue;

            if (journal.Phase == PhoenixServerMigrationPhase.CleanupPending)
            {
                try
                {
                    if (useProductionDestination)
                        PhoenixServerProductionDataRootSecurity.CleanupRollbackEntries(paths);
                    UpdateJournalPhase(backup, PhoenixServerMigrationPhase.RestoreCompleted);
                }
                catch (Exception ex)
                {
                    log($"PhoenixServer 旧目标清理尚未完成，已保留隔离副本: {backupDirectory} - {ex.Message}", "WARNING");
                }

                continue;
            }

            if (journal.Phase == PhoenixServerMigrationPhase.BackupCompleted && requireInstallerConfirmation)
            {
                RestartLegacyServiceAfterUncertainInstaller(backup, serviceController, log);
                ReportMigrationRequiresAttention(
                    journal.PlanId,
                    "PhoenixServer 安装器尚未启动，已恢复旧服务并保留数据备份。请重新发起升级。",
                    log,
                    markPlanFailed);
                continue;
            }

            if (journal.Phase == PhoenixServerMigrationPhase.InstallerStarting && requireInstallerConfirmation)
            {
                if (journal.InstallerProcessId is null)
                {
                    ReportMigrationRequiresAttention(
                        journal.PlanId,
                        $"PhoenixServer 安装器启动状态无法确认，已保留备份等待人工恢复: {backupDirectory}",
                        log,
                        markPlanFailed);
                    continue;
                }

                if (IsInstallerProcessRunning(journal))
                {
                    log($"PhoenixServer 安装器仍在运行，延后数据恢复: {backupDirectory}", "WARNING");
                    continue;
                }

                if (!PhoenixServerProductionDataRootSecurity.IsNormalizedRootPresent(paths))
                {
                    RestartLegacyServiceAfterUncertainInstaller(backup, serviceController, log);
                    ReportMigrationRequiresAttention(
                        journal.PlanId,
                        "PhoenixServer 安装器已退出但未创建规范数据目录，已恢复旧服务并保留备份。",
                        log,
                        markPlanFailed);
                    continue;
                }
            }

            log($"检测到未完成的 PhoenixServer 数据恢复: {backupDirectory}", "WARNING");
            RestoreWithServiceHandling(
                backup,
                paths,
                serviceController,
                log,
                useProductionDestination);
        }
    }

    private static void RestartLegacyServiceAfterUncertainInstaller(
        PhoenixServerMigrationBackup backup,
        IPhoenixServerServiceController serviceController,
        Action<string, string> log)
    {
        if (!backup.ServiceWasRunning)
            return;

        try
        {
            serviceController.StartAndWait(PhoenixServerServiceName);
            log("安装器启动状态不确定，已尝试恢复旧版 PhoenixServer 服务。", "WARNING");
        }
        catch (InvalidOperationException)
        {
            log("安装器启动状态不确定，尚未检测到可启动的 PhoenixServer 服务，已保留备份等待恢复。", "WARNING");
        }
    }

    private static void ReportMigrationRequiresAttention(
        string planId,
        string message,
        Action<string, string> log,
        Action<string, string>? markPlanFailed)
    {
        log(message, "ERROR");
        if (markPlanFailed is null)
            return;

        try
        {
            markPlanFailed(planId, message);
        }
        catch (Exception ex)
        {
            log($"无法将 PhoenixServer 迁移任务标记为失败: {planId} - {ex.Message}", "ERROR");
        }
    }

    private static bool IsInstallerProcessRunning(PhoenixServerMigrationJournal journal)
    {
        if (journal.InstallerProcessId is not int processId)
            return false;

        try
        {
            using var process = Process.GetProcessById(processId);
            if (process.HasExited)
                return false;

            if (journal.InstallerStartedAt is not DateTimeOffset startedAt)
                return true;

            var actualStartedAt = new DateTimeOffset(process.StartTime);
            return actualStartedAt.UtcTicks == startedAt.UtcTicks;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    private static void RestartLegacyServiceAfterBackupFailure(
        IPhoenixServerServiceController serviceController,
        bool serviceWasRunning,
        Exception backupException)
    {
        if (!serviceWasRunning)
            throw new InvalidOperationException("备份 PhoenixServer 数据失败。", backupException);

        try
        {
            serviceController.StartAndWait(PhoenixServerServiceName);
        }
        catch (Exception restartException)
        {
            throw new InvalidOperationException(
                "备份 PhoenixServer 数据失败，且无法重新启动旧服务。",
                new AggregateException(backupException, restartException));
        }

        throw new InvalidOperationException("备份 PhoenixServer 数据失败，旧服务已重新启动。", backupException);
    }

    private static string GetBackupDirectory(string backupRoot, string planId)
    {
        if (!Guid.TryParseExact(planId, "N", out var parsedPlanId))
            throw new InvalidOperationException("安装任务编号无效，无法创建 PhoenixServer 数据备份。");

        return Path.Combine(backupRoot, parsedPlanId.ToString("N"));
    }

    private static void WriteJournal(
        PhoenixServerMigrationBackup backup,
        string planId,
        PhoenixServerMigrationPhase phase,
        int? installerProcessId = null,
        DateTimeOffset? installerStartedAt = null)
    {
        var journal = new PhoenixServerMigrationJournal(
            planId,
            backup.HasServerData,
            backup.HasConfig,
            backup.HasWorkspace,
            backup.ServiceWasRunning,
            phase,
            installerProcessId,
            installerStartedAt);
        var journalPath = Path.Combine(backup.BackupDirectory, JournalFileName);
        var temporaryPath = string.Concat(journalPath, ".tmp");
        var json = System.Text.Json.JsonSerializer.Serialize(
            journal,
            AppConfigJsonContext.Default.PhoenixServerMigrationJournal);

        using (var temporaryFile = new FileStream(
                   temporaryPath,
                   FileMode.Create,
                   FileAccess.Write,
                   FileShare.None))
        using (var writer = new StreamWriter(temporaryFile))
        {
            writer.Write(json);
            writer.Flush();
            temporaryFile.Flush(flushToDisk: true);
        }

        File.Move(temporaryPath, journalPath, overwrite: true);
    }

    private static PhoenixServerMigrationJournal? ReadJournal(string backupDirectory)
    {
        var journalPath = Path.Combine(backupDirectory, JournalFileName);
        if (!File.Exists(journalPath))
            return null;

        EnsureNotReparsePoint(journalPath);
        var json = File.ReadAllText(journalPath);
        return System.Text.Json.JsonSerializer.Deserialize(
            json,
            AppConfigJsonContext.Default.PhoenixServerMigrationJournal);
    }

    private static void UpdateJournalPhase(
        PhoenixServerMigrationBackup backup,
        PhoenixServerMigrationPhase phase,
        int? installerProcessId = null,
        DateTimeOffset? installerStartedAt = null)
    {
        var journal = ReadJournal(backup.BackupDirectory)
            ?? throw new InvalidOperationException($"PhoenixServer 迁移记录不存在: {backup.BackupDirectory}");

        WriteJournal(
            backup,
            journal.PlanId,
            phase,
            installerProcessId ?? journal.InstallerProcessId,
            installerStartedAt ?? journal.InstallerStartedAt);
    }

    private static void PrepareBackupDirectory(string directoryPath, bool useProtectedStorage)
    {
        if (useProtectedStorage)
        {
            PhoenixServerMigrationStorage.EnsureSecureDirectory(directoryPath);
            return;
        }

        Directory.CreateDirectory(directoryPath);
    }

    private static void CopyDirectory(
        string sourceDirectory,
        string destinationDirectory,
        bool useProductionDestination = false,
        PhoenixServerMigrationReplacementTransaction? replacementTransaction = null)
    {
        EnsureNotReparsePoint(sourceDirectory);
        if (!useProductionDestination)
        {
            CopyDirectoryContents(sourceDirectory, destinationDirectory);
            return;
        }

        var destinationParent = Path.GetDirectoryName(destinationDirectory)
            ?? throw new InvalidOperationException($"无法解析迁移目标目录: {destinationDirectory}");
        var destinationName = Path.GetFileName(destinationDirectory);
        var stagingDirectory = Path.Combine(
            destinationParent,
            $".{destinationName}.migration-{Guid.NewGuid():N}");

        try
        {
            CopyDirectoryContents(sourceDirectory, stagingDirectory);
            ReplaceDestinationEntry(stagingDirectory, destinationDirectory, replacementTransaction);
        }
        catch
        {
            TryDeleteDirectory(stagingDirectory);
            throw;
        }
    }

    private static void CopyDirectoryContents(string sourceDirectory, string destinationDirectory)
    {
        EnsureNotReparsePoint(sourceDirectory);
        Directory.CreateDirectory(destinationDirectory);

        foreach (var sourceEntry in Directory.EnumerateFileSystemEntries(sourceDirectory))
        {
            EnsureNotReparsePoint(sourceEntry);
            var destinationEntry = Path.Combine(destinationDirectory, Path.GetFileName(sourceEntry));
            if (Directory.Exists(sourceEntry))
            {
                CopyDirectoryContents(sourceEntry, destinationEntry);
            }
            else
            {
                File.Copy(sourceEntry, destinationEntry, overwrite: true);
            }
        }
    }

    private static void CopyFile(
        string sourcePath,
        string destinationPath,
        bool useProductionDestination = false,
        PhoenixServerMigrationReplacementTransaction? replacementTransaction = null)
    {
        EnsureNotReparsePoint(sourcePath);
        var destinationDirectory = Path.GetDirectoryName(destinationPath)
            ?? throw new InvalidOperationException($"无法解析迁移目标文件目录: {destinationPath}");

        if (!useProductionDestination)
        {
            Directory.CreateDirectory(destinationDirectory);
            File.Copy(sourcePath, destinationPath, overwrite: true);
            return;
        }

        var stagingPath = Path.Combine(
            destinationDirectory,
            $".{Path.GetFileName(destinationPath)}.migration-{Guid.NewGuid():N}");
        try
        {
            File.Copy(sourcePath, stagingPath, overwrite: false);
            ReplaceDestinationEntry(stagingPath, destinationPath, replacementTransaction);
        }
        catch
        {
            TryDeleteFile(stagingPath);
            throw;
        }
    }

    private static void ReplaceDestinationEntry(
        string stagingPath,
        string destinationPath,
        PhoenixServerMigrationReplacementTransaction? replacementTransaction)
    {
        (replacementTransaction ?? throw new InvalidOperationException("缺少 PhoenixServer 迁移替换事务。"))
            .Replace(stagingPath, destinationPath);
    }

    private static void TryDeleteDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
            Directory.Delete(directoryPath, recursive: true);
    }

    private static void TryDeleteFile(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    private static void EnsureNotReparsePoint(string path) =>
        PhoenixServerMigrationStorage.EnsureNotReparsePoint(path);
}
