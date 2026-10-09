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

    public IntPtr WindowHandle => _window is null ? IntPtr.Zero : WinRT.Interop.WindowNative.GetWindowHandle(_window);

    public ElementTheme Theme { get; private set; } = ElementTheme.Default;

    public void SetTheme(ElementTheme theme)
    {
        Theme = theme;
        _window?.ApplyTheme(theme);
    }

    private static Mutex? _instanceLock;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var cli = Clarion.Core.Profiles.CliOptions.Parse(Environment.GetCommandLineArgs().Skip(1).ToList());
        if (cli.IsCli)
        {
            var code = Clarion.Engine.HeadlessRunner.Run(cli, Clarion.Engine.HeadlessRunner.OpenConsole(), Services.Log.Write);
            Environment.Exit(code);
            return;
        }

        _instanceLock = new Mutex(true, "Global\\Clarion.SingleInstance", out var first);
        if (!first)
        {
            // A second launch should show the copy that is already running, not just disappear.
            Services.Shell.BringExistingToFront("Clarion");
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
