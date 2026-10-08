using Clarion.Core.Model;

namespace Clarion.Core.Abstractions;

/// <summary>Knows how to inspect, run and undo one family of operations.</summary>
public interface IOperationHandler
{
    bool Handles(Operation op);

    /// <summary>False when the target does not exist on this machine, so the step is skipped.</summary>
    bool IsApplicable(Operation op);

    bool IsSatisfied(Operation op);

    /// <summary>
    /// True when the goal is met only because there is nothing to act on, such as removing an
    /// app that was never installed. Such steps do not count as something Clarion changed.
    /// </summary>
    bool IsVacuous(Operation op) => false;

    /// <summary>Loads anything slow up front for these operations, so reading state afterward is quick.</summary>
    void Warm(IReadOnlyList<Operation> operations) { }

    /// <summary>Forgets anything cached, so the next read reflects what Windows looks like now.</summary>
    void Invalidate() { }

    /// <summary>Builds the operation that puts the current state back.</summary>
    Operation CaptureUndo(Operation op);

    void Execute(Operation op);
}
