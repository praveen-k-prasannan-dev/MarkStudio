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

    private static readonly Dictionary<string, string> SampleContent = new()
    {
        ["home/font.md"] = "Bold and italic toggle like Word.",
        ["home/paragraph.md"] = "Headings, bullet lists, and numbered lists.",
        ["mermaid/flowchart.md"] = "Insert a row above or below is not mentioned here.",
        ["mermaid/sequence.md"] = "Participants and messages between them.",
    };

    private static string GetSampleContent(HelpTopic topic) => SampleContent[topic.File!];

    [Fact]
    public void SearchWithContent_finds_topics_whose_title_does_not_match_but_body_does()
    {
        var results = HelpSearch.SearchWithContent(SampleToc(), "insert a row", GetSampleContent);

        Assert.Contains(results, t => t.Title == "Flowcharts");
    }

    [Fact]
    public void SearchWithContent_ranks_title_matches_before_content_only_matches()
    {
        var toc = new[]
        {
            new HelpTopic { Title = "Lists", File = "a.md" },
            new HelpTopic { Title = "Something Else", File = "b.md" },
        };
        var content = new Dictionary<string, string>
        {
            ["a.md"] = "no relevant body text",
            ["b.md"] = "this topic mentions lists in its body",
        };

        var results = HelpSearch.SearchWithContent(toc, "lists", t => content[t.File!]);

        Assert.Equal(["Lists", "Something Else"], results.Select(t => t.Title));
    }

    [Fact]
    public void SearchWithContent_content_match_is_case_insensitive()
    {
        var results = HelpSearch.SearchWithContent(SampleToc(), "PARTICIPANTS", GetSampleContent);

        Assert.Contains(results, t => t.Title == "Sequence Diagrams");
    }

    [Fact]
    public void SearchWithContent_with_empty_query_returns_all_leaf_topics()
    {
        var results = HelpSearch.SearchWithContent(SampleToc(), "", GetSampleContent);

        Assert.Equal(4, results.Count);
    }

    [Fact]
    public void SearchWithContent_does_not_duplicate_a_topic_that_matches_both_title_and_content()
    {
        var toc = new[] { new HelpTopic { Title = "Flowcharts", File = "a.md" } };
        var content = new Dictionary<string, string> { ["a.md"] = "flowcharts are diagrams" };

        var results = HelpSearch.SearchWithContent(toc, "flow", t => content[t.File!]);

        Assert.Single(results);
    }
}
