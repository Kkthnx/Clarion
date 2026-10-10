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
        Services.AppServices.Instance.UpdateSettings(s => s with { Theme = theme == ElementTheme.Light ? "Light" : theme == ElementTheme.Dark ? "Dark" : "System" });
        _window?.ApplyTheme(theme);
    }

    private static Mutex? _instanceLock;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var cli = Clarion.Core.Profiles.CliOptions.Parse(Environment.GetCommandLineArgs().Skip(1).ToList());
        if (cli.IsCli)
        {
            int code;
            using (var console = Clarion.Engine.HeadlessRunner.OpenConsole()) code = Clarion.Engine.HeadlessRunner.Run(cli, console, Services.Log.Write);
            Environment.Exit(code);
            return;
        }

        // The program starts without administrator rights, so the read only command line commands need none. The window asks for them
        // here, before it takes the single instance lock, by starting itself again with the usual Windows prompt. When that is
        // refused it carries on in the limited mode the pages already handle, and Home offers to ask again.
        // A copy that was itself started for this must not ask again. If Windows did not give it administrator rights, asking again
        // would start copies without end, so it stays in the limited mode.
        var startedForRights = Environment.GetCommandLineArgs().Contains(Clarion.Core.Profiles.CliOptions.RelaunchMarker);
        if (!Clarion.Engine.WindowsMachine.IsElevated() && !startedForRights && Environment.GetEnvironmentVariable("CLARION_NO_ELEVATE") != "1"
            && Services.Elevation.RestartAsAdministrator() == Services.ElevationOutcome.Started)
        {
            Exit();
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
        // The theme and Expert mode were chosen last time. Without this they went back to the defaults on every start.
        Theme = Services.AppServices.Instance.Settings.Theme switch { "Light" => ElementTheme.Light, "Dark" => ElementTheme.Dark, _ => ElementTheme.Default };
        _window = new MainWindow();
        _window.Activate();
    }

    /// <summary>
    /// Starts a copy with administrator rights and closes this one. The lock is let go first, or the new copy would take this one for
    /// a second launch and leave. When the prompt is refused the lock is taken again and nothing changes.
    /// </summary>
    public bool RestartAsAdministrator()
    {
        _instanceLock?.Dispose();
        _instanceLock = null;
        var outcome = Services.Elevation.RestartAsAdministrator();
        Services.Log.Write($"Restart as administrator: {outcome}");
        if (outcome == Services.ElevationOutcome.Started)
        {
            Exit();
            return true;
        }
        _instanceLock = new Mutex(true, "Global\\Clarion.SingleInstance", out _);
        return false;
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
