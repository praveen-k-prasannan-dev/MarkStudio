using System.Diagnostics;
using System.IO;
using System.Windows;
using MarkdownEditor.App.Views;
using MarkdownEditor.Core.Markdown;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace MarkdownEditor.App;

/// <summary>
/// Export to PDF/HTML and printing. PDF uses the preview WebView2's PrintToPdfAsync:
/// the document is re-rendered with the light theme, printed, then the preview is restored.
/// </summary>
public partial class MainWindow
{
    private bool _exporting;

    private async void ExportPdf()
    {
        if (!_webViewReady)
        {
            MessageBox.Show(this, "The preview engine is not available, so PDF export cannot run.",
                "Export to PDF", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var options = new ExportPdfDialog { Owner = this };
        if (options.ShowDialog() != true)
            return;

        var saveDialog = new SaveFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            FileName = Path.ChangeExtension(_vm.DocumentTitle, ".pdf"),
        };
        if (saveDialog.ShowDialog(this) != true)
            return;

        _exporting = true;
        _previewDebounce.Stop();
        try
        {
            // Always print with the light theme regardless of the preview setting.
            string page = HtmlDocumentBuilder.BuildPage(
                _renderer.ToHtml(Editor.Text), LoadCss("preview-light.css"),
                _vm.DocumentTitle, MapDocumentFolder());
            await NavigateAndWaitAsync(page);

            var settings = Preview.CoreWebView2.Environment.CreatePrintSettings();
            settings.Orientation = options.IsLandscape
                ? CoreWebView2PrintOrientation.Landscape
                : CoreWebView2PrintOrientation.Portrait;
            settings.PageWidth = options.PageWidthInches;
            settings.PageHeight = options.PageHeightInches;
            settings.MarginTop = options.MarginInches;
            settings.MarginBottom = options.MarginInches;
            settings.MarginLeft = options.MarginInches;
            settings.MarginRight = options.MarginInches;
            settings.ShouldPrintBackgrounds = options.PrintBackgrounds;
            settings.ShouldPrintHeaderAndFooter = false;

            bool success = await Preview.CoreWebView2.PrintToPdfAsync(saveDialog.FileName, settings);
            if (success)
            {
                var open = MessageBox.Show(this,
                    $"PDF exported to:\n{saveDialog.FileName}\n\nOpen it now?",
                    "Export to PDF", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (open == MessageBoxResult.Yes)
                    Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show(this, "The PDF could not be written. Is the file open in another program?",
                    "Export to PDF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"PDF export failed:\n{ex.Message}",
                "Export to PDF", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _exporting = false;
            await RefreshPreviewAsync(); // restore the normal (possibly dark) preview
        }
    }

    private void ExportHtml()
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "HTML files (*.html)|*.html",
            DefaultExt = ".html",
            FileName = Path.ChangeExtension(_vm.DocumentTitle, ".html"),
        };
        if (saveDialog.ShowDialog(this) != true)
            return;

        try
        {
            // No base href: relative image paths stay relative to wherever the HTML is saved.
            string page = HtmlDocumentBuilder.BuildPage(
                _renderer.ToHtml(Editor.Text), LoadCss("preview-light.css"), _vm.DocumentTitle);
            File.WriteAllText(saveDialog.FileName, page);
            MessageBox.Show(this, $"HTML exported to:\n{saveDialog.FileName}",
                "Export to HTML", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"HTML export failed:\n{ex.Message}",
                "Export to HTML", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PrintPreview()
    {
        if (!_webViewReady)
            return;
        Preview.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser);
    }

    private Task NavigateAndWaitAsync(string html)
    {
        var completion = new TaskCompletionSource();

        void Handler(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Preview.CoreWebView2.NavigationCompleted -= Handler;
            completion.TrySetResult();
        }

        Preview.CoreWebView2.NavigationCompleted += Handler;
        Preview.NavigateToString(html);
        return completion.Task;
    }
}
