using System.Text;

namespace MarkdownEditor.Core.Services;

public sealed class FileService : IFileService
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Reads the file, honoring a BOM if present, defaulting to UTF-8.</summary>
    public string Load(string path) => File.ReadAllText(path);

    /// <summary>Writes UTF-8 without BOM (the de-facto standard for Markdown files).</summary>
    public void Save(string path, string text) => File.WriteAllText(path, text, Utf8NoBom);
}
