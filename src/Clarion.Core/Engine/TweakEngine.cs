using Clarion.Core.Abstractions;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

/// <summary>How many steps of a setting could be checked, and which of them no longer hold.</summary>
public sealed record BrokenStep(Operation Operation, string Reason, bool AppReturned);

public sealed record DriftCheck(int Checked, IReadOnlyList<BrokenStep> Broken);

public sealed record TweakResult(bool Success, string? Error = null)
{
    public static TweakResult Ok() => new(true);
    public static TweakResult Fail(string error) => new(false, error);
}

/// <summary>
/// Applies and reverts tweaks. Every step captures its undo first, runs, checks the
/// result, and is recorded in the journal. A failed step rolls back the earlier ones.
/// </summary>
public sealed class TweakEngine(IEnumerable<IOperationHandler> handlers, ChangeJournal journal, TimeProvider? clock = null)
{
    private readonly IReadOnlyList<IOperationHandler> _handlers = handlers.ToList();
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;

    /// <summary>Runs every handler warm-up at the same time. Slow lookups overlap instead of queueing.</summary>
    public void WarmUp(IEnumerable<Tweak> tweaks)
    {
        var ops = tweaks.SelectMany(t => t.Apply).ToList();
        WarmUpProblem = null;
        try { Parallel.ForEach(_handlers, h => h.Warm(ops)); }
        catch (AggregateException ex)
        {
            // Warming up only saves time, so a failure does not stop anything, but the reason is kept for the log.
            WarmUpProblem = string.Join("; ", ex.Flatten().InnerExceptions.Select(e => e.Message));
        }
    }

    /// <summary>Why the last warm up did not finish, or null when it did. Reads are only slower when this is set.</summary>
    public string? WarmUpProblem { get; private set; }

    /// <summary>Clears every handler cache so the next read is fresh.</summary>
    public void Invalidate()
    {
        foreach (var h in _handlers) h.Invalidate();
    }

    public IReadOnlyList<string> Plan(Tweak tweak) => tweak.Apply.Select(o => o.Describe()).ToList();

    public TweakState Detect(Tweak tweak)
    {
        // A setting made of an operation this build does not know how to run is simply not available here.
        if (tweak.Apply.Any(o => !_handlers.Any(h => h.Handles(o)))) return TweakState.Unavailable;

        var applicable = tweak.Apply.Where(o => HandlerFor(o).IsApplicable(o)).ToList();
        if (applicable.Count == 0) return TweakState.Unavailable;
        var matched = applicable.Count(o => HandlerFor(o).IsSatisfied(o));
        if (matched == applicable.Count)
        {
            var allVacuous = applicable.All(o => HandlerFor(o).IsVacuous(o));
            if (allVacuous && journal.OutstandingFor(tweak.Id).Count == 0) return TweakState.Unavailable;
            return TweakState.Applied;
        }
        return matched == 0 ? TweakState.NotApplied : TweakState.Partial;
    }

    public TweakResult Apply(Tweak tweak, Guid batchId)
    {
        var state = Detect(tweak);
        if (state == TweakState.Applied) return TweakResult.Ok();
        if (state == TweakState.Unavailable) return TweakResult.Fail("Not available on this system.");

        var done = new List<JournalEntry>();
        foreach (var op in tweak.Apply)
        {
            var handler = HandlerFor(op);
            if (!handler.IsApplicable(op)) continue;

            JournalEntry? pending = null;
            try
            {
                var undo = handler.CaptureUndo(op);
                // The record is written before the change. If the process dies in between, the history holds a change that may not
                // have happened, which is harmless to revert. The other order could leave a change that nothing remembers.
                var entry = new JournalEntry(batchId, tweak.Id, JournalAction.Apply, _clock.GetUtcNow(), op, undo);
                journal.Append(entry);
                pending = entry;
                handler.Execute(op);
                if (!handler.IsSatisfied(op))
                    return Failed(tweak.Id, batchId, done, pending, $"Verification failed: {op.Describe()}");
                done.Add(entry);
                pending = null;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                return Failed(tweak.Id, batchId, done, pending, $"{op.Describe()}: {ex.Message}");
            }
        }
        return TweakResult.Ok();
    }

    /// <summary>
    /// Undoes what this run had done so far and builds the failure. When some of the undoing fails too, the message says so, because
    /// the setting is then partly applied. Its earlier steps stay in the history as still on, so Safety can revert them.
    /// </summary>
    private TweakResult Failed(string tweakId, Guid batchId, List<JournalEntry> done, JournalEntry? pending, string reason)
    {
        var stuck = RollBack(tweakId, batchId, done, pending);
        if (stuck.Count == 0) return TweakResult.Fail(reason);
        return TweakResult.Fail($"{reason} Going back also failed for {stuck.Count} earlier step{(stuck.Count == 1 ? "" : "s")}, so this setting is partly applied: {string.Join("; ", stuck)}. Revert it from Safety to try again.");
    }

    /// <summary>
    /// Checks the steps Clarion recorded running for a tweak against Windows now. The recorded steps are the truth
    /// about what was done, for example an app removal that also stopped new accounts getting it. When nothing is
    /// recorded the catalog steps are used.
    /// </summary>
    public DriftCheck CheckRecorded(Tweak tweak, IReadOnlyList<JournalEntry>? recorded = null)
    {
        var ops = (recorded ?? journal.OutstandingFor(tweak.Id)).Select(e => e.Operation).ToList();
        if (ops.Count == 0) ops = tweak.Apply.ToList();
        var applicable = ops.Where(o => _handlers.Any(h => h.Handles(o)) && HandlerFor(o).IsApplicable(o)).ToList();
        var broken = applicable.Where(o => !HandlerFor(o).IsSatisfied(o))
            .Select(o => new BrokenStep(o, HandlerFor(o).WhyBroken(o) ?? o.Describe(), HandlerFor(o).IsReturnedApp(o)))
            .ToList();
        return new DriftCheck(applicable.Count, broken);
    }

    /// <summary>
    /// Starts checking a setting again that the person had told Clarion to stop checking. Windows is not changed. The earlier
    /// recorded steps are recorded again as the current ones, so Verify compares against what Clarion originally applied.
    /// </summary>
    public int Resume(Tweak tweak, Guid batchId)
    {
        if (!journal.ReleasedByTweak().TryGetValue(tweak.Id, out var steps)) return 0;
        foreach (var step in steps)
            journal.Append(new JournalEntry(batchId, tweak.Id, JournalAction.Apply, _clock.GetUtcNow(), step.Operation, step.Undo));
        return steps.Count;
    }

    /// <summary>Stops tracking a tweak without touching Windows. Its recorded changes are kept for the history.</summary>
    public void Release(Tweak tweak, Guid batchId)
    {
        foreach (var entry in journal.OutstandingFor(tweak.Id))
            journal.Append(new JournalEntry(batchId, tweak.Id, JournalAction.Release, _clock.GetUtcNow(), entry.Operation, entry.Undo));
    }

    public TweakResult Revert(Tweak tweak, Guid batchId)
    {
        var outstanding = journal.OutstandingFor(tweak.Id);
        try
        {
            if (outstanding.Count > 0)
            {
                foreach (var entry in outstanding.Reverse())
                {
                    RunAndRecord(tweak.Id, batchId, JournalAction.Revert, entry.Undo, entry.Operation, verify: true);
                }
                return TweakResult.Ok();
            }

            if (tweak.Undo.Count == 0) return TweakResult.Fail("Nothing recorded to revert and no default is defined.");

            foreach (var op in tweak.Undo)
            {
                var handler = HandlerFor(op);
                if (!handler.IsApplicable(op)) continue;
                RunAndRecord(tweak.Id, batchId, JournalAction.Revert, op, handler.CaptureUndo(op), verify: true);
            }
            return TweakResult.Ok();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return TweakResult.Fail(ex.Message);
        }
    }

    private void RunAndRecord(string tweakId, Guid batchId, JournalAction action, Operation op, Operation inverse, bool verify)
    {
        var handler = HandlerFor(op);
        handler.Execute(op);
        if (verify && !handler.IsSatisfied(op)) throw new InvalidOperationException($"Verification failed: {op.Describe()}");
        journal.Append(new JournalEntry(batchId, tweakId, action, _clock.GetUtcNow(), op, inverse));
    }

    /// <summary>
    /// Puts back what this run had changed and returns what could not be put back, one line each. The undo is written to the
    /// history only when all of it worked. The history counts a setting as still on until something other than an apply follows it,
    /// so writing a partial undo would make a half-undone setting look finished and hide it from Revert.
    /// </summary>
    private List<string> RollBack(string tweakId, Guid batchId, List<JournalEntry> done, JournalEntry? pending)
    {
        var stuck = new List<string>();
        var steps = new List<JournalEntry>();
        if (pending is not null) steps.Add(pending);
        steps.AddRange(Enumerable.Reverse(done));

        foreach (var entry in steps)
        {
            try
            {
                var handler = HandlerFor(entry.Undo);
                // A step that failed may not have changed anything. Putting back what is already there is skipped.
                if (ReferenceEquals(entry, pending) && handler.IsSatisfied(entry.Undo)) continue;
                handler.Execute(entry.Undo);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException) { stuck.Add($"{entry.Operation.Describe()} ({ex.Message})"); }
        }

        if (stuck.Count > 0) return stuck;

        foreach (var entry in steps)
        {
            try { journal.Append(new JournalEntry(batchId, tweakId, JournalAction.Revert, _clock.GetUtcNow(), entry.Undo, entry.Operation)); }
            catch (IOException) { break; } // the PC is back as it was, and the history still lists the setting, which a later Revert settles
        }
        return stuck;
    }

    private IOperationHandler HandlerFor(Operation op) =>
        _handlers.FirstOrDefault(h => h.Handles(op))
        ?? throw new InvalidOperationException($"No handler for {op.GetType().Name}");
}
