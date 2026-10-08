using System.Text;

namespace Clarion.Core.Appx;

/// <summary>
/// Builds the PowerShell used for app packages. Names are checked first and scripts are passed
/// as encoded commands, so no text from a catalog or a user can change what runs.
/// </summary>
public static class AppxScripts
{
    private const string Prelude = "$ErrorActionPreference='Stop'; [Console]::OutputEncoding=[Text.Encoding]::UTF8; ";

    public static string Inventory() => Prelude +
        "$i=@(Get-AppxPackage -AllUsers | Select-Object Name,PackageFullName,PackageFamilyName,NonRemovable,IsFramework,@{n='SignatureKind';e={\"$($_.SignatureKind)\"}}); " +
        "$c=@(Get-AppxPackage | ForEach-Object { $_.Name }); " +
        "$p=@(); $k=$true; try { $p=@(Get-AppxProvisionedPackage -Online | ForEach-Object { $_.DisplayName }) } catch { $k=$false }; " +
        "ConvertTo-Json -InputObject @{installed=$i;current=$c;provisioned=$p;provisionedKnown=$k} -Depth 4 -Compress";

    public static string Remove(string name, bool allUsers, bool deprovision)
    {
        RequireSafeName(name);
        var sb = new StringBuilder(Prelude);
        sb.Append(allUsers
            ? $"Get-AppxPackage -AllUsers -Name '{name}' | Remove-AppxPackage -AllUsers; "
            : $"Get-AppxPackage -Name '{name}' | Remove-AppxPackage; ");
        if (deprovision)
        {
            sb.Append($"Get-AppxProvisionedPackage -Online | Where-Object {{ $_.DisplayName -eq '{name}' }} | ");
            sb.Append("ForEach-Object { Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName | Out-Null }; ");
        }
        return sb.ToString();
    }

    public static string Restore(string familyName)
    {
        RequireSafeName(familyName);
        return Prelude + $"Add-AppxPackage -RegisterByFamilyName -MainPackage '{familyName}'";
    }

    /// <summary>Command line arguments that run a script without any shell quoting.</summary>
    public static string ToArguments(string script)
    {
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        return $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encoded}";
    }

    private static void RequireSafeName(string value)
    {
        if (!AppxSafety.IsValidName(value)) throw new ArgumentException($"Not a valid package name: {value}");
    }
}
