using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using MarkdownEditor.Core.Documents;

namespace MarkdownEditor.App;

/// <summary>
/// Multi-document tabs: one shared AvalonEdit <see cref="TextEditor"/> control whose
/// <c>Document</c> is swapped between per-tab <see cref="TextDocument"/> buffers, keeping each
/// tab's own text and undo history intact while switching. <see cref="Core.Documents.DocumentManager"/>
/// (Core) is the source of truth for which tabs exist and which is active; this partial only
/// maintains the WPF-specific pairing and the tab-strip UI.
/// </summary>
public partial class MainWindow
{
    private readonly Dictionary<DocumentState, TextDocument> _editorDocuments = [];

    /// <summary>Creates a new blank tab, makes it active, and returns its document.</summary>
    private DocumentState CreateTab(string initialText = "")
    {
        var doc = _vm.AddNewDocument();
        _editorDocuments[doc] = new TextDocument(initialText);
        SwitchToDocument(doc);
        RefreshTabStrip();
        return doc;
    }

    /// <summary>Opens a file into a new tab, or switches to it if it's already open somewhere.</summary>
    private void OpenFileIntoTab(string path)
    {
        var existing = _vm.Documents.FindByPath(path);
        if (existing is not null)
        {
            SwitchToDocument(existing);
            RefreshTabStrip();
            return;
        }

        try
        {
            string text = _vm.LoadFile(path);
            var doc = _vm.LoadIntoNewTab(path, text);
            _editorDocuments[doc] = new TextDocument(text);
            SwitchToDocument(doc);
            RefreshTabStrip();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Could not open the file:\n{ex.Message}",
                "Open failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>Swaps the shared editor onto the given tab's text buffer and makes it active.</summary>
    private void SwitchToDocument(DocumentState doc)
    {
        _vm.Documents.SetActive(doc);
        _mappedFolder = null; // force MapDocumentFolder to re-map for this tab's own folder

        _suppressTextEvents = true;
        Editor.Document = _editorDocuments[doc];
        _suppressTextEvents = false;

        _vm.RefreshForActiveTab();
        _ = RefreshPreviewAsync();
        Editor.Focus();
    }

    /// <summary>Closes a tab, prompting to save first if it has unsaved changes.</summary>
    private void CloseTab(DocumentState doc)
    {
        if (doc.IsDirty)
        {
            if (_vm.Documents.Active != doc)
                SwitchToDocument(doc); // bring it to front so the user can see what they're saving/discarding

            var choice = MessageBox.Show(this,
                $"Do you want to save changes to {doc.Title}?",
                "Unsaved changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            if (choice == MessageBoxResult.Cancel)
                return;
            if (choice == MessageBoxResult.Yes && !SaveDocument())
                return; // save failed, or the Save As dialog was cancelled
        }

        int index = _vm.Documents.IndexOf(doc);
        _editorDocuments.Remove(doc);
        int newActiveIndex = _vm.Documents.Close(index);

        if (newActiveIndex < 0)
        {
            CreateTab(); // never leave the app with zero tabs
            return;
        }

        SwitchToDocument(_vm.Documents.Documents[newActiveIndex]);
        RefreshTabStrip();
    }

    private void NewTabButton_Click(object sender, RoutedEventArgs e) => CreateTab();

    /// <summary>Rebuilds the tab strip to match the current set of open documents.</summary>
    private void RefreshTabStrip()
    {
        TabStripPanel.Children.Clear();

        int tabIndex = 0;
        foreach (var doc in _vm.Documents.Documents)
        {
            bool isActive = doc == _vm.Documents.Active;
            int capturedIndex = tabIndex++;

            var closeButton = new Button
            {
                Content = "✕",
                Width = 18,
                Height = 18,
                Padding = new Thickness(0),
                Margin = new Thickness(6, 0, 0, 0),
                FontSize = 10,
                Focusable = false,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                ToolTip = "Close",
            };
            closeButton.Click += (_, _) => CloseTab(doc);

            var title = new TextBlock
            {
                Text = doc.Title + (doc.IsDirty ? " ●" : ""),
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = isActive ? FontWeights.SemiBold : FontWeights.Normal,
            };

            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(title);
            content.Children.Add(closeButton);

            var tab = new Border
            {
                Child = content,
                Padding = new Thickness(10, 6, 8, 6),
                Margin = new Thickness(2, 4, 0, 0),
                CornerRadius = new CornerRadius(4, 4, 0, 0),
                Background = isActive ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xE8)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
                BorderThickness = new Thickness(1, 1, 1, isActive ? 0 : 1),
                Cursor = Cursors.Hand,
                ToolTip = doc.FilePath ?? doc.Title,
            };
            AutomationProperties.SetAutomationId(tab, $"DocTab_{capturedIndex}");
            tab.MouseLeftButtonUp += (_, _) =>
            {
                SwitchToDocument(doc);
                RefreshTabStrip();
            };

            TabStripPanel.Children.Add(tab);
        }
    }
}
