using FluentAssertions;
using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.Core.Tests;

public class InlineFormatterTests
{
    [Fact]
    public void Wraps_selection_and_selects_inner_text()
    {
        var result = InlineFormatter.Toggle(new TextSelection("abc", 0, 3), InlineFormatter.Bold);

        result.NewText.Should().Be("**abc**");
        result.NewSelectionStart.Should().Be(2);
        result.NewSelectionLength.Should().Be(3);
    }

    [Fact]
    public void Unwraps_when_selection_includes_the_markers()
    {
        var result = InlineFormatter.Toggle(new TextSelection("**abc**", 0, 7), InlineFormatter.Bold);

        result.NewText.Should().Be("abc");
        result.NewSelectionStart.Should().Be(0);
        result.NewSelectionLength.Should().Be(3);
    }

    [Fact]
    public void Unwraps_when_markers_surround_the_selection()
    {
        var result = InlineFormatter.Toggle(new TextSelection("**abc**", 2, 3), InlineFormatter.Bold);

        result.NewText.Should().Be("abc");
        result.NewSelectionStart.Should().Be(0);
        result.NewSelectionLength.Should().Be(3);
    }

    [Fact]
    public void Empty_selection_inserts_marker_pair_with_caret_in_middle()
    {
        var result = InlineFormatter.Toggle(new TextSelection("hello ", 6, 0), InlineFormatter.Bold);

        result.NewText.Should().Be("hello ****");
        result.NewSelectionStart.Should().Be(8);
        result.NewSelectionLength.Should().Be(0);
    }

    [Fact]
    public void Markers_hug_the_word_when_selection_has_trailing_whitespace()
    {
        var result = InlineFormatter.Toggle(new TextSelection("abc ", 0, 4), InlineFormatter.Bold);

        result.NewText.Should().Be("**abc** ");
        result.NewSelectionStart.Should().Be(2);
        result.NewSelectionLength.Should().Be(3);
    }

    [Theory]
    [InlineData("**", "**word**")]
    [InlineData("*", "*word*")]
    [InlineData("~~", "~~word~~")]
    [InlineData("`", "`word`")]
    [InlineData("==", "==word==")]
    public void Each_marker_type_wraps_correctly(string marker, string expected)
    {
        var result = InlineFormatter.Toggle(new TextSelection("word", 0, 4), marker);

        result.NewText.Should().Be(expected);
    }

    [Fact]
    public void Toggle_twice_returns_to_original_text()
    {
        var once = InlineFormatter.Toggle(new TextSelection("abc def", 4, 3), InlineFormatter.Italic);
        var twice = InlineFormatter.Toggle(
            new TextSelection(once.NewText, once.NewSelectionStart, once.NewSelectionLength),
            InlineFormatter.Italic);

        twice.NewText.Should().Be("abc def");
    }
}
