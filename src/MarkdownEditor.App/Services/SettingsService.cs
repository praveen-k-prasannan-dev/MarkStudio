using System.IO;
using System.Text.Json;

namespace MarkdownEditor.App.Services;

public sealed class AppSettings
{
    public bool DarkPreview { get; set; }
    public string ViewMode { get; set; } = "Split"; // Split | Editor | Preview
    public double EditorFontSize { get; set; } = 14;
    public bool SyncScroll { get; set; } = true;
    public bool ShowOutline { get; set; }
    public double WindowWidth { get; set; } = 1280;
    public double WindowHeight { get; set; } = 820;
    public bool WindowMaximized { get; set; }
}

/// <summary>Loads/saves user preferences at %APPDATA%\MarkdownEditor\settings.json.</summary>
public static class SettingsService
{
    private static string StorePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MarkdownEditor", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(StorePath))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(StorePath)) ?? new AppSettings();
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
            // Corrupt settings are not worth failing startup over; fall back to defaults.
        }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
            File.WriteAllText(StorePath,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Losing preferences is preferable to crashing on exit.
        }
    }
}
