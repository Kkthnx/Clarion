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
        var outstanding = journal.TweakIdsWithOutstanding();
        var tracked = all.Where(t => outstanding.Contains(t.Id) && profile.Supports(t.Requires)).ToList();
        engine.WarmUp(tracked);

        var items = new List<DriftItem>();
        var unreadable = new List<string>();
        var read = 0;
        foreach (var tweak in tracked)
        {
            TweakState current;
            try { current = engine.Detect(tweak); }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                unreadable.Add(tweak.Id);
                continue;
            }
            read++;
            if (current is not (TweakState.NotApplied or TweakState.Partial)) continue;
            if (SupersededByGroupMember(tweak, all)) continue;

            var broken = engine.FindDrifted(tweak);
            var returned = broken.OfType<RemoveAppxPackage>().Select(a => a.Name).ToList();
            var applied = journal.OutstandingFor(tweak.Id).FirstOrDefault()?.Time ?? DateTimeOffset.MinValue;
            items.Add(new DriftItem(tweak, current, applied, broken.Select(o => o.Describe()).ToList(), returned));
        }

        var now = _clock.GetUtcNow();
        state?.Save(profile.Build, now);
        return new DriftReport(now, profile.Build, previous, read, items, unreadable);
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
