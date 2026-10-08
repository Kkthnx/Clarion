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
/// Applies and reverts tweaks. Every change reads the prior state first, writes,
/// reads back to verify, and records the result in the journal.
/// </summary>
public sealed class TweakEngine(IRegistryStore registry, ChangeJournal journal, TimeProvider? clock = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;

    public IReadOnlyList<string> Plan(Tweak tweak) => tweak.Apply.Select(o => o.Describe()).ToList();

    public TweakState Detect(Tweak tweak)
    {
        var matched = 0;
        foreach (var op in tweak.Apply)
        {
            if (Satisfied(op)) matched++;
        }
        if (matched == tweak.Apply.Count) return TweakState.Applied;
        return matched == 0 ? TweakState.NotApplied : TweakState.Partial;
    }

    public TweakResult Apply(Tweak tweak, Guid batchId)
    {
        if (Detect(tweak) == TweakState.Applied) return TweakResult.Ok();

        var done = new List<JournalEntry>();
        foreach (var op in tweak.Apply)
        {
            var target = TargetOf(op);
            RegistrySnapshot prior;
            try
            {
                prior = registry.Read(target);
                Execute(op);
                var after = registry.Read(target);
                if (!Satisfied(op, after))
                {
                    Rollback(tweak.Id, batchId, done, target, prior);
                    return TweakResult.Fail($"Verification failed for {target}");
                }
                var entry = new JournalEntry(batchId, tweak.Id, JournalAction.Apply, _clock.GetUtcNow(), target, prior, after);
                journal.Append(entry);
                done.Add(entry);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException or InvalidOperationException)
            {
                Rollback(tweak.Id, batchId, done, null, null);
                return TweakResult.Fail($"{target}: {ex.Message}");
            }
        }
        return TweakResult.Ok();
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
                    RestoreAndRecord(tweak.Id, batchId, entry.Target, entry.Prior);
                }
                return TweakResult.Ok();
            }

            if (tweak.Undo.Count == 0) return TweakResult.Fail("Nothing recorded to revert and no default is defined.");

            foreach (var op in tweak.Undo)
            {
                var target = TargetOf(op);
                var prior = registry.Read(target);
                Execute(op);
                var after = registry.Read(target);
                journal.Append(new JournalEntry(batchId, tweak.Id, JournalAction.Revert, _clock.GetUtcNow(), target, prior, after));
            }
            return TweakResult.Ok();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException or InvalidOperationException)
        {
            return TweakResult.Fail(ex.Message);
        }
    }

    private void Rollback(string tweakId, Guid batchId, List<JournalEntry> done, RegistryTarget? failed, RegistrySnapshot? failedPrior)
    {
        if (failed is not null && failedPrior is not null)
        {
            Restore(failed, failedPrior);
        }
        foreach (var entry in Enumerable.Reverse(done))
        {
            RestoreAndRecord(tweakId, batchId, entry.Target, entry.Prior);
        }
    }

    private void RestoreAndRecord(string tweakId, Guid batchId, RegistryTarget target, RegistrySnapshot to)
    {
        var before = registry.Read(target);
        Restore(target, to);
        var after = registry.Read(target);
        journal.Append(new JournalEntry(batchId, tweakId, JournalAction.Revert, _clock.GetUtcNow(), target, before, after));
    }

    private void Restore(RegistryTarget target, RegistrySnapshot to)
    {
        if (to.Exists && to.Data is not null)
        {
            registry.Write(target, to.Data);
            return;
        }
        registry.DeleteValue(target);
        registry.PruneEmptyKeys(target, to.DeepestExistingKey);
    }

    private void Execute(Operation op)
    {
        switch (op)
        {
            case SetRegistryValue set:
                registry.Write(set.Target, set.Data);
                break;
            case DeleteRegistryValue del:
                registry.DeleteValue(del.Target);
                break;
            default:
                throw new InvalidOperationException($"Unsupported operation {op.GetType().Name}");
        }
    }

    private bool Satisfied(Operation op) => Satisfied(op, registry.Read(TargetOf(op)));

    private static bool Satisfied(Operation op, RegistrySnapshot now) => op switch
    {
        SetRegistryValue set => now.Exists && now.Data is not null && SameData(now.Data, set.Data),
        DeleteRegistryValue => !now.Exists,
        _ => false,
    };

    private static bool SameData(RegistryData a, RegistryData b) =>
        a.Kind == b.Kind && string.Equals(a.Value, b.Value, StringComparison.OrdinalIgnoreCase);

    private static RegistryTarget TargetOf(Operation op) => op switch
    {
        SetRegistryValue set => set.Target,
        DeleteRegistryValue del => del.Target,
        _ => throw new InvalidOperationException($"Unsupported operation {op.GetType().Name}"),
    };
}
