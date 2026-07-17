using FluentAssertions;
using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.Core.Tests;

public class BlockFormatterTests
{
    [Fact]
    public void SetHeading_adds_prefix_to_plain_line()
    {
        var result = BlockFormatter.SetHeading(new TextSelection("Hello", 0, 0), 2);

        result.NewText.Should().Be("## Hello");
    }

    [Fact]
    public void SetHeading_replaces_existing_level()
    {
        var result = BlockFormatter.SetHeading(new TextSelection("#### Hello", 0, 0), 2);

        result.NewText.Should().Be("## Hello");
    }

    [Fact]
    public void SetHeading_zero_removes_heading()
    {
        var result = BlockFormatter.SetHeading(new TextSelection("## Hello", 0, 0), 0);

        result.NewText.Should().Be("Hello");
    }

    [Fact]
    public void SetHeading_applies_to_every_selected_line()
    {
        var result = BlockFormatter.SetHeading(new TextSelection("One\nTwo", 0, 7), 1);

        result.NewText.Should().Be("# One\n# Two");
    }

    [Fact]
    public void SetHeading_works_on_caret_only_mid_line()
    {
        var result = BlockFormatter.SetHeading(new TextSelection("first\nsecond\nthird", 8, 0), 3);

        result.NewText.Should().Be("first\n### second\nthird");
    }

    [Fact]
    public void ToggleBlockquote_adds_and_removes_prefix()
    {
        var quoted = BlockFormatter.ToggleBlockquote(new TextSelection("One\nTwo", 0, 7));
        quoted.NewText.Should().Be("> One\n> Two");

        var unquoted = BlockFormatter.ToggleBlockquote(
            new TextSelection(quoted.NewText, quoted.NewSelectionStart, quoted.NewSelectionLength));
        unquoted.NewText.Should().Be("One\nTwo");
    }

    [Fact]
    public void ToggleCodeFence_wraps_and_unwraps()
    {
        var fenced = BlockFormatter.ToggleCodeFence(new TextSelection("var x;", 0, 6), "csharp");
        fenced.NewText.Should().Be("```csharp\nvar x;\n```");

        var unfenced = BlockFormatter.ToggleCodeFence(
            new TextSelection(fenced.NewText, fenced.NewSelectionStart, fenced.NewSelectionLength));
        unfenced.NewText.Should().Be("var x;");
    }

    [Fact]
    public void InsertHorizontalRule_separates_from_previous_text_with_blank_line()
    {
        // "abc\n---" would parse as a setext heading, so a blank line must be inserted.
        var result = BlockFormatter.InsertHorizontalRule(new TextSelection("abc", 3, 0));

        result.NewText.Should().Be("abc\n\n---\n");
    }

    [Fact]
    public void InsertHorizontalRule_in_empty_document_needs_no_separation()
    {
        var result = BlockFormatter.InsertHorizontalRule(new TextSelection("", 0, 0));

        result.NewText.Should().Be("---\n");
    }
}
