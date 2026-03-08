using System;
using System.IO;
using System.Text.Json;
using CrtOverlayApp.Models;

namespace CrtOverlayApp.Services;

public static class SettingsPersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static string SettingsDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrtOverlayApp");
    private static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    public static OverlaySettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new OverlaySettings();
            }

            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<OverlaySettings>(json, JsonOptions) ?? new OverlaySettings();
        }
        catch
        {
            return new OverlaySettings();
        }
    }

    public static void Save(OverlaySettings settings)
    {
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }
}
