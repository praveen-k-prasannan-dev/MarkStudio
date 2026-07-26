namespace MarkdownEditor.Core.Palette;

/// <summary>Whether <c>text</c> matched the query, and how good the match was (higher = better).</summary>
public readonly record struct FuzzyMatchResult(bool IsMatch, int Score);

/// <summary>
/// VS Code-style fuzzy matching: the query's characters must appear in <c>text</c> in order
/// (not necessarily contiguous), case-insensitive. Used to power the command palette's
/// as-you-type filtering.
/// </summary>
public static class FuzzyMatcher
{
    public static FuzzyMatchResult Match(string text, string query)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(query);

        if (query.Length == 0)
            return new FuzzyMatchResult(true, 0);

        int score = 0;
        int searchFrom = 0;
        bool previousMatched = false;

        foreach (char q in query)
        {
            int foundAt = -1;
            for (int i = searchFrom; i < text.Length; i++)
            {
                if (char.ToLowerInvariant(text[i]) == char.ToLowerInvariant(q))
                {
                    foundAt = i;
                    break;
                }
            }
            if (foundAt == -1)
                return new FuzzyMatchResult(false, 0);

            bool atWordStart = foundAt == 0 || !char.IsLetterOrDigit(text[foundAt - 1]);
            bool consecutive = previousMatched && foundAt == searchFrom;

            score += 10;
            if (atWordStart) score += 15;
            if (consecutive) score += 20;

            searchFrom = foundAt + 1;
            previousMatched = true;
        }

        score += Math.Max(0, 50 - text.Length); // prefer shorter, more precise matches

        return new FuzzyMatchResult(true, score);
    }

    /// <summary>Filters and ranks <paramref name="items"/> by how well they match <paramref name="query"/>.</summary>
    public static IReadOnlyList<T> Filter<T>(IEnumerable<T> items, Func<T, string> textSelector, string query)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(textSelector);
        ArgumentNullException.ThrowIfNull(query);

        return items
            .Select(item => (Item: item, Result: Match(textSelector(item), query)))
            .Where(x => x.Result.IsMatch)
            .OrderByDescending(x => x.Result.Score)
            .Select(x => x.Item)
            .ToList();
    }
}
