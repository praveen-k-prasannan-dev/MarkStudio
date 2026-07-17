using System.Text.RegularExpressions;

namespace MarkdownEditor.Core.Editing;

/// <summary>Line-level formatting: headings, blockquotes, code fences, horizontal rules.</summary>
public static class BlockFormatter
{
    private static readonly Regex HeadingPrefix = new(@"^#{1,6}\s+", RegexOptions.Compiled);
    private static readonly Regex QuotePrefix = new(@"^>\s?", RegexOptions.Compiled);

    /// <summary>Sets the heading level (1–6) of every selected line; 0 removes the heading.</summary>
    public static EditResult SetHeading(TextSelection selection, int level)
    {
        ArgumentNullException.ThrowIfNull(selection);
        if (level is < 0 or > 6)
            throw new ArgumentOutOfRangeException(nameof(level));

        return SelectionLines.Transform(selection, (line, _) =>
        {
            if (line.Length == 0)
                return line;
            string stripped = HeadingPrefix.Replace(line, "");
            return level == 0 ? stripped : new string('#', level) + " " + stripped;
        });
    }

    /// <summary>Adds "&gt; " to every selected line, or removes it if all lines are already quoted.</summary>
    public static EditResult ToggleBlockquote(TextSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        var lines = SelectionLines.GetLineContents(selection);
        bool allQuoted = lines.Where(l => l.Length > 0).All(l => QuotePrefix.IsMatch(l));
        bool hasContent = lines.Any(l => l.Length > 0);

        return SelectionLines.Transform(selection, (line, _) =>
        {
            if (line.Length == 0)
                return line;
            return allQuoted && hasContent ? QuotePrefix.Replace(line, "") : "> " + line;
        });
    }

    /// <summary>Wraps the selected lines in a fenced code block, or unwraps an existing fence.</summary>
    public static EditResult ToggleCodeFence(TextSelection selection, string? language = null)
    {
        ArgumentNullException.ThrowIfNull(selection);

        var lines = SelectionLines.GetLineContents(selection);
        bool isFenced = lines.Length >= 2
            && lines[0].StartsWith("```", StringComparison.Ordinal)
            && lines[^1].TrimEnd() == "```";

        if (isFenced)
        {
            string inner = string.Join("\n", lines[1..^1]);
            return SelectionLines.ReplaceBlock(selection, inner);
        }

        string body = string.Join("\n", lines);
        string fenced = "```" + (language ?? "") + "\n" + body + "\n```";
        return SelectionLines.ReplaceBlock(selection, fenced);
    }

    /// <summary>
    /// Inserts a horizontal rule after the caret, always separated from preceding text by a
    /// blank line (otherwise "text\n---" would parse as a setext heading, not a rule).
    /// </summary>
    public static EditResult InsertHorizontalRule(TextSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        string text = selection.Text;
        int pos = selection.SelectionEnd;
        string before = text[..pos];

        string separation =
            before.Length == 0 ? "" :
            before.EndsWith("\n\n", StringComparison.Ordinal) || before.EndsWith("\n\r\n", StringComparison.Ordinal) ? "" :
            before.EndsWith('\n') ? "\n" : "\n\n";

        string insertion = separation + "---\n";
        string newText = text.Insert(pos, insertion);
        return new EditResult(newText, pos + insertion.Length, 0);
    }
}
