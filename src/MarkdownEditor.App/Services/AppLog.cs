using System.IO;

namespace MarkdownEditor.App.Services;

/// <summary>Best-effort diagnostic log at %APPDATA%\MarkdownEditor\app.log.</summary>
public static class AppLog
{
    private static readonly object Gate = new();

    private static string LogPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MarkdownEditor", "app.log");

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{Environment.ProcessId}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never take the app down.
        }
    }
}
