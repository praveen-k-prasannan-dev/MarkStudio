using System.Windows;

namespace MarkdownEditor.App;

/// <summary>Export to PDF/HTML and printing. Implemented in Phase 5.</summary>
public partial class MainWindow
{
    private void ExportPdf() =>
        MessageBox.Show(this, "PDF export arrives in Phase 5.", "Not yet available");

    private void ExportHtml() =>
        MessageBox.Show(this, "HTML export arrives in Phase 5.", "Not yet available");

    private void PrintPreview() =>
        MessageBox.Show(this, "Printing arrives in Phase 5.", "Not yet available");
}
