using System.IO;
using System.Text.Json;
using MarkdownEditor.Core.Documents;

namespace MarkdownEditor.App.Services;

/// <summary>Loads/saves the writing-stats day totals at %APPDATA%\MarkdownEditor\writing-stats.json.</summary>
public static class WritingStatsService
{
    private static string StorePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MarkdownEditor", "writing-stats.json");

    public static WritingStatsTracker Load()
    {
        try
        {
            if (File.Exists(StorePath))
            {
                var raw = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(StorePath)) ?? [];
                var parsed = new Dictionary<DateOnly, int>();
                foreach (var (key, value) in raw)
                    if (DateOnly.TryParse(key, out var date))
                        parsed[date] = value;
                return new WritingStatsTracker(parsed);
            }
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
            // Corrupt stats are not worth failing startup over; fall back to an empty tracker.
        }
        return new WritingStatsTracker();
    }

    public static void Save(WritingStatsTracker tracker)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
            var raw = tracker.WordsPerDay.ToDictionary(kv => kv.Key.ToString("yyyy-MM-dd"), kv => kv.Value);
            File.WriteAllText(StorePath, JsonSerializer.Serialize(raw, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Losing stats is preferable to crashing on exit.
        }
    }
}
