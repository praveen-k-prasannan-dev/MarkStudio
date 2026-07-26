using System.Net;

namespace MarkdownEditor.Core.Markdown;

/// <summary>Wraps a rendered HTML body fragment into a complete standalone page.</summary>
public static class HtmlDocumentBuilder
{
    /// <param name="extraHeadHtml">
    /// Raw HTML appended to &lt;head&gt; after the stylesheet — e.g. script tags for optional
    /// renderers (Mermaid, MathJax) that the caller decides whether the document actually needs.
    /// </param>
    public static string BuildPage(string bodyHtml, string cssText, string title, string? baseHref = null, string extraHeadHtml = "")
    {
        ArgumentNullException.ThrowIfNull(bodyHtml);
        ArgumentNullException.ThrowIfNull(cssText);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(extraHeadHtml);

        string baseTag = baseHref is null
            ? ""
            : $"<base href=\"{WebUtility.HtmlEncode(baseHref)}\" />";

        return $"""
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset="utf-8" />
            {baseTag}
            <title>{WebUtility.HtmlEncode(title)}</title>
            <style>
            {cssText}
            </style>
            {extraHeadHtml}
            </head>
            <body>
            <article class="markdown-body">
            {bodyHtml}
            </article>
            </body>
            </html>
            """;
    }
}
