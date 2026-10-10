using Clarion.Core.Drift;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Settings;

namespace Clarion.Tests;

public sealed class FeedbackBeta8Tests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-fb8-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    [Fact]
    public void Starting_without_the_prompt_is_remembered_and_can_be_taken_back()
    {
        var pref = new LaunchPreference(Path.Combine(_dir, "sub", "start-limited.flag"));
        Assert.False(pref.StartLimited);

        Assert.True(pref.SetStartLimited(true));
        Assert.True(pref.StartLimited);
        Assert.True(new LaunchPreference(Path.Combine(_dir, "sub", "start-limited.flag")).StartLimited);

        Assert.True(pref.SetStartLimited(false));
        Assert.False(pref.StartLimited);
        Assert.True(pref.SetStartLimited(false));
    }

    [Fact]
    public void A_folder_that_cannot_be_written_says_so_instead_of_pretending()
    {
        // A file where the folder should be, so the flag can neither be made nor found.
        Directory.CreateDirectory(_dir);
        var blocker = Path.Combine(_dir, "blocker");
        File.WriteAllText(blocker, "x");

        var pref = new LaunchPreference(Path.Combine(blocker, "start-limited.flag"));

        Assert.False(pref.SetStartLimited(true));
        Assert.False(pref.StartLimited);
    }

    [Fact]
    public void A_setting_applied_and_then_reverted_is_not_queued_when_going_back_to_an_earlier_day()
    {
        var journal = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"), null);
        var op = new SetRegistryValue(new RegistryTarget(RegistryHive.LocalMachine, "Software\\Test", "A"), new RegistryData(RegistryKind.DWord, "1"));
        var kept = new SetRegistryValue(new RegistryTarget(RegistryHive.LocalMachine, "Software\\Test", "B"), new RegistryData(RegistryKind.DWord, "1"));
        var oct6 = new DateTimeOffset(2026, 10, 6, 12, 0, 0, TimeSpan.Zero);

        journal.Append(new JournalEntry(Guid.NewGuid(), "reverted-later", JournalAction.Apply, oct6, op, op, null));
        journal.Append(new JournalEntry(Guid.NewGuid(), "reverted-later", JournalAction.Revert, oct6.AddDays(1), op, op, null));
        journal.Append(new JournalEntry(Guid.NewGuid(), "still-on", JournalAction.Apply, oct6.AddHours(2), kept, kept, null));

        var outstanding = journal.OutstandingByTweak().Select(kv => (kv.Key, kv.Value[0].Time));
        var ids = HistoryRange.AppliedAfter(outstanding, new DateTime(2026, 10, 3));

        Assert.Equal(["still-on"], ids);
    }

    [Fact]
    public void An_app_restore_with_nothing_to_put_back_says_so_in_the_plan_and_does_nothing()
    {
        var nothing = new RestoreAppxPackage("Contoso.App", null);
        var something = new RestoreAppxPackage("Contoso.App", "Contoso.App_abc");

        Assert.Contains("nothing to put back", nothing.Describe());
        Assert.DoesNotContain("nothing to put back", something.Describe());

        var store = new FakeAppxStore();
        new AppxHandler(store).Execute(nothing);
        Assert.Empty(store.Calls);
    }

    private DriftHistory History() => new(Path.Combine(_dir, "history", "drift-history.jsonl"));

    private static readonly DateTimeOffset Day1 = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_setting_that_stays_changed_back_across_scans_counts_once_until_it_holds_and_is_undone_again()
    {
        var history = History();
        history.Record(Day1, 26100, ["a"]);                 // undone
        history.Record(Day1.AddDays(1), 26100, ["a"]);      // still undone, same problem
        history.Record(Day1.AddDays(2), 26100, []);         // put back, holding
        history.Record(Day1.AddDays(3), 26200, ["a", "b"]); // a is undone a second time, b for the first

        var counts = history.TimesChangedBack();

        Assert.Equal(2, counts["a"]);
        Assert.Equal(1, counts["b"]);
        Assert.Equal(2, counts.Count);
    }

    [Fact]
    public void With_no_history_nothing_is_counted_and_a_damaged_line_is_skipped()
    {
        Assert.Empty(History().TimesChangedBack());

        var history = History();
        history.Record(Day1, 26100, ["a"]);
        File.AppendAllText(Path.Combine(_dir, "history", "drift-history.jsonl"), "this is not json\n");
        history.Record(Day1.AddDays(1), 26100, []);
        history.Record(Day1.AddDays(2), 26100, ["a"]);

        Assert.Equal(2, history.TimesChangedBack()["a"]);
    }

    [Fact]
    public void The_history_file_does_not_grow_for_ever()
    {
        var history = History();
        for (var i = 0; i < 500; i++) history.Record(Day1.AddHours(i), 26100, i % 2 == 0 ? ["a"] : []);

        var lines = File.ReadAllLines(Path.Combine(_dir, "history", "drift-history.jsonl")).Length;

        Assert.InRange(lines, 200, 400);
        Assert.True(history.TimesChangedBack()["a"] > 50);
    }

    [Fact]
    public void A_history_folder_that_cannot_be_written_is_not_an_error()
    {
        Directory.CreateDirectory(_dir);
        var blocker = Path.Combine(_dir, "blocker");
        File.WriteAllText(blocker, "x");
        var history = new DriftHistory(Path.Combine(blocker, "drift-history.jsonl"));

        history.Record(Day1, 26100, ["a"]);

        Assert.Empty(history.TimesChangedBack());
    }
}
