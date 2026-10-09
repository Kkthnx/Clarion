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
    public DriftReport Scan(IEnumerable<Tweak> catalog, MachineProfile profile, bool fresh = true)
    {
        var previous = state?.Load().LastBuild;
        if (fresh) engine.Invalidate();

        var all = catalog.ToList();
        var outstanding = journal.OutstandingByTweak();
        var tracked = all.Where(t => outstanding.ContainsKey(t.Id) && profile.Supports(t.Requires)).ToList();
        engine.WarmUp(tracked);

        var items = new List<DriftItem>();
        var unreadable = new List<string>();
        var read = 0;
        var holding = new List<string>();
        foreach (var tweak in tracked)
        {
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
            if (SupersededByGroupMember(tweak, all)) continue;

            var returned = check.Broken.Where(b => b.AppReturned).Select(b => ((RemoveAppxPackage)b.Operation).Name).ToList();
            var tweakState = check.Broken.Count == check.Checked ? TweakState.NotApplied : TweakState.Partial;
            items.Add(new DriftItem(tweak, tweakState, recorded[0].Time, check.Broken.Select(b => b.Reason).ToList(), returned));
        }

        var now = _clock.GetUtcNow();
        state?.Save(profile.Build, now);
        return new DriftReport(now, profile.Build, previous, read, items, unreadable) { Holding = holding };
    }

    // Picking another choice in an exclusive group (a power plan, a DNS provider) replaces this one on purpose.
    private bool SupersededByGroupMember(Tweak tweak, IEnumerable<Tweak> catalog) =>
        tweak.ExclusiveGroup is { } group &&
        catalog.Any(o => o.Id != tweak.Id && o.ExclusiveGroup == group && IsApplied(o));

    private bool IsApplied(Tweak tweak)
    {
        try { return engine.Detect(tweak) == TweakState.Applied; }
        catch (Exception ex) when (ex is not OutOfMemoryException) { return false; }
    }
}
