using MarkdownEditor.Core.Help;

namespace MarkdownEditor.Core.Tests;

public class HelpTocLoaderTests
{
    [Fact]
    public void Parses_flat_list_of_categories_with_leaf_topics()
    {
        const string json = """
        [
            { "title": "Getting Started", "children": [
                { "title": "Welcome", "file": "getting-started/welcome.md" }
            ] }
        ]
        """;

        var toc = HelpTocLoader.Parse(json);

        Assert.Single(toc);
        Assert.Equal("Getting Started", toc[0].Title);
        Assert.Null(toc[0].File);
        Assert.Single(toc[0].Children);
        Assert.Equal("Welcome", toc[0].Children[0].Title);
        Assert.Equal("getting-started/welcome.md", toc[0].Children[0].File);
    }

    [Fact]
    public void Supports_nested_categories()
    {
        const string json = """
        [
            { "title": "Root", "children": [
                { "title": "Sub", "children": [
                    { "title": "Leaf", "file": "a.md" }
                ] }
            ] }
        ]
        """;

        var toc = HelpTocLoader.Parse(json);

        var leaf = toc[0].Children[0].Children[0];
        Assert.Equal("Leaf", leaf.Title);
        Assert.Equal("a.md", leaf.File);
    }

    [Fact]
    public void Category_without_children_key_gets_empty_children_list()
    {
        const string json = """[ { "title": "Lonely Leaf", "file": "a.md" } ]""";

        var toc = HelpTocLoader.Parse(json);

        Assert.Empty(toc[0].Children);
    }

    [Fact]
    public void Empty_array_parses_to_empty_list()
    {
        var toc = HelpTocLoader.Parse("[]");

        Assert.Empty(toc);
    }
}
