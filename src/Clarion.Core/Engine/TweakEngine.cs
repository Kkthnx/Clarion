using Clarion.Core.Abstractions;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

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
        try { Parallel.ForEach(_handlers, h => h.Warm(ops)); }
        catch (AggregateException) { }
    }

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

            Operation? undo = null;
            try
            {
                undo = handler.CaptureUndo(op);
                handler.Execute(op);
                if (!handler.IsSatisfied(op))
                {
                    RollBack(tweak.Id, batchId, done, undo);
                    return TweakResult.Fail($"Verification failed: {op.Describe()}");
                }
                var entry = new JournalEntry(batchId, tweak.Id, JournalAction.Apply, _clock.GetUtcNow(), op, undo);
                journal.Append(entry);
                done.Add(entry);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                RollBack(tweak.Id, batchId, done, undo);
                return TweakResult.Fail($"{op.Describe()}: {ex.Message}");
            }
        }
        return TweakResult.Ok();
    }

    /// <summary>Steps of a tweak that no longer hold, such as a value Windows set back or an app that returned.</summary>
    public IReadOnlyList<Operation> FindDrifted(Tweak tweak) =>
        tweak.Apply.Where(o => _handlers.Any(h => h.Handles(o)) && HandlerFor(o).IsApplicable(o) && !HandlerFor(o).IsSatisfied(o)).ToList();

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

    private void RollBack(string tweakId, Guid batchId, List<JournalEntry> done, Operation? failedUndo)
    {
        if (failedUndo is not null)
        {
            try
            {
                var handler = HandlerFor(failedUndo);
                if (!handler.IsSatisfied(failedUndo)) handler.Execute(failedUndo);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException) { }
        }
        foreach (var entry in Enumerable.Reverse(done))
        {
            try { RunAndRecord(tweakId, batchId, JournalAction.Revert, entry.Undo, entry.Operation, verify: false); }
            catch (Exception ex) when (ex is not OutOfMemoryException) { }
        }
    }

    private IOperationHandler HandlerFor(Operation op) =>
        _handlers.FirstOrDefault(h => h.Handles(op))
        ?? throw new InvalidOperationException($"No handler for {op.GetType().Name}");
}
