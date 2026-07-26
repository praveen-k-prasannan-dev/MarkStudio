namespace MarkdownEditor.Core.Documents;

/// <summary>
/// Owns the set of open documents (tabs) and which one is active. Untitled documents are
/// numbered monotonically ("Untitled", "Untitled 2", "Untitled 3", ...) and numbers are never
/// reused within a session, even after the tab that held them is closed.
/// </summary>
public sealed class DocumentManager
{
    private readonly List<DocumentState> _documents = [];
    private int _activeIndex = -1;
    private int _nextUntitledNumber = 1;

    public IReadOnlyList<DocumentState> Documents => _documents;
    public int Count => _documents.Count;
    public int ActiveIndex => _activeIndex;
    public DocumentState? Active => _activeIndex >= 0 && _activeIndex < _documents.Count ? _documents[_activeIndex] : null;

    /// <summary>Creates a new blank document, makes it active, and returns it.</summary>
    public DocumentState AddNew()
    {
        var document = new DocumentState { UntitledNumber = _nextUntitledNumber++ };
        _documents.Add(document);
        _activeIndex = _documents.Count - 1;
        return document;
    }

    /// <summary>Case-insensitive lookup by file path; documents with no path (unsaved) never match.</summary>
    public DocumentState? FindByPath(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return _documents.FirstOrDefault(d => d.FilePath is not null &&
            string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase));
    }

    public int IndexOf(DocumentState document) => _documents.IndexOf(document);

    public void SetActive(int index)
    {
        if (index < 0 || index >= _documents.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _activeIndex = index;
    }

    public void SetActive(DocumentState document)
    {
        int index = _documents.IndexOf(document);
        if (index < 0)
            throw new ArgumentException("Document is not managed by this DocumentManager.", nameof(document));
        _activeIndex = index;
    }

    /// <summary>Closes the document at <paramref name="index"/>. Returns the new active index (-1 if none remain).</summary>
    public int Close(int index)
    {
        if (index < 0 || index >= _documents.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _documents.RemoveAt(index);

        if (_documents.Count == 0)
        {
            _activeIndex = -1;
        }
        else if (_activeIndex > index)
        {
            _activeIndex--;
        }
        else if (_activeIndex == index)
        {
            _activeIndex = Math.Min(index, _documents.Count - 1);
        }
        // _activeIndex < index: the active tab is unaffected by removing a later one.

        return _activeIndex;
    }
}
