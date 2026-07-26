using System.Net;
using System.Text.RegularExpressions;
using Markdig;

namespace MarkdownEditor.Core.Markdown;

/// <summary>
/// Converts Markdown text to an HTML body fragment.
/// Raw HTML in the source is intentionally allowed so documents can use
/// inline HTML (e.g. &lt;br&gt;, sized &lt;img&gt;) like most Markdown editors.
/// </summary>
public sealed class MarkdownRenderer
{
    // Matches a fenced code block Markdig renders for ```mermaid, so it can be swapped for the
    // plain <pre class="mermaid"> element the Mermaid.js library expects (raw, unescaped text).
    private static readonly Regex MermaidCodeBlock = new(
        @"<pre><code class=""language-mermaid"">(.*?)</code></pre>",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions() // pipe tables, task lists, emphasis extras (==mark==), footnotes, auto-ids…
        .UseMathematics()        // $inline$ and $$block$$ math -> \(...\) / \[...\], ready for MathJax
        .Build();

    public string ToHtml(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        string html = Markdig.Markdown.ToHtml(markdown, _pipeline);
        return MermaidCodeBlock.Replace(html, m => $"<pre class=\"mermaid\">{WebUtility.HtmlDecode(m.Groups[1].Value)}</pre>");
    }
}
