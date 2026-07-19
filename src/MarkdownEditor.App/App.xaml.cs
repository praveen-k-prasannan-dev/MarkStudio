using System.Windows;
using MarkdownEditor.App.Services;
using MarkdownEditor.App.Views;

namespace MarkdownEditor.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            AppLog.Write($"DispatcherUnhandledException: {args.Exception}");
            MessageBox.Show(
                $"An unexpected error occurred:\n{args.Exception.Message}\n\n" +
                "The error has been logged. The application will try to continue.",
                "MarkStudio Editor", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            AppLog.Write($"UnhandledException (terminating={args.IsTerminating}): {args.ExceptionObject}");
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            AppLog.Write($"UnobservedTaskException: {args.Exception}");
            args.SetObserved();
        };

        AppLog.Write("App starting");

        // Keep the app alive while the splash hands over to the main window.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var splash = new SplashWindow();
        splash.Completed += (_, _) =>
        {
            AppLog.Write("Splash completed; opening main window");
            var main = new MainWindow();
            MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
            splash.Close();
        };
        splash.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppLog.Write($"App exiting (code {e.ApplicationExitCode})");
        base.OnExit(e);
    }
}
