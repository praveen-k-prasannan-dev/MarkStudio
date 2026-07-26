namespace MarkdownEditor.Core.Documents;

/// <summary>Tracks the open document: its file path, text, and unsaved-changes flag.</summary>
public sealed class DocumentState
{
    public string? FilePath { get; private set; }
    public string Text { get; private set; } = "";
    public bool IsDirty { get; private set; }

    /// <summary>
    /// Set by <see cref="DocumentManager"/> to disambiguate multiple unsaved tabs
    /// ("Untitled", "Untitled 2", "Untitled 3", ...). 0/1 both mean the plain "Untitled".
    /// </summary>
    public int UntitledNumber { get; set; }

    public string Title => FilePath is null
        ? (UntitledNumber > 1 ? $"Untitled {UntitledNumber}" : "Untitled")
        : Path.GetFileName(FilePath);

    /// <summary>Flags unsaved changes without the cost of syncing the full text (used per keystroke).</summary>
    public void MarkDirty() => IsDirty = true;

    public void SetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text == Text)
            return;
        Text = text;
        IsDirty = true;
    }

    public void LoadFrom(string path, string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(text);
        FilePath = path;
        Text = text;
        IsDirty = false;
    }

    public void MarkSaved(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        FilePath = path;
        IsDirty = false;
    }

    public void Reset()
    {
        FilePath = null;
        Text = "";
        IsDirty = false;
    }
}
