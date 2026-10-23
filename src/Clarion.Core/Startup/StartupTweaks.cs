using Clarion.Core.Model;

namespace Clarion.Core.Startup;

/// <summary>
/// Turns one startup entry on or off as an ordinary change, so it goes through the same engine, history and revert as every other setting.
/// Nothing is deleted: the entry stays where it is and only Windows' own on and off mark changes, as Task Manager does it.
/// </summary>
public static class StartupTweaks
{
    private const string Prefix = "startup.";

    private static string Key(StartupOrigin origin) => origin switch
    {
        StartupOrigin.UserRun => "user-run",
        StartupOrigin.MachineRun => "machine-run",
        StartupOrigin.MachineRun32 => "machine-run32",
        StartupOrigin.UserFolder => "user-folder",
        _ => "machine-folder",
    };

    public static string IdOf(StartupOrigin origin, string name) => $"{Prefix}{Key(origin)}.{name}";

    public static bool IsStartupId(string id) => TryParse(id, out _, out _);

    public static bool TryParse(string id, out StartupOrigin origin, out string name)
    {
        origin = default;
        name = "";
        if (!id.StartsWith(Prefix, StringComparison.Ordinal)) return false;
        var rest = id[Prefix.Length..];
        var dot = rest.IndexOf('.');
        if (dot <= 0 || dot == rest.Length - 1) return false;
        foreach (var candidate in Enum.GetValues<StartupOrigin>())
        {
            if (Key(candidate) != rest[..dot]) continue;
            origin = candidate;
            name = rest[(dot + 1)..];
            return true;
        }
        return false;
    }

    /// <summary>An entry rebuilt from its id alone, enough to turn it back on from the history when the list has not been read.</summary>
    public static StartupEntry? EntryFromId(string id) =>
        TryParse(id, out var origin, out var name) ? new StartupEntry(origin, name, "", true, null, new StartupProgram(null, false, "", ""), null) : null;

    /// <summary>The name to show for a startup change in the history, worked out from its id alone.</summary>
    public static string NameOf(string id) =>
        TryParse(id, out _, out var name) ? $"Stop {name} starting with Windows" : id;

    /// <summary>
    /// The change that turns an entry off. Turning it back on is its Revert, which puts back exactly what was there when .Clarion recorded
    /// the change, and otherwise the on mark in Undo, for an entry something else had turned off.
    /// </summary>
    public static Tweak TurnOff(StartupEntry entry, DateTimeOffset now)
    {
        var hex = StartupApproval.DisabledHex(now);
        var target = new RegistryTarget(entry.NeedsAdmin ? RegistryHive.LocalMachine : RegistryHive.CurrentUser, StartupApproval.KeyOf(entry.Origin), entry.Name);
        return new Tweak
        {
            Id = entry.Id,
            Category = "Startup",
            Name = $"Stop {entry.Name} starting with Windows",
            Summary = "It no longer starts when you sign in.",
            What = $"Marks {entry.Name} as turned off in the list Task Manager's Startup tab reads. The entry and the program are not deleted.",
            Benefit = "A faster sign in and less running in the background.",
            Risk = "None. Turn it back on from this page or from Safety.",
            Evidence = Evidence.Situational,
            RiskLevel = RiskLevel.Safe,
            Scope = entry.NeedsAdmin ? TweakScope.Machine : TweakScope.User,
            Apply = [new SetRegistryValue(target, new RegistryData(RegistryKind.Binary, hex))],
            // Used when nothing was recorded, for example when Task Manager turned it off. A recorded change puts back exactly what was there.
            Undo = [new SetRegistryValue(target, new RegistryData(RegistryKind.Binary, StartupApproval.EnabledHex))],
        };
    }
}
