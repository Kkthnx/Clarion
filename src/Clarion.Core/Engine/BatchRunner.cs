using Clarion.Core.Model;

namespace Clarion.Core.Engine;

public sealed record BatchOptions
{
    public bool CreateRestorePoint { get; init; } = true;

    /// <summary>When creating the restore point fails, continue only if the user accepted the risk.</summary>
    public bool ContinueWithoutRestorePoint { get; init; }
}

public sealed record BatchItem(string TweakId, TweakResult Result, bool Skipped = false);

public sealed record BatchResult(Guid BatchId, IReadOnlyList<BatchItem> Items, string? Blocked = null)
{
    public bool AllSucceeded => Blocked is null && Items.All(i => i.Result.Success);
}

/// <summary>Runs pre-flight checks, makes one restore point, then applies each tweak.</summary>
public sealed class BatchRunner(TweakEngine engine, IRestorePointService restorePoints, Func<bool> isElevated)
{
    public BatchResult Apply(IReadOnlyList<Tweak> tweaks, MachineProfile profile, BatchOptions options, Action<string>? log = null)
    {
        var batch = Guid.NewGuid();
        var items = new List<BatchItem>();
        var runnable = new List<Tweak>();
        var elevated = isElevated();

        foreach (var t in tweaks)
        {
            if (!profile.Supports(t.Requires))
                items.Add(new BatchItem(t.Id, TweakResult.Fail("Not supported on this Windows version or edition."), Skipped: true));
            else if (t.Scope == TweakScope.Machine && !elevated)
                items.Add(new BatchItem(t.Id, TweakResult.Fail("Needs administrator rights."), Skipped: true));
            else if (engine.Detect(t) == TweakState.Applied)
                items.Add(new BatchItem(t.Id, TweakResult.Ok(), Skipped: true));
            else
                runnable.Add(t);
        }

        if (runnable.Count == 0) return new BatchResult(batch, items);

        if (options.CreateRestorePoint)
        {
            log?.Invoke("Creating restore point");
            var rp = restorePoints.Create($"Clarion {DateTime.Now:yyyy-MM-dd HH:mm}");
            if (!rp.Success && !options.ContinueWithoutRestorePoint)
                return new BatchResult(batch, items, $"No restore point, nothing was changed. {rp.Error}");
            if (!rp.Success) log?.Invoke($"Restore point failed, continuing by request: {rp.Error}");
        }

        foreach (var t in runnable)
        {
            log?.Invoke($"Applying {t.Name}");
            items.Add(new BatchItem(t.Id, engine.Apply(t, batch)));
        }
        return new BatchResult(batch, items);
    }

    public BatchResult Revert(IReadOnlyList<Tweak> tweaks, Action<string>? log = null)
    {
        var batch = Guid.NewGuid();
        var items = new List<BatchItem>();
        foreach (var t in tweaks)
        {
            log?.Invoke($"Reverting {t.Name}");
            items.Add(new BatchItem(t.Id, engine.Revert(t, batch)));
        }
        return new BatchResult(batch, items);
    }
}
