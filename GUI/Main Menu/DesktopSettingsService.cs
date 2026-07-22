using System;
using System.IO;
using System.Text.Json;

namespace GUI;

internal static class DesktopSettingsService
{
    private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HebiKaioPokedex", "settings.json");
    public static DesktopSettings Load() { try { return File.Exists(SettingsPath) ? JsonSerializer.Deserialize<DesktopSettings>(File.ReadAllText(SettingsPath)) ?? new() : new(); } catch { return new(); } }
    public static void Save(DesktopSettings settings) { Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!); File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true })); }
}

internal sealed class DesktopSettings { public bool StrictGender { get; set; } }
