namespace MarkdownEditor.Core.Editing;

/// <summary>An immutable snapshot of the editor text and the user's current selection.</summary>
public sealed record TextSelection
{
    public TextSelection(string text, int selectionStart, int selectionLength)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (selectionStart < 0 || selectionStart > text.Length)
            throw new ArgumentOutOfRangeException(nameof(selectionStart));
        if (selectionLength < 0 || selectionStart + selectionLength > text.Length)
            throw new ArgumentOutOfRangeException(nameof(selectionLength));

        Text = text;
        SelectionStart = selectionStart;
        SelectionLength = selectionLength;
    }

    public string Text { get; }
    public int SelectionStart { get; }
    public int SelectionLength { get; }

    public int SelectionEnd => SelectionStart + SelectionLength;
    public string SelectedText => Text.Substring(SelectionStart, SelectionLength);
}
