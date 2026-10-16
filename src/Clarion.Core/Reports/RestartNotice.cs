namespace Clarion.Core.Reports;

/// <summary>The sentence on Home and Verify when Windows is waiting for a restart, so both say the same thing and neither says "the 0 settings".</summary>
public static class RestartNotice
{
    public static string Message(IEnumerable<string> reasons, int checkedCount, bool scanAgain = false)
    {
        var why = string.Join(". ", reasons) + ". ";
        if (checkedCount <= 0) return why + "Once you have applied settings, .Clarion checks them again after a restart like this.";
        var settings = checkedCount == 1 ? "1 setting" : $"{checkedCount} settings";
        return why + $"After it restarts, .Clarion will check the {settings} it applied again, because updates are when changes are most likely to be undone." + (scanAgain ? " Scan again then." : "");
    }
}
