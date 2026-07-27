using System.Windows;
using System.Windows.Input;
using MarkdownEditor.Core.Documents;

namespace MarkdownEditor.App.Views;

public partial class NewFromTemplateDialog : Window
{
    public NewFromTemplateDialog()
    {
        InitializeComponent();
        TemplateList.ItemsSource = DocumentTemplates.BuiltIn;
        TemplateList.SelectedIndex = 0;
        Loaded += (_, _) => TemplateList.Focus();
    }

    public DocumentTemplate? SelectedTemplate => TemplateList.SelectedItem as DocumentTemplate;

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTemplate is null)
            return;
        DialogResult = true;
    }

    private void TemplateList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedTemplate is not null)
            DialogResult = true;
    }
}
