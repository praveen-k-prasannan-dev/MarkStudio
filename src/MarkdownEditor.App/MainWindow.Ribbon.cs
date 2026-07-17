using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MarkdownEditor.App.Views;
using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.App;

/// <summary>Ribbon toolbox wiring: formatting commands, insert dialogs, find/replace, view options.</summary>
public partial class MainWindow
{
    private static readonly string[] CodeLanguages =
        ["(none)", "csharp", "javascript", "typescript", "python", "json", "sql", "bash", "xml", "html", "css", "yaml"];

    private static readonly string[] Emojis =
        ["😀", "🙂", "👍", "✅", "❌", "⚠️", "💡", "📌", "🔥", "🎯", "❤️", "🚀"];

    private const int TablePickerRows = 8;
    private const int TablePickerCols = 10;

    private bool _suppressHeadingCombo;
    private bool _suppressOutlineSelection;

    private void InitializeRibbon()
    {
        BuildTablePicker();
        Editor.TextArea.TextView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
    }

    // ---------- Selection plumbing ----------

    private TextSelection GetSelection() =>
        new(Editor.Text, Editor.SelectionStart, Editor.SelectionLength);

    /// <summary>Applies an EditResult as a single, minimal document replacement (one undo step).</summary>
    private void ApplyEdit(EditResult result)
    {
        string oldText = Editor.Text;
        string newText = result.NewText;

        int prefix = 0;
        int maxPrefix = Math.Min(oldText.Length, newText.Length);
        while (prefix < maxPrefix && oldText[prefix] == newText[prefix])
            prefix++;

        int suffix = 0;
        while (suffix < oldText.Length - prefix && suffix < newText.Length - prefix
               && oldText[oldText.Length - 1 - suffix] == newText[newText.Length - 1 - suffix])
            suffix++;

        Editor.Document.Replace(prefix, oldText.Length - prefix - suffix,
            newText.Substring(prefix, newText.Length - prefix - suffix));

        Editor.Select(result.NewSelectionStart, result.NewSelectionLength);
        Editor.TextArea.Caret.BringCaretToView();
        Editor.Focus();
    }

    private void InsertAtCaret(string text)
    {
        int pos = Editor.SelectionStart;
        Editor.Document.Replace(pos, Editor.SelectionLength, text);
        Editor.Select(pos + text.Length, 0);
        Editor.Focus();
    }

    /// <summary>Inserts block-level Markdown at the caret, separated from surrounding text by blank lines.</summary>
    private void InsertBlock(string blockText)
    {
        string text = Editor.Text;
        int pos = Editor.SelectionStart + Editor.SelectionLength;
        string before = text[..pos];

        string separation =
            before.Length == 0 ? "" :
            before.EndsWith("\n\n", StringComparison.Ordinal) ? "" :
            before.EndsWith('\n') ? "\n" : "\n\n";

        string insertion = separation + blockText;
        if (!insertion.EndsWith('\n'))
            insertion += "\n";

        Editor.Document.Insert(pos, insertion);
        Editor.Select(pos + insertion.Length, 0);
        Editor.TextArea.Caret.BringCaretToView();
        Editor.Focus();
    }

    // ---------- Formatting commands ----------

    private void Formatting_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var selection = GetSelection();
        EditResult? result = null;

        if (e.Command == EditorCommands.Bold) result = InlineFormatter.Toggle(selection, InlineFormatter.Bold);
        else if (e.Command == EditorCommands.Italic) result = InlineFormatter.Toggle(selection, InlineFormatter.Italic);
        else if (e.Command == EditorCommands.Strikethrough) result = InlineFormatter.Toggle(selection, InlineFormatter.Strikethrough);
        else if (e.Command == EditorCommands.InlineCode) result = InlineFormatter.Toggle(selection, InlineFormatter.Code);
        else if (e.Command == EditorCommands.Highlight) result = InlineFormatter.Toggle(selection, InlineFormatter.Highlight);
        else if (e.Command == EditorCommands.Heading1) result = BlockFormatter.SetHeading(selection, 1);
        else if (e.Command == EditorCommands.Heading2) result = BlockFormatter.SetHeading(selection, 2);
        else if (e.Command == EditorCommands.Heading3) result = BlockFormatter.SetHeading(selection, 3);
        else if (e.Command == EditorCommands.Heading4) result = BlockFormatter.SetHeading(selection, 4);
        else if (e.Command == EditorCommands.Heading5) result = BlockFormatter.SetHeading(selection, 5);
        else if (e.Command == EditorCommands.Heading6) result = BlockFormatter.SetHeading(selection, 6);
        else if (e.Command == EditorCommands.ClearHeading) result = BlockFormatter.SetHeading(selection, 0);
        else if (e.Command == EditorCommands.BulletList) result = ListFormatter.ToggleList(selection, ListKind.Bullet);
        else if (e.Command == EditorCommands.NumberedList) result = ListFormatter.ToggleList(selection, ListKind.Numbered);
        else if (e.Command == EditorCommands.TaskList) result = ListFormatter.ToggleList(selection, ListKind.Task);
        else if (e.Command == EditorCommands.Blockquote) result = BlockFormatter.ToggleBlockquote(selection);
        else if (e.Command == EditorCommands.CodeBlock) result = BlockFormatter.ToggleCodeFence(selection);
        else if (e.Command == EditorCommands.HorizontalRule) result = BlockFormatter.InsertHorizontalRule(selection);

        if (result is not null)
            ApplyEdit(result);
    }

    private void HeadingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressHeadingCombo || HeadingCombo.SelectedIndex < 0)
            return;

        int level = HeadingCombo.SelectedIndex; // 0 = normal text, 1..6 = heading level
        ApplyEdit(BlockFormatter.SetHeading(GetSelection(), level));

        _suppressHeadingCombo = true;
        HeadingCombo.SelectedIndex = -1;
        _suppressHeadingCombo = false;
    }

    // ---------- Clipboard group ----------

    private void Paste_Click(object sender, RoutedEventArgs e) { Editor.Paste(); Editor.Focus(); }
    private void Cut_Click(object sender, RoutedEventArgs e) { Editor.Cut(); Editor.Focus(); }
    private void Copy_Click(object sender, RoutedEventArgs e) { Editor.Copy(); Editor.Focus(); }
    private void Undo_Click(object sender, RoutedEventArgs e) { Editor.Undo(); Editor.Focus(); }
    private void Redo_Click(object sender, RoutedEventArgs e) { Editor.Redo(); Editor.Focus(); }
    private void SelectAll_Click(object sender, RoutedEventArgs e) { Editor.SelectAll(); Editor.Focus(); }

    // ---------- Insert: links, images, tables, blocks ----------

    private void InsertLink_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new InsertLinkDialog { Owner = this };
        if (Editor.SelectionLength > 0)
            dialog.LinkText = Editor.SelectedText;

        if (dialog.ShowDialog() == true)
            InsertAtCaret($"[{dialog.LinkText}]({dialog.Url})");
    }

    private void InsertImage_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new InsertImageDialog(_vm.FilePath) { Owner = this };
        if (dialog.ShowDialog() == true)
            InsertAtCaret($"![{dialog.AltText}]({dialog.ImagePath})");
    }

    private void InsertTableDialog_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        TableDropdownButton.IsChecked = false;
        var dialog = new InsertTableDialog { Owner = this };
        if (dialog.ShowDialog() == true)
            InsertBlock(TableBuilder.Create(dialog.TableRows, dialog.TableColumns));
    }

    private void Footnote_Click(object sender, RoutedEventArgs e)
    {
        int number = Regex.Matches(Editor.Text, @"\[\^\d+\]:").Count + 1;
        string text = Editor.Text;

        InsertAtCaret($"[^{number}]");
        string suffix = text.EndsWith('\n') || text.Length == 0 ? "\n" : "\n\n";
        Editor.Document.Insert(Editor.Document.TextLength, $"{suffix}[^{number}]: ");
    }

    private void InsertDate_Click(object sender, RoutedEventArgs e) =>
        InsertAtCaret(DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));

    private void CodeBlock_Click(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu { PlacementTarget = CodeBlockButton };
        foreach (string lang in CodeLanguages)
        {
            var item = new MenuItem { Header = lang };
            string captured = lang;
            item.Click += (_, _) => ApplyEdit(
                BlockFormatter.ToggleCodeFence(GetSelection(), captured == "(none)" ? null : captured));
            menu.Items.Add(item);
        }
        menu.IsOpen = true;
    }

    private void Emoji_Click(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu { PlacementTarget = EmojiButton };
        foreach (string emoji in Emojis)
        {
            var item = new MenuItem { Header = emoji };
            string captured = emoji;
            item.Click += (_, _) => InsertAtCaret(captured);
            menu.Items.Add(item);
        }
        menu.IsOpen = true;
    }

    // ---------- Table grid picker (Word-style) ----------

    private void BuildTablePicker()
    {
        for (int r = 1; r <= TablePickerRows; r++)
        {
            for (int c = 1; c <= TablePickerCols; c++)
            {
                var cell = new Border
                {
                    Width = 17,
                    Height = 17,
                    Margin = new Thickness(1),
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xB0, 0xB0, 0xB0)),
                    BorderThickness = new Thickness(1),
                    Tag = (r, c),
                };
                cell.MouseEnter += TablePickerCell_MouseEnter;
                cell.MouseLeftButtonUp += TablePickerCell_Click;
                TablePickerGrid.Children.Add(cell);
            }
        }
    }

    private void TablePickerCell_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        var (rows, cols) = ((int, int))((Border)sender).Tag;
        var highlight = new SolidColorBrush(Color.FromRgb(0xCC, 0xE4, 0xF7));
        foreach (Border cell in TablePickerGrid.Children)
        {
            var (r, c) = ((int, int))cell.Tag;
            cell.Background = r <= rows && c <= cols ? highlight : Brushes.White;
        }
        TablePickerLabel.Text = $"{rows} × {cols} table";
    }

    private void TablePickerCell_Click(object sender, MouseButtonEventArgs e)
    {
        var (rows, cols) = ((int, int))((Border)sender).Tag;
        TableDropdownButton.IsChecked = false;
        InsertBlock(TableBuilder.Create(rows, cols));
    }

    // ---------- Find / replace ----------

    private void Find_Executed(object sender, ExecutedRoutedEventArgs e) => ShowFindPanel(focusReplace: false);
    private void Replace_Executed(object sender, ExecutedRoutedEventArgs e) => ShowFindPanel(focusReplace: true);
    private void FindNext_Executed(object sender, ExecutedRoutedEventArgs e) => FindCore(backward: false);
    private void FindPrevious_Executed(object sender, ExecutedRoutedEventArgs e) => FindCore(backward: true);
    private void FindNextBtn_Click(object sender, RoutedEventArgs e) => FindCore(backward: false);
    private void FindPrevBtn_Click(object sender, RoutedEventArgs e) => FindCore(backward: true);

    private void ShowFindPanel(bool focusReplace)
    {
        FindPanel.Visibility = Visibility.Visible;
        if (Editor.SelectionLength > 0 && !Editor.SelectedText.Contains('\n'))
            FindBox.Text = Editor.SelectedText;

        var box = focusReplace ? ReplaceBox : FindBox;
        box.Focus();
        box.SelectAll();
    }

    private void CloseFindPanel_Click(object sender, RoutedEventArgs e)
    {
        FindPanel.Visibility = Visibility.Collapsed;
        FindStatus.Text = "";
        Editor.Focus();
    }

    private void FindBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            FindCore(backward: false);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            CloseFindPanel_Click(sender, e);
            e.Handled = true;
        }
    }

    private bool FindCore(bool backward)
    {
        string query = FindBox.Text;
        string text = Editor.Text;
        if (query.Length == 0 || text.Length == 0)
            return false;

        var comparison = MatchCaseCheck.IsChecked == true
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        int index;
        if (!backward)
        {
            int from = Math.Min(Editor.SelectionStart + Editor.SelectionLength, text.Length);
            index = text.IndexOf(query, from, comparison);
            if (index < 0)
                index = text.IndexOf(query, 0, comparison); // wrap around
        }
        else
        {
            int from = Editor.SelectionStart - 1;
            index = from >= 0 ? text.LastIndexOf(query, Math.Min(from, text.Length - 1), comparison) : -1;
            if (index < 0)
                index = text.LastIndexOf(query, text.Length - 1, comparison); // wrap around
        }

        if (index < 0)
        {
            FindStatus.Text = "No matches";
            return false;
        }

        FindStatus.Text = "";
        Editor.Select(index, query.Length);
        var location = Editor.Document.GetLocation(index);
        Editor.ScrollTo(location.Line, location.Column);
        return true;
    }

    private void ReplaceBtn_Click(object sender, RoutedEventArgs e)
    {
        string query = FindBox.Text;
        if (query.Length == 0)
            return;

        var comparison = MatchCaseCheck.IsChecked == true
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        if (Editor.SelectionLength > 0 && string.Equals(Editor.SelectedText, query, comparison))
            Editor.Document.Replace(Editor.SelectionStart, Editor.SelectionLength, ReplaceBox.Text);

        FindCore(backward: false);
    }

    private void ReplaceAllBtn_Click(object sender, RoutedEventArgs e)
    {
        string query = FindBox.Text;
        if (query.Length == 0)
            return;

        var options = MatchCaseCheck.IsChecked == true ? RegexOptions.None : RegexOptions.IgnoreCase;
        var regex = new Regex(Regex.Escape(query), options);

        string text = Editor.Text;
        int count = regex.Matches(text).Count;
        if (count > 0)
        {
            string replacement = ReplaceBox.Text;
            Editor.Document.Replace(0, text.Length, regex.Replace(text, _ => replacement));
        }
        FindStatus.Text = $"{count} occurrence(s) replaced";
    }

    // ---------- View tab ----------

    private void ViewMode_Changed(object sender, RoutedEventArgs e)
    {
        if (Splitter is null || Editor is null || Preview is null)
            return; // fires during InitializeComponent

        bool editorVisible = ViewPreviewOnly.IsChecked != true;
        bool previewVisible = ViewEditorOnly.IsChecked != true;

        EditorColumn.Width = editorVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        PreviewColumn.Width = previewVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        Editor.Visibility = editorVisible ? Visibility.Visible : Visibility.Collapsed;
        Preview.Visibility = previewVisible ? Visibility.Visible : Visibility.Collapsed;
        Splitter.Visibility = editorVisible && previewVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Theme_Changed(object sender, RoutedEventArgs e)
    {
        if (Editor is null)
            return;
        bool dark = DarkThemeToggle.IsChecked == true;
        _css = LoadCss(dark ? "preview-dark.css" : "preview-light.css");
        Preview.DefaultBackgroundColor = dark
            ? System.Drawing.Color.FromArgb(0x0D, 0x11, 0x17)
            : System.Drawing.Color.White;
        _ = RefreshPreviewAsync();
    }

    private void FontSizeUp_Click(object sender, RoutedEventArgs e) =>
        Editor.FontSize = Math.Clamp(Editor.FontSize + 1, 8, 32);

    private void FontSizeDown_Click(object sender, RoutedEventArgs e) =>
        Editor.FontSize = Math.Clamp(Editor.FontSize - 1, 8, 32);

    // ---------- Outline panel ----------

    private void OutlineToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (OutlineColumn is null)
            return;
        bool show = OutlineToggle.IsChecked == true;
        OutlineColumn.Width = show ? new GridLength(220) : new GridLength(0);
        if (show)
            UpdateOutline(Editor.Text);
    }

    private void UpdateOutline(string text)
    {
        if (OutlineToggle?.IsChecked != true)
            return;

        _suppressOutlineSelection = true;
        OutlineList.Items.Clear();

        var lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var match = Regex.Match(lines[i].TrimEnd('\r'), @"^(#{1,6})\s+(.+)$");
            if (!match.Success)
                continue;

            int level = match.Groups[1].Length;
            OutlineList.Items.Add(new ListBoxItem
            {
                Content = new string(' ', (level - 1) * 3) + match.Groups[2].Value,
                Tag = i + 1, // 1-based line number
                FontWeight = level == 1 ? FontWeights.Bold : FontWeights.Normal,
            });
        }
        _suppressOutlineSelection = false;
    }

    private void OutlineList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressOutlineSelection || OutlineList.SelectedItem is not ListBoxItem { Tag: int line })
            return;

        line = Math.Min(line, Editor.Document.LineCount);
        Editor.CaretOffset = Editor.Document.GetLineByNumber(line).Offset;
        Editor.ScrollTo(line, 1);
        Editor.Focus();
    }

    // ---------- Synchronized scrolling ----------

    private async void TextView_ScrollOffsetChanged(object? sender, EventArgs e)
    {
        if (!_webViewReady || SyncScrollCheck?.IsChecked != true)
            return;

        var view = Editor.TextArea.TextView;
        double max = view.DocumentHeight - view.ActualHeight;
        if (max <= 0)
            return;

        double fraction = Math.Clamp(view.ScrollOffset.Y / max, 0, 1);
        try
        {
            await Preview.CoreWebView2.ExecuteScriptAsync(
                $"window.scrollTo(0, {fraction.ToString(CultureInfo.InvariantCulture)} * " +
                "(document.documentElement.scrollHeight - window.innerHeight));");
        }
        catch (InvalidOperationException)
        {
            // WebView busy navigating; skip this sync tick.
        }
    }

    // ---------- Export tab (implemented in Phase 5) ----------

    private void ExportPdf_Click(object sender, RoutedEventArgs e) => ExportPdf();
    private void ExportHtml_Click(object sender, RoutedEventArgs e) => ExportHtml();
    private void Print_Click(object sender, RoutedEventArgs e) => PrintPreview();
}
