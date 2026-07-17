using System.Text.RegularExpressions;

namespace MarkdownEditor.Core.Editing;

public enum ListKind
{
    Bullet,
    Numbered,
    Task,
}

/// <summary>
/// Toggles bullet / numbered / task list formatting across all selected lines.
/// Applying the kind the lines already have removes it; a different kind converts in place.
/// Blank lines inside the selection are left untouched.
/// </summary>
public static class ListFormatter
{
    private static readonly Regex TaskPrefix = new(@"^[-*+]\s+\[[ xX]\]\s+", RegexOptions.Compiled);
    private static readonly Regex BulletPrefix = new(@"^[-*+]\s+", RegexOptions.Compiled);
    private static readonly Regex NumberPrefix = new(@"^\d+\.\s+", RegexOptions.Compiled);

    public static EditResult ToggleList(TextSelection selection, ListKind kind)
    {
        ArgumentNullException.ThrowIfNull(selection);

        var lines = SelectionLines.GetLineContents(selection);
        var contentLines = lines.Where(l => l.Length > 0).ToArray();
        bool alreadyKind = contentLines.Length > 0 && contentLines.All(l => IsKind(l, kind));

        int number = 0;
        return SelectionLines.Transform(selection, (line, _) =>
        {
            if (line.Length == 0)
                return line;

            string content = StripListPrefix(line);
            if (alreadyKind)
                return content;

            number++;
            return kind switch
            {
                ListKind.Bullet => "- " + content,
                ListKind.Numbered => $"{number}. " + content,
                ListKind.Task => "- [ ] " + content,
                _ => content,
            };
        });
    }

    private static bool IsKind(string line, ListKind kind) => kind switch
    {
        ListKind.Task => TaskPrefix.IsMatch(line),
        ListKind.Bullet => BulletPrefix.IsMatch(line) && !TaskPrefix.IsMatch(line),
        ListKind.Numbered => NumberPrefix.IsMatch(line),
        _ => false,
    };

    private static string StripListPrefix(string line)
    {
        if (TaskPrefix.IsMatch(line))
            return TaskPrefix.Replace(line, "");
        if (BulletPrefix.IsMatch(line))
            return BulletPrefix.Replace(line, "");
        return NumberPrefix.Replace(line, "");
    }
}
