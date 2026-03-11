using System.Diagnostics;
using PhoenixManager.Models;

namespace PhoenixManager.Services;

public static class SchedulerService
{
    private const string FetchTaskName = "FetchPhoenixInstaller";
    private const string CleanupTaskName = "CleanupPhoenixInstaller";

    public static string Install(AppConfig config)
    {
        var exePath = GetExePath();
        var results = new List<string>();

        // --- Fetch task: daily with repetition interval ---
        UnregisterIfExists(FetchTaskName);

        var fetchInterval = config.FetchIntervalMinutes;
        var fetchXml = BuildFetchTaskXml(exePath, fetchInterval);
        var fetchResult = RegisterFromXml(FetchTaskName, fetchXml);
        results.Add($"[Fetch] {fetchResult}");

        // --- Cleanup task: weekly Mon-Fri at configured time ---
        UnregisterIfExists(CleanupTaskName);

        var cleanupXml = BuildCleanupTaskXml(exePath, config.CleanupTime);
        var cleanupResult = RegisterFromXml(CleanupTaskName, cleanupXml);
        results.Add($"[Cleanup] {cleanupResult}");

        return string.Join(Environment.NewLine, results);
    }

    public static string Uninstall()
    {
        var results = new List<string>();

        foreach (var taskName in new[] { FetchTaskName, CleanupTaskName })
        {
            var result = UnregisterIfExists(taskName);
            results.Add($"[{taskName}] {result}");
        }

        return string.Join(Environment.NewLine, results);
    }

    public static (bool fetchInstalled, bool cleanupInstalled) GetStatus()
    {
        return (TaskExists(FetchTaskName), TaskExists(CleanupTaskName));
    }

    private static string GetExePath()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath))
        {
            exePath = Process.GetCurrentProcess().MainModule?.FileName
                      ?? throw new InvalidOperationException("Cannot determine exe path.");
        }
        return exePath;
    }

    private static bool TaskExists(string taskName)
    {
        var (exitCode, _) = RunSchtasks($"/Query /TN \"{taskName}\"");
        return exitCode == 0;
    }

    private static string UnregisterIfExists(string taskName)
    {
        if (!TaskExists(taskName))
            return $"{taskName} not found (nothing to remove).";

        var (exitCode, output) = RunSchtasks($"/Delete /TN \"{taskName}\" /F");
        return exitCode == 0
            ? $"{taskName} removed."
            : $"Failed to remove {taskName}: {output}";
    }

    private static string RegisterFromXml(string taskName, string xml)
    {
        // Write XML to temp file, register, then delete temp file
        var tempFile = Path.Combine(Path.GetTempPath(), $"{taskName}_{Guid.NewGuid():N}.xml");
        try
        {
            File.WriteAllText(tempFile, xml);
            var (exitCode, output) = RunSchtasks($"/Create /TN \"{taskName}\" /XML \"{tempFile}\" /F");
            return exitCode == 0
                ? $"{taskName} registered successfully."
                : $"Failed to register {taskName}: {output}";
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static string BuildFetchTaskXml(string exePath, int intervalMinutes)
    {
        // Calculate repetition interval in ISO 8601 duration
        var hours = intervalMinutes / 60;
        var mins = intervalMinutes % 60;
        var duration = hours > 0 ? $"PT{hours}H{mins}M" : $"PT{mins}M";

        var username = $"{Environment.UserDomainName}\\{Environment.UserName}";

        return $"""
            <?xml version="1.0" encoding="UTF-16"?>
            <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
              <RegistrationInfo>
                <Description>Fetch latest Phoenix Designer and Server installers (every {intervalMinutes} min)</Description>
              </RegistrationInfo>
              <Triggers>
                <CalendarTrigger>
                  <Repetition>
                    <Interval>{duration}</Interval>
                    <Duration>P1D</Duration>
                    <StopAtDurationEnd>false</StopAtDurationEnd>
                  </Repetition>
                  <StartBoundary>2024-01-01T00:00:00</StartBoundary>
                  <Enabled>true</Enabled>
                  <ScheduleByDay>
                    <DaysInterval>1</DaysInterval>
                  </ScheduleByDay>
                </CalendarTrigger>
              </Triggers>
              <Principals>
                <Principal id="Author">
                  <UserId>{username}</UserId>
                  <LogonType>InteractiveToken</LogonType>
                  <RunLevel>HighestAvailable</RunLevel>
                </Principal>
              </Principals>
              <Settings>
                <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
                <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
                <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
                <AllowHardTerminate>true</AllowHardTerminate>
                <StartWhenAvailable>true</StartWhenAvailable>
                <AllowStartOnDemand>true</AllowStartOnDemand>
                <Enabled>true</Enabled>
                <ExecutionTimeLimit>PT10M</ExecutionTimeLimit>
              </Settings>
              <Actions Context="Author">
                <Exec>
                  <Command>{SecurityElement(exePath)}</Command>
                  <Arguments>--fetch</Arguments>
                </Exec>
              </Actions>
            </Task>
            """;
    }

    private static string BuildCleanupTaskXml(string exePath, string cleanupTime)
    {
        var username = $"{Environment.UserDomainName}\\{Environment.UserName}";

        return $"""
            <?xml version="1.0" encoding="UTF-16"?>
            <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
              <RegistrationInfo>
                <Description>Cleanup old Phoenix installers (Mon-Fri at {cleanupTime})</Description>
              </RegistrationInfo>
              <Triggers>
                <CalendarTrigger>
                  <StartBoundary>2024-01-01T{cleanupTime}:00</StartBoundary>
                  <Enabled>true</Enabled>
                  <ScheduleByWeek>
                    <WeeksInterval>1</WeeksInterval>
                    <DaysOfWeek>
                      <Monday />
                      <Tuesday />
                      <Wednesday />
                      <Thursday />
                      <Friday />
                    </DaysOfWeek>
                  </ScheduleByWeek>
                </CalendarTrigger>
              </Triggers>
              <Principals>
                <Principal id="Author">
                  <UserId>{username}</UserId>
                  <LogonType>InteractiveToken</LogonType>
                  <RunLevel>HighestAvailable</RunLevel>
                </Principal>
              </Principals>
              <Settings>
                <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
                <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
                <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
                <AllowHardTerminate>true</AllowHardTerminate>
                <StartWhenAvailable>true</StartWhenAvailable>
                <AllowStartOnDemand>true</AllowStartOnDemand>
                <Enabled>true</Enabled>
                <ExecutionTimeLimit>PT5M</ExecutionTimeLimit>
              </Settings>
              <Actions Context="Author">
                <Exec>
                  <Command>{SecurityElement(exePath)}</Command>
                  <Arguments>--cleanup</Arguments>
                </Exec>
              </Actions>
            </Task>
            """;
    }

    private static string SecurityElement(string value)
    {
        return System.Security.SecurityElement.Escape(value) ?? value;
    }

    private static (int exitCode, string output) RunSchtasks(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "schtasks.exe",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        var output = string.IsNullOrWhiteSpace(stderr) ? stdout : $"{stdout}\n{stderr}";
        return (process.ExitCode, output.Trim());
    }
}
