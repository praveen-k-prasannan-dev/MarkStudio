using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using MarkdownEditor.Core.Clipboard;

namespace MarkdownEditor.Core.Tests;

public class ClipboardHtmlFormatTests
{
    [Fact]
    public void Build_includes_the_required_cf_html_header_fields()
    {
        string result = ClipboardHtmlFormat.Build("<p>hello</p>");

        result.Should().Contain("Version:0.9");
        result.Should().MatchRegex(@"StartHTML:\d{10}");
        result.Should().MatchRegex(@"EndHTML:\d{10}");
        result.Should().MatchRegex(@"StartFragment:\d{10}");
        result.Should().MatchRegex(@"EndFragment:\d{10}");
    }

    [Fact]
    public void Offsets_point_at_the_correct_substrings()
    {
        const string body = "<p>hello</p>";
        string result = ClipboardHtmlFormat.Build(body);
        byte[] bytes = Encoding.UTF8.GetBytes(result);

        int startHtml = ExtractOffset(result, "StartHTML");
        int endHtml = ExtractOffset(result, "EndHTML");
        int startFragment = ExtractOffset(result, "StartFragment");
        int endFragment = ExtractOffset(result, "EndFragment");

        Encoding.UTF8.GetString(bytes, startHtml, 6).Should().Be("<html>");
        Encoding.UTF8.GetString(bytes, startFragment, body.Length).Should().Be(body);
        (endHtml - startHtml).Should().BeGreaterThan(0);
        (endFragment - startFragment).Should().Be(Encoding.UTF8.GetByteCount(body));
    }

    [Fact]
    public void Handles_empty_body()
    {
        string result = ClipboardHtmlFormat.Build("");

        result.Should().Contain("<!--StartFragment--><!--EndFragment-->");
    }

    [Fact]
    public void Null_body_throws()
    {
        var act = () => ClipboardHtmlFormat.Build(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private static int ExtractOffset(string header, string key)
    {
        var match = Regex.Match(header, $@"{key}:(\d{{10}})");
        return int.Parse(match.Groups[1].Value);
    }
}
