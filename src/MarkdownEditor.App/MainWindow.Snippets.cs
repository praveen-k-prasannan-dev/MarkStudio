using System.Windows;
using System.Windows.Input;
using MarkdownEditor.App.Views;
using MarkdownEditor.Core.Editing;

namespace MarkdownEditor.App;

/// <summary>Text-expansion snippets: type a trigger word (e.g. "todo") and press Tab to expand it.</summary>
public partial class MainWindow
{
    private void NewFromTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new NewFromTemplateDialog { Owner = this };
        if (dialog.ShowDialog() == true && dialog.SelectedTemplate is { } template)
            CreateTab(template.Content);
    }

    private void InitializeSnippets()
    {
        // Runs after the table-navigation Tab handler, so e.Handled is already set when the
        // caret is inside a table (cell navigation takes priority over snippet expansion there).
        Editor.PreviewKeyDown += (_, e) =>
        {
            if (e.Handled || e.Key != Key.Tab || Keyboard.Modifiers != ModifierKeys.None)
                return;

            var expansion = SnippetExpander.TryExpand(Editor.Text, Editor.CaretOffset);
            if (expansion is null)
                return;

            string oldText = Editor.Text;
            string newText = oldText[..expansion.Value.ReplaceStart]
                + expansion.Value.Replacement
                + oldText[(expansion.Value.ReplaceStart + expansion.Value.ReplaceLength)..];

            ApplyEdit(new EditResult(newText, expansion.Value.NewCaretPosition, 0));
            e.Handled = true;
        };
    }
}
