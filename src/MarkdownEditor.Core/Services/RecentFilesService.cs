using System.Text.Json;

namespace MarkdownEditor.Core.Services;

/// <summary>Most-recently-used file list, persisted as JSON at the given store path.</summary>
public sealed class RecentFilesService : IRecentFilesService
{
    public const int Capacity = 10;

    private readonly string _storePath;
    private readonly List<string> _items;

    public RecentFilesService(string storePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(storePath);
        _storePath = storePath;
        _items = Load();
    }

    public IReadOnlyList<string> Items => _items;

    public void Add(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        _items.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
        _items.Insert(0, path);
        if (_items.Count > Capacity)
            _items.RemoveRange(Capacity, _items.Count - Capacity);
        Save();
    }

    public void Clear()
    {
        _items.Clear();
        Save();
    }

    private List<string> Load()
    {
        try
        {
            if (File.Exists(_storePath))
                return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_storePath)) ?? [];
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
            // A corrupt or unreadable MRU store is not worth failing startup over.
        }
        return [];
    }

    private void Save()
    {
        var dir = Path.GetDirectoryName(_storePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(_storePath, JsonSerializer.Serialize(_items));
    }
}
