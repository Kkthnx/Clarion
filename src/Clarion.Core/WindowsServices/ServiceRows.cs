using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Core.WindowsServices;

/// <summary>One Windows service as read from this PC.</summary>
/// <param name="MemoryBytes">The working set of the process that hosts it. Null when it is not running or shares its process with others.</param>
/// <param name="StartsOnDemand">True when Windows has trigger events that can start it, so Manual does not mean it never runs.</param>
public sealed record ServiceSnapshot(
    string Name, string DisplayName, ServiceStartType? Start, bool Running, bool StartsOnDemand, long? MemoryBytes, bool SharedHost);

/// <summary>Reads every Windows service on this PC. Read only: nothing is changed.</summary>
public interface IServiceLister
{
    IReadOnlyList<ServiceSnapshot> List();
}

/// <summary>A service next to what Microsoft says about it and what .Clarion offers for it.</summary>
public sealed record ServiceRow(ServiceSnapshot Service, GuideEntry? Guide, bool Protected, Tweak? Setting, bool SettingOn)
{
    public string Name => Service.Name;

    /// <summary>
    /// The start type differs from the default in Microsoft's table. That table is for Windows IoT Enterprise, whose defaults differ from Windows 11
    /// Home and Pro for some services, so this is not proof that anyone changed it. Only known when the table lists the service.
    /// </summary>
    public bool DiffersFromGuide => Service.Start is { } s && Guide is { } g && s != g.Default;
}

/// <summary>What the Services view can narrow its list to.</summary>
public enum ServiceFilter { All, DiffersFromGuide, Running, HasSetting, OkToDisable, DontDisable, NotListed }

public static class ServiceRows
{
    public static IReadOnlyList<ServiceRow> Build(IEnumerable<ServiceSnapshot> snapshots, ServiceGuide guide, IEnumerable<Tweak> tweaks, Func<Tweak, bool> isOn)
    {
        // The first setting that changes a service is the one shown for it. Most services have none.
        var setting = new Dictionary<string, Tweak>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in tweaks)
            foreach (var op in t.Apply.OfType<SetServiceStartType>())
                setting.TryAdd(op.Name, t);

        return snapshots
            .Select(s =>
            {
                setting.TryGetValue(s.Name, out var t);
                return new ServiceRow(s, guide.Find(s.Name), CatalogLoader.ProtectedServices.Contains(s.Name), t, t is not null && isOn(t));
            })
            .OrderBy(r => r.Service.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<ServiceRow> Filter(IEnumerable<ServiceRow> rows, ServiceFilter filter, string query)
    {
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return rows.Where(r => Matches(r, filter) && words.All(w => Haystack(r).Contains(w, StringComparison.OrdinalIgnoreCase))).ToList();
    }

    private static bool Matches(ServiceRow r, ServiceFilter f) => f switch
    {
        ServiceFilter.DiffersFromGuide => r.DiffersFromGuide,
        ServiceFilter.Running => r.Service.Running,
        ServiceFilter.HasSetting => r.Setting is not null,
        ServiceFilter.OkToDisable => r.Guide?.Advice is ServiceAdvice.OkToDisable or ServiceAdvice.ShouldDisable,
        ServiceFilter.DontDisable => r.Guide?.Advice is ServiceAdvice.DontDisable,
        ServiceFilter.NotListed => r.Guide is null,
        _ => true,
    };

    private static string Haystack(ServiceRow r) => $"{r.Service.DisplayName} {r.Name} {r.Setting?.Name}";
}
