using MarkdownEditor.Core.Help;

namespace MarkdownEditor.Core.Tests;

public class HelpSearchTests
{
    private static IReadOnlyList<HelpTopic> SampleToc() =>
    [
        new HelpTopic
        {
            Title = "Home Tab",
            Children =
            [
                new HelpTopic { Title = "Bold, Italic, Strikethrough", File = "home/font.md" },
                new HelpTopic { Title = "Headings and Lists", File = "home/paragraph.md" },
            ],
        },
        new HelpTopic
        {
            Title = "Mermaid Diagrams",
            Children =
            [
                new HelpTopic { Title = "Flowcharts", File = "mermaid/flowchart.md" },
                new HelpTopic { Title = "Sequence Diagrams", File = "mermaid/sequence.md" },
            ],
        },
    ];

    [Fact]
    public void Flatten_returns_only_leaf_topics_in_document_order()
    {
        var flat = HelpSearch.Flatten(SampleToc());

        Assert.Equal(4, flat.Count);
        Assert.Equal(
            ["Bold, Italic, Strikethrough", "Headings and Lists", "Flowcharts", "Sequence Diagrams"],
            flat.Select(t => t.Title));
    }

    [Fact]
    public void Flatten_excludes_category_nodes_themselves()
    {
        var flat = HelpSearch.Flatten(SampleToc());

        Assert.DoesNotContain(flat, t => t.Title is "Home Tab" or "Mermaid Diagrams");
    }

    [Fact]
    public void Search_matches_by_fuzzy_subsequence_on_title()
    {
        var results = HelpSearch.Search(SampleToc(), "flow");

        Assert.Single(results);
        Assert.Equal("Flowcharts", results[0].Title);
    }

    [Fact]
    public void Search_with_empty_query_returns_all_leaf_topics()
    {
        var results = HelpSearch.Search(SampleToc(), "");

        Assert.Equal(4, results.Count);
    }

    [Fact]
    public void Search_with_no_match_returns_empty()
    {
        var results = HelpSearch.Search(SampleToc(), "xyz123");

        Assert.Empty(results);
    }
}
