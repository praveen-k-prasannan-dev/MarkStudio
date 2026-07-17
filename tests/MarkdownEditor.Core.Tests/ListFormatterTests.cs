using FluentAssertions;
using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.Core.Tests;

public class ListFormatterTests
{
    private static TextSelection All(string text) => new(text, 0, text.Length);

    [Fact]
    public void Toggle_bullet_adds_then_removes_prefixes()
    {
        var listed = ListFormatter.ToggleList(All("One\nTwo\nThree"), ListKind.Bullet);
        listed.NewText.Should().Be("- One\n- Two\n- Three");

        var unlisted = ListFormatter.ToggleList(
            new TextSelection(listed.NewText, listed.NewSelectionStart, listed.NewSelectionLength),
            ListKind.Bullet);
        unlisted.NewText.Should().Be("One\nTwo\nThree");
    }

    [Fact]
    public void Toggle_numbered_numbers_sequentially()
    {
        var result = ListFormatter.ToggleList(All("One\nTwo\nThree"), ListKind.Numbered);

        result.NewText.Should().Be("1. One\n2. Two\n3. Three");
    }

    [Fact]
    public void Toggle_task_produces_unchecked_items()
    {
        var result = ListFormatter.ToggleList(All("One\nTwo"), ListKind.Task);

        result.NewText.Should().Be("- [ ] One\n- [ ] Two");
    }

    [Fact]
    public void Converts_bullet_list_to_task_list_preserving_text()
    {
        var result = ListFormatter.ToggleList(All("- One\n- Two"), ListKind.Task);

        result.NewText.Should().Be("- [ ] One\n- [ ] Two");
    }

    [Fact]
    public void Converts_bullet_list_to_numbered_with_renumbering()
    {
        var result = ListFormatter.ToggleList(All("- One\n- Two\n- Three"), ListKind.Numbered);

        result.NewText.Should().Be("1. One\n2. Two\n3. Three");
    }

    [Fact]
    public void Blank_lines_in_selection_are_left_untouched()
    {
        var result = ListFormatter.ToggleList(All("One\n\nTwo"), ListKind.Bullet);

        result.NewText.Should().Be("- One\n\n- Two");
    }

    [Fact]
    public void Numbering_skips_blank_lines_but_stays_sequential()
    {
        var result = ListFormatter.ToggleList(All("One\n\nTwo"), ListKind.Numbered);

        result.NewText.Should().Be("1. One\n\n2. Two");
    }
}
