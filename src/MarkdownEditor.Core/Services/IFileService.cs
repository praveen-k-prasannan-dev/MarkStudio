namespace MarkdownEditor.Core.Services;

public interface IFileService
{
    string Load(string path);
    void Save(string path, string text);
}
