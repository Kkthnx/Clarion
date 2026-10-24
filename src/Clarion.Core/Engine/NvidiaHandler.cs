using Clarion.Core.Abstractions;
using Clarion.Core.Model;
using Clarion.Core.Nvidia;

namespace Clarion.Core.Engine;

/// <summary>Sets values in the NVIDIA driver's global profile and remembers what was there, including that nothing was stored.</summary>
public sealed class NvidiaHandler(INvidiaDriverStore store) : IOperationHandler
{
    private readonly object _gate = new();
    private IReadOnlyDictionary<uint, uint?>? _values;

    public bool Handles(Operation op) => op is SetNvidiaSetting or ResetNvidiaSetting;

    // Without an NVIDIA card and driver there is nothing to set, so the step is left out and the setting reads as not available.
    public bool IsApplicable(Operation op) => store.IsAvailable();

    // Opening the driver's settings takes about a tenth of a second, so the settings the catalog reads are read in one go and kept until
    // something is written or Invalidate is called. A value that was not part of that read is asked for on its own.
    public void Warm(IReadOnlyList<Operation> operations)
    {
        var ids = operations.Select(IdOf).OfType<uint>().Distinct().ToList();
        if (ids.Count == 0 || !store.IsAvailable()) return;
        var read = store.ReadMany(ids);
        lock (_gate) _values = read;
    }

    public void Invalidate()
    {
        lock (_gate) _values = null;
    }

    private uint? Stored(uint id)
    {
        lock (_gate)
        {
            if (_values is not null && _values.TryGetValue(id, out var cached)) return cached;
        }
        return store.Read(id);
    }

    private static uint? IdOf(Operation op) => op switch
    {
        SetNvidiaSetting s => s.Id,
        ResetNvidiaSetting r => r.Id,
        _ => null,
    };

    public bool IsSatisfied(Operation op) => op switch
    {
        SetNvidiaSetting s => Stored(s.Id) == s.Value,
        ResetNvidiaSetting r => Stored(r.Id) is null,
        _ => false,
    };

    // What an undo records is read from the driver itself, not from what was kept, so a change made in NVIDIA's own app since the last read
    // is never overwritten by an older value.
    public Operation CaptureUndo(Operation op)
    {
        var id = IdOf(op) ?? throw new InvalidOperationException("Not an NVIDIA operation");
        return store.Read(id) is { } prior ? new SetNvidiaSetting(id, prior) : new ResetNvidiaSetting(id);
    }

    public void Execute(Operation op)
    {
        try
        {
            switch (op)
            {
                case SetNvidiaSetting s:
                    CheckListed(s.Id);
                    store.Write(s.Id, s.Value);
                    break;
                case ResetNvidiaSetting r:
                    CheckListed(r.Id);
                    store.Reset(r.Id);
                    break;
            }
        }
        finally { Invalidate(); }
    }

    // A recorded step is read back from the history file, so the setting it names is checked again before it is written. The value is
    // not: putting back what was there before may be a number the catalog never offered, and that is exactly what an undo must do.
    private static void CheckListed(uint id)
    {
        if (NvidiaSettings.Find(id) is null) throw new InvalidOperationException($"NVIDIA driver setting 0x{id:X8} is not one .Clarion changes.");
    }
}
