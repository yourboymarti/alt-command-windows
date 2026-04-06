using System.Text.Json;

namespace AltCommand.Windows.Configuration;

internal sealed class ConfigStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public ConfigStore()
    {
        ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AltCommand");

        ConfigPath = Path.Combine(ConfigDirectory, "hotkeys.json");
    }

    public string ConfigDirectory { get; }

    public string ConfigPath { get; }

    public AppConfig Load()
    {
        Directory.CreateDirectory(ConfigDirectory);

        if (!File.Exists(ConfigPath))
        {
            Save(AppConfig.CreateDefault());
        }

        var json = File.ReadAllText(ConfigPath);
        var config = JsonSerializer.Deserialize<AppConfig>(json, SerializerOptions);

        return config ?? throw new InvalidOperationException("Configuration file is empty or invalid.");
    }

    public void Save(AppConfig config)
    {
        Directory.CreateDirectory(ConfigDirectory);
        var json = JsonSerializer.Serialize(config, SerializerOptions);
        File.WriteAllText(ConfigPath, json);
    }
}
