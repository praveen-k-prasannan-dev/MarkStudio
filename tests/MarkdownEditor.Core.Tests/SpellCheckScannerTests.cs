using FluentAssertions;
using MarkdownEditor.Core.Spelling;

namespace MarkdownEditor.Core.Tests;

public class SpellCheckScannerTests
{
    // A tiny fake dictionary: only these words (case-insensitive) are "correctly spelled".
    private static readonly HashSet<string> Known = new(StringComparer.OrdinalIgnoreCase)
        { "the", "quick", "brown", "fox", "hello", "world", "dont", "stop" };

    private static bool IsCorrect(string word) => Known.Contains(word);

    [Fact]
    public void Flags_a_single_misspelled_word()
    {
        var results = SpellCheckScanner.FindMisspelledWords("hello wrold", IsCorrect);

        results.Should().ContainSingle();
        var span = results[0];
        "hello wrold".Substring(span.Start, span.Length).Should().Be("wrold");
    }

    [Fact]
    public void Does_not_flag_correctly_spelled_words()
    {
        var results = SpellCheckScanner.FindMisspelledWords("the quick brown fox", IsCorrect);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Flags_multiple_misspelled_words_with_correct_spans()
    {
        const string text = "xylophone quick zzzsome";
        var results = SpellCheckScanner.FindMisspelledWords(text, IsCorrect);

        results.Should().HaveCount(2);
        results.Select(r => text.Substring(r.Start, r.Length)).Should().BeEquivalentTo("xylophone", "zzzsome");
    }

    [Fact]
    public void Empty_text_returns_no_results()
    {
        SpellCheckScanner.FindMisspelledWords("", IsCorrect).Should().BeEmpty();
    }

    [Fact]
    public void Words_inside_a_fenced_code_block_are_not_checked()
    {
        const string text = """
            Some misspeled text.

            ```
            xxinvalidxx yyinvalidyy
            ```

            More misspeled text.
            """;

        var results = SpellCheckScanner.FindMisspelledWords(text, IsCorrect);

        results.Should().NotBeEmpty(); // "misspeled" outside the fence should still be flagged
        results.Select(r => text.Substring(r.Start, r.Length)).Should().NotContain("xxinvalidxx").And.NotContain("yyinvalidyy");
    }

    [Fact]
    public void Words_inside_inline_code_are_not_checked()
    {
        const string text = "Use `xxinvalidxx` in your code, not misspeled.";

        var results = SpellCheckScanner.FindMisspelledWords(text, IsCorrect);

        results.Select(r => text.Substring(r.Start, r.Length)).Should().NotContain("xxinvalidxx").And.Contain("misspeled");
    }

    [Fact]
    public void Urls_are_not_checked()
    {
        const string text = "Visit https://xxinvalidxx.example/path for misspeled info.";

        var results = SpellCheckScanner.FindMisspelledWords(text, IsCorrect);

        results.Select(r => text.Substring(r.Start, r.Length)).Should().NotContain("xxinvalidxx").And.Contain("misspeled");
    }

    [Fact]
    public void Contractions_are_treated_as_a_single_token()
    {
        // "dont" is in the fake dictionary (without the apostrophe) but "don't" (with it) is not,
        // so the whole contraction should be flagged as one misspelled span, not split apart.
        const string text = "don't stop";

        var results = SpellCheckScanner.FindMisspelledWords(text, IsCorrect);

        results.Should().ContainSingle();
        text.Substring(results[0].Start, results[0].Length).Should().Be("don't");
    }

    [Fact]
    public void Null_text_throws()
    {
        var act = () => SpellCheckScanner.FindMisspelledWords(null!, IsCorrect);
        act.Should().Throw<ArgumentNullException>();
    }
}
