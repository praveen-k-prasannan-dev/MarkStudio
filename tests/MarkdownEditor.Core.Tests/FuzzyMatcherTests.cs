using FluentAssertions;
using MarkdownEditor.Core.Palette;

namespace MarkdownEditor.Core.Tests;

public class FuzzyMatcherTests
{
    [Fact]
    public void Empty_query_matches_everything()
    {
        FuzzyMatcher.Match("Bold", "").IsMatch.Should().BeTrue();
    }

    [Fact]
    public void Exact_substring_matches()
    {
        FuzzyMatcher.Match("Insert Table", "table").IsMatch.Should().BeTrue();
    }

    [Fact]
    public void Subsequence_matches_even_when_not_contiguous()
    {
        FuzzyMatcher.Match("Insert Table", "intbl").IsMatch.Should().BeTrue();
    }

    [Fact]
    public void Out_of_order_characters_do_not_match()
    {
        FuzzyMatcher.Match("Bold", "ob").IsMatch.Should().BeFalse();
    }

    [Fact]
    public void Missing_character_does_not_match()
    {
        FuzzyMatcher.Match("Bold", "boldx").IsMatch.Should().BeFalse();
    }

    [Fact]
    public void Matching_is_case_insensitive()
    {
        FuzzyMatcher.Match("Export to PDF", "PDF").IsMatch.Should().BeTrue();
        FuzzyMatcher.Match("Export to PDF", "pdf").IsMatch.Should().BeTrue();
    }

    [Fact]
    public void Prefix_match_scores_higher_than_suffix_only_match()
    {
        var prefix = FuzzyMatcher.Match("Bold", "bo");
        var suffix = FuzzyMatcher.Match("Bold", "ld");

        prefix.Score.Should().BeGreaterThan(suffix.Score);
    }

    [Fact]
    public void Word_boundary_match_scores_higher_than_mid_word_match()
    {
        var atBoundary = FuzzyMatcher.Match("Table", "t");
        var midWord = FuzzyMatcher.Match("xTable", "t"); // greedy match lands on 'x', not the boundary 'T'

        atBoundary.Score.Should().BeGreaterThan(midWord.Score);
    }

    [Fact]
    public void Shorter_text_scores_higher_than_longer_text_for_the_same_match()
    {
        var shortText = FuzzyMatcher.Match("Bold", "bold");
        var longText = FuzzyMatcher.Match("Bold and more text after it", "bold");

        shortText.Score.Should().BeGreaterThan(longText.Score);
    }

    [Fact]
    public void Filter_returns_only_matching_items()
    {
        var items = new[] { "Bold", "Italic", "Insert Table", "Insert Link", "Export to PDF" };

        var results = FuzzyMatcher.Filter(items, x => x, "in");

        results.Should().Contain("Insert Table").And.Contain("Insert Link");
        results.Should().NotContain("Bold").And.NotContain("Export to PDF");
    }

    [Fact]
    public void Filter_orders_best_matches_first()
    {
        var items = new[] { "Insert Table", "Table" };

        var results = FuzzyMatcher.Filter(items, x => x, "table");

        results[0].Should().Be("Table"); // exact prefix match beats a later substring match
    }

    [Fact]
    public void Filter_with_empty_query_returns_all_items_unfiltered_in_order()
    {
        var items = new[] { "Bold", "Italic", "Underline" };

        var results = FuzzyMatcher.Filter(items, x => x, "");

        results.Should().BeEquivalentTo(items);
    }
}
