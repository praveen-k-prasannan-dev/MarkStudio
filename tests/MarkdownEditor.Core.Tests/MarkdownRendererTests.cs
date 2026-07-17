using FluentAssertions;
using MarkdownEditor.Core.Markdown;

namespace MarkdownEditor.Core.Tests;

public class MarkdownRendererTests
{
    private readonly MarkdownRenderer _renderer = new();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Heading_renders_matching_level(int level)
    {
        var markdown = new string('#', level) + " Title";

        var html = _renderer.ToHtml(markdown);

        html.Should().Contain($"<h{level}").And.Contain("Title").And.Contain($"</h{level}>");
    }

    [Theory]
    [InlineData("**bold**", "<strong>bold</strong>")]
    [InlineData("*italic*", "<em>italic</em>")]
    [InlineData("~~gone~~", "<del>gone</del>")]
    [InlineData("==note==", "<mark>note</mark>")]
    [InlineData("`code`", "<code>code</code>")]
    public void Inline_emphasis_renders_expected_tag(string markdown, string expectedFragment)
    {
        _renderer.ToHtml(markdown).Should().Contain(expectedFragment);
    }

    [Fact]
    public void Pipe_table_renders_header_and_rows()
    {
        const string markdown = """
            | Name | Age |
            | ---- | --- |
            | Ann  | 30  |
            | Bob  | 40  |
            """;

        var html = _renderer.ToHtml(markdown);

        html.Should().Contain("<table>");
        html.Should().Contain("<th>Name</th>").And.Contain("<th>Age</th>");
        System.Text.RegularExpressions.Regex.Matches(html, "<tr>").Count.Should().Be(3);
    }

    [Fact]
    public void Task_list_renders_checkboxes()
    {
        const string markdown = """
            - [x] done
            - [ ] todo
            """;

        var html = _renderer.ToHtml(markdown);

        html.Should().Contain("type=\"checkbox\"");
        html.Should().Contain("checked");
        html.Should().Contain("done").And.Contain("todo");
    }

    [Fact]
    public void Fenced_code_block_gets_language_class_and_escapes_html()
    {
        const string markdown = """
            ```csharp
            var x = new List<int>();
            ```
            """;

        var html = _renderer.ToHtml(markdown);

        html.Should().Contain("<pre><code class=\"language-csharp\">");
        html.Should().Contain("List&lt;int&gt;");
    }

    [Fact]
    public void Link_renders_href()
    {
        _renderer.ToHtml("[site](https://example.com)")
            .Should().Contain("<a href=\"https://example.com\">site</a>");
    }

    [Fact]
    public void Image_renders_src_and_alt()
    {
        _renderer.ToHtml("![logo](assets/logo.png)")
            .Should().Contain("src=\"assets/logo.png\"").And.Contain("alt=\"logo\"");
    }

    [Fact]
    public void Raw_html_passes_through_unchanged()
    {
        // Documented behavior: raw HTML is allowed (needed for <br>, sized <img>, etc.).
        _renderer.ToHtml("before <b>raw</b> after").Should().Contain("<b>raw</b>");
    }

    [Fact]
    public void Empty_input_returns_blank_html_without_throwing()
    {
        _renderer.ToHtml(string.Empty).Trim().Should().BeEmpty();
    }

    [Fact]
    public void Null_input_throws_argument_null()
    {
        var act = () => _renderer.ToHtml(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}

public class HtmlDocumentBuilderTests
{
    [Fact]
    public void BuildPage_wraps_body_css_and_title_into_full_document()
    {
        var page = HtmlDocumentBuilder.BuildPage("<p>hello</p>", "body { color: red; }", "My Doc");

        page.Should().StartWith("<!DOCTYPE html>");
        page.Should().Contain("<title>My Doc</title>");
        page.Should().Contain("body { color: red; }");
        page.Should().Contain("<p>hello</p>");
    }

    [Fact]
    public void BuildPage_escapes_html_in_title()
    {
        var page = HtmlDocumentBuilder.BuildPage("<p/>", "", "a <b> & c");

        page.Should().Contain("<title>a &lt;b&gt; &amp; c</title>");
        page.Should().NotContain("<title>a <b>");
    }

    [Fact]
    public void BuildPage_uses_utf8_charset()
    {
        HtmlDocumentBuilder.BuildPage("<p/>", "", "t").Should().Contain("charset=\"utf-8\"");
    }
}
