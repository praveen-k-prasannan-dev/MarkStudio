namespace MarkdownEditor.App.Services;

/// <summary>
/// Builds the extra &lt;head&gt; scripts (Mermaid, MathJax) for a rendered preview page, loaded only
/// when the body actually uses them - both are multi-megabyte scripts, so loading them
/// unconditionally would make every preview refresh sluggish. Shared by the live preview and the
/// Help window, which both render Markdown through the same virtual-host-mapped Assets folder.
/// </summary>
public static class PreviewScripts
{
    public const string AssetsHost = "app-assets.local";

    public static string BuildExtraHeadScripts(string bodyHtml)
    {
        var scripts = new System.Text.StringBuilder();

        if (bodyHtml.Contains("class=\"mermaid\"", StringComparison.Ordinal))
        {
            scripts.Append($"<script src=\"https://{AssetsHost}/lib/mermaid.min.js\"></script>");
            scripts.Append("<script>mermaid.initialize({ startOnLoad: true, securityLevel: 'strict' });</script>");
        }

        if (bodyHtml.Contains("class=\"math\"", StringComparison.Ordinal))
            scripts.Append($"<script src=\"https://{AssetsHost}/lib/mathjax-tex-svg.js\"></script>");

        return scripts.ToString();
    }
}
