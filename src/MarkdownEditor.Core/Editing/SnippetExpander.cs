namespace MarkdownEditor.Core.Editing;

/// <summary>Where a snippet expansion replaces text and where the caret should land afterward.</summary>
public readonly record struct SnippetExpansionResult(int ReplaceStart, int ReplaceLength, string Replacement, int NewCaretPosition);

/// <summary>Expands a trigger word immediately before the caret into a snippet, triggered by pressing Tab.</summary>
public static class SnippetExpander
{
    private const string CaretMarker = "$0";

    public static IReadOnlyList<Snippet> BuiltIn { get; } =
    [
        new Snippet("table", "| Header 1 | Header 2 | Header 3 |\n| --- | --- | --- |\n| $0 |  |  |\n|  |  |  |"),
        new Snippet("todo", "- [ ] $0"),
        new Snippet("meeting", "## Meeting Notes\n\n**Date:** \n**Attendees:** \n\n### Agenda\n\n- $0\n\n### Action Items\n\n- [ ] "),
        new Snippet("code", "```$0\n```"),
    ];

    /// <summary>Returns the expansion for the word immediately before <paramref name="caretPosition"/>, or null if it doesn't match a known trigger.</summary>
    public static SnippetExpansionResult? TryExpand(string text, int caretPosition, IReadOnlyList<Snippet>? snippets = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        snippets ??= BuiltIn;

        int start = caretPosition;
        while (start > 0 && char.IsLetterOrDigit(text[start - 1]))
            start--;
        if (start == caretPosition)
            return null;

        string trigger = text[start..caretPosition];
        Snippet? snippet = snippets.FirstOrDefault(s => string.Equals(s.Trigger, trigger, StringComparison.OrdinalIgnoreCase));
        if (snippet is null)
            return null;

        int markerIndex = snippet.Expansion.IndexOf(CaretMarker, StringComparison.Ordinal);
        string replacement = markerIndex >= 0
            ? snippet.Expansion.Remove(markerIndex, CaretMarker.Length)
            : snippet.Expansion;
        int newCaretPosition = start + (markerIndex >= 0 ? markerIndex : replacement.Length);

        return new SnippetExpansionResult(start, caretPosition - start, replacement, newCaretPosition);
    }
}
