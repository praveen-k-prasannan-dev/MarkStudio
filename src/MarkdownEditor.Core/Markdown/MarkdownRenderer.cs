using Markdig;

namespace MarkdownEditor.Core.Markdown;

/// <summary>
/// Converts Markdown text to an HTML body fragment.
/// Raw HTML in the source is intentionally allowed so documents can use
/// inline HTML (e.g. &lt;br&gt;, sized &lt;img&gt;) like most Markdown editors.
/// </summary>
public sealed class MarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions() // pipe tables, task lists, emphasis extras (==mark==), footnotes, auto-ids…
        .Build();

    public string ToHtml(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        return Markdig.Markdown.ToHtml(markdown, _pipeline);
    }
}
