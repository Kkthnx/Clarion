namespace Clarion.Core.Startup;

/// <summary>Puts together what starts with Windows on this PC, from the places Windows reads and the marks that turn entries off.</summary>
public static class StartupList
{
    // Parts of Windows that start at sign in. Stopping the Security tray hides virus and threat alerts, so it is not offered.
    private static readonly string[] ProtectedNames = ["SecurityHealth", "WindowsDefender", "Windows Defender"];

    public static bool IsProtected(StartupEntry e) =>
        ProtectedNames.Contains(e.Name, StringComparer.OrdinalIgnoreCase) ||
        e.Command.Contains("SecurityHealthSystray", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<StartupEntry> Build(IStartupSource source)
    {
        var list = new List<StartupEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in source.Items())
        {
            if (!seen.Add(StartupTweaks.IdOf(raw.Origin, raw.Name))) continue;
            var mark = source.Approval(raw.Origin, raw.Name);
            var program = source.Describe(raw.Command);
            var entry = new StartupEntry(raw.Origin, raw.Name, raw.Command, StartupApproval.IsEnabled(mark), StartupApproval.DisabledAt(mark), program, null);
            list.Add(entry with { Warning = WarningFor(entry) });
        }
        return list
            .OrderByDescending(e => e.Enabled)
            .ThenBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static string? WarningFor(StartupEntry e)
    {
        if (e.IsProtected) return "Part of Windows Security. .Clarion leaves it on, so virus and threat alerts keep showing.";
        if (!e.Program.Found && e.Program.Path is not null && !e.Origin.ToString().EndsWith("Folder", StringComparison.Ordinal))
            return "The program this points to is not there any more, so nothing starts from it.";
        return null;
    }
}
