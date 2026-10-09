using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Core.Drift;

/// <summary>
/// Compares what Clarion recorded applying with what Windows looks like now. Only settings the
/// journal says are still on are checked, so a setting the user reverted never shows up here.
/// </summary>
public sealed class DriftScanner(TweakEngine engine, ChangeJournal journal, DriftState? state = null, TimeProvider? clock = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;

    /// <param name="fresh">Drop cached reads first. Leave on for a real scan, since a stale read hides what came back.</param>
    /// <param name="progress">Told after each setting is checked, with how many are done and how many there are.</param>
    public DriftReport Scan(IEnumerable<Tweak> catalog, MachineProfile profile, bool fresh = true, IProgress<(int Done, int Total)>? progress = null)
    {
        var previous = state?.Load().LastBuild;
        if (fresh) engine.Invalidate();

        var all = catalog.ToList();
        var outstanding = journal.OutstandingByTweak();
        var order = journal.LastApplyOrder();
        var stillOn = outstanding.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tracked = all.Where(t => outstanding.ContainsKey(t.Id) && profile.Supports(t.Requires)).ToList();
        engine.WarmUp(tracked);

        var items = new List<DriftItem>();
        var unreadable = new List<string>();
        var read = 0;
        var holding = new List<string>();
        var done = 0;
        foreach (var tweak in tracked)
        {
            progress?.Report((done++, tracked.Count));
            var recorded = outstanding[tweak.Id];
            DriftCheck check;
            try { check = engine.CheckRecorded(tweak, recorded); }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                unreadable.Add(tweak.Id);
                continue;
            }
            if (check.Checked == 0) continue;
            read++;
            if (check.Broken.Count == 0) { holding.Add(tweak.Id); continue; }
            if (ReplacedInsideClarion(tweak, all, stillOn, order)) continue;

            var returned = check.Broken.Where(b => b.AppReturned).Select(b => ((RemoveAppxPackage)b.Operation).Name).ToList();
            var tweakState = check.Broken.Count == check.Checked ? TweakState.NotApplied : TweakState.Partial;
            items.Add(new DriftItem(tweak, tweakState, recorded[0].Time, check.Broken.Select(b => b.Reason).ToList(), returned));
        }

        progress?.Report((tracked.Count, tracked.Count));
        var now = _clock.GetUtcNow();
        state?.Save(profile.Build, now);
        return new DriftReport(now, profile.Build, previous, read, items, unreadable) { Holding = holding };
    }

    // Picking another choice in an exclusive group (a power plan, a DNS provider) inside Clarion replaces this one on
    // purpose and leaves it recorded as on. The journal tells that: a later choice from the same group that Clarion
    // applied and nobody reverted. A change made outside Clarion leaves no such record, so it still counts as drift.
    private static bool ReplacedInsideClarion(Tweak tweak, IEnumerable<Tweak> catalog, IReadOnlySet<string> stillOn, IReadOnlyDictionary<string, int> order) =>
        tweak.ExclusiveGroup is { } group &&
        order.TryGetValue(tweak.Id, out var mine) &&
        catalog.Any(o => o.Id != tweak.Id && o.ExclusiveGroup == group && stillOn.Contains(o.Id) && order.TryGetValue(o.Id, out var theirs) && theirs > mine);
}
