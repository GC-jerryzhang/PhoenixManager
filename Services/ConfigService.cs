using System.Text.Json;
using System.Text.Json.Serialization;
using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

[JsonSerializable(typeof(AppConfig))]
[JsonSerializable(typeof(CloseBehaviorPreference))]
[JsonSerializable(typeof(FetchedPackageInfo))]
[JsonSerializable(typeof(InstallPlan))]
[JsonSerializable(typeof(Uri))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AppConfigJsonContext : JsonSerializerContext;

public static class ConfigService
{
    public static string ConfigPath
    {
        get
        {
            return Path.Combine(RuntimeModeService.StorageRoot, "config.json");
        }
    }

    public static AppConfig Load()
    {
        var path = ConfigPath;
        if (!File.Exists(path))
        {
            var defaults = CreateDefaultConfig();
            Save(defaults);
            return defaults;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize(json, AppConfigJsonContext.Default.AppConfig) ?? CreateDefaultConfig();
    }

    public static void Save(AppConfig config)
    {
        var configDirectory = Path.GetDirectoryName(ConfigPath);
        if (!string.IsNullOrWhiteSpace(configDirectory))
            Directory.CreateDirectory(configDirectory);

        var json = JsonSerializer.Serialize(config, AppConfigJsonContext.Default.AppConfig);
        File.WriteAllText(ConfigPath, json);
    }

    private static AppConfig CreateDefaultConfig()
    {
        if (!RuntimeModeService.IsDevelopment)
            return new AppConfig();

        return new AppConfig(
            LocalBaseDir: Path.Combine(RuntimeModeService.StorageRoot, "historyPackage"));
    }
}
