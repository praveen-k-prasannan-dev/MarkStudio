using System.Windows;
using System.Windows.Input;
using MarkdownEditor.App.Views;

namespace MarkdownEditor.App;

/// <summary>F1 / Help menu: opens the in-app Help window (non-modal, so it can stay open while you keep editing).</summary>
public partial class MainWindow
{
    private HelpWindow? _helpWindow;

    private void OpenHelp_Click(object sender, RoutedEventArgs e) => OpenHelp();

    private void OpenHelp_Executed(object sender, ExecutedRoutedEventArgs e) => OpenHelp();

    private void OpenHelp()
    {
        if (_helpWindow is not null)
        {
            _helpWindow.Activate();
            return;
        }

        _helpWindow = new HelpWindow(_activeThemeIndex == 1) { Owner = this };
        _helpWindow.Closed += (_, _) => _helpWindow = null;
        _helpWindow.Show();
    }
}
