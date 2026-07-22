using System.Text;

namespace MarkdownEditor.Core.Clipboard;

/// <summary>
/// Builds the Windows "HTML Format" (CF_HTML) clipboard payload so pasting into rich-text
/// targets (Word, Outlook, browsers) reproduces formatted content instead of raw markup.
/// See: https://learn.microsoft.com/windows/win32/dataxchg/html-clipboard-format
/// </summary>
public static class ClipboardHtmlFormat
{
    /// <summary>Wraps an HTML body fragment into the full CF_HTML payload string.</summary>
    public static string Build(string bodyHtml)
    {
        ArgumentNullException.ThrowIfNull(bodyHtml);

        const string header =
            "Version:0.9\r\n" +
            "StartHTML:0000000000\r\n" +
            "EndHTML:0000000000\r\n" +
            "StartFragment:0000000000\r\n" +
            "EndFragment:0000000000\r\n";

        const string docStart = "<html>\r\n<body>\r\n<!--StartFragment-->";
        const string docEnd = "<!--EndFragment-->\r\n</body>\r\n</html>";

        // Offsets are byte counts (UTF-8) from the start of the whole payload, per the CF_HTML spec.
        int startHtml = Utf8Length(header);
        int startFragment = startHtml + Utf8Length(docStart);
        int endFragment = startFragment + Utf8Length(bodyHtml);
        int endHtml = endFragment + Utf8Length(docEnd);

        string filledHeader =
            $"Version:0.9\r\n" +
            $"StartHTML:{startHtml:D10}\r\n" +
            $"EndHTML:{endHtml:D10}\r\n" +
            $"StartFragment:{startFragment:D10}\r\n" +
            $"EndFragment:{endFragment:D10}\r\n";

        return filledHeader + docStart + bodyHtml + docEnd;
    }

    private static int Utf8Length(string s) => Encoding.UTF8.GetByteCount(s);
}
