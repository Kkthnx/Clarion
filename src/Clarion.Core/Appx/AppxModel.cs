using System.Text.Json;
using System.Text.RegularExpressions;

namespace Clarion.Core.Appx;

public sealed record AppxPackageInfo(string Name, string FullName, string FamilyName, bool NonRemovable, bool IsFramework, string SignatureKind);

public sealed record AppxSnapshot(
    IReadOnlyList<AppxPackageInfo> Installed,
    IReadOnlySet<string> CurrentUserNames,
    IReadOnlySet<string> ProvisionedNames,
    bool ProvisionedKnown = true)
{
    public AppxPackageInfo? Find(string name) =>
        Installed.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    public bool IsInstalledForAnyone(string name) => Find(name) is not null;
    public bool IsInstalledForCurrentUser(string name) => CurrentUserNames.Contains(name);
    public bool IsProvisioned(string name) => ProvisionedNames.Contains(name);

    public static AppxSnapshot Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var installed = new List<AppxPackageInfo>();
        foreach (var e in root.GetProperty("installed").EnumerateArray())
        {
            installed.Add(new AppxPackageInfo(
                e.GetProperty("Name").GetString() ?? "",
                e.GetProperty("PackageFullName").GetString() ?? "",
                e.GetProperty("PackageFamilyName").GetString() ?? "",
                e.GetProperty("NonRemovable").GetBoolean(),
                e.GetProperty("IsFramework").GetBoolean(),
                e.GetProperty("SignatureKind").GetString() ?? ""));
        }

        var current = root.GetProperty("current").EnumerateArray().Select(e => e.GetString() ?? "")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var provisioned = root.GetProperty("provisioned").EnumerateArray().Select(e => e.GetString() ?? "")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var known = !root.TryGetProperty("provisionedKnown", out var k) || k.GetBoolean();
        return new AppxSnapshot(installed, current, provisioned, known);
    }
}

public interface IAppxStore
{
    AppxSnapshot GetSnapshot();
    void Remove(string name, bool allUsers, bool deprovision);
    void Restore(string familyName);
}

/// <summary>Rules that keep the app remover away from anything Windows needs.</summary>
public static partial class AppxSafety
{
    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$")]
    private static partial Regex NamePattern();

    /// <summary>Exact package names that must never be removed.</summary>
    public static readonly IReadOnlySet<string> ProtectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Microsoft.WindowsStore", "Microsoft.StorePurchaseApp", "Microsoft.DesktopAppInstaller", "Microsoft.Winget.Source",
        "Microsoft.SecHealthUI", "Microsoft.Windows.ShellExperienceHost", "Microsoft.Windows.StartMenuExperienceHost",
        "MicrosoftWindows.Client.CBS", "MicrosoftWindows.Client.Core", "MicrosoftWindows.Client.FileExp",
        "Microsoft.Windows.CloudExperienceHost", "Microsoft.AAD.BrokerPlugin", "Microsoft.AccountsControl",
        "Microsoft.LockApp", "Microsoft.Windows.SecureAssessmentBrowser", "windows.immersivecontrolpanel",
        "Microsoft.Windows.ContentDeliveryManager", "Microsoft.Windows.OOBENetworkConnectionFlow",
        "Microsoft.Windows.PeopleExperienceHost", "Microsoft.Win32WebViewHost", "Microsoft.UI.Xaml.CBS",
        "Microsoft.WindowsAppRuntime.CBS", "MicrosoftWindows.UndockedDevKit", "Microsoft.ECApp",
    };

    /// <summary>Name prefixes for runtimes and frameworks that other apps depend on.</summary>
    public static readonly IReadOnlyList<string> ProtectedPrefixes =
    [
        "Microsoft.VCLibs", "Microsoft.UI.Xaml", "Microsoft.NET.Native", "Microsoft.WindowsAppRuntime",
        "MicrosoftCorporationII.WinAppRuntime", "Microsoft.Services.Store", "Microsoft.DirectXRuntime",
        "Microsoft.WebpImageExtension", "Microsoft.Windows.Search", "Microsoft.Windows.Apprep",
        "Microsoft.Windows.Client", "MicrosoftWindows.Client.", "Windows.", "Microsoft.Windows.XGpuEjectDialog",
    ];

    public static bool IsValidName(string name) => NamePattern().IsMatch(name);

    public static bool IsProtected(string name) =>
        ProtectedNames.Contains(name) || ProtectedPrefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    /// <summary>True when Windows itself marks the package as not removable, a framework, or a system package.</summary>
    public static bool IsSystemOwned(AppxPackageInfo p) =>
        p.NonRemovable || p.IsFramework || string.Equals(p.SignatureKind, "System", StringComparison.OrdinalIgnoreCase);
}
