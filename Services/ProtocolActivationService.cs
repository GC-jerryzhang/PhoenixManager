using System.Diagnostics;
using Microsoft.Win32;

namespace PhoenixToolkit.Services;

public static class ProtocolActivationService
{
    public const string Scheme = "phoenixtoolkit";
    private const string InstallHost = "install";

    public static void EnsureRegistered()
    {
        if (RuntimeModeService.IsDevelopment)
            return;

        var exePath = Environment.ProcessPath
            ?? Process.GetCurrentProcess().MainModule?.FileName
            ?? throw new InvalidOperationException("Cannot determine exe path.");

        using var rootKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{Scheme}");
        rootKey?.SetValue(string.Empty, "URL:PhoenixToolkit Protocol");
        rootKey?.SetValue("URL Protocol", string.Empty);

        using var iconKey = rootKey?.CreateSubKey("DefaultIcon");
        iconKey?.SetValue(string.Empty, $"\"{exePath}\",0");

        using var commandKey = rootKey?.CreateSubKey(@"shell\open\command");
        commandKey?.SetValue(string.Empty, $"\"{exePath}\" \"%1\"");
    }

    public static bool TryHandleLaunchArgs(string[] args)
    {
        var protocolArg = args.FirstOrDefault(arg =>
            arg.StartsWith($"{Scheme}:", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(protocolArg))
            return false;

        if (!Uri.TryCreate(protocolArg, UriKind.Absolute, out var activationUri) ||
            !string.Equals(activationUri.Scheme, Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(activationUri.Host, InstallHost, StringComparison.OrdinalIgnoreCase))
        {
            NotificationService.ShowInstallFailure($"不支持的通知操作: {activationUri.Host}");
            return true;
        }

        if (!TryGetQueryValue(activationUri, "planId", out var planId) ||
            string.IsNullOrWhiteSpace(planId))
        {
            NotificationService.ShowInstallFailure("通知缺少安装任务编号，无法执行安装。");
            return true;
        }

        InstallActivationService.ExecuteInstallPlan(planId);
        return true;
    }

    public static Uri BuildInstallUri(string planId)
    {
        return new Uri($"{Scheme}://{InstallHost}?planId={Uri.EscapeDataString(planId)}");
    }

    private static bool TryGetQueryValue(Uri uri, string key, out string value)
    {
        value = string.Empty;

        var query = uri.Query;
        if (string.IsNullOrEmpty(query))
            return false;

        foreach (var segment in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Split('=', 2);
            var currentKey = Uri.UnescapeDataString(parts[0]);
            if (!string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
                continue;

            value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
            return true;
        }

        return false;
    }
}
