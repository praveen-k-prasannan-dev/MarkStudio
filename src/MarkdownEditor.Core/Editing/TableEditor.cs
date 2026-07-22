using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownEditor.Core.Editing;

/// <summary>Caret position expressed as a line index (0-based) and column (0-based, in characters).</summary>
public readonly record struct CaretPosition(int Line, int Column);

/// <summary>
/// Detects Markdown pipe tables around the caret and supports Word/Excel-style cell-to-cell
/// navigation (Tab / Shift+Tab), including growing the table when tabbing past the last cell.
/// </summary>
public static class TableEditor
{
    private static readonly Regex SeparatorRow = new(@"^\s*\|?(\s*:?-{3,}:?\s*\|)+\s*:?-{3,}:?\s*\|?\s*$", RegexOptions.Compiled);
    private static readonly Regex TableRowLine = new(@"^\s*\|.*\|\s*$", RegexOptions.Compiled);

    /// <summary>Returns the [startLine, endLine] (inclusive, 0-based) of the table containing <paramref name="line"/>, or null.</summary>
    public static (int Start, int End)? FindTableBounds(string[] lines, int line)
    {
        if (line < 0 || line >= lines.Length || !LooksLikeTableRow(lines[line]))
            return null;

        // A lone row without a separator isn't a table yet.
        bool hasSeparator = false;
        int start = line, end = line;
        while (start > 0 && LooksLikeTableRow(lines[start - 1])) start--;
        while (end < lines.Length - 1 && LooksLikeTableRow(lines[end + 1])) end++;

        for (int i = start; i <= end; i++)
        {
            if (SeparatorRow.IsMatch(lines[i])) { hasSeparator = true; break; }
        }
        return hasSeparator ? (start, end) : null;
    }

    private static bool LooksLikeTableRow(string line) => TableRowLine.IsMatch(line);

    /// <summary>Splits a pipe-table row into its cell texts (leading/trailing pipes consumed, not counted as cells).</summary>
    public static string[] SplitCells(string line)
    {
        string trimmed = line.Trim();
        if (trimmed.StartsWith('|')) trimmed = trimmed[1..];
        if (trimmed.EndsWith('|')) trimmed = trimmed[..^1];
        return trimmed.Split('|');
    }

    /// <summary>
    /// Moves the caret to the next cell (Tab). If already in the last cell of the last row,
    /// a new body row is appended (mirroring Word/Excel) and the caret moves into its first cell.
    /// </summary>
    public static EditResult MoveToNextCell(string text, CaretPosition caret)
    {
        var lines = SplitLines(text);
        var bounds = FindTableBounds(lines, caret.Line) ?? throw new InvalidOperationException("Caret is not inside a table.");

        int cellIndex = GetCellIndex(lines[caret.Line], caret.Column);
        int cellCount = SplitCells(lines[caret.Line]).Length;

        if (cellIndex < cellCount - 1)
            return SelectCell(text, lines, caret.Line, cellIndex + 1);

        // Last cell of this row.
        int lastBodyRow = bounds.End;
        if (caret.Line < lastBodyRow)
        {
            int nextLine = caret.Line + 1;
            if (SeparatorRow.IsMatch(lines[nextLine]) && nextLine < lastBodyRow)
                nextLine++; // skip the separator row itself
            return SelectCell(text, lines, nextLine, 0);
        }

        // Last cell of the last row: append a new row with the same column count.
        int columnCount = SplitCells(lines[bounds.End]).Length;
        string newRow = "|" + string.Join("|", Enumerable.Repeat("   ", columnCount)) + "|";
        var newLines = lines.ToList();
        newLines.Insert(bounds.End + 1, newRow);
        string newText = string.Join("\n", newLines);
        return SelectCell(newText, newLines.ToArray(), bounds.End + 1, 0);
    }

    /// <summary>Moves the caret to the previous cell (Shift+Tab). No-op at the very first cell.</summary>
    public static EditResult MoveToPreviousCell(string text, CaretPosition caret)
    {
        var lines = SplitLines(text);
        var bounds = FindTableBounds(lines, caret.Line) ?? throw new InvalidOperationException("Caret is not inside a table.");

        int cellIndex = GetCellIndex(lines[caret.Line], caret.Column);
        if (cellIndex > 0)
            return SelectCell(text, lines, caret.Line, cellIndex - 1);

        int prevLine = caret.Line - 1;
        if (prevLine >= bounds.Start && SeparatorRow.IsMatch(lines[prevLine]))
            prevLine--;

        if (prevLine < bounds.Start)
        {
            int offset = LineStartOffset(lines, caret.Line) + caret.Column;
            return new EditResult(text, offset, 0); // already at the first cell
        }

        int lastCellIndex = SplitCells(lines[prevLine]).Length - 1;
        return SelectCell(text, lines, prevLine, lastCellIndex);
    }

    /// <summary>Returns which cell (0-based) a character column within a table row line falls into.</summary>
    public static int GetCellIndex(string line, int column)
    {
        string trimmed = line.TrimStart();
        int leadingWs = line.Length - trimmed.Length;
        int pos = leadingWs + (trimmed.StartsWith('|') ? 1 : 0);
        var cells = SplitCells(line);

        for (int i = 0; i < cells.Length; i++)
        {
            int cellEnd = pos + cells[i].Length;
            if (column <= cellEnd) return i;
            pos = cellEnd + 1; // skip the '|'
        }
        return cells.Length - 1;
    }

    private static EditResult SelectCell(string text, string[] lines, int lineIndex, int cellIndex)
    {
        string line = lines[lineIndex];
        string trimmed = line.TrimStart();
        int leadingWs = line.Length - trimmed.Length;
        int pos = leadingWs + (trimmed.StartsWith('|') ? 1 : 0);
        var cells = SplitCells(line);
        cellIndex = Math.Clamp(cellIndex, 0, cells.Length - 1);

        for (int i = 0; i < cellIndex; i++)
            pos += cells[i].Length + 1;

        string cellContent = cells[cellIndex];
        int trimStart = cellContent.Length - cellContent.TrimStart().Length;
        int trimEnd = cellContent.Length - cellContent.TrimEnd().Length;
        int selStart = pos + trimStart;
        int selLength = Math.Max(0, cellContent.Length - trimStart - trimEnd);

        int lineOffset = LineStartOffset(lines, lineIndex);
        return new EditResult(text, lineOffset + selStart, selLength);
    }

    private static int LineStartOffset(string[] lines, int lineIndex)
    {
        int offset = 0;
        for (int i = 0; i < lineIndex; i++)
            offset += lines[i].Length + 1; // + newline
        return offset;
    }

    private static string[] SplitLines(string text) => text.Replace("\r\n", "\n").Split('\n');
}
