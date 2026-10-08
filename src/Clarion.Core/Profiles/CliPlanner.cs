using Clarion.Core.Model;

namespace Clarion.Core.Profiles;

public sealed record ApplyPlan(
    IReadOnlyList<Tweak> ToApply,
    IReadOnlyList<string> AlreadyOn,
    IReadOnlyList<string> NotSupported,
    IReadOnlyList<string> Unknown);

/// <summary>Works out what a setup file or a preset would change on this PC, without changing anything.</summary>
public static class CliPlanner
{
    public static ApplyPlan Plan(
        IEnumerable<string> ids, IReadOnlyList<Tweak> catalog, MachineProfile machine, Func<Tweak, TweakState> detect)
    {
        var byId = catalog.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
        var apply = new List<Tweak>();
        var already = new List<string>();
        var unsupported = new List<string>();
        var unknown = new List<string>();

        foreach (var id in ids.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!byId.TryGetValue(id, out var t)) { unknown.Add(id); continue; }
            if (!machine.Supports(t.Requires)) { unsupported.Add(id); continue; }
            switch (detect(t))
            {
                case TweakState.Applied: already.Add(id); break;
                case TweakState.Unavailable: unsupported.Add(id); break;
                default: apply.Add(t); break;
            }
        }

        // One choice per exclusive group, the last one listed wins.
        var chosen = new Dictionary<string, Tweak>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in apply.Where(t => t.ExclusiveGroup is not null)) chosen[t.ExclusiveGroup!] = t;
        var final = apply.Where(t => t.ExclusiveGroup is null || ReferenceEquals(chosen[t.ExclusiveGroup], t)).ToList();
        return new ApplyPlan(final, already, unsupported, unknown);
    }
}
