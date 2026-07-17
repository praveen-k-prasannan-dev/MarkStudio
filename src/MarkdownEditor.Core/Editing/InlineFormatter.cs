namespace MarkdownEditor.Core.Editing;

/// <summary>
/// Toggles inline Markdown emphasis markers (**bold**, *italic*, ~~strike~~, `code`, ==highlight==)
/// around the current selection, Word-style: applying a format that is already present removes it.
/// </summary>
public static class InlineFormatter
{
    public const string Bold = "**";
    public const string Italic = "*";
    public const string Strikethrough = "~~";
    public const string Code = "`";
    public const string Highlight = "==";

    public static EditResult Toggle(TextSelection selection, string marker)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentException.ThrowIfNullOrEmpty(marker);

        string text = selection.Text;
        int start = selection.SelectionStart;
        int length = selection.SelectionLength;
        int m = marker.Length;

        // Trim whitespace out of the selection so markers hug the word: "abc " -> "**abc** "
        while (length > 0 && char.IsWhiteSpace(text[start]))
        {
            start++;
            length--;
        }
        while (length > 0 && char.IsWhiteSpace(text[start + length - 1]))
            length--;

        if (length == 0)
        {
            // No (non-whitespace) selection: insert an empty marker pair, caret in the middle.
            string newText = text.Insert(start, marker + marker);
            return new EditResult(newText, start + m, 0);
        }

        string selected = text.Substring(start, length);

        // Selection includes the markers themselves ("**abc**" selected) -> unwrap.
        if (length >= 2 * m && selected.StartsWith(marker, StringComparison.Ordinal)
                            && selected.EndsWith(marker, StringComparison.Ordinal))
        {
            string inner = selected.Substring(m, length - 2 * m);
            string newText = text.Remove(start, length).Insert(start, inner);
            return new EditResult(newText, start, inner.Length);
        }

        // Markers directly surround the selection ("abc" selected inside "**abc**") -> unwrap.
        if (start >= m && start + length + m <= text.Length
            && text.Substring(start - m, m) == marker
            && text.Substring(start + length, m) == marker)
        {
            string newText = text.Remove(start + length, m).Remove(start - m, m);
            return new EditResult(newText, start - m, length);
        }

        // Otherwise wrap the selection.
        string wrapped = text.Insert(start + length, marker).Insert(start, marker);
        return new EditResult(wrapped, start + m, length);
    }
}
