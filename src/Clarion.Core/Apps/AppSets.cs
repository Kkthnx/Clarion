using Clarion.Core.Model;

namespace Clarion.Core.Apps;

/// <summary>
/// Moves a set of apps between .Clarion and a winget file. Only packages the catalog already pins can be queued, because .Clarion installs an
/// exact package ID it has looked at, never one a file names. Anything else in the file is reported, not installed.
/// </summary>
public static class AppSets
{
    private static string? PackageOf(Tweak t) => t.Apply.OfType<InstallWingetPackage>().FirstOrDefault()?.Id;

    /// <summary>The package IDs of the listed catalog settings that are apps, in a stable order.</summary>
    public static IReadOnlyList<string> PackagesOf(IEnumerable<Tweak> catalog, IEnumerable<string> tweakIds)
    {
        var wanted = tweakIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return catalog.Where(t => wanted.Contains(t.Id)).Select(PackageOf).OfType<string>().Order(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// The catalog settings that install the listed packages, and the package IDs the catalog does not know. IDs are compared without regard to
    /// case, since winget does the same.
    /// </summary>
    public static (IReadOnlyList<string> Matched, IReadOnlyList<string> Unknown) FromPackages(IEnumerable<Tweak> catalog, IEnumerable<string> packageIds)
    {
        var byPackage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in catalog)
            if (PackageOf(t) is { } id) byPackage.TryAdd(id, t.Id);

        var matched = new List<string>();
        var unknown = new List<string>();
        foreach (var id in packageIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (byPackage.TryGetValue(id, out var tweakId)) matched.Add(tweakId);
            else unknown.Add(id);
        }
        return (matched, unknown.Order(StringComparer.OrdinalIgnoreCase).ToList());
    }
}
