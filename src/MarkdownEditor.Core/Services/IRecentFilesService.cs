namespace MarkdownEditor.Core.Services;

public interface IRecentFilesService
{
    IReadOnlyList<string> Items { get; }
    void Add(string path);
    void Clear();
}
