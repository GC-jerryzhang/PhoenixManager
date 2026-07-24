using System.Diagnostics;

namespace PhoenixToolkit.Services;

internal sealed class PhoenixServerServiceController : IPhoenixServerServiceController
{
    private const int StopTimeoutSeconds = 120;
    private const int PollIntervalMilliseconds = 1000;

    public bool StopAndWait(string serviceName)
    {
        var initialStatus = QueryStatus(serviceName);
        if (initialStatus is ServiceStatus.NotInstalled or ServiceStatus.Stopped)
            return false;

        var stopResult = RunServiceCommand("stop", serviceName);
        if (stopResult.ExitCode != 0 && !ContainsErrorCode(stopResult.Output, 1062))
            throw new InvalidOperationException($"停止服务 {serviceName} 失败: {stopResult.Output}");

        for (var attempt = 0; attempt < StopTimeoutSeconds; attempt++)
        {
            Thread.Sleep(PollIntervalMilliseconds);
            var status = QueryStatus(serviceName);
            if (status is ServiceStatus.NotInstalled or ServiceStatus.Stopped)
                return true;
        }

        throw new TimeoutException($"等待服务 {serviceName} 停止超时。");
    }

    public void StartAndWait(string serviceName)
    {
        var initialStatus = QueryStatus(serviceName);
        if (initialStatus == ServiceStatus.Running)
            return;
        if (initialStatus == ServiceStatus.NotInstalled)
            throw new InvalidOperationException($"服务 {serviceName} 不存在，无法启动。");

        var startResult = RunServiceCommand("start", serviceName);
        if (startResult.ExitCode != 0 && !ContainsErrorCode(startResult.Output, 1056))
            throw new InvalidOperationException($"启动服务 {serviceName} 失败: {startResult.Output}");

        for (var attempt = 0; attempt < StopTimeoutSeconds; attempt++)
        {
            Thread.Sleep(PollIntervalMilliseconds);
            if (QueryStatus(serviceName) == ServiceStatus.Running)
                return;
        }

        throw new TimeoutException($"等待服务 {serviceName} 启动超时。");
    }

    private static ServiceStatus QueryStatus(string serviceName)
    {
        var result = RunServiceCommand("query", serviceName);
        if (result.ExitCode != 0)
        {
            if (ContainsErrorCode(result.Output, 1060))
                return ServiceStatus.NotInstalled;

            throw new InvalidOperationException($"查询服务 {serviceName} 状态失败: {result.Output}");
        }

        return result.Output.Contains("STOPPED", StringComparison.OrdinalIgnoreCase)
            ? ServiceStatus.Stopped
            : ServiceStatus.Running;
    }

    private static ServiceCommandResult RunServiceCommand(string command, string serviceName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.SystemDirectory, "sc.exe"),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add(command);
        startInfo.ArgumentList.Add(serviceName);

        if (!File.Exists(startInfo.FileName))
            throw new FileNotFoundException("未找到 Windows 服务控制命令。", startInfo.FileName);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"无法启动服务控制命令: {command}");
        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ServiceCommandResult(process.ExitCode, output.Trim());
    }

    private static bool ContainsErrorCode(string output, int errorCode) =>
        output.Contains(errorCode.ToString(), StringComparison.Ordinal);

    private enum ServiceStatus
    {
        Running,
        Stopped,
        NotInstalled
    }

    private sealed record ServiceCommandResult(int ExitCode, string Output);
}
