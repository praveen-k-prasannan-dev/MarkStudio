using FluentAssertions;
using MarkdownEditor.Core.Documents;

namespace MarkdownEditor.Core.Tests;

public class DocumentStatisticsTests
{
    [Fact]
    public void Counts_plain_words()
    {
        DocumentStatistics.Compute("Hello world").Words.Should().Be(2);
    }

    [Fact]
    public void Empty_text_has_zero_everything()
    {
        DocumentStatistics.Compute("").Should().Be(new DocumentStatistics(0, 0, 0));
    }

    [Fact]
    public void Markdown_markers_are_not_counted_as_words()
    {
        DocumentStatistics.Compute("# Hello **world**").Words.Should().Be(2);
    }

    [Fact]
    public void Counts_lines_and_characters()
    {
        var stats = DocumentStatistics.Compute("a\nb\nc");

        stats.Lines.Should().Be(3);
        stats.Characters.Should().Be(5);
    }

    [Fact]
    public void Contractions_count_as_single_words()
    {
        DocumentStatistics.Compute("don't stop").Words.Should().Be(2);
    }

    [Fact]
    public void Empty_document_has_zero_reading_time()
    {
        DocumentStatistics.Compute("").ReadingTimeMinutes.Should().Be(0);
    }

    [Fact]
    public void A_few_words_round_up_to_one_minute()
    {
        DocumentStatistics.Compute("Hello world").ReadingTimeMinutes.Should().Be(1);
    }

    [Fact]
    public void Reading_time_rounds_up_at_200_words_per_minute()
    {
        string text = string.Join(" ", Enumerable.Repeat("word", 201));

        DocumentStatistics.Compute(text).ReadingTimeMinutes.Should().Be(2);
    }

    [Fact]
    public void Exactly_200_words_is_one_minute()
    {
        string text = string.Join(" ", Enumerable.Repeat("word", 200));

        DocumentStatistics.Compute(text).ReadingTimeMinutes.Should().Be(1);
    }
}
