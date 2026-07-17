using System.Windows;
using MarkdownEditor.App.Views;

namespace MarkdownEditor.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Keep the app alive while the splash hands over to the main window.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var splash = new SplashWindow();
        splash.Completed += (_, _) =>
        {
            var main = new MainWindow();
            MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
            splash.Close();
        };
        splash.Show();
    }
}
