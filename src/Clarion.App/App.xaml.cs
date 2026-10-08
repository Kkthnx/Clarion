using Microsoft.UI.Xaml;

namespace Clarion.App;

public partial class App : Application
{
    private MainWindow? _window;

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            LogCrash(e.Exception);
            e.Handled = true;
        };
    }

    public ElementTheme Theme { get; private set; } = ElementTheme.Default;

    public void SetTheme(ElementTheme theme)
    {
        Theme = theme;
        _window?.ApplyTheme(theme);
    }

    private static Mutex? _instanceLock;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _instanceLock = new Mutex(true, "Global\\Clarion.SingleInstance", out var first);
        if (!first)
        {
            Exit();
            return;
        }
        _window = new MainWindow();
        _window.Activate();
    }

    private static void LogCrash(Exception ex)
    {
        try
        {
            var dir = Clarion.Engine.EngineFactory.DefaultDataDirectory;
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "crash.log"), $"{DateTime.Now:u} {ex}\n\n");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
