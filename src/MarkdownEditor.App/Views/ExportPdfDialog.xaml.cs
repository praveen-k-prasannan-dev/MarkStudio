using System.Windows;

namespace MarkdownEditor.App.Views;

public partial class ExportPdfDialog : Window
{
    // Remember the last-used settings for the session.
    private static int s_pageSizeIndex;
    private static int s_orientationIndex;
    private static int s_marginsIndex = 1;
    private static bool s_printBackgrounds = true;

    public ExportPdfDialog()
    {
        InitializeComponent();
        PageSizeCombo.SelectedIndex = s_pageSizeIndex;
        OrientationCombo.SelectedIndex = s_orientationIndex;
        MarginsCombo.SelectedIndex = s_marginsIndex;
        BackgroundsCheck.IsChecked = s_printBackgrounds;
    }

    public bool IsLandscape => OrientationCombo.SelectedIndex == 1;
    public bool PrintBackgrounds => BackgroundsCheck.IsChecked == true;

    public double PageWidthInches => PageSizeCombo.SelectedIndex == 1 ? 8.5 : 8.27;   // Letter : A4
    public double PageHeightInches => PageSizeCombo.SelectedIndex == 1 ? 11.0 : 11.69;

    public double MarginInches => MarginsCombo.SelectedIndex switch
    {
        0 => 0.4,
        2 => 1.0,
        _ => 0.75,
    };

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        s_pageSizeIndex = PageSizeCombo.SelectedIndex;
        s_orientationIndex = OrientationCombo.SelectedIndex;
        s_marginsIndex = MarginsCombo.SelectedIndex;
        s_printBackgrounds = PrintBackgrounds;
        DialogResult = true;
    }
}
