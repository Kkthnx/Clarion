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

    /// <summary>The person's choice to start without being asked for administrator rights. It lives in their own folder.</summary>
    public static Clarion.Core.Settings.LaunchPreference Launch { get; } =
        new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Clarion", "start-limited.flag"));

    /// <summary>
    /// Takes the lock that keeps one window open. It is open to every signed in account, because a copy with administrator rights and
    /// one without are different integrity levels, and the plain default would refuse the second one even a look at it.
    /// False when another copy has it.
    /// </summary>
    private static bool TakeInstanceLock()
    {
        try
        {
            var security = new System.Security.AccessControl.MutexSecurity();
            security.AddAccessRule(new System.Security.AccessControl.MutexAccessRule(
                new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.AuthenticatedUserSid, null),
                System.Security.AccessControl.MutexRights.Synchronize | System.Security.AccessControl.MutexRights.ReadPermissions,
                System.Security.AccessControl.AccessControlType.Allow));
            _instanceLock = System.Threading.MutexAcl.Create(true, "Global\\Clarion.SingleInstance", out var first, security);
            return first;
        }
        catch (UnauthorizedAccessException)
        {
            // Another copy owns it and would not let this one open it, which also means another copy is running.
            return false;
        }
    }

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

        // The single instance lock comes first. A second launch must show the window that is already open and leave, with no Windows
        // prompt on the way. Asking for rights before this made a second launch of a limited copy show the prompt for nothing.
        if (!TakeInstanceLock())
        {
            // A second launch should show the copy that is already running, not just disappear.
            Services.Shell.BringExistingToFront(".Clarion");
            Exit();
            return;
        }

        // The program starts without administrator rights, so the read only command line commands need none. The window asks for them
        // here by starting itself again with the usual Windows prompt. When that is refused it carries on in the limited mode the pages
        // already handle, and Home offers to ask again. Someone who always wants the limited mode can say so in Settings, and then
        // there is no prompt at all.
        // A copy that was itself started for this must not ask again. If Windows did not give it administrator rights, asking again
        // would start copies without end, so it stays in the limited mode.
        var startedForRights = Environment.GetCommandLineArgs().Contains(Clarion.Core.Profiles.CliOptions.RelaunchMarker);
        if (!Clarion.Engine.WindowsMachine.IsElevated() && !startedForRights && Environment.GetEnvironmentVariable("CLARION_NO_ELEVATE") != "1"
            && !Launch.StartLimited && RestartAsAdministrator())
            return;
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
        TakeInstanceLock();
        return false;
    }

    private static void LogCrash(Exception ex)
    {
        try
        {
            var dir = Clarion.Engine.EngineFactory.DefaultDataDirectory;
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, "crash.log");
            // Every error here is handled and the app carries on, so one that repeats must not fill the disk. Same limit as the main log.
            if (File.Exists(file) && new FileInfo(file).Length > 1_000_000) File.Move(file, file + ".old", overwrite: true);
            File.AppendAllText(file, $"{DateTime.Now:u} {ex}\n\n");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
