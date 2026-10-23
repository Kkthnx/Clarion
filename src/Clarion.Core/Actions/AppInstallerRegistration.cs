namespace Clarion.Core.Actions;

/// <summary>The one fixed command behind the Repair winget job. It is a constant so that nothing in a catalog file can change what runs.</summary>
public static class AppInstallerRegistration
{
    /// <summary>The package family of Desktop App Installer, the Microsoft package that provides winget.</summary>
    public const string FamilyName = "Microsoft.DesktopAppInstaller_8wekyb3d8bbwe";

    /// <summary>
    /// Add-AppxPackage with -RegisterByFamilyName and -MainPackage is the form Microsoft documents for registering a package that is already on the PC.
    /// It prints one plain sentence either way, so the log never holds a raw PowerShell error, and it ends with exit code 1 or 2 when it did not work.
    /// </summary>
    public static readonly string Script =
        // Progress records are written to the error stream as XML when the output is a pipe, and that would fill the log with markup.
        "$ProgressPreference = 'SilentlyContinue'; " +
        "try { " +
        $"Add-AppxPackage -RegisterByFamilyName -MainPackage '{FamilyName}'; " +
        "$p = Get-AppxPackage -Name 'Microsoft.DesktopAppInstaller' | Select-Object -First 1; " +
        "if ($null -eq $p) { 'App Installer is not on this PC, so there is nothing to register. Install App Installer from the Microsoft Store.'; exit 2 } " +
        "'App Installer ' + $p.Version + ' is registered for this account.'; 'winget should work in a new window.' " +
        "} catch { 'Windows could not register App Installer: ' + $_.Exception.Message; exit 1 }";

    /// <summary>The tool, as a path below the System32 folder.</summary>
    public const string Tool = "WindowsPowerShell\\v1.0\\powershell.exe";
}
