using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace MarkdownEditor.App.Views;

public partial class InsertImageDialog : Window
{
    private readonly string? _documentPath;

    public InsertImageDialog(string? documentPath)
    {
        _documentPath = documentPath;
        InitializeComponent();
    }

    public string AltText => AltBox.Text;

    /// <summary>Path relative to the document folder when possible; slashes normalized for Markdown.</summary>
    public string ImagePath { get; private set; } = "";

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Images (*.png;*.jpg;*.jpeg;*.gif;*.svg;*.webp;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.svg;*.webp;*.bmp|All files (*.*)|*.*",
        };
        if (dialog.ShowDialog(this) == true)
        {
            PathBox.Text = dialog.FileName;
            if (AltBox.Text.Length == 0)
                AltBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        string path = PathBox.Text.Trim();
        if (path.Length == 0)
        {
            MessageBox.Show(this, "Please choose an image file.", "Insert Image",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        ImagePath = MakeMarkdownPath(path);
        DialogResult = true;
    }

    private string MakeMarkdownPath(string path)
    {
        string? documentFolder = _documentPath is null ? null : Path.GetDirectoryName(_documentPath);
        if (documentFolder is not null && Path.IsPathRooted(path))
        {
            string relative = Path.GetRelativePath(documentFolder, path);
            if (!relative.StartsWith("..", StringComparison.Ordinal))
                path = relative;
        }
        return path.Replace('\\', '/');
    }
}
