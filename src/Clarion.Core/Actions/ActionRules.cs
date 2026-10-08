namespace Clarion.Core.Actions;

/// <summary>The only tools and folders an action may touch. Anything else is refused at load time and again at run time.</summary>
public static class ActionRules
{
    public static readonly IReadOnlySet<string> AllowedTools = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "sfc.exe", "dism.exe", "ipconfig.exe", "netsh.exe", "net.exe", "chkdsk.exe", "wsreset.exe", "defrag.exe",
    };

    /// <summary>Folder paths, with environment variables, that actions may clean, rename or delete.</summary>
    public static readonly IReadOnlyList<string> AllowedFolders =
    [
        "%TEMP%",
        "%SystemRoot%\\Temp",
        "%SystemRoot%\\SoftwareDistribution",
        "%SystemRoot%\\SoftwareDistribution.old",
        "%SystemRoot%\\System32\\catroot2",
        "%SystemRoot%\\System32\\catroot2.old",
    ];

    public static bool IsAllowedTool(string file) => AllowedTools.Contains(file);

    public static bool IsAllowedFolder(string folder, Func<string, string>? expand = null)
    {
        expand ??= Environment.ExpandEnvironmentVariables;
        var target = Normalize(expand(folder));
        return AllowedFolders.Any(a => Normalize(expand(a)).Equals(target, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>A rename may only change the folder name to a plain name, never move it.</summary>
    public static bool IsPlainName(string name) =>
        name.Length > 0 && name.IndexOfAny(['\\', '/', ':']) < 0 && name != "." && name != "..";

    private static string Normalize(string path) => path.TrimEnd('\\', '/');
}
