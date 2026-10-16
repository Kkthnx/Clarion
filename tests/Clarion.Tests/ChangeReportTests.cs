using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Reports;

namespace Clarion.Tests;

public sealed class ChangeReportTests
{

    private static JournalEntry Entry(string id, DateTimeOffset when, string name)
    {
        var set = new SetRegistryValue(new RegistryTarget(RegistryHive.LocalMachine, "Software\\Test", name), new RegistryData(RegistryKind.DWord, "1"));
        var back = new SetRegistryValue(new RegistryTarget(RegistryHive.LocalMachine, "Software\\Test", name), new RegistryData(RegistryKind.DWord, "0"));
        return new JournalEntry(Guid.NewGuid(), id, JournalAction.Apply, when, set, back);
    }

    [Fact]
    public void The_report_lists_newest_first_with_what_was_set_and_what_revert_puts_back()
    {
        var older = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
        var newer = older.AddDays(5);
        var outstanding = new Dictionary<string, IReadOnlyList<JournalEntry>>
        {
            ["a"] = [Entry("a", older, "A")],
            ["b"] = [Entry("b", newer, "B")],
        };
        var names = new Dictionary<string, string> { ["a"] = "First setting", ["b"] = "Second setting" };

        var text = ChangeReport.Build(outstanding, names, newer, "1.2.3");

        Assert.Contains("version 1.2.3", text);
        Assert.Contains("2 settings changed", text);
        Assert.True(text.IndexOf("Second setting (b)", StringComparison.Ordinal) < text.IndexOf("First setting (a)", StringComparison.Ordinal));
        Assert.Contains("Set:      Set HKLM\\Software\\Test\\B to 1", text);
        Assert.Contains("Revert:   Set HKLM\\Software\\Test\\B to 0", text);
    }

    [Fact]
    public void A_setting_missing_from_the_catalog_is_listed_by_its_id()
    {
        var outstanding = new Dictionary<string, IReadOnlyList<JournalEntry>> { ["gone"] = [Entry("gone", DateTimeOffset.Now, "G")] };
        Assert.Contains("gone (gone)", ChangeReport.Build(outstanding, new Dictionary<string, string>(), DateTimeOffset.Now, "1.0.0"));
    }

    [Fact]
    public void An_empty_history_says_so_plainly()
    {
        var text = ChangeReport.Build(new Dictionary<string, IReadOnlyList<JournalEntry>>(), new Dictionary<string, string>(), DateTimeOffset.Now, "1.0.0");
        Assert.Contains("has not changed anything that is still on", text);
    }
}
