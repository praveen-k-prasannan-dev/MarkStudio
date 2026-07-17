using FluentAssertions;
using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.Core.Tests;

public class TableBuilderTests
{
    private static string[] Lines(string table) =>
        table.TrimEnd('\n').Split('\n');

    private static int Pipes(string line) => line.Count(c => c == '|');

    [Fact]
    public void Create_produces_header_separator_and_body_rows()
    {
        var lines = Lines(TableBuilder.Create(2, 3));

        lines.Should().HaveCount(4); // header + separator + 2 body rows
        lines[0].Should().Contain("Header 1").And.Contain("Header 3");
        lines[1].Should().Contain("---");
    }

    [Fact]
    public void Create_gives_every_line_a_consistent_column_count()
    {
        var lines = Lines(TableBuilder.Create(3, 4));

        lines.Should().OnlyContain(l => Pipes(l) == 5); // cols + 1 pipe characters
    }

    [Fact]
    public void Create_aligns_columns_to_equal_width()
    {
        var lines = Lines(TableBuilder.Create(2, 2));

        lines.Select(l => l.Length).Distinct().Should().HaveCount(1);
    }

    [Fact]
    public void Create_rejects_non_positive_dimensions()
    {
        var actRows = () => TableBuilder.Create(0, 3);
        var actCols = () => TableBuilder.Create(3, 0);

        actRows.Should().Throw<ArgumentOutOfRangeException>();
        actCols.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void InsertRow_adds_a_valid_body_row()
    {
        var table = TableBuilder.Create(2, 3);

        var lines = Lines(TableBuilder.InsertRow(table, 1));

        lines.Should().HaveCount(5);
        lines.Should().OnlyContain(l => Pipes(l) == 4);
    }

    [Fact]
    public void InsertColumn_adds_a_column_to_every_row()
    {
        var table = TableBuilder.Create(2, 3);

        var lines = Lines(TableBuilder.InsertColumn(table, 1));

        lines.Should().OnlyContain(l => Pipes(l) == 5);
        lines[1].Should().MatchRegex(@"---"); // separator row still valid
    }
}
