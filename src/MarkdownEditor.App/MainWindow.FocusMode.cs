using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MarkdownEditor.App;

/// <summary>Distraction-free writing (F11): hides the ribbon, tab strip, and status bar, and centers the editor.</summary>
public partial class MainWindow
{
    private bool _focusMode;
    private bool _preFocusOutlineVisible;
    private GridLength _preFocusWorkspaceWidth = new(0);
    private GridLength _preFocusEditorWidth = new(1, GridUnitType.Star);
    private GridLength _preFocusPreviewWidth = new(1, GridUnitType.Star);
    private Visibility _preFocusSplitterVisibility = Visibility.Visible;
    private Visibility _preFocusPreviewVisibility = Visibility.Visible;
    private bool _preFocusShowLineNumbers = true;

    private void InitializeFocusMode()
    {
        // F11 has no text-editing meaning in AvalonEdit, but Escape does (it's also used to close
        // the find bar), so both are handled explicitly here rather than relying on the implicit
        // command gesture - the same precaution that Ctrl+I and Ctrl+Shift+P needed.
        Editor.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.F11)
            {
                ToggleFocusMode();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && _focusMode)
            {
                ToggleFocusMode();
                e.Handled = true;
            }
        };
    }

    private void ToggleFocusMode_Executed(object sender, ExecutedRoutedEventArgs e) => ToggleFocusMode();

    private void ToggleFocusMode()
    {
        if (_focusMode)
            ExitFocusMode();
        else
            EnterFocusMode();
    }

    private void EnterFocusMode()
    {
        _focusMode = true;

        _preFocusEditorWidth = EditorColumn.Width;
        _preFocusPreviewWidth = PreviewColumn.Width;
        _preFocusSplitterVisibility = Splitter.Visibility;
        _preFocusPreviewVisibility = Preview.Visibility;
        _preFocusOutlineVisible = OutlineColumn.Width.Value > 0;
        _preFocusWorkspaceWidth = WorkspaceColumn.Width;
        _preFocusShowLineNumbers = Editor.ShowLineNumbers;

        MainMenu.Visibility = Visibility.Collapsed;
        RibbonTabControl.Visibility = Visibility.Collapsed;
        TabStripBorder.Visibility = Visibility.Collapsed;
        MainStatusBar.Visibility = Visibility.Collapsed;
        if (FindPanel.Visibility == Visibility.Visible)
            CloseFindPanel_Click(this, new RoutedEventArgs());

        OutlineColumn.Width = new GridLength(0);
        WorkspaceColumn.Width = new GridLength(0);
        EditorColumn.Width = new GridLength(1, GridUnitType.Star);
        PreviewColumn.Width = new GridLength(0);
        Preview.Visibility = Visibility.Collapsed;
        Splitter.Visibility = Visibility.Collapsed;

        Editor.ShowLineNumbers = false;
        Editor.HorizontalAlignment = HorizontalAlignment.Center;
        Editor.MaxWidth = 900;

        ExitFocusModeButton.Visibility = Visibility.Visible;
        Editor.Focus();
    }

    private void ExitFocusMode()
    {
        _focusMode = false;

        MainMenu.Visibility = Visibility.Visible;
        RibbonTabControl.Visibility = Visibility.Visible;
        TabStripBorder.Visibility = Visibility.Visible;
        MainStatusBar.Visibility = Visibility.Visible;

        OutlineColumn.Width = _preFocusOutlineVisible ? new GridLength(220) : new GridLength(0);
        WorkspaceColumn.Width = _preFocusWorkspaceWidth;
        EditorColumn.Width = _preFocusEditorWidth;
        PreviewColumn.Width = _preFocusPreviewWidth;
        Preview.Visibility = _preFocusPreviewVisibility;
        Splitter.Visibility = _preFocusSplitterVisibility;

        Editor.ShowLineNumbers = _preFocusShowLineNumbers;
        Editor.HorizontalAlignment = HorizontalAlignment.Stretch;
        Editor.MaxWidth = double.PositiveInfinity;

        ExitFocusModeButton.Visibility = Visibility.Collapsed;
        Editor.Focus();
    }
}
