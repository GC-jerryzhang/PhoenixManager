using System.Text.Json;
using System.Text.Json.Serialization;
using PhoenixManager.Models;

namespace PhoenixManager.Services;

[JsonSerializable(typeof(AppConfig))]
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
            var exeDir = AppContext.BaseDirectory;
            return Path.Combine(exeDir, "config.json");
        }
    }

    public static AppConfig Load()
    {
        var path = ConfigPath;
        if (!File.Exists(path))
        {
            var defaults = new AppConfig();
            Save(defaults);
            return defaults;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize(json, AppConfigJsonContext.Default.AppConfig) ?? new AppConfig();
    }

    public static void Save(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config, AppConfigJsonContext.Default.AppConfig);
        File.WriteAllText(ConfigPath, json);
    }
}
