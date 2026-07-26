using System.Text.RegularExpressions;

namespace MarkdownEditor.Core.Spelling;

/// <summary>A misspelled word's location within the scanned text.</summary>
public readonly record struct MisspelledSpan(int Start, int Length);

/// <summary>
/// Finds misspelled words in Markdown text, skipping fenced code blocks, inline code, and URLs
/// so code identifiers and links aren't flagged as spelling errors.
/// </summary>
public static class SpellCheckScanner
{
    private static readonly Regex WordPattern = new(@"[\p{L}]+(?:['’][\p{L}]+)*", RegexOptions.Compiled);
    private static readonly Regex FencedCodeBlock = new(@"^[ \t]*```.*?^[ \t]*```", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex InlineCode = new(@"`[^`\n]+`", RegexOptions.Compiled);
    private static readonly Regex Url = new(@"https?://\S+", RegexOptions.Compiled);

    /// <param name="isCorrect">Dictionary lookup: true if the word is spelled correctly.</param>
    public static IReadOnlyList<MisspelledSpan> FindMisspelledWords(string text, Func<string, bool> isCorrect)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(isCorrect);

        if (text.Length == 0)
            return [];

        bool[] excluded = ComputeExcludedMask(text);
        var results = new List<MisspelledSpan>();

        foreach (Match m in WordPattern.Matches(text))
        {
            if (excluded[m.Index])
                continue;
            if (!isCorrect(m.Value))
                results.Add(new MisspelledSpan(m.Index, m.Length));
        }

        return results;
    }

    private static bool[] ComputeExcludedMask(string text)
    {
        var mask = new bool[text.Length];

        void MarkRanges(Regex pattern)
        {
            foreach (Match m in pattern.Matches(text))
                for (int i = m.Index; i < m.Index + m.Length && i < mask.Length; i++)
                    mask[i] = true;
        }

        MarkRanges(FencedCodeBlock);
        MarkRanges(InlineCode);
        MarkRanges(Url);
        return mask;
    }
}
