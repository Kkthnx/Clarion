namespace Clarion.Core.Startup;

/// <summary>Where Windows finds a program to start at sign in. Each one has its own place for the on and off mark that Task Manager reads.</summary>
public enum StartupOrigin { UserRun, MachineRun, MachineRun32, UserFolder, MachineFolder }

/// <summary>One program as the places Windows starts programs from list it, before anything is looked up about the program.</summary>
public sealed record RawStartupItem(StartupOrigin Origin, string Name, string Command);

/// <summary>What is known about the program a startup entry points at.</summary>
public sealed record StartupProgram(string? Path, bool Found, string Publisher, string Description);

/// <summary>Reads the places Windows starts programs from. The real one reads the registry and the Startup folders.</summary>
public interface IStartupSource
{
    IReadOnlyList<RawStartupItem> Items();

    /// <summary>The on and off mark Windows keeps for an entry, or null when it has none, which counts as on.</summary>
    byte[]? Approval(StartupOrigin origin, string name);

    StartupProgram Describe(string command);
}

public sealed record StartupEntry(
    StartupOrigin Origin,
    string Name,
    string Command,
    bool Enabled,
    DateTimeOffset? DisabledAt,
    StartupProgram Program,
    string? Warning)
{
    public string Id => StartupTweaks.IdOf(Origin, Name);

    /// <summary>Entries kept for every account need administrator rights to change.</summary>
    public bool NeedsAdmin => Origin is StartupOrigin.MachineRun or StartupOrigin.MachineRun32 or StartupOrigin.MachineFolder;

    /// <summary>A part of Windows or its security that .Clarion does not offer to stop. The reason is in <see cref="Warning"/>.</summary>
    public bool IsProtected => StartupList.IsProtected(this);

    public string Source => Origin switch
    {
        StartupOrigin.UserRun => "Your account",
        StartupOrigin.UserFolder => "Your Startup folder",
        StartupOrigin.MachineFolder => "Every account, Startup folder",
        _ => "Every account",
    };
}

/// <summary>The on and off mark Windows keeps under StartupApproved, which is what the Startup tab of Task Manager reads and writes.</summary>
public static class StartupApproval
{
    /// <summary>Twelve bytes. An even first byte is on and an odd one is off, and an off mark carries the time it was made in the last eight.</summary>
    public const string EnabledHex = "020000000000000000000000";

    /// <summary>No mark at all counts as on.</summary>
    public static bool IsEnabled(byte[]? mark) => mark is null || mark.Length == 0 || (mark[0] & 1) == 0;

    public static DateTimeOffset? DisabledAt(byte[]? mark)
    {
        if (IsEnabled(mark) || mark is null || mark.Length < 12) return null;
        var ticks = BitConverter.ToInt64(mark, 4);
        try { return ticks > 0 ? DateTimeOffset.FromFileTime(ticks) : null; }
        catch (ArgumentOutOfRangeException) { return null; }
    }

    public static string DisabledHex(DateTimeOffset when)
    {
        var bytes = new byte[12];
        bytes[0] = 3;
        BitConverter.GetBytes(when.ToFileTime()).CopyTo(bytes, 4);
        return Convert.ToHexString(bytes);
    }

    /// <summary>The key under the hive's Explorer folder that holds the mark for entries of this origin.</summary>
    public static string KeyOf(StartupOrigin origin) =>
        "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\" + origin switch
        {
            StartupOrigin.UserRun or StartupOrigin.MachineRun => "Run",
            StartupOrigin.MachineRun32 => "Run32",
            _ => "StartupFolder",
        };
}

/// <summary>Works out which program a startup command starts, without running anything.</summary>
public static class StartupCommand
{
    public static string? ProgramPath(string command)
    {
        var text = Environment.ExpandEnvironmentVariables(command.Trim());
        if (text.Length == 0) return null;
        if (text[0] == '"')
        {
            var end = text.IndexOf('"', 1);
            return end > 1 ? text[1..end] : null;
        }
        // A path with spaces is often not quoted, so the program ends at the first ".exe".
        var exe = text.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (exe >= 0) return text[..(exe + 4)];
        var space = text.IndexOf(' ');
        return space < 0 ? text : text[..space];
    }
}
