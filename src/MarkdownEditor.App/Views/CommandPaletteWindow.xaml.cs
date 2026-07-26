using System.Windows;
using System.Windows.Input;
using MarkdownEditor.Core.Palette;

namespace MarkdownEditor.App.Views;

/// <summary>A VS Code-style "type to run a command" overlay, filtered live via <see cref="FuzzyMatcher"/>.</summary>
public partial class CommandPaletteWindow : Window
{
    public sealed record Entry(string Name, Action Execute);

    private readonly IReadOnlyList<Entry> _allEntries;
    private IReadOnlyList<Entry> _filtered;
    private bool _closing;

    public CommandPaletteWindow(IReadOnlyList<Entry> entries)
    {
        InitializeComponent();
        _allEntries = entries;
        _filtered = entries;
        Loaded += (_, _) =>
        {
            SearchBox.Focus();
            UpdateList("");
        };
    }

    private void UpdateList(string query)
    {
        _filtered = FuzzyMatcher.Filter(_allEntries, e => e.Name, query);
        ResultsList.ItemsSource = _filtered.Select(e => e.Name).ToList();
        if (_filtered.Count > 0)
            ResultsList.SelectedIndex = 0;
        EmptyLabel.Visibility = _filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        UpdateList(SearchBox.Text);

    private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down: MoveSelection(1); e.Handled = true; break;
            case Key.Up: MoveSelection(-1); e.Handled = true; break;
            case Key.Enter: ExecuteSelected(); e.Handled = true; break;
            case Key.Escape: CloseOnce(); e.Handled = true; break;
        }
    }

    private void MoveSelection(int delta)
    {
        if (ResultsList.Items.Count == 0)
            return;
        int next = Math.Clamp(ResultsList.SelectedIndex + delta, 0, ResultsList.Items.Count - 1);
        ResultsList.SelectedIndex = next;
        ResultsList.ScrollIntoView(ResultsList.Items[next]);
    }

    private void ExecuteSelected()
    {
        if (ResultsList.SelectedIndex < 0 || ResultsList.SelectedIndex >= _filtered.Count)
        {
            CloseOnce();
            return;
        }
        var entry = _filtered[ResultsList.SelectedIndex];
        CloseOnce();
        entry.Execute();
    }

    private void ResultsList_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) =>
        ExecuteSelected();

    private void Window_Deactivated(object sender, EventArgs e) => CloseOnce();

    // Close() itself triggers Deactivated as part of its teardown, and calling Close() again
    // while already closing throws - so every close path must funnel through this guard.
    private void CloseOnce()
    {
        if (_closing)
            return;
        _closing = true;
        Close();
    }
}
