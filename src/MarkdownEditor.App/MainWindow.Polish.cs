using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using MarkdownEditor.App.Services;

namespace MarkdownEditor.App;

/// <summary>Settings persistence, autosave/crash recovery, dropped-image handling, About.</summary>
public partial class MainWindow
{
    private static readonly string[] ImageExtensions =
        [".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp", ".bmp"];

    private static string AutosaveDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MarkdownEditor", "autosave");

    private static string AutosavePath => Path.Combine(AutosaveDir, "recovery.md");

    private AppSettings _settings = new();
    private DispatcherTimer? _autosaveTimer;

    // ---------- Settings ----------

    private void ApplySettings()
    {
        _settings = SettingsService.Load();

        Width = Math.Max(600, _settings.WindowWidth);
        Height = Math.Max(400, _settings.WindowHeight);
        if (_settings.WindowMaximized)
            WindowState = WindowState.Maximized;

        Editor.FontSize = Math.Clamp(_settings.EditorFontSize, 8, 32);
        SyncScrollCheck.IsChecked = _settings.SyncScroll;
        DarkThemeToggle.IsChecked = _settings.DarkPreview;
        OutlineToggle.IsChecked = _settings.ShowOutline;

        switch (_settings.ViewMode)
        {
            case "Editor": ViewEditorOnly.IsChecked = true; break;
            case "Preview": ViewPreviewOnly.IsChecked = true; break;
            default: ViewSplit.IsChecked = true; break;
        }
    }

    private void SaveSettings()
    {
        _settings.WindowMaximized = WindowState == WindowState.Maximized;
        if (WindowState == WindowState.Normal)
        {
            _settings.WindowWidth = Width;
            _settings.WindowHeight = Height;
        }
        _settings.EditorFontSize = Editor.FontSize;
        _settings.SyncScroll = SyncScrollCheck.IsChecked == true;
        _settings.DarkPreview = DarkThemeToggle.IsChecked == true;
        _settings.ShowOutline = OutlineToggle.IsChecked == true;
        _settings.ViewMode =
            ViewEditorOnly.IsChecked == true ? "Editor" :
            ViewPreviewOnly.IsChecked == true ? "Preview" : "Split";

        SettingsService.Save(_settings);
    }

    // ---------- Autosave & crash recovery ----------

    private void StartAutosave()
    {
        _autosaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        _autosaveTimer.Tick += (_, _) => WriteAutosave();
        _autosaveTimer.Start();
    }

    private void WriteAutosave()
    {
        try
        {
            if (_vm.IsDirty)
            {
                Directory.CreateDirectory(AutosaveDir);
                File.WriteAllText(AutosavePath, Editor.Text);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Autosave is best-effort; never interrupt the user over it.
        }
    }

    private void DeleteAutosave()
    {
        try
        {
            if (File.Exists(AutosavePath))
                File.Delete(AutosavePath);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }

    /// <summary>Offers to restore an unsaved draft left behind by a crash.</summary>
    private void OfferRecovery()
    {
        if (!File.Exists(AutosavePath))
            return;

        var choice = MessageBox.Show(this,
            "Unsaved changes from a previous session were found.\nDo you want to restore them?",
            "Recover document", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (choice == MessageBoxResult.Yes)
        {
            try
            {
                SetEditorText(File.ReadAllText(AutosavePath));
                _vm.NewDocument();
                _vm.MarkDirty();
                _ = RefreshPreviewAsync();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, $"Could not read the recovery file:\n{ex.Message}",
                    "Recover document", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        DeleteAutosave();
    }

    // ---------- Dropped images ----------

    private static bool IsImageFile(string path) =>
        ImageExtensions.Contains(Path.GetExtension(path).ToLowerInvariant());

    /// <summary>
    /// Inserts a dropped image. For saved documents the file is copied into an "assets" folder
    /// next to the .md and referenced relatively; otherwise the absolute path is used.
    /// </summary>
    private void InsertDroppedImage(string imagePath)
    {
        string markdownPath;
        try
        {
            string? documentFolder = _vm.FilePath is null ? null : Path.GetDirectoryName(_vm.FilePath);
            if (documentFolder is not null)
            {
                string assetsDir = Path.Combine(documentFolder, "assets");
                Directory.CreateDirectory(assetsDir);
                string destination = GetUniqueDestination(assetsDir, Path.GetFileName(imagePath));
                if (!string.Equals(destination, imagePath, StringComparison.OrdinalIgnoreCase))
                    File.Copy(imagePath, destination);
                markdownPath = "assets/" + Path.GetFileName(destination);
            }
            else
            {
                markdownPath = imagePath.Replace('\\', '/');
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Could not copy the image:\n{ex.Message}",
                "Insert image", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        InsertAtCaret($"![{Path.GetFileNameWithoutExtension(imagePath)}]({markdownPath})");
        _ = RefreshPreviewAsync();
    }

    private static string GetUniqueDestination(string folder, string fileName)
    {
        string candidate = Path.Combine(folder, fileName);
        if (!File.Exists(candidate))
            return candidate;

        string stem = Path.GetFileNameWithoutExtension(fileName);
        string ext = Path.GetExtension(fileName);
        for (int i = 1; ; i++)
        {
            candidate = Path.Combine(folder, $"{stem}_{i}{ext}");
            if (!File.Exists(candidate))
                return candidate;
        }
    }

    // ---------- About ----------

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        MessageBox.Show(this,
            $"Markdown Editor {version}\n\n" +
            "A Markdown document viewer and editor with a Word-style toolbox,\n" +
            "live preview, and PDF export.\n\n" +
            "Built with WPF, Markdig, AvalonEdit, and WebView2.",
            "About Markdown Editor", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
