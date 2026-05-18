using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

public static class RuntimeModeService
{
    private const string DevArgument = "--dev";
    private const string DevRootEnvironmentVariable = "PHOENIXTOOLKIT_DEV_ROOT";

    private static AppRuntimeMode _currentMode = AppRuntimeMode.Release;

    public static AppRuntimeMode CurrentMode => _currentMode;

    public static bool IsDevelopment => _currentMode == AppRuntimeMode.Development;

    public static string DisplaySuffix => IsDevelopment ? " [DEV]" : string.Empty;

    public static string StorageRoot => IsDevelopment
        ? ResolveDevelopmentStorageRoot()
        : AppContext.BaseDirectory;

    public static string[] Initialize(string[] args)
    {
        _currentMode = args.Any(arg =>
            string.Equals(arg, DevArgument, StringComparison.OrdinalIgnoreCase))
            ? AppRuntimeMode.Development
            : AppRuntimeMode.Release;

        return args
            .Where(arg => !string.Equals(arg, DevArgument, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static string ResolveDevelopmentStorageRoot()
    {
        var configuredRoot = Environment.GetEnvironmentVariable(DevRootEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configuredRoot))
            return configuredRoot;

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PhoenixToolkit",
            "dev");
    }
}
