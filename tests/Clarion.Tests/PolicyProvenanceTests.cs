using System.Xml.Linq;
using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

/// <summary>
/// Checks every setting that claims a Microsoft policy as its source against the policy
/// definition files that ship with Windows. A wrong key or value name fails here, not on a user PC.
/// Skipped when the definition folder is not present.
/// </summary>
public sealed class PolicyProvenanceTests
{
    private static readonly string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "PolicyDefinitions");

    private sealed record PolicyDef(string Key, HashSet<string> ValueNames);

    private static PolicyDef? Load(string file, string policyName)
    {
        var path = Path.Combine(Folder, file);
        if (!File.Exists(path)) return null;
        var doc = XDocument.Load(path);
        var ns = doc.Root!.Name.Namespace;
        var policy = doc.Descendants(ns + "policy").FirstOrDefault(p => (string?)p.Attribute("name") == policyName);
        if (policy is null) return null;

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var main = (string?)policy.Attribute("valueName");
        if (main is not null) names.Add(main);
        foreach (var el in policy.Descendants().Where(e => e.Attribute("valueName") is not null))
            names.Add((string)el.Attribute("valueName")!);
        return new PolicyDef((string?)policy.Attribute("key") ?? "", names);
    }

    // Server editions ship a smaller set of policy files, so only a client install is a fair reference.
    private static bool IsClientWindows() =>
        OperatingSystem.IsWindows() &&
        Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallationType", "") as string == "Client";

    [Fact]
    public void Every_policy_backed_setting_matches_the_policy_definitions_on_this_machine()
    {
        if (!Directory.Exists(Folder) || !IsClientWindows()) return;

        var checkedCount = 0;
        foreach (var t in CatalogLoader.LoadEmbedded().Where(t => t.Provenance.Count > 0))
        {
            var defs = new List<PolicyDef>();
            foreach (var prov in t.Provenance)
            {
                var parts = prov.Split(' ', 2);
                var def = Load(parts[0], parts[1]);
                Assert.True(def is not null, $"{t.Id}: policy {prov} was not found in {Folder}");
                defs.Add(def!);
            }

            foreach (var op in t.Apply.OfType<SetRegistryValue>())
            {
                var match = defs.Any(d =>
                    d.Key.Equals(op.Target.Path, StringComparison.OrdinalIgnoreCase) &&
                    d.ValueNames.Contains(op.Target.Name));
                Assert.True(match, $"{t.Id}: {op.Target} is not defined by {string.Join(", ", t.Provenance)}");
                checkedCount++;
            }
        }
        Assert.True(checkedCount > 40, $"Only {checkedCount} policy values were checked");
    }

    [Fact]
    public void Provenance_entries_have_the_file_and_policy_name()
    {
        foreach (var t in CatalogLoader.LoadEmbedded())
            Assert.All(t.Provenance, p => Assert.Matches(@"^\S+\.admx \S+$", p));
    }
}
