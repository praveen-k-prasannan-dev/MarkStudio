using System.Windows;

namespace MarkdownEditor.App.Views;

public partial class InsertLinkDialog : Window
{
    public InsertLinkDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => (TextBox.Text.Length > 0 ? UrlBox : TextBox).Focus();
    }

    public string LinkText
    {
        get => TextBox.Text;
        set => TextBox.Text = value;
    }

    public string Url => UrlBox.Text.Trim();

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (Url.Length == 0)
        {
            MessageBox.Show(this, "Please enter a URL.", "Insert Link",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        DialogResult = true;
    }
}
