using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Core.Reports;

/// <summary>
/// What Revert is about to do for one setting, in the same words the review uses for a change. It follows <c>TweakEngine.Revert</c>: when the
/// history holds what .Clarion changed, the recorded undo steps run newest first, and when it holds nothing the setting's own default undo runs.
/// </summary>
public static class RevertPreview
{
    public sealed record Result(IReadOnlyList<string> Lines, bool FromHistory);

    public static Result For(IReadOnlyList<JournalEntry> outstanding, IReadOnlyList<Operation> defaultUndo) =>
        outstanding.Count > 0
            ? new Result(outstanding.Reverse().Select(e => e.Undo.Describe()).ToList(), true)
            : new Result(defaultUndo.Select(o => o.Describe()).ToList(), false);
}
