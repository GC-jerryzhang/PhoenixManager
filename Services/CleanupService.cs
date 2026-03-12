using System.Text.RegularExpressions;
using PhoenixManager.Models;

namespace PhoenixManager.Services;

public static class CleanupService
{
    public static string Execute(AppConfig config)
    {
        var logFile = Path.Combine(config.LogDir, $"cleanup-phoenix_{DateTime.Now:yyyyMMdd}.log");
        var log = new List<string>();

        void Log(string message, string level = "INFO")
        {
            var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            log.Add(entry);
        }

        try
        {
            Directory.CreateDirectory(config.LogDir);
            Log("=== Cleanup started ===");

            TieredCleanup(config.DesignerDir, "Designer", config.CleanupWeeks, Log);
            TieredCleanup(config.ServerDir, "Server", config.CleanupWeeks, Log);
            CleanupLogs(config.LogDir, 30, Log);

            Log("=== Cleanup completed ===");
        }
        catch (Exception ex)
        {
            Log($"Unexpected error: {ex.Message}", "ERROR");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(logFile)!);
        File.AppendAllLines(logFile, log);
        return string.Join(Environment.NewLine, log);
    }

    private static void TieredCleanup(
        string folder, string label, CleanupWeeks weeks, Action<string, string> log)
    {
        if (!Directory.Exists(folder))
        {
            log($"{label} folder not found: {folder}", "WARNING");
            return;
        }

        var allFiles = Directory.GetFiles(folder, "*.exe")
            .Select(f => new FileInfo(f))
            .ToList();

        if (allFiles.Count == 0)
        {
            log($"{label}: no files to process.", "INFO");
            return;
        }

        log($"--- {label} cleanup: {allFiles.Count} files ---", "INFO");

        var now = DateTime.Now;
        var cutoffKeepAll = now.AddDays(-weeks.KeepAllWeeks * 7);
        var cutoffKeepDaily = now.AddDays(-weeks.KeepDailyWeeks * 7);
        var cutoffDelete = now.AddDays(-weeks.DeleteAfterWeeks * 7);

        int deleted = 0, kept = 0;

        foreach (var file in allFiles)
        {
            // Bad filename check
            if (Regex.IsMatch(file.Name, @"Setu\.exe$|Setup \.exe$"))
            {
                log($"Delete (bad name): {file.Name}", "WARNING");
                file.Delete();
                deleted++;
                continue;
            }

            bool keep;

            if (file.LastWriteTime > cutoffKeepAll)
            {
                // Within keepAllWeeks: keep all
                keep = true;
            }
            else if (file.LastWriteTime > cutoffDelete)
            {
                // Between keepAllWeeks and deleteAfterWeeks: keep first build per day
                keep = IsFirstBuildOfDay(file, allFiles);
            }
            else
            {
                // Older than deleteAfterWeeks: delete
                keep = false;
            }

            if (keep)
            {
                log($"Keep: {file.Name}", "INFO");
                kept++;
            }
            else
            {
                log($"Delete: {file.Name}", "WARNING");
                file.Delete();
                deleted++;
            }
        }

        log($"{label} summary: kept={kept}, deleted={deleted}", "INFO");
    }

    private static bool IsFirstBuildOfDay(FileInfo file, List<FileInfo> allFiles)
    {
        var day = file.LastWriteTime.Date;
        var firstOfDay = allFiles
            .Where(f => f.LastWriteTime.Date == day)
            .Where(f => !Regex.IsMatch(f.Name, @"Setu\.exe$|Setup \.exe$"))
            .OrderBy(f => GetSortKey(f.Name))
            .FirstOrDefault();

        return firstOfDay != null &&
               string.Equals(file.Name, firstOfDay.Name, StringComparison.OrdinalIgnoreCase);
    }

    private static long GetSortKey(string name)
    {
        var match = Regex.Match(name, @"^(\d+)");
        return match.Success ? long.Parse(match.Groups[1].Value) : 0;
    }

    private static void CleanupLogs(string logDir, int retainDays, Action<string, string> log)
    {
        if (!Directory.Exists(logDir))
            return;

        var cutoff = DateTime.Now.AddDays(-retainDays);
        var logFiles = Directory.GetFiles(logDir, "*.log")
            .Select(f => new FileInfo(f))
            .Where(f => f.LastWriteTime < cutoff)
            .ToList();

        if (logFiles.Count == 0)
        {
            log($"Logs: nothing to clean (retaining {retainDays} days).", "INFO");
            return;
        }

        log($"--- Log cleanup: {logFiles.Count} files older than {retainDays} days ---", "INFO");

        foreach (var file in logFiles)
        {
            log($"Delete log: {file.Name}", "INFO");
            file.Delete();
        }
    }
}
