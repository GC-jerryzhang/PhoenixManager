using System.Diagnostics;
using System.Text.RegularExpressions;
using PhoenixManager.Models;

namespace PhoenixManager.Services;

public static class FetchService
{
    private const int StabilityWaitMs = 5000;

    public static string Execute(AppConfig config)
    {
        var logFile = Path.Combine(config.LogDir, $"fetch-phoenix_{DateTime.Now:yyyyMMdd}.log");
        var log = new List<string>();

        void Log(string message, string level = "INFO")
        {
            var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            log.Add(entry);
        }

        try
        {
            EnsureDirectories(config);
            Log("=== Fetch cycle started ===");

            if (!Directory.Exists(config.SourceDir))
            {
                Log($"Source directory not accessible: {config.SourceDir}", "ERROR");
                WriteLog(logFile, log);
                return string.Join(Environment.NewLine, log);
            }

            var copiedDesigner = FetchDesigner(config, Log);
            var copiedServer = FetchServer(config, Log);

            Log("=== Fetch cycle completed ===");

            NotificationService.ShowFetchResult(copiedDesigner, copiedServer);
        }
        catch (Exception ex)
        {
            Log($"Unexpected error: {ex.Message}", "ERROR");
        }

        WriteLog(logFile, log);
        return string.Join(Environment.NewLine, log);
    }

    private static string? FetchDesigner(AppConfig config, Action<string, string> log)
    {
        log("--- Designer check ---", "INFO");

        var designerFiles = Directory.GetFiles(config.SourceDir, "Phoenix-Windows-*.exe")
            .Select(f => new FileInfo(f))
            .Where(f => !f.Name.Contains("Server", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => f.LastWriteTime)
            .ToList();

        if (designerFiles.Count == 0)
        {
            log("No Designer installer found in source.", "WARNING");
            return null;
        }

        var latest = designerFiles[0];
        log($"Latest Designer: {latest.Name} (Modified: {latest.LastWriteTime})", "INFO");

        if (!IsFileStable(latest.FullName))
        {
            log("Designer file still being written, skipping this cycle.", "WARNING");
            return null;
        }

        var versionInfo = FileVersionInfo.GetVersionInfo(latest.FullName);
        var buildNum = versionInfo.FilePrivatePart;

        if (buildNum == 0)
        {
            log($"Could not read FilePrivatePart from {latest.Name}, skipping.", "WARNING");
            return null;
        }

        var destName = $"{buildNum}Phoenix-Windows-0.0.1-Setup.exe";
        var destPath = Path.Combine(config.DesignerDir, destName);

        if (File.Exists(destPath))
        {
            log($"Designer already exists: {destName} (skipped)", "INFO");
            return null;
        }

        log($"Copying Designer -> {destName}", "INFO");
        File.Copy(latest.FullName, destPath, overwrite: false);
        var size = new FileInfo(destPath).Length;
        log($"Designer copied successfully: {destName} (Size: {size} bytes)", "INFO");
        return destName;
    }

    private static string? FetchServer(AppConfig config, Action<string, string> log)
    {
        log("--- Server check ---", "INFO");

        var serverFiles = Directory.GetFiles(config.SourceDir, "Phoenix-Server-Windows-*.exe")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .ToList();

        if (serverFiles.Count == 0)
        {
            log("No Server installer found in source.", "WARNING");
            return null;
        }

        var latest = serverFiles[0];
        log($"Latest Server: {latest.Name} (Modified: {latest.LastWriteTime})", "INFO");

        if (!IsFileStable(latest.FullName))
        {
            log("Server file still being written, skipping this cycle.", "WARNING");
            return null;
        }

        var match = Regex.Match(latest.Name, @"Phoenix-Server-Windows-(\d{12})\.exe");
        if (!match.Success)
        {
            log($"Could not parse timestamp from Server filename: {latest.Name}", "WARNING");
            return null;
        }

        var timestamp = match.Groups[1].Value;
        var destName = $"{timestamp}Phoenix-Server-Windows-Setup.exe";
        var destPath = Path.Combine(config.ServerDir, destName);

        if (File.Exists(destPath))
        {
            log($"Server already exists: {destName} (skipped)", "INFO");
            return null;
        }

        log($"Copying Server -> {destName}", "INFO");
        File.Copy(latest.FullName, destPath, overwrite: false);
        var size = new FileInfo(destPath).Length;
        log($"Server copied successfully: {destName} (Size: {size} bytes)", "INFO");
        return destName;
    }

    private static bool IsFileStable(string path)
    {
        var size1 = new FileInfo(path).Length;
        Thread.Sleep(StabilityWaitMs);
        var size2 = new FileInfo(path).Length;
        return size1 == size2;
    }

    private static void EnsureDirectories(AppConfig config)
    {
        Directory.CreateDirectory(config.DesignerDir);
        Directory.CreateDirectory(config.ServerDir);
        Directory.CreateDirectory(config.LogDir);
    }

    private static void WriteLog(string logFile, List<string> log)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logFile)!);
        File.AppendAllLines(logFile, log);
    }
}
