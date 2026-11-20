using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Core.Reports;

/// <summary>
/// What Revert is about to do for one setting, in the same words the review uses for a change. It follows <c>TweakEngine.Revert</c>: when the
/// history holds what .Clarion changed, the recorded undo steps run newest first, and when it holds nothing the setting's own default undo runs.
/// A recorded step that another setting still on also made is left in place, and the preview says so.
/// </summary>
public static class RevertPreview
{
    public sealed record Result(IReadOnlyList<string> Lines, bool FromHistory);

    public static Result For(IReadOnlyList<JournalEntry> outstanding, IReadOnlyList<Operation> defaultUndo, IReadOnlySet<Operation>? heldByOthers = null)
    {
        if (outstanding.Count == 0) return new Result(defaultUndo.Select(o => o.Describe()).ToList(), false);
        var lines = outstanding.Reverse()
            .Select(e => heldByOthers?.Contains(e.Operation) == true
                ? $"Left as it is, another setting that is on needs it: {e.Operation.Describe()}"
                : e.Undo.Describe())
            .ToList();
        return new Result(lines, true);
    }
}
