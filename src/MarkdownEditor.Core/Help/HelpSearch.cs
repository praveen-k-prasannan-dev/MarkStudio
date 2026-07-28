using MarkdownEditor.Core.Palette;

namespace MarkdownEditor.Core.Help;

/// <summary>Flattens the Help topic tree and fuzzy-searches leaf topics by title, mirroring the command palette's search.</summary>
public static class HelpSearch
{
    public static IReadOnlyList<HelpTopic> Flatten(IReadOnlyList<HelpTopic> topics)
    {
        var result = new List<HelpTopic>();
        Walk(topics, result);
        return result;
    }

    private static void Walk(IReadOnlyList<HelpTopic> nodes, List<HelpTopic> result)
    {
        foreach (var node in nodes)
        {
            if (node.File is not null)
                result.Add(node);
            if (node.Children.Count > 0)
                Walk(node.Children, result);
        }
    }

    public static IReadOnlyList<HelpTopic> Search(IReadOnlyList<HelpTopic> topics, string query) =>
        FuzzyMatcher.Filter(Flatten(topics), t => t.Title, query);

    /// <summary>
    /// Title matches (fuzzy, ranked, same as <see cref="Search"/>) first, then topics whose body
    /// text contains the query as a plain substring - so searching a word that appears in a
    /// topic's content but not its title (e.g. "row" for "The Table Tab") still finds it.
    /// </summary>
    public static IReadOnlyList<HelpTopic> SearchWithContent(
        IReadOnlyList<HelpTopic> topics, string query, Func<HelpTopic, string> getContent)
    {
        ArgumentNullException.ThrowIfNull(topics);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(getContent);

        var flat = Flatten(topics);
        if (query.Length == 0)
            return flat;

        var titleMatches = FuzzyMatcher.Filter(flat, t => t.Title, query);
        var titleMatchSet = new HashSet<HelpTopic>(titleMatches);

        var contentMatches = flat
            .Where(t => !titleMatchSet.Contains(t))
            .Where(t => getContent(t).Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return [.. titleMatches, .. contentMatches];
    }
}
