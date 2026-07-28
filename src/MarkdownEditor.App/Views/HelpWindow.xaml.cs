using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MarkdownEditor.App.Services;
using MarkdownEditor.Core.Help;
using MarkdownEditor.Core.Markdown;
using Microsoft.Web.WebView2.Core;

namespace MarkdownEditor.App.Views;

/// <summary>F1 / Help menu: a searchable topic tree rendered through the app's own Markdown pipeline, so Mermaid/math examples in the help content render live.</summary>
public partial class HelpWindow : Window
{
    private const string HelpHost = "help-content.local";
    private static readonly string HelpRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "help");

    private readonly bool _darkTheme;
    private IReadOnlyList<HelpTopic> _toc = [];
    private readonly MarkdownRenderer _renderer = new();
    private bool _webViewReady;

    public HelpWindow(bool darkTheme)
    {
        InitializeComponent();
        _darkTheme = darkTheme;
        Loaded += HelpWindow_Loaded;
        Loaded += (_, _) => SearchBox.Focus();
    }

    private async void HelpWindow_Loaded(object sender, RoutedEventArgs e)
    {
        string tocPath = Path.Combine(HelpRoot, "toc.json");
        _toc = File.Exists(tocPath) ? HelpTocLoader.Parse(File.ReadAllText(tocPath)) : [];
        Tree.ItemsSource = _toc;

        try
        {
            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MarkdownEditor", "WebView2Help");
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await ContentView.EnsureCoreWebView2Async(environment);
            ContentView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            ContentView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            ContentView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                PreviewScripts.AssetsHost, Path.Combine(AppContext.BaseDirectory, "Assets"),
                CoreWebView2HostResourceAccessKind.Allow);
            ContentView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                HelpHost, HelpRoot, CoreWebView2HostResourceAccessKind.Allow);
            ContentView.CoreWebView2.NavigationStarting += ContentView_NavigationStarting;
            _webViewReady = true;
        }
        catch (Exception ex) when (ex is WebView2RuntimeNotFoundException or InvalidOperationException)
        {
            AppLog.Write($"Help window WebView2 initialization failed: {ex}");
        }

        var first = HelpSearch.Flatten(_toc).FirstOrDefault();
        if (first is not null)
            SelectTopic(first);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string query = SearchBox.Text;
        Tree.ItemsSource = string.IsNullOrWhiteSpace(query) ? _toc : HelpSearch.Search(_toc, query);
    }

    private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is HelpTopic { File: not null } topic)
            SelectTopic(topic);
    }

    private void SelectTopic(HelpTopic topic)
    {
        if (topic.File is null || !_webViewReady)
            return;

        string path = Path.Combine(HelpRoot, topic.File.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            ContentView.NavigateToString($"<html><body><p>Topic file not found: {topic.File}</p></body></html>");
            return;
        }

        string markdown = File.ReadAllText(path);
        string body = _renderer.ToHtml(markdown);
        string extraHead = PreviewScripts.BuildExtraHeadScripts(body);
        string css = LoadCss(_darkTheme ? "preview-dark.css" : "preview-light.css");

        // Base href matches the topic's own folder, so links inside each topic file use normal
        // relative-path conventions ("../other.md", "sibling.md") rather than always being
        // relative to the help root.
        string topicFolder = Path.GetDirectoryName(topic.File)?.Replace('\\', '/') ?? "";
        string baseHref = topicFolder.Length > 0 ? $"https://{HelpHost}/{topicFolder}/" : $"https://{HelpHost}/";

        string page = HtmlDocumentBuilder.BuildPage(body, css, topic.Title, baseHref, extraHead);
        ContentView.NavigateToString(page);
    }

    /// <summary>Clicking a relative link to another help topic (e.g. "mermaid/flowchart.md") loads that topic in place, instead of navigating the WebView2 away from the rendered page.</summary>
    private void ContentView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (!e.IsUserInitiated)
            return;

        string uri = e.Uri;
        if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            if (uri.StartsWith($"https://{HelpHost}/", StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;
                string relative = Uri.UnescapeDataString(uri[$"https://{HelpHost}/".Length..]).Split('#', '?')[0];
                var target = HelpSearch.Flatten(_toc)
                    .FirstOrDefault(t => string.Equals(t.File, relative, StringComparison.OrdinalIgnoreCase));
                if (target is not null)
                    SelectTopic(target);
                return;
            }

            e.Cancel = true;
            try { Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true }); }
            catch (System.ComponentModel.Win32Exception) { }
        }
    }

    private static string LoadCss(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }
}
