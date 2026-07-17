using System.Text.RegularExpressions;

namespace MarkdownEditor.Core.Documents;

/// <summary>Word / character / line counts for the status bar.</summary>
public readonly record struct DocumentStatistics(int Words, int Characters, int Lines)
{
    // A "word" is a run of letters/digits (apostrophes and hyphens allowed inside),
    // so Markdown punctuation (#, **, `, >) never counts as words.
    private static readonly Regex WordPattern =
        new(@"[\p{L}\p{Nd}]+(?:['’-][\p{L}\p{Nd}]+)*", RegexOptions.Compiled);

    public static DocumentStatistics Compute(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0)
            return new DocumentStatistics(0, 0, 0);

        int words = WordPattern.Matches(text).Count;
        int lines = text.Count(c => c == '\n') + 1;
        return new DocumentStatistics(words, text.Length, lines);
    }
}
