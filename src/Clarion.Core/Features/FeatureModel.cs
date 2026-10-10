using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Clarion.Core.Features;

public interface IFeatureStore
{
    /// <summary>Reads the state of these features in one go so later lookups are instant.</summary>
    void Prefetch(IReadOnlyList<string> featureNames);

    /// <summary>Drops cached lists so the next lookup reads Windows again.</summary>
    void Invalidate();

    /// <summary>True when enabled, false when off or only partly installed, null when the feature does not exist here.</summary>
    bool? IsFeatureEnabled(string name);
    void SetFeature(string name, bool enabled);

    /// <summary>True when installed, false when not, null when the capability does not exist here.</summary>
    bool? IsCapabilityInstalled(string name);
    void SetCapability(string name, bool installed);
}

public static partial class FeatureRules
{
    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._~-]{0,127}$")]
    private static partial Regex NamePattern();

    private static readonly string[] NeverDisable =
    [
        "Windows-Defender", "NetFx4", "Microsoft-Windows-NetFx4", "Microsoft-Windows-Client",
        "Printing-Foundation-Features", "Windows-Identity-Foundation", "SearchEngine-Client-Package",
    ];

    public static bool IsValidName(string name) => NamePattern().IsMatch(name);

    public static bool CanDisable(string name) =>
        !NeverDisable.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    /// <summary>Maps the state names DISM reports to on or off.</summary>
    public static bool IsEnabledState(string state) =>
        state.Equals("Enabled", StringComparison.OrdinalIgnoreCase) ||
        state.Equals("EnablePending", StringComparison.OrdinalIgnoreCase);

    public static bool IsInstalledState(string state) =>
        state.Equals("Installed", StringComparison.OrdinalIgnoreCase);
}

public static class FeatureScripts
{
    /// <summary>
    /// The state of just the named features, in one PowerShell run. Asking for every feature on the PC took about 16 seconds in a
    /// measurement here, and asking for the eleven the catalog uses about 8. A name this PC does not have gives nothing back.
    /// </summary>
    public static string FeatureStates(IEnumerable<string> names)
    {
        var list = names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var n in list) Require(n);
        var quoted = string.Join(",", list.Select(n => $"'{n}'"));
        return
            $"$f=@(foreach ($n in @({quoted})) {{ Get-WindowsOptionalFeature -Online -FeatureName $n -ErrorAction SilentlyContinue | Select-Object FeatureName,@{{n='State';e={{$_.State.ToString()}}}} }}); " +
            "ConvertTo-Json -InputObject @{features=$f} -Depth 3 -Compress";
    }

    public static string CapabilityState(string name)
    {
        Require(name);
        return
            $"$c=@(Get-WindowsCapability -Online -Name '{name}' | Select-Object Name,@{{n='State';e={{\"$($_.State)\"}}}}); " +
            "ConvertTo-Json -InputObject @{capabilities=$c} -Depth 3 -Compress";
    }

    public static string SetFeature(string name, bool enabled)
    {
        Require(name);
        return (enabled
            ? $"Enable-WindowsOptionalFeature -Online -FeatureName '{name}' -All -NoRestart | Out-Null"
            : $"Disable-WindowsOptionalFeature -Online -FeatureName '{name}' -NoRestart | Out-Null");
    }

    public static string SetCapability(string name, bool installed)
    {
        Require(name);
        return (installed
            ? $"Add-WindowsCapability -Online -Name '{name}' | Out-Null"
            : $"Remove-WindowsCapability -Online -Name '{name}' | Out-Null");
    }

    public static Dictionary<string, string> ParseStates(string json, string arrayName, string nameProp)
    {
        using var doc = JsonDocument.Parse(json);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in doc.RootElement.GetProperty(arrayName).EnumerateArray())
        {
            map[e.GetProperty(nameProp).GetString() ?? ""] = e.GetProperty("State").GetString() ?? "";
        }
        return map;
    }

    private static void Require(string name)
    {
        if (!FeatureRules.IsValidName(name)) throw new ArgumentException($"Not a valid feature name: {name}");
    }
}
