using Clarion.Core.Abstractions;
using Clarion.Core.Model;
using Clarion.Core.Nvidia;

namespace Clarion.Core.Engine;

/// <summary>Sets values in the NVIDIA driver's global profile and remembers what was there, including that nothing was stored.</summary>
public sealed class NvidiaHandler(INvidiaDriverStore store) : IOperationHandler
{
    public bool Handles(Operation op) => op is SetNvidiaSetting or ResetNvidiaSetting;

    // Without an NVIDIA card and driver there is nothing to set, so the step is left out and the setting reads as not available.
    public bool IsApplicable(Operation op) => store.IsAvailable();

    public bool IsSatisfied(Operation op) => op switch
    {
        SetNvidiaSetting s => store.Read(s.Id) == s.Value,
        ResetNvidiaSetting r => store.Read(r.Id) is null,
        _ => false,
    };

    public Operation CaptureUndo(Operation op)
    {
        var id = op switch
        {
            SetNvidiaSetting s => s.Id,
            ResetNvidiaSetting r => r.Id,
            _ => throw new InvalidOperationException("Not an NVIDIA operation"),
        };
        return store.Read(id) is { } prior ? new SetNvidiaSetting(id, prior) : new ResetNvidiaSetting(id);
    }

    public void Execute(Operation op)
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

    // A recorded step is read back from the history file, so the setting it names is checked again before it is written. The value is
    // not: putting back what was there before may be a number the catalog never offered, and that is exactly what an undo must do.
    private static void CheckListed(uint id)
    {
        if (NvidiaSettings.Find(id) is null) throw new InvalidOperationException($"NVIDIA driver setting 0x{id:X8} is not one .Clarion changes.");
    }
}
