using System.Net;

namespace MarkdownEditor.Core.Markdown;

/// <summary>Wraps a rendered HTML body fragment into a complete standalone page.</summary>
public static class HtmlDocumentBuilder
{
    public static string BuildPage(string bodyHtml, string cssText, string title)
    {
        ArgumentNullException.ThrowIfNull(bodyHtml);
        ArgumentNullException.ThrowIfNull(cssText);
        ArgumentNullException.ThrowIfNull(title);

        return $"""
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset="utf-8" />
            <title>{WebUtility.HtmlEncode(title)}</title>
            <style>
            {cssText}
            </style>
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
