using System.Windows;
using System.Windows.Input;
using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.App;

/// <summary>
/// Interactive table editing: a contextual "Table" ribbon tab (shown only while the caret is
/// inside a pipe table, like Word's Table Tools), Tab/Shift+Tab cell-to-cell navigation, and
/// row/column insert/delete commands.
/// </summary>
public partial class MainWindow
{
    private void InitializeTableEditing()
    {
        Editor.TextArea.Caret.PositionChanged += (_, _) => UpdateTableTabVisibility();
        Editor.PreviewKeyDown += Editor_PreviewKeyDown_TableNavigation;
    }

    private CaretPosition GetCaretPosition() =>
        new(Editor.TextArea.Caret.Line - 1, Editor.TextArea.Caret.Column - 1);

    private string[] GetNormalizedLines() => Editor.Text.Replace("\r\n", "\n").Split('\n');

    private void UpdateTableTabVisibility()
    {
        var bounds = TableEditor.FindTableBounds(GetNormalizedLines(), GetCaretPosition().Line);
        TableTab.Visibility = bounds is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Editor_PreviewKeyDown_TableNavigation(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab)
            return;

        var caret = GetCaretPosition();
        var lines = GetNormalizedLines();
        if (TableEditor.FindTableBounds(lines, caret.Line) is null)
            return; // not in a table: let AvalonEdit handle Tab normally (indent)

        bool backward = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        var result = backward
            ? TableEditor.MoveToPreviousCell(Editor.Text, caret)
            : TableEditor.MoveToNextCell(Editor.Text, caret);

        ApplyEdit(result);
        e.Handled = true;
    }

    // ---------- Row / column commands (Table tab) ----------

    private (int Start, int End) GetCurrentTableBounds() =>
        TableEditor.FindTableBounds(GetNormalizedLines(), GetCaretPosition().Line)
            ?? throw new InvalidOperationException("The caret is not inside a table.");

    private string GetTableText((int Start, int End) bounds)
    {
        var lines = GetNormalizedLines();
        return string.Join("\n", lines[bounds.Start..(bounds.End + 1)]);
    }

    /// <summary>Body-row index (0 = first row after the header) that the caret currently sits on.</summary>
    private int GetBodyRowIndex((int Start, int End) bounds) =>
        Math.Max(0, GetCaretPosition().Line - (bounds.Start + 2));

    private int GetCurrentColumnIndex()
    {
        var lines = GetNormalizedLines();
        var caret = GetCaretPosition();
        return TableEditor.GetCellIndex(lines[caret.Line], caret.Column);
    }

    /// <summary>Replaces the whole table block and moves the caret onto <paramref name="landOnBodyRow"/>.</summary>
    private void ReplaceTable((int Start, int End) bounds, string newTableText, int landOnBodyRow)
    {
        var lines = GetNormalizedLines().ToList();
        lines.RemoveRange(bounds.Start, bounds.End - bounds.Start + 1);
        var newTableLines = newTableText.TrimEnd('\n').Split('\n');
        lines.InsertRange(bounds.Start, newTableLines);

        string newText = string.Join("\n", lines);

        int targetLine = Math.Clamp(bounds.Start + 2 + landOnBodyRow, bounds.Start, bounds.Start + newTableLines.Length - 1);
        int offset = 0;
        for (int i = 0; i < targetLine; i++)
            offset += lines[i].Length + 1;

        ApplyEdit(new EditResult(newText, Math.Min(offset, newText.Length), 0));
    }

    private void InsertRowAbove_Click(object sender, RoutedEventArgs e) => InsertTableRow(above: true);
    private void InsertRowBelow_Click(object sender, RoutedEventArgs e) => InsertTableRow(above: false);

    private void InsertTableRow(bool above)
    {
        var bounds = GetCurrentTableBounds();
        int bodyRow = GetBodyRowIndex(bounds);
        int insertAt = above ? bodyRow : bodyRow + 1;

        string updated = TableBuilder.InsertRow(GetTableText(bounds), insertAt);
        ReplaceTable(bounds, updated, insertAt);
    }

    private void DeleteRow_Click(object sender, RoutedEventArgs e)
    {
        var bounds = GetCurrentTableBounds();
        string tableText = GetTableText(bounds);
        int bodyRowCount = tableText.TrimEnd('\n').Split('\n').Length - 2;
        if (bodyRowCount <= 0)
            return; // only header + separator left: nothing to delete

        int deleteAt = Math.Clamp(GetBodyRowIndex(bounds), 0, bodyRowCount - 1);
        string updated = TableBuilder.DeleteRow(tableText, deleteAt);
        int landRow = Math.Clamp(deleteAt, 0, Math.Max(0, bodyRowCount - 2));
        ReplaceTable(bounds, updated, landRow);
    }

    private void InsertColumnLeft_Click(object sender, RoutedEventArgs e) => InsertTableColumn(left: true);
    private void InsertColumnRight_Click(object sender, RoutedEventArgs e) => InsertTableColumn(left: false);

    private void InsertTableColumn(bool left)
    {
        var bounds = GetCurrentTableBounds();
        int bodyRow = GetBodyRowIndex(bounds);
        int columnIndex = GetCurrentColumnIndex();
        int insertAt = left ? columnIndex : columnIndex + 1;

        string updated = TableBuilder.InsertColumn(GetTableText(bounds), insertAt);
        ReplaceTable(bounds, updated, bodyRow);
    }

    private void DeleteColumn_Click(object sender, RoutedEventArgs e)
    {
        var bounds = GetCurrentTableBounds();
        int bodyRow = GetBodyRowIndex(bounds);
        int columnIndex = GetCurrentColumnIndex();

        try
        {
            string updated = TableBuilder.DeleteColumn(GetTableText(bounds), columnIndex);
            ReplaceTable(bounds, updated, bodyRow);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Delete Column", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
