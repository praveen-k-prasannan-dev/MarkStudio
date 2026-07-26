using FluentAssertions;
using MarkdownEditor.Core.Spelling;

namespace MarkdownEditor.Core.Tests;

public class SpellCheckerTests
{
    private static SpellChecker CreateTestChecker() =>
        SpellChecker.CreateFromWords(["hello", "world", "spelling", "checker"]);

    [Fact]
    public void Recognizes_a_known_word_as_correct()
    {
        CreateTestChecker().IsCorrect("hello").Should().BeTrue();
    }

    [Fact]
    public void Flags_an_unknown_word_as_incorrect()
    {
        CreateTestChecker().IsCorrect("wrold").Should().BeFalse();
    }

    [Fact]
    public void Suggest_offers_at_least_one_close_match_for_a_typo()
    {
        var checker = CreateTestChecker();

        var suggestions = checker.Suggest("helo");

        suggestions.Should().Contain("hello");
    }

    [Fact]
    public void AddWord_makes_a_previously_unknown_word_correct()
    {
        var checker = CreateTestChecker();
        checker.IsCorrect("markstudio").Should().BeFalse();

        checker.AddWord("markstudio");

        checker.IsCorrect("markstudio").Should().BeTrue();
    }

    [Fact]
    public void Null_word_throws()
    {
        var checker = CreateTestChecker();

        var act = () => checker.IsCorrect(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
