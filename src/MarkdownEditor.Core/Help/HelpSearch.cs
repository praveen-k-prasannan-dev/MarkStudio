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
}
