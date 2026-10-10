using Clarion.Core.Model;

namespace Clarion.Core.Engine;

public sealed record BatchOptions
{
    public bool CreateRestorePoint { get; init; } = true;

    /// <summary>When creating the restore point fails, continue only if the user accepted the risk.</summary>
    public bool ContinueWithoutRestorePoint { get; init; }

    /// <summary>When the restore point cannot be made, turn on System Protection for the system drive and try again.</summary>
    public bool TurnOnSystemProtection { get; init; }

    /// <summary>Called as each setting starts and finishes, so a window or console can show live progress.</summary>
    public Action<BatchStep>? Step { get; init; }
}

public enum BatchStepState { Running, Done, Skipped, Failed }

/// <summary>One live update. Index counts from 1 across the apply and revert parts of the same run.</summary>
public sealed record BatchStep(int Index, int Total, string TweakId, string Name, bool Reverting, BatchStepState State, string? Error = null);

internal sealed class StepCounter(int total, Action<BatchStep>? report)
{
    private int _index;

    public int Begin(Tweak t, bool reverting)
    {
        var i = ++_index;
        report?.Invoke(new BatchStep(i, total, t.Id, t.Name, reverting, BatchStepState.Running));
        return i;
    }

    public void End(int index, Tweak t, bool reverting, TweakResult result, bool skipped = false) =>
        report?.Invoke(new BatchStep(index, total, t.Id, t.Name, reverting,
            !result.Success ? BatchStepState.Failed : skipped ? BatchStepState.Skipped : BatchStepState.Done, result.Error));

    public void Skip(Tweak t, TweakResult result) => End(Begin(t, false), t, false, result, skipped: true);
}

public sealed record BatchItem(string TweakId, TweakResult Result, bool Skipped = false);

public sealed record BatchResult(Guid BatchId, IReadOnlyList<BatchItem> Items, string? Blocked = null)
{
    public bool AllSucceeded => Blocked is null && Items.All(i => i.Result.Success);
}

/// <summary>Runs pre-flight checks, makes one restore point, then applies each tweak.</summary>
public sealed class BatchRunner(TweakEngine engine, IRestorePointService restorePoints, Func<bool> isElevated)
{
    public TweakEngine Engine => engine;
    public bool IsElevated => isElevated();

    /// <summary>Applies the first list and reverts the second as one run, with a single restore point.</summary>
    public BatchResult Execute(IReadOnlyList<Tweak> toApply, IReadOnlyList<Tweak> toRevert, MachineProfile profile, BatchOptions options, Action<string>? log = null)
    {
        var counter = new StepCounter(toApply.Count + toRevert.Count, options.Step);
        var gate = new RestorePointGate(restorePoints, options, log);
        var applied = toApply.Count > 0 ? ApplyCore(toApply, profile, options, log, counter, gate) : new BatchResult(Guid.NewGuid(), []);
        if (applied.Blocked is not null || toRevert.Count == 0) return applied;

        // The dialog offers the restore point for the whole run, so a run that only reverts gets one too.
        var blocked = gate.Ensure();
        if (blocked is not null) return new BatchResult(applied.BatchId, applied.Items, blocked);

        var reverted = RevertCore(toRevert, log, counter);
        return new BatchResult(applied.BatchId, applied.Items.Concat(reverted.Items).ToList());
    }

    public BatchResult Apply(IReadOnlyList<Tweak> tweaks, MachineProfile profile, BatchOptions options, Action<string>? log = null) =>
        ApplyCore(tweaks, profile, options, log, new StepCounter(tweaks.Count, options.Step), new RestorePointGate(restorePoints, options, log));

    /// <summary>Makes the restore point once per run, the first time something is about to change, and remembers the answer.</summary>
    private sealed class RestorePointGate(IRestorePointService service, BatchOptions options, Action<string>? log)
    {
        private bool _done;
        private string? _blocked;

        /// <summary>Null when it is fine to go on, otherwise why nothing may change.</summary>
        public string? Ensure()
        {
            if (_done) return _blocked;
            _done = true;
            if (!options.CreateRestorePoint) return null;

            log?.Invoke("Creating restore point");
            var rp = service.CreateEnsuring($"Clarion {DateTime.Now:yyyy-MM-dd HH:mm}", options.TurnOnSystemProtection, log);
            if (rp.Success) return null;
            if (!options.ContinueWithoutRestorePoint)
                _blocked = $"No restore point, nothing was changed. {rp.Error}";
            else
                log?.Invoke($"Restore point failed, continuing by request: {rp.Error}");
            return _blocked;
        }
    }

    private BatchResult ApplyCore(IReadOnlyList<Tweak> tweaks, MachineProfile profile, BatchOptions options, Action<string>? log, StepCounter counter, RestorePointGate gate)
    {
        var batch = Guid.NewGuid();
        var items = new List<BatchItem>();
        var runnable = new List<Tweak>();
        var elevated = isElevated();

        foreach (var t in tweaks)
        {
            var skip = !profile.Supports(t.Requires) ? TweakResult.Fail("Not supported on this Windows version or edition.")
                : t.Scope == TweakScope.Machine && !elevated ? TweakResult.Fail("Needs administrator rights.")
                : engine.Detect(t) == TweakState.Applied ? TweakResult.Ok()
                : null;
            if (skip is null) runnable.Add(t);
            else
            {
                items.Add(new BatchItem(t.Id, skip, Skipped: true));
                counter.Skip(t, skip);
            }
        }

        if (runnable.Count == 0) return new BatchResult(batch, items);

        var blocked = gate.Ensure();
        if (blocked is not null) return new BatchResult(batch, items, blocked);

        foreach (var t in runnable)
        {
            log?.Invoke($"Applying {t.Name}");
            var index = counter.Begin(t, false);
            var result = engine.Apply(t, batch);
            counter.End(index, t, false, result);
            items.Add(new BatchItem(t.Id, result));
        }
        return new BatchResult(batch, items);
    }

    public BatchResult Revert(IReadOnlyList<Tweak> tweaks, Action<string>? log = null, Action<BatchStep>? step = null) =>
        RevertCore(tweaks, log, new StepCounter(tweaks.Count, step));

    private BatchResult RevertCore(IReadOnlyList<Tweak> tweaks, Action<string>? log, StepCounter counter)
    {
        var batch = Guid.NewGuid();
        var items = new List<BatchItem>();
        foreach (var t in tweaks)
        {
            log?.Invoke($"Reverting {t.Name}");
            var index = counter.Begin(t, true);
            var result = engine.Revert(t, batch);
            counter.End(index, t, true, result);
            items.Add(new BatchItem(t.Id, result));
        }
        return new BatchResult(batch, items);
    }
}
