using FluentAssertions;
using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.Core.Tests;

public class TableEditorTests
{
    private const string SimpleTable =
        "| Name | Age |\n" +
        "| ---- | --- |\n" +
        "| Ann  | 30  |\n" +
        "| Bob  | 40  |";

    [Fact]
    public void FindTableBounds_locates_full_table_from_any_row()
    {
        var lines = SimpleTable.Split('\n');

        TableEditor.FindTableBounds(lines, 2).Should().Be((0, 3));
        TableEditor.FindTableBounds(lines, 0).Should().Be((0, 3));
    }

    [Fact]
    public void FindTableBounds_returns_null_outside_a_table()
    {
        var lines = new[] { "not a table", "| Name | Age |" }; // no separator row

        TableEditor.FindTableBounds(lines, 0).Should().BeNull();
        TableEditor.FindTableBounds(lines, 1).Should().BeNull();
    }

    [Fact]
    public void MoveToNextCell_moves_within_the_same_row()
    {
        // Caret in "Ann" cell (row 2, inside first cell)
        var result = TableEditor.MoveToNextCell(SimpleTable, new CaretPosition(2, 3));

        result.NewSelectionLength.Should().Be(2); // "30" selected
        SelectedText(result).Should().Be("30");
    }

    [Fact]
    public void MoveToNextCell_wraps_to_first_cell_of_next_row()
    {
        // Caret in last cell of row 2 ("30")
        var result = TableEditor.MoveToNextCell(SimpleTable, new CaretPosition(2, 10));

        SelectedText(result).Should().Be("Bob");
    }

    [Fact]
    public void MoveToNextCell_on_last_cell_of_last_row_appends_a_new_row()
    {
        // Caret in last cell of the last row ("40")
        var result = TableEditor.MoveToNextCell(SimpleTable, new CaretPosition(3, 10));

        result.NewText.Split('\n').Should().HaveCount(5);
        SelectedText(result).Should().BeEmpty(); // new cell is empty
    }

    [Fact]
    public void MoveToPreviousCell_moves_backward_across_rows()
    {
        // Caret in first cell of row 3 ("Bob")
        var result = TableEditor.MoveToPreviousCell(SimpleTable, new CaretPosition(3, 3));

        SelectedText(result).Should().Be("30");
    }

    [Fact]
    public void MoveToPreviousCell_at_first_cell_is_a_no_op()
    {
        var result = TableEditor.MoveToPreviousCell(SimpleTable, new CaretPosition(2, 3));

        result.NewText.Should().Be(SimpleTable);
    }

    [Fact]
    public void DeleteRow_removes_the_body_row()
    {
        var result = TableBuilder.DeleteRow(SimpleTable, 0);

        result.Should().NotContain("Ann");
        result.Should().Contain("Bob");
    }

    [Fact]
    public void DeleteColumn_removes_the_column_from_every_row()
    {
        var result = TableBuilder.DeleteColumn(SimpleTable, 1);

        result.Should().NotContain("Age").And.NotContain("30").And.NotContain("40");
        result.Should().Contain("Name").And.Contain("Ann");
    }

    [Fact]
    public void DeleteColumn_refuses_to_remove_the_last_column()
    {
        const string oneColumn = "| Name |\n| ---- |\n| Ann  |";

        var act = () => TableBuilder.DeleteColumn(oneColumn, 0);

        act.Should().Throw<InvalidOperationException>();
    }

    private static string SelectedText(EditResult result) =>
        result.NewText.Substring(result.NewSelectionStart, result.NewSelectionLength);
}
