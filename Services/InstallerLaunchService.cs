using System.Diagnostics;
using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

public static class InstallerLaunchService
{
    public static void ExecutePlan(AppConfig config, InstallPlan plan, Action<string, string> log)
    {
        var isDevelopment = RuntimeModeService.IsDevelopment;
        ExecutePlanWithProcessObserver(
            config,
            plan,
            log,
            LaunchAndWait,
            isDevelopment
                ? PhoenixServerDataMigrationPaths.CreateDevelopment(config.LocalBaseDir)
                : PhoenixServerDataMigrationPaths.CreateDefault(),
            isDevelopment
                ? new NoOpPhoenixServerServiceController()
                : new PhoenixServerServiceController(),
            isDevelopment ? config.ServerMigrationDir : null);
    }

    internal static void ExecutePlan(
        AppConfig config,
        InstallPlan plan,
        Action<string, string> log,
        Action<FetchedPackageInfo> launchAndWait,
        PhoenixServerDataMigrationPaths migrationPaths,
        IPhoenixServerServiceController serviceController,
        string? migrationRoot = null)
    {
        ExecutePlanWithProcessObserver(
            config,
            plan,
            log,
            (package, _) => launchAndWait(package),
            migrationPaths,
            serviceController,
            migrationRoot);
    }

    private static void ExecutePlanWithProcessObserver(
        AppConfig config,
        InstallPlan plan,
        Action<string, string> log,
        Action<FetchedPackageInfo, Action<InstallerProcessIdentity>> launchAndWait,
        PhoenixServerDataMigrationPaths migrationPaths,
        IPhoenixServerServiceController serviceController,
        string? migrationRoot = null)
    {
        if (plan.DesignerPackage is null && plan.ServerPackage is null)
            throw new InvalidOperationException("安装任务中没有可执行的安装包。");

        if (plan.DesignerPackage is not null)
            launchAndWait(plan.DesignerPackage, _ => { });

        if (plan.ServerPackage is not null)
        {
            var useProductionDestination = migrationRoot is null;
            var effectiveMigrationRoot = migrationRoot ?? PhoenixServerMigrationStorage.GetDefaultRoot();
            using var migrationLock = PhoenixServerMigrationLock.Acquire(effectiveMigrationRoot);
            var backup = PhoenixServerDataMigrationService.Backup(
                config,
                plan.Id,
                migrationPaths,
                serviceController,
                log,
                effectiveMigrationRoot,
                useProtectedStorage: useProductionDestination);

            if (backup is not null)
                PhoenixServerDataMigrationService.MarkInstallerStarting(backup);

            try
            {
                launchAndWait(
                    plan.ServerPackage,
                    processIdentity =>
                    {
                        if (backup is not null)
                        {
                            PhoenixServerDataMigrationService.MarkInstallerStarted(
                                backup,
                                processIdentity.ProcessId,
                                processIdentity.StartedAt);
                        }
                    });
            }
            catch (Exception installerException)
            {
                RestoreAfterFailedInstaller(
                    backup,
                    migrationPaths,
                    serviceController,
                    log,
                    useProductionDestination,
                    installerException);
                throw;
            }

            if (backup is not null)
            {
                PhoenixServerDataMigrationService.MarkInstallerCompleted(backup);
                RestoreAfterInstaller(backup, migrationPaths, serviceController, log, useProductionDestination);
            }
        }
    }

    private static void RestoreAfterInstaller(
        PhoenixServerMigrationBackup backup,
        PhoenixServerDataMigrationPaths migrationPaths,
        IPhoenixServerServiceController serviceController,
        Action<string, string> log,
        bool useProductionDestination)
    {
        PhoenixServerDataMigrationService.RestoreWithServiceHandling(
            backup,
            migrationPaths,
            serviceController,
            log,
            useProductionDestination);
    }

    private static void RestoreAfterFailedInstaller(
        PhoenixServerMigrationBackup? backup,
        PhoenixServerDataMigrationPaths migrationPaths,
        IPhoenixServerServiceController serviceController,
        Action<string, string> log,
        bool useProductionDestination,
        Exception installerException)
    {
        if (backup is null)
            return;

        try
        {
            PhoenixServerDataMigrationService.RestoreWithServiceHandling(
                backup,
                migrationPaths,
                serviceController,
                log,
                useProductionDestination,
                allowMissingServiceAfterFailedInstaller: true);
        }
        catch (Exception restoreException)
        {
            throw new InvalidOperationException(
                $"Server 安装失败，且恢复 PhoenixServer 数据失败，备份保留在: {backup.BackupDirectory}",
                new AggregateException(installerException, restoreException));
        }
    }

    private static void LaunchAndWait(
        FetchedPackageInfo package,
        Action<InstallerProcessIdentity> processStarted)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = package.FullPath,
            WorkingDirectory = Path.GetDirectoryName(package.FullPath) ?? AppContext.BaseDirectory,
            UseShellExecute = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"无法启动安装包: {package.FileName}");

        WaitForInstallerProcessAfterStart(
            process,
            () => processStarted(new InstallerProcessIdentity(
                process.Id,
                new DateTimeOffset(process.StartTime))));
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"安装包执行失败: {package.FileName}，退出码 {process.ExitCode}");
    }

    internal static void WaitForInstallerProcessAfterStart(Process process, Action processStarted)
    {
        try
        {
            processStarted();
        }
        catch (Exception journalException)
        {
            try
            {
                process.WaitForExit();
            }
            catch (Exception waitException)
            {
                throw new InvalidOperationException(
                    "安装器已启动，但无法持久化迁移记录且无法确认安装器已退出。",
                    new AggregateException(journalException, waitException));
            }

            throw new InvalidOperationException(
                "安装器已启动，但无法持久化迁移记录；已等待安装器退出后终止恢复流程。",
                journalException);
        }

        process.WaitForExit();
    }

    private sealed record InstallerProcessIdentity(int ProcessId, DateTimeOffset StartedAt);
}
