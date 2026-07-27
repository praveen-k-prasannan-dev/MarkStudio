using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.Core.Tests;

public class SnippetExpanderTests
{
    [Fact]
    public void Expands_a_known_trigger_immediately_before_the_caret()
    {
        string text = "todo";

        var result = SnippetExpander.TryExpand(text, text.Length);

        Assert.NotNull(result);
        Assert.Equal(0, result!.Value.ReplaceStart);
        Assert.Equal(4, result.Value.ReplaceLength);
        Assert.Equal("- [ ] ", result.Value.Replacement);
    }

    [Fact]
    public void Places_the_caret_at_the_dollar_zero_marker()
    {
        const string trigger = "todo";

        var result = SnippetExpander.TryExpand(trigger, trigger.Length);

        // "- [ ] $0" -> marker removed, caret lands right after "- [ ] "
        Assert.Equal(6, result!.Value.NewCaretPosition);
    }

    [Fact]
    public void Places_the_caret_at_the_end_when_expansion_has_no_marker()
    {
        var snippets = new[] { new Snippet("sig", "Best regards,\nPraveen") };
        const string trigger = "sig";

        var result = SnippetExpander.TryExpand(trigger, trigger.Length, snippets);

        Assert.Equal("Best regards,\nPraveen".Length, result!.Value.NewCaretPosition);
    }

    [Fact]
    public void Returns_null_when_there_is_no_word_immediately_before_the_caret()
    {
        const string text = "   ";

        var result = SnippetExpander.TryExpand(text, text.Length);

        Assert.Null(result);
    }

    [Fact]
    public void Returns_null_for_an_unknown_trigger()
    {
        const string text = "xyzzy";

        var result = SnippetExpander.TryExpand(text, text.Length);

        Assert.Null(result);
    }

    [Fact]
    public void Matching_is_case_insensitive()
    {
        const string text = "TODO";

        var result = SnippetExpander.TryExpand(text, text.Length);

        Assert.NotNull(result);
        Assert.Equal("- [ ] ", result!.Value.Replacement);
    }

    [Fact]
    public void Only_the_contiguous_word_immediately_before_the_caret_is_considered()
    {
        const string text = "xtodo"; // not a known trigger even though it ends with "todo"

        var result = SnippetExpander.TryExpand(text, text.Length);

        Assert.Null(result);
    }

    [Fact]
    public void Expands_mid_document_leaving_surrounding_text_untouched()
    {
        const string text = "Notes\ntodo\nMore text";
        int caretAfterTrigger = "Notes\ntodo".Length;

        var result = SnippetExpander.TryExpand(text, caretAfterTrigger);

        Assert.Equal("Notes\n".Length, result!.Value.ReplaceStart);
        Assert.Equal(4, result.Value.ReplaceLength);
    }

    [Fact]
    public void A_custom_snippet_list_overrides_the_built_in_set()
    {
        var custom = new[] { new Snippet("hello", "world") };
        const string text = "todo"; // a built-in trigger, but not in the custom list

        var result = SnippetExpander.TryExpand(text, text.Length, custom);

        Assert.Null(result);
    }

    [Fact]
    public void Built_in_snippets_cover_table_meeting_and_code()
    {
        Assert.Contains(SnippetExpander.BuiltIn, s => s.Trigger == "table");
        Assert.Contains(SnippetExpander.BuiltIn, s => s.Trigger == "meeting");
        Assert.Contains(SnippetExpander.BuiltIn, s => s.Trigger == "code");
    }
}
