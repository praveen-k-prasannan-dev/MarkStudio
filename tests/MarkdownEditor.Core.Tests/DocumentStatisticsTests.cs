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
}
