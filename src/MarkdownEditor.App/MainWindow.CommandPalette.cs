using System.Windows;
using System.Windows.Input;
using MarkdownEditor.App.Views;
using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.App;

/// <summary>Ctrl+Shift+P command palette: a searchable registry of everything the ribbon can do.</summary>
public partial class MainWindow
{
    private void OpenCommandPalette_Executed(object sender, ExecutedRoutedEventArgs e) => OpenCommandPalette();

    private void OpenCommandPalette()
    {
        var palette = new CommandPaletteWindow(BuildPaletteEntries()) { Owner = this };
        palette.Left = Left + Math.Max(0, (Width - palette.Width) / 2);
        palette.Top = Top + 90;
        palette.ShowDialog();
    }

    private List<CommandPaletteWindow.Entry> BuildPaletteEntries()
    {
        var entries = new List<CommandPaletteWindow.Entry>
        {
            new("New Document", () => ApplicationCommands.New.Execute(null, Editor)),
            new("New From Template…", () => NewFromTemplate_Click(this, new RoutedEventArgs())),
            new("Open File…", () => ApplicationCommands.Open.Execute(null, Editor)),
            new("Open Folder…", () => OpenFolder_Click(this, new RoutedEventArgs())),
            new("Save", () => ApplicationCommands.Save.Execute(null, Editor)),
            new("Save As…", () => ApplicationCommands.SaveAs.Execute(null, Editor)),

            new("Bold", () => EditorCommands.Bold.Execute(null, Editor)),
            new("Italic", () => EditorCommands.Italic.Execute(null, Editor)),
            new("Strikethrough", () => EditorCommands.Strikethrough.Execute(null, Editor)),
            new("Inline Code", () => EditorCommands.InlineCode.Execute(null, Editor)),
            new("Highlight", () => EditorCommands.Highlight.Execute(null, Editor)),

            new("Heading 1", () => EditorCommands.Heading1.Execute(null, Editor)),
            new("Heading 2", () => EditorCommands.Heading2.Execute(null, Editor)),
            new("Heading 3", () => EditorCommands.Heading3.Execute(null, Editor)),
            new("Heading 4", () => EditorCommands.Heading4.Execute(null, Editor)),
            new("Heading 5", () => EditorCommands.Heading5.Execute(null, Editor)),
            new("Heading 6", () => EditorCommands.Heading6.Execute(null, Editor)),
            new("Normal Text (Clear Heading)", () => EditorCommands.ClearHeading.Execute(null, Editor)),
            new("Bullet List", () => EditorCommands.BulletList.Execute(null, Editor)),
            new("Numbered List", () => EditorCommands.NumberedList.Execute(null, Editor)),
            new("Task List", () => EditorCommands.TaskList.Execute(null, Editor)),
            new("Blockquote", () => EditorCommands.Blockquote.Execute(null, Editor)),

            new("Code Block", () => EditorCommands.CodeBlock.Execute(null, Editor)),
            new("Horizontal Rule", () => EditorCommands.HorizontalRule.Execute(null, Editor)),
            new("Insert Link…", () => EditorCommands.InsertLink.Execute(null, Editor)),
            new("Insert Image…", () => EditorCommands.InsertImage.Execute(null, Editor)),
            new("Insert Table…", () => EditorCommands.InsertTable.Execute(null, Editor)),
            new("Insert Footnote", () => Footnote_Click(this, new RoutedEventArgs())),
            new("Insert Date/Time", () => InsertDate_Click(this, new RoutedEventArgs())),

            new("Find", () => EditorCommands.Find.Execute(null, Editor)),
            new("Replace", () => EditorCommands.Replace.Execute(null, Editor)),
            new("Find Next", () => EditorCommands.FindNext.Execute(null, Editor)),
            new("Find Previous", () => EditorCommands.FindPrevious.Execute(null, Editor)),
            new("Select All", () => SelectAll_Click(this, new RoutedEventArgs())),

            new("Cut", () => Cut_Click(this, new RoutedEventArgs())),
            new("Copy", () => Copy_Click(this, new RoutedEventArgs())),
            new("Paste", () => Paste_Click(this, new RoutedEventArgs())),
            new("Undo", () => Undo_Click(this, new RoutedEventArgs())),
            new("Redo", () => Redo_Click(this, new RoutedEventArgs())),
            new("Copy as HTML", () => CopyAsHtml_Click(this, new RoutedEventArgs())),

            new("Split View", () => ViewSplit.IsChecked = true),
            new("Editor Only View", () => ViewEditorOnly.IsChecked = true),
            new("Preview Only View", () => ViewPreviewOnly.IsChecked = true),
            new("Toggle Sync Scrolling", () => SyncScrollCheck.IsChecked = !(SyncScrollCheck.IsChecked ?? false)),
            new("Switch to Light Theme", () => ThemeCombo.SelectedIndex = 0),
            new("Switch to Dark Theme", () => ThemeCombo.SelectedIndex = 1),
            new("Custom Preview Theme…", () => ThemeCombo.SelectedIndex = 2),
            new("Increase Editor Font Size", () => FontSizeUp_Click(this, new RoutedEventArgs())),
            new("Decrease Editor Font Size", () => FontSizeDown_Click(this, new RoutedEventArgs())),
            new("Toggle Document Outline", () => OutlineToggle.IsChecked = !(OutlineToggle.IsChecked ?? false)),
            new("Toggle Focus Mode", ToggleFocusMode),
            new("Show Writing Stats", () => StatsDropdownButton.IsChecked = true),

            new("Export to PDF…", ExportPdf),
            new("Export to HTML…", ExportHtml),
            new("Export to Word…", ExportWord),
            new("Print…", PrintPreview),

            new("About MarkStudio Editor", () => About_Click(this, new RoutedEventArgs())),
        };

        bool inTable = TableEditor.FindTableBounds(GetNormalizedLines(), GetCaretPosition().Line) is not null;
        if (inTable)
        {
            entries.AddRange(
            [
                new("Insert Row Above", () => InsertRowAbove_Click(this, new RoutedEventArgs())),
                new("Insert Row Below", () => InsertRowBelow_Click(this, new RoutedEventArgs())),
                new("Delete Row", () => DeleteRow_Click(this, new RoutedEventArgs())),
                new("Insert Column Left", () => InsertColumnLeft_Click(this, new RoutedEventArgs())),
                new("Insert Column Right", () => InsertColumnRight_Click(this, new RoutedEventArgs())),
                new("Delete Column", () => DeleteColumn_Click(this, new RoutedEventArgs())),
            ]);
        }

        return entries;
    }
}
