using System.Text.Json;
using System.Text.Json.Serialization;
using PhoenixManager.Models;

namespace PhoenixManager.Services;

public static class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

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
        return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
    }

    public static void Save(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }
}
