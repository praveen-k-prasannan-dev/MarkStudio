using System.Text;

namespace MarkdownEditor.Core.Editing;

/// <summary>
/// Helpers for line-oriented formatting: expands a selection to whole lines and
/// rewrites them. Handles both \n and \r\n line endings.
/// </summary>
internal static class SelectionLines
{
    /// <summary>Expands the selection to whole-line boundaries (excluding the trailing newline).</summary>
    public static (int BlockStart, int BlockEnd) GetBlockBounds(TextSelection selection)
    {
        string text = selection.Text;
        int start = selection.SelectionStart;
        int end = selection.SelectionEnd;

        // A selection ending exactly at the start of a line does not include that line.
        if (selection.SelectionLength > 0 && end > 0 && text[end - 1] == '\n')
            end--;

        int blockStart = start == 0 ? 0 : text.LastIndexOf('\n', start - 1) + 1;
        int newlineAfter = end >= text.Length ? -1 : text.IndexOf('\n', end);
        int blockEnd = newlineAfter == -1 ? text.Length : newlineAfter;
        return (blockStart, blockEnd);
    }

    /// <summary>Returns the selected lines' contents (carriage returns stripped) for inspection.</summary>
    public static string[] GetLineContents(TextSelection selection)
    {
        var (blockStart, blockEnd) = GetBlockBounds(selection);
        string block = selection.Text[blockStart..blockEnd];
        var lines = block.Split('\n');
        for (int i = 0; i < lines.Length; i++)
            lines[i] = lines[i].TrimEnd('\r');
        return lines;
    }

    /// <summary>Rewrites each selected line via <paramref name="transformLine"/> (content, lineIndex).</summary>
    public static EditResult Transform(TextSelection selection, Func<string, int, string> transformLine)
    {
        var (blockStart, blockEnd) = GetBlockBounds(selection);
        string text = selection.Text;
        string block = text[blockStart..blockEnd];
        var lines = block.Split('\n');

        var sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            bool hadCr = line.EndsWith('\r');
            string content = hadCr ? line[..^1] : line;

            if (i > 0) sb.Append('\n');
            sb.Append(transformLine(content, i));
            if (hadCr) sb.Append('\r');
        }

        string newBlock = sb.ToString();
        string newText = text[..blockStart] + newBlock + text[blockEnd..];
        return new EditResult(newText, blockStart, newBlock.Length);
    }

    /// <summary>Replaces the whole selected line block with <paramref name="newBlock"/>.</summary>
    public static EditResult ReplaceBlock(TextSelection selection, string newBlock)
    {
        var (blockStart, blockEnd) = GetBlockBounds(selection);
        string newText = selection.Text[..blockStart] + newBlock + selection.Text[blockEnd..];
        return new EditResult(newText, blockStart, newBlock.Length);
    }
}
