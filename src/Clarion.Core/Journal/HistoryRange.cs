namespace Clarion.Core.Journal;

/// <summary>Picks settings out of the history by when they were applied.</summary>
public static class HistoryRange
{
    /// <summary>
    /// The settings whose latest apply came after the end of the chosen day, in local time, so "go back to how it was on Oct 3" keeps
    /// everything done on or before Oct 3 and queues what came later. A setting applied again later counts by its latest apply, because
    /// that is the one whose recorded undo Revert uses.
    /// </summary>
    public static IReadOnlyList<string> AppliedAfter(IEnumerable<(string Id, DateTimeOffset At)> applied, DateTime day)
    {
        var from = day.Date.AddDays(1);
        return applied.Where(a => a.At.LocalDateTime >= from).OrderBy(a => a.At).Select(a => a.Id).ToList();
    }
}
