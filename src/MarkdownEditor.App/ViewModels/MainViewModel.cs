using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using MarkdownEditor.Core.Documents;
using MarkdownEditor.Core.Services;

namespace MarkdownEditor.App.ViewModels;

/// <summary>
/// Holds the open documents (tabs) and status-bar text for whichever one is active.
/// Dialogs and editor interaction live in the window code-behind; this class stays free of WPF types.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IFileService _fileService = new FileService();

    public DocumentManager Documents { get; } = new();

    public IRecentFilesService RecentFiles { get; } = new RecentFilesService(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MarkdownEditor", "recent.json"));

    [ObservableProperty]
    private string _windowTitle = "MarkStudio Editor";

    [ObservableProperty]
    private string _statusInfo = "0 words";

    [ObservableProperty]
    private string _caretInfo = "Ln 1, Col 1";

    public string? FilePath => Documents.Active?.FilePath;
    public bool IsDirty => Documents.Active?.IsDirty ?? false;
    public string DocumentTitle => Documents.Active?.Title ?? "Untitled";

    /// <summary>Creates a new blank document/tab, makes it active, and returns it.</summary>
    public DocumentState AddNewDocument()
    {
        var doc = Documents.AddNew();
        RefreshForActiveTab();
        return doc;
    }

    /// <summary>Cheap per-keystroke dirty flag on the active document; full text sync happens on the preview debounce.</summary>
    public void MarkDirty()
    {
        Documents.Active?.MarkDirty();
        UpdateTitle();
    }

    public void SyncText(string text)
    {
        Documents.Active?.SetText(text);
        UpdateStatistics(text);
        UpdateTitle();
    }

    public string LoadFile(string path) => _fileService.Load(path);

    /// <summary>Loads a file into a new tab, makes it active, and returns the new document.</summary>
    public DocumentState LoadIntoNewTab(string path, string text)
    {
        var doc = Documents.AddNew();
        doc.LoadFrom(path, text);
        RecentFiles.Add(path);
        RefreshForActiveTab();
        return doc;
    }

    public void Save(string path, string text)
    {
        var active = Documents.Active;
        if (active is null)
            return;
        _fileService.Save(path, text);
        active.SetText(text);
        active.MarkSaved(path);
        RecentFiles.Add(path);
        UpdateStatistics(text);
        UpdateTitle();
    }

    public void UpdateCaret(int line, int column) => CaretInfo = $"Ln {line}, Col {column}";

    /// <summary>Refreshes title/status bar from whichever document is now active (call after switching tabs).</summary>
    public void RefreshForActiveTab()
    {
        UpdateStatistics(Documents.Active?.Text ?? "");
        UpdateTitle();
    }

    private void UpdateStatistics(string text)
    {
        var stats = DocumentStatistics.Compute(text);
        string readingTime = stats.ReadingTimeMinutes > 0 ? $"    ~{stats.ReadingTimeMinutes} min read" : "";
        StatusInfo = $"{stats.Words} words    {stats.Characters} characters    {stats.Lines} lines{readingTime}";
    }

    private void UpdateTitle()
    {
        var active = Documents.Active;
        WindowTitle = active is null
            ? "MarkStudio Editor"
            : $"{active.Title}{(active.IsDirty ? " ●" : "")} — MarkStudio Editor";
    }
}
