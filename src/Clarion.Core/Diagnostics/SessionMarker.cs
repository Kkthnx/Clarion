using System.Diagnostics;
using System.Globalization;

namespace Clarion.Core.Diagnostics;

/// <summary>A session that was running when the program last stopped, and never said it was closing.</summary>
public sealed record EndedSession(string Version, DateTime StartedUtc, string LastPage);

/// <summary>
/// A small file kept while the program runs and removed when it closes. A crash inside Windows' own interface code, a kill from Task
/// Manager and a power cut write nothing, so the program cannot report them itself. The file left behind lets the next start say that
/// the last session ended without closing, and which page it was on, which is the first thing a bug report about a closing window needs.
/// The page is the only thing written during the session. It holds no names, paths or settings.
/// </summary>
public sealed class SessionMarker
{
    private readonly string _path;
    private readonly Func<int, bool> _isRunning;
    private readonly Func<DateTime> _bootUtc;
    private bool _owned;
    private string _version = "";
    private int _pid;
    private DateTime _started;

    public SessionMarker(string path, Func<int, bool>? isRunning = null, Func<DateTime>? bootUtc = null)
    {
        _path = path;
        _isRunning = isRunning ?? IsClarionRunning;
        _bootUtc = bootUtc ?? (() => DateTime.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64));
    }

    /// <summary>
    /// Takes the marker for this session. Returns the session that left one behind, or null when the last one closed normally, ended
    /// because Windows was restarted or shut down, or is still running (in which case this one leaves the marker alone).
    /// </summary>
    public EndedSession? Begin(int pid, string version, DateTime nowUtc)
    {
        EndedSession? ended = null;
        try
        {
            if (Read() is { } old)
            {
                if (old.Pid != pid && _isRunning(old.Pid)) return null;
                // Windows was started again after that session began, so it ended with the restart or the shutdown.
                if (old.Started >= _bootUtc()) ended = new EndedSession(old.Version, old.Started, old.Page);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or ArgumentException or NotSupportedException) { }

        _pid = pid;
        _version = version;
        _started = nowUtc;
        _owned = true;
        Write("");
        return ended;
    }

    /// <summary>Notes the page now shown.</summary>
    public void Page(string page)
    {
        if (_owned) Write(page);
    }

    /// <summary>Removes the marker. Called when the program closes on its own terms.</summary>
    public void End()
    {
        if (!_owned) return;
        _owned = false;
        try { File.Delete(_path); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) { }
    }

    private void Write(string page)
    {
        try
        {
            var dir = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            // A page name never holds a line break, but the file is one value to a line, so it is made certain here.
            var clean = page.Replace('\r', ' ').Replace('\n', ' ');
            File.WriteAllText(_path, string.Join('\n', _pid.ToString(CultureInfo.InvariantCulture), _version,
                _started.ToString("O", CultureInfo.InvariantCulture), clean));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) { }
    }

    private (int Pid, string Version, DateTime Started, string Page)? Read()
    {
        if (!File.Exists(_path)) return null;
        var lines = File.ReadAllText(_path).Split('\n');
        if (lines.Length < 3) return null;
        if (!int.TryParse(lines[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pid)) return null;
        if (!DateTime.TryParse(lines[2], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var started)) return null;
        return (pid, lines[1], started.ToUniversalTime(), lines.Length > 3 ? lines[3] : "");
    }

    private static bool IsClarionRunning(int pid)
    {
        try
        {
            using var p = Process.GetProcessById(pid);
            return p.ProcessName.Equals("Clarion", StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException) { return false; }
        catch (InvalidOperationException) { return false; }
    }
}
