using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MarkdownEditor.Core.Workspace;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace MarkdownEditor.App;

/// <summary>Open Folder: a sidebar file tree plus cross-file text search over a workspace folder.</summary>
public partial class MainWindow
{
    private sealed record WorkspaceSearchResultItem(string DisplayPath, string FilePath, int LineNumber, string LineText);

    private string? _workspaceRoot;
    private DispatcherTimer? _workspaceSearchDebounce;

    private void InitializeWorkspace()
    {
        _workspaceSearchDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _workspaceSearchDebounce.Tick += (_, _) =>
        {
            _workspaceSearchDebounce!.Stop();
            RunWorkspaceSearch();
        };
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Open Folder as Workspace" };
        if (dialog.ShowDialog(this) != true)
            return;

        _workspaceRoot = dialog.FolderName;
        WorkspaceTree.ItemsSource = new[] { WorkspaceScanner.Scan(_workspaceRoot) };
        WorkspaceSearchBox.Clear();
        WorkspaceToggle.IsChecked = true;
    }

    private void WorkspaceToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (WorkspaceColumn is null)
            return;
        WorkspaceColumn.Width = WorkspaceToggle.IsChecked == true ? new GridLength(240) : new GridLength(0);
    }

    private void WorkspaceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is WorkspaceNode { IsFolder: false } node)
            OpenFileIntoTab(node.FullPath);
    }

    private void WorkspaceSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _workspaceSearchDebounce!.Stop();
        _workspaceSearchDebounce.Start();
    }

    private void RunWorkspaceSearch()
    {
        string query = WorkspaceSearchBox.Text;
        if (_workspaceRoot is null || query.Length == 0)
        {
            WorkspaceSearchResults.Visibility = Visibility.Collapsed;
            WorkspaceTree.Visibility = Visibility.Visible;
            return;
        }

        var files = EnumerateWorkspaceFiles(_workspaceRoot);
        var results = WorkspaceSearch.Search(files, query);

        WorkspaceSearchResults.ItemsSource = results.Select(r => new WorkspaceSearchResultItem(
            Path.GetRelativePath(_workspaceRoot, r.FilePath),
            r.FilePath,
            r.LineNumber,
            r.LineText)).ToList();

        WorkspaceTree.Visibility = Visibility.Collapsed;
        WorkspaceSearchResults.Visibility = Visibility.Visible;
    }

    private static IEnumerable<(string Path, string Content)> EnumerateWorkspaceFiles(string root)
    {
        foreach (string ext in new[] { "*.md", "*.markdown" })
        {
            foreach (string path in Directory.EnumerateFiles(root, ext, SearchOption.AllDirectories))
            {
                string content;
                try { content = File.ReadAllText(path); }
                catch (IOException) { continue; }
                yield return (path, content);
            }
        }
    }

    private void WorkspaceSearchResults_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (WorkspaceSearchResults.SelectedItem is not WorkspaceSearchResultItem item)
            return;

        OpenFileIntoTab(item.FilePath);
        int line = Math.Min(item.LineNumber, Editor.Document.LineCount);
        Editor.CaretOffset = Editor.Document.GetLineByNumber(line).Offset;
        Editor.ScrollTo(line, 1);
        Editor.Focus();
    }

    /// <summary>
    /// Cross-document links: a relative link to another .md/.markdown file opens in a new tab
    /// instead of navigating the preview away from the rendered document; external http(s) links
    /// open in the system browser instead of inside the embedded preview.
    /// </summary>
    private void Preview_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (!e.IsUserInitiated)
            return; // our own NavigateToString calls

        string uri = e.Uri;
        if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            try { Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true }); }
            catch (System.ComponentModel.Win32Exception) { }
            return;
        }

        if (_mappedFolder is null)
            return; // no document folder to resolve relative links against

        string prefix = $"https://{PreviewHost}/";
        if (!uri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return;

        string relative = Uri.UnescapeDataString(uri[prefix.Length..]).Split('#', '?')[0];
        string extension = Path.GetExtension(relative);
        if (!string.Equals(extension, ".md", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extension, ".markdown", StringComparison.OrdinalIgnoreCase))
            return; // not a Markdown link (e.g. a same-page heading anchor) - let it resolve normally

        e.Cancel = true;
        string fullPath = Path.GetFullPath(Path.Combine(_mappedFolder, relative));
        if (File.Exists(fullPath))
            OpenFileIntoTab(fullPath);
    }
}
