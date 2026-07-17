using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using MarkdownEditor.Core.Documents;
using MarkdownEditor.Core.Services;

namespace MarkdownEditor.App.ViewModels;

/// <summary>
/// Holds the document state and status-bar text. Dialogs and editor interaction
/// live in the window code-behind; this class stays free of WPF types.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IFileService _fileService = new FileService();
    private readonly DocumentState _document = new();

    public IRecentFilesService RecentFiles { get; } = new RecentFilesService(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MarkdownEditor", "recent.json"));

    [ObservableProperty]
    private string _windowTitle = "Untitled — Markdown Editor";

    [ObservableProperty]
    private string _statusInfo = "0 words";

    [ObservableProperty]
    private string _caretInfo = "Ln 1, Col 1";

    public string? FilePath => _document.FilePath;
    public bool IsDirty => _document.IsDirty;
    public string DocumentTitle => _document.Title;

    /// <summary>Cheap per-keystroke dirty flag; full text sync happens on the preview debounce.</summary>
    public void MarkDirty()
    {
        _document.MarkDirty();
        UpdateTitle();
    }

    public void SyncText(string text)
    {
        _document.SetText(text);
        UpdateStatistics(text);
        UpdateTitle();
    }

    public void NewDocument()
    {
        _document.Reset();
        UpdateStatistics("");
        UpdateTitle();
    }

    public string LoadFile(string path) => _fileService.Load(path);

    public void DocumentLoaded(string path, string text)
    {
        _document.LoadFrom(path, text);
        RecentFiles.Add(path);
        UpdateStatistics(text);
        UpdateTitle();
    }

    public void Save(string path, string text)
    {
        _fileService.Save(path, text);
        _document.SetText(text);
        _document.MarkSaved(path);
        RecentFiles.Add(path);
        UpdateStatistics(text);
        UpdateTitle();
    }

    public void UpdateCaret(int line, int column) => CaretInfo = $"Ln {line}, Col {column}";

    private void UpdateStatistics(string text)
    {
        var stats = DocumentStatistics.Compute(text);
        StatusInfo = $"{stats.Words} words    {stats.Characters} characters    {stats.Lines} lines";
    }

    private void UpdateTitle() =>
        WindowTitle = $"{_document.Title}{(_document.IsDirty ? " ●" : "")} — Markdown Editor";
}
