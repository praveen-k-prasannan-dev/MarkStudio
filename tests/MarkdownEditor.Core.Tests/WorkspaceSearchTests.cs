using MarkdownEditor.Core.Workspace;

namespace MarkdownEditor.Core.Tests;

public class WorkspaceSearchTests
{
    [Fact]
    public void Finds_matches_across_multiple_files()
    {
        var files = new[]
        {
            ("a.md", "Hello world\nSecond line"),
            ("b.md", "Nothing here"),
            ("c.md", "Another world reference"),
        };

        var results = WorkspaceSearch.Search(files, "world");

        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.FilePath == "a.md" && r.LineNumber == 1);
        Assert.Contains(results, r => r.FilePath == "c.md" && r.LineNumber == 1);
    }

    [Fact]
    public void Match_is_case_insensitive()
    {
        var files = new[] { ("a.md", "HELLO") };

        var results = WorkspaceSearch.Search(files, "hello");

        Assert.Single(results);
    }

    [Fact]
    public void Reports_the_matching_line_text_trimmed()
    {
        var files = new[] { ("a.md", "   indented match line   ") };

        var results = WorkspaceSearch.Search(files, "match");

        Assert.Equal("indented match line", results[0].LineText);
    }

    [Fact]
    public void Multiple_matches_in_the_same_file_are_all_returned()
    {
        var files = new[] { ("a.md", "cat\ndog\ncat again") };

        var results = WorkspaceSearch.Search(files, "cat");

        Assert.Equal(2, results.Count);
        Assert.Equal([1, 3], results.Select(r => r.LineNumber));
    }

    [Fact]
    public void Empty_query_returns_no_results()
    {
        var files = new[] { ("a.md", "anything") };

        var results = WorkspaceSearch.Search(files, "");

        Assert.Empty(results);
    }

    [Fact]
    public void No_match_returns_empty_list()
    {
        var files = new[] { ("a.md", "nothing relevant") };

        var results = WorkspaceSearch.Search(files, "xyz123");

        Assert.Empty(results);
    }
}
