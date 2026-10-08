using Clarion.Core.Model;

namespace Clarion.Core.Abstractions;

/// <summary>Knows how to inspect, run and undo one family of operations.</summary>
public interface IOperationHandler
{
    bool Handles(Operation op);

    /// <summary>False when the target does not exist on this machine, so the step is skipped.</summary>
    bool IsApplicable(Operation op);

    bool IsSatisfied(Operation op);

    /// <summary>Builds the operation that puts the current state back.</summary>
    Operation CaptureUndo(Operation op);

    void Execute(Operation op);
}
