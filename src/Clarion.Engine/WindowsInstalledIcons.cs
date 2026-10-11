using System.Runtime.Versioning;
using Clarion.Core.Apps;
using Microsoft.Win32;

namespace Clarion.Engine;

/// <summary>
/// Finds the icon file of an installed app the way Windows lists it: the DisplayIcon of its Uninstall key, looked up by the code
/// the winget package record names. Nothing is guessed from a program name.
/// </summary>
[SupportedOSPlatform("windows")]
public static class WindowsInstalledIcons
{
    private const string UninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";

    private static readonly (RegistryHive Hive, RegistryView View)[] Places =
    [
        (RegistryHive.LocalMachine, RegistryView.Registry64),
        (RegistryHive.LocalMachine, RegistryView.Registry32),
        (RegistryHive.CurrentUser, RegistryView.Registry64),
    ];

    public static string? FindIconFile(IEnumerable<string> productCodes)
    {
        foreach (var code in productCodes)
        {
            if (code.Length is 0 or > 120 || code.Contains('\\') || code.Contains('/')) continue;
            foreach (var (hive, view) in Places)
            {
                try
                {
                    using var root = RegistryKey.OpenBaseKey(hive, view);
                    using var key = root.OpenSubKey(UninstallPath + code);
                    var file = DisplayIcons.ResolveFile(key?.GetValue("DisplayIcon") as string);
                    if (file is not null && File.Exists(file)) return file;
                }
                catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
                {
                    // A key that cannot be read is the same as no icon.
                }
            }
        }
        return null;
    }
}
