using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Highlighting;
using MarkdownEditor.App.ViewModels;
using MarkdownEditor.Core.Markdown;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace MarkdownEditor.App;

public partial class MainWindow : Window
{
    private const string FileDialogFilter =
        "Markdown files (*.md;*.markdown)|*.md;*.markdown|Text files (*.txt)|*.txt|All files (*.*)|*.*";
    private const string PreviewHost = "preview.local";

    private readonly MainViewModel _vm = new();
    private readonly MarkdownRenderer _renderer = new();
    private readonly DispatcherTimer _previewDebounce;

    private string _css = "";
    private bool _webViewReady;
    private bool _suppressTextEvents;
    private double _pendingScrollY;
    private string? _mappedFolder;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;

        _css = LoadCss("preview-light.css");
        Editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("MarkDown");

        _previewDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _previewDebounce.Tick += async (_, _) =>
        {
            _previewDebounce.Stop();
            await RefreshPreviewAsync();
        };

        Editor.TextChanged += Editor_TextChanged;
        Editor.TextArea.Caret.PositionChanged += (_, _) =>
            _vm.UpdateCaret(Editor.TextArea.Caret.Line, Editor.TextArea.Caret.Column);

        InitializeRibbon();

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializeWebViewAsync();

        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && File.Exists(args[1]))
            OpenFile(args[1]);
        else
            await RefreshPreviewAsync();

        Editor.Focus();
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MarkdownEditor", "WebView2");
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await Preview.EnsureCoreWebView2Async(environment);

            Preview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            Preview.CoreWebView2.Settings.AreDevToolsEnabled = false;
            Preview.CoreWebView2.NavigationCompleted += async (_, _) =>
            {
                if (_pendingScrollY > 0)
                    await Preview.CoreWebView2.ExecuteScriptAsync(
                        $"window.scrollTo(0, {_pendingScrollY.ToString(CultureInfo.InvariantCulture)});");
            };

            _webViewReady = true;
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(this,
                "The Microsoft Edge WebView2 Runtime is required for the preview but was not found.\n\n" +
                "Download it from: https://developer.microsoft.com/microsoft-edge/webview2/",
                "WebView2 Runtime missing", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ---------- Preview ----------

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        if (_suppressTextEvents)
            return;
        _vm.MarkDirty();
        _previewDebounce.Stop();
        _previewDebounce.Start();
    }

    private async Task RefreshPreviewAsync()
    {
        if (_exporting)
            return; // don't clobber the print-rendered page mid-export

        string text = Editor.Text;
        _vm.SyncText(text);
        UpdateOutline(text);

        if (!_webViewReady)
            return;

        try
        {
            string scrollY = await Preview.CoreWebView2.ExecuteScriptAsync("window.scrollY");
            if (double.TryParse(scrollY, NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
                _pendingScrollY = y;
        }
        catch (InvalidOperationException)
        {
            // WebView not ready for scripts yet; keep the last known scroll position.
        }

        string? baseHref = MapDocumentFolder();
        string body = _renderer.ToHtml(text);
        string page = HtmlDocumentBuilder.BuildPage(body, _css, _vm.DocumentTitle, baseHref);
        Preview.NavigateToString(page);
    }

    /// <summary>Maps the document's folder to a virtual host so relative image paths resolve.</summary>
    private string? MapDocumentFolder()
    {
        string? folder = _vm.FilePath is null ? null : Path.GetDirectoryName(_vm.FilePath);
        if (folder is null)
            return null;

        if (!string.Equals(folder, _mappedFolder, StringComparison.OrdinalIgnoreCase))
        {
            Preview.CoreWebView2.SetVirtualHostNameToFolderMapping(
                PreviewHost, folder, CoreWebView2HostResourceAccessKind.Allow);
            _mappedFolder = folder;
        }
        return $"https://{PreviewHost}/";
    }

    private static string LoadCss(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    // ---------- File handling ----------

    private void New_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges())
            return;
        SetEditorText("");
        _vm.NewDocument();
        _ = RefreshPreviewAsync();
    }

    private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges())
            return;

        var dialog = new OpenFileDialog { Filter = FileDialogFilter };
        if (dialog.ShowDialog(this) == true)
            OpenFile(dialog.FileName);
    }

    private void Save_Executed(object sender, ExecutedRoutedEventArgs e) => SaveDocument();

    private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e) => SaveDocumentAs();

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void OpenFile(string path)
    {
        try
        {
            string text = _vm.LoadFile(path);
            SetEditorText(text);
            _vm.DocumentLoaded(path, text);
            _ = RefreshPreviewAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Could not open the file:\n{ex.Message}",
                "Open failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool SaveDocument()
    {
        if (_vm.FilePath is null)
            return SaveDocumentAs();
        return SaveTo(_vm.FilePath);
    }

    private bool SaveDocumentAs()
    {
        var dialog = new SaveFileDialog
        {
            Filter = FileDialogFilter,
            DefaultExt = ".md",
            FileName = _vm.FilePath is null ? "Untitled.md" : Path.GetFileName(_vm.FilePath),
        };
        if (dialog.ShowDialog(this) != true)
            return false;
        return SaveTo(dialog.FileName);
    }

    private bool SaveTo(string path)
    {
        try
        {
            _vm.Save(path, Editor.Text);
            _ = RefreshPreviewAsync(); // re-render: the base href may have changed with the folder
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Could not save the file:\n{ex.Message}",
                "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>Returns true if it is OK to discard/replace the current document.</summary>
    private bool ConfirmDiscardChanges()
    {
        if (!_vm.IsDirty)
            return true;

        var choice = MessageBox.Show(this,
            $"Do you want to save changes to {_vm.DocumentTitle}?",
            "Unsaved changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

        return choice switch
        {
            MessageBoxResult.Yes => SaveDocument(),
            MessageBoxResult.No => true,
            _ => false,
        };
    }

    private void SetEditorText(string text)
    {
        _suppressTextEvents = true;
        Editor.Text = text;
        _suppressTextEvents = false;
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!ConfirmDiscardChanges())
            e.Cancel = true;
    }

    // ---------- Recent files ----------

    private void RecentMenu_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        RecentMenu.Items.Clear();
        if (_vm.RecentFiles.Items.Count == 0)
        {
            RecentMenu.Items.Add(new MenuItem { Header = "(empty)", IsEnabled = false });
            return;
        }

        foreach (string path in _vm.RecentFiles.Items)
        {
            var item = new MenuItem { Header = path.Replace("_", "__") };
            string captured = path;
            item.Click += (_, _) =>
            {
                if (ConfirmDiscardChanges())
                    OpenFile(captured);
            };
            RecentMenu.Items.Add(item);
        }
    }

    // ---------- Drag & drop ----------

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0)
            return;
        if (ConfirmDiscardChanges())
            OpenFile(files[0]);
    }
}
