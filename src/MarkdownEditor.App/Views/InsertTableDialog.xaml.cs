using System.Windows;

namespace MarkdownEditor.App.Views;

public partial class InsertTableDialog : Window
{
    public InsertTableDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => RowsBox.Focus();
    }

    public int TableRows { get; private set; }
    public int TableColumns { get; private set; }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(RowsBox.Text, out int rows) || rows is < 1 or > 100 ||
            !int.TryParse(ColsBox.Text, out int cols) || cols is < 1 or > 30)
        {
            MessageBox.Show(this, "Please enter 1–100 rows and 1–30 columns.", "Insert Table",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        TableRows = rows;
        TableColumns = cols;
        DialogResult = true;
    }
}
