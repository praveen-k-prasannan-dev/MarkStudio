using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace MarkdownEditor.App.Views;

/// <summary>
/// Startup splash. Shows for 60 seconds (with a live countdown) unless the user clicks Skip,
/// then raises <see cref="Completed"/> exactly once so App can open the main window.
/// </summary>
public partial class SplashWindow : Window
{
    private const int DisplaySeconds = 60;

    private readonly DispatcherTimer _timer;
    private int _elapsed;
    private bool _completed;

    public event EventHandler? Completed;

    public SplashWindow()
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        VersionText.Text = $"Version {version}";

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _elapsed++;
        LoadingBar.Value = _elapsed;
        SkipButton.Content = $"Skip ({Math.Max(0, DisplaySeconds - _elapsed)}s)";

        if (_elapsed >= DisplaySeconds)
            Complete();
    }

    private void Skip_Click(object sender, RoutedEventArgs e) => Complete();

    private void Complete()
    {
        if (_completed)
            return;
        _completed = true;
        _timer.Stop();
        Completed?.Invoke(this, EventArgs.Empty);
    }
}
