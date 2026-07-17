using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownEditor.Core.Editing;

/// <summary>Creates and edits well-formed, column-aligned Markdown pipe tables.</summary>
public static class TableBuilder
{
    private static readonly Regex SeparatorCell = new(@"^:?-{3,}:?$", RegexOptions.Compiled);

    /// <summary>Builds a pipe table with <paramref name="rows"/> body rows and <paramref name="cols"/> columns.</summary>
    public static string Create(int rows, int cols, bool hasHeader = true)
    {
        if (rows < 1) throw new ArgumentOutOfRangeException(nameof(rows));
        if (cols < 1) throw new ArgumentOutOfRangeException(nameof(cols));

        var headers = Enumerable.Range(1, cols)
            .Select(i => hasHeader ? $"Header {i}" : "")
            .ToArray();
        int width = Math.Max(3, headers.Max(h => h.Length));

        var sb = new StringBuilder();
        AppendRow(sb, headers, width);
        AppendRow(sb, Enumerable.Repeat(new string('-', width), cols).ToArray(), width);
        for (int r = 0; r < rows; r++)
            AppendRow(sb, Enumerable.Repeat("", cols).ToArray(), width);
        return sb.ToString();
    }

    /// <summary>Inserts an empty body row at the given body-row index (0 = first row after the header).</summary>
    public static string InsertRow(string tableText, int index)
    {
        var lines = SplitLines(tableText);
        var cells = ParseCells(lines[0]);
        int width = cells.Max(c => c.Length);

        var row = new StringBuilder();
        AppendRow(row, Enumerable.Repeat("", cells.Count).ToArray(), Math.Max(3, width));

        int insertAt = Math.Clamp(2 + index, 2, lines.Count); // after header + separator
        lines.Insert(insertAt, row.ToString().TrimEnd('\n'));
        return string.Join("\n", lines) + "\n";
    }

    /// <summary>Inserts an empty column at the given column index (0 = first column).</summary>
    public static string InsertColumn(string tableText, int index)
    {
        var lines = SplitLines(tableText);
        for (int i = 0; i < lines.Count; i++)
        {
            var cells = ParseCells(lines[i]);
            int at = Math.Clamp(index, 0, cells.Count);
            bool isSeparator = cells.All(c => SeparatorCell.IsMatch(c.Trim()));
            cells.Insert(at, isSeparator ? "---" : "   ");
            lines[i] = "| " + string.Join(" | ", cells.Select(c => c.Trim().PadRight(3))) + " |";
        }
        return string.Join("\n", lines) + "\n";
    }

    private static void AppendRow(StringBuilder sb, string[] cells, int width)
    {
        sb.Append('|');
        foreach (var cell in cells)
        {
            sb.Append(' ').Append(cell.PadRight(width)).Append(" |");
        }
        sb.Append('\n');
    }

    private static List<string> SplitLines(string tableText) =>
        tableText.Replace("\r\n", "\n").TrimEnd('\n').Split('\n').ToList();

    private static List<string> ParseCells(string line)
    {
        string trimmed = line.Trim();
        if (trimmed.StartsWith('|')) trimmed = trimmed[1..];
        if (trimmed.EndsWith('|')) trimmed = trimmed[..^1];
        return trimmed.Split('|').Select(c => c.Trim()).ToList();
    }
}
