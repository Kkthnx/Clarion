using Clarion.Core.Catalog;
using Clarion.Core.Drift;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Profiles;
using Clarion.Core.Reports;
using Clarion.Core.SystemInfo;
using Clarion.Core.Troubleshoot;

namespace Clarion.Tests;

public sealed class ThirdPassTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-third-" + Guid.NewGuid().ToString("N"));
    private readonly FakeRegistry _reg = new();
    private readonly ChangeJournal _journal;
    private readonly TweakEngine _engine;
    private readonly DriftScanner _scanner;
    private long _tick;
    private static readonly MachineProfile Pro = new(26100, "Professional");

    public ThirdPassTests()
    {
        _journal = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"));
        _engine = new TweakEngine([new RegistryHandler(_reg)], _journal, new Step(() => ++_tick));
        _scanner = new DriftScanner(_engine, _journal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private sealed class Step(Func<long> next) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(next());
    }

    private static RegistryTarget T(string name) => new(RegistryHive.CurrentUser, "Software\\Test", name);

    private static Tweak Make(string id, string topic = "Search") => new()
    {
        Id = id, Category = "Test", Topic = topic, Name = id, Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe,
        Apply = [new SetRegistryValue(T(id), new RegistryData(RegistryKind.DWord, "0"))],
    };

    private void WindowsSets(string name) => _reg.Write(T(name), new RegistryData(RegistryKind.DWord, "1"));

    // ---- "that was me" and watching again ----

    [Fact]
    public void A_setting_the_person_stopped_checking_is_listed_and_can_be_watched_again()
    {
        var tweak = Make("a");
        _engine.Apply(tweak, Guid.NewGuid());
        WindowsSets("a");
        _engine.Release(tweak, Guid.NewGuid());

        Assert.Contains("a", _journal.ReleasedByTweak().Keys);
        Assert.False(_scanner.Scan([tweak], Pro).HasDrift);

        Assert.Equal(1, _engine.Resume(tweak, Guid.NewGuid()));

        Assert.DoesNotContain("a", _journal.ReleasedByTweak().Keys);
        Assert.True(_scanner.Scan([tweak], Pro).HasDrift);
    }

    [Fact]
    public void Watching_again_changes_nothing_in_windows()
    {
        var tweak = Make("a");
        _engine.Apply(tweak, Guid.NewGuid());
        WindowsSets("a");
        _engine.Release(tweak, Guid.NewGuid());

        _engine.Resume(tweak, Guid.NewGuid());

        Assert.Equal("1", _reg.Read(T("a")).Data!.Value);
    }

    [Fact]
    public void A_setting_that_was_never_released_cannot_be_resumed()
    {
        var tweak = Make("a");
        _engine.Apply(tweak, Guid.NewGuid());

        Assert.Equal(0, _engine.Resume(tweak, Guid.NewGuid()));
        Assert.Empty(_journal.ReleasedByTweak());
    }

    [Fact]
    public void A_released_setting_that_is_applied_or_reverted_afterwards_is_no_longer_in_the_released_list()
    {
        var tweak = Make("a");
        _engine.Apply(tweak, Guid.NewGuid());
        _engine.Release(tweak, Guid.NewGuid());
        WindowsSets("a");
        _engine.Apply(tweak, Guid.NewGuid());

        Assert.Empty(_journal.ReleasedByTweak());
        Assert.Contains("a", _journal.TweakIdsWithOutstanding());
    }

    // ---- progress during a scan ----

    [Fact]
    public void A_scan_reports_progress_from_nothing_done_to_everything_done()
    {
        var tweaks = new[] { Make("a"), Make("b"), Make("c") };
        foreach (var t in tweaks) _engine.Apply(t, Guid.NewGuid());
        var seen = new List<(int Done, int Total)>();

        _scanner.Scan(tweaks, Pro, progress: new SyncProgress<(int, int)>(p => seen.Add(p)));

        Assert.Equal((0, 3), seen[0]);
        Assert.Equal((3, 3), seen[^1]);
        Assert.Equal(seen.Select(s => s.Done).Order(), seen.Select(s => s.Done));
    }

    private sealed class SyncProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    // ---- "it worked until roughly" ----

    [Fact]
    public void Changes_made_before_it_last_worked_are_left_out_of_the_suspects()
    {
        var old = Make("old");
        var recent = Make("recent");
        _engine.Apply(old, Guid.NewGuid());      // day 1
        _engine.Apply(recent, Guid.NewGuid());   // day 2
        var symptom = new Symptom("s", "t", "tip", ["Search"], []);
        var shooter = new Troubleshooter(_journal);
        var workedUntil = new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero); // the clock gives the first change day 2 and the second day 3

        var all = shooter.Suspects(symptom, [old, recent]);
        var filtered = shooter.Suspects(symptom, [old, recent], workedUntil);

        Assert.Equal(2, all.Count);
        Assert.Equal("recent", Assert.Single(filtered).Tweak.Id);
    }

    [Fact]
    public void A_date_after_every_change_leaves_no_suspects()
    {
        var tweak = Make("a");
        _engine.Apply(tweak, Guid.NewGuid());
        var symptom = new Symptom("s", "t", "tip", ["Search"], []);

        Assert.Empty(new Troubleshooter(_journal).Suspects(symptom, [tweak], DateTimeOffset.UtcNow.AddYears(5)));
    }

    // ---- pending restart ----

    private sealed class Probe(bool update, bool servicing, bool installer) : IPendingRestartProbe
    {
        public bool UpdateRebootRequired() => update;
        public bool ServicingRebootPending() => servicing;
        public bool UpdateInstallerActive() => installer;
    }

    [Fact]
    public void Nothing_waiting_means_no_notice()
    {
        var info = PendingRestart.Check(new Probe(false, false, false));
        Assert.False(info.IsPending);
        Assert.Empty(info.Reasons);
    }

    [Theory]
    [InlineData(true, false, false, "Windows Update")]
    [InlineData(false, true, false, "servicing")]
    [InlineData(false, false, true, "installer")]
    public void Each_marker_gives_its_own_plain_reason(bool update, bool servicing, bool installer, string expected)
    {
        var info = PendingRestart.Check(new Probe(update, servicing, installer));

        Assert.True(info.IsPending);
        Assert.Contains(expected, Assert.Single(info.Reasons));
    }

    private sealed class DeniedProbe : IPendingRestartProbe
    {
        public bool UpdateRebootRequired() => throw new UnauthorizedAccessException();
        public bool ServicingRebootPending() => throw new System.Security.SecurityException();
        public bool UpdateInstallerActive() => false;
    }

    [Fact]
    public void A_marker_that_cannot_be_read_is_treated_as_not_set()
    {
        Assert.False(PendingRestart.Check(new DeniedProbe()).IsPending);
    }

    // ---- command line ----

    [Fact]
    public void The_read_only_commands_are_parsed()
    {
        Assert.Equal(CliMode.Verify, CliOptions.Parse(["--verify"]).Mode);
        Assert.Equal(CliMode.History, CliOptions.Parse(["--history"]).Mode);
        Assert.Equal(CliMode.ListSymptoms, CliOptions.Parse(["--list-symptoms"]).Mode);

        var w = CliOptions.Parse(["--what-broke", "search-broken", "--since", "2026-09-01"]);
        Assert.Equal(CliMode.WhatBroke, w.Mode);
        Assert.Equal("search-broken", w.Symptom);
        Assert.Equal(new DateTime(2026, 9, 1), w.Since!.Value.DateTime);
    }

    [Theory]
    [InlineData("--what-broke")]
    [InlineData("--what-broke", "--verify")]
    public void What_broke_needs_a_symptom_name(params string[] args) =>
        Assert.NotNull(CliOptions.Parse(args).Error);

    [Fact]
    public void A_bad_date_is_refused_with_a_clear_message()
    {
        var o = CliOptions.Parse(["--what-broke", "x", "--since", "last tuesday"]);
        Assert.Contains("--since", o.Error);
    }

    [Fact]
    public void The_help_text_lists_the_read_only_commands()
    {
        foreach (var word in new[] { "--verify", "--what-broke", "--since", "--list-symptoms", "--history" })
            Assert.Contains(word, CliOptions.HelpText);
    }

    // ---- command line text ----

    [Fact]
    public void The_verify_text_names_what_changed_back_and_exits_with_one()
    {
        var tweak = Make("a");
        _engine.Apply(tweak, Guid.NewGuid());
        WindowsSets("a");
        var report = _scanner.Scan([tweak], Pro);

        var text = CliReports.Verify(report, new PendingRestartInfo(["Windows Update is waiting for a restart"]));

        Assert.Contains("CHANGED BACK", text);
        Assert.Contains("(a)", text);
        Assert.Contains("waiting for a restart", text);
        Assert.Equal(1, CliReports.VerifyExitCode(report));
    }

    [Fact]
    public void The_verify_text_says_everything_holds_and_exits_with_zero()
    {
        var tweak = Make("a");
        _engine.Apply(tweak, Guid.NewGuid());
        var report = _scanner.Scan([tweak], Pro);

        Assert.Contains("still in place", CliReports.Verify(report));
        Assert.Equal(0, CliReports.VerifyExitCode(report));
    }

    [Fact]
    public void An_unreadable_setting_is_not_reported_as_all_clear()
    {
        var report = new DriftReport(DateTimeOffset.UtcNow, 26100, null, 1, [], ["x"]);

        Assert.Equal(1, CliReports.VerifyExitCode(report));
        Assert.DoesNotContain("still in place", CliReports.Verify(report));
    }

    [Fact]
    public void The_what_broke_text_lists_suspects_or_names_the_usual_other_causes()
    {
        var symptom = new Symptom("s", "Search is broken", "A tip", ["Search"], []);
        var tweak = Make("a");
        _engine.Apply(tweak, Guid.NewGuid());
        var shooter = new Troubleshooter(_journal);

        Assert.Contains("(a)", CliReports.WhatBroke(symptom, shooter.Suspects(symptom, [tweak])));

        var none = CliReports.WhatBroke(symptom, [], new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero));
        Assert.Contains("customized Windows image", none);
        Assert.Contains("another cleanup tool", none);
        Assert.Contains("2026-09-01", none);
    }

    [Fact]
    public void The_history_text_lists_what_is_still_on_newest_first()
    {
        var a = Make("a"); var b = Make("b");
        _engine.Apply(a, Guid.NewGuid());
        _engine.Apply(b, Guid.NewGuid());

        var lines = CliReports.History(_journal.OutstandingByTweak(), [a, b]).Split(Environment.NewLine);

        Assert.Contains("(b)", lines[0]);
        Assert.Contains("(a)", lines[1]);
        Assert.Equal("Clarion has not changed anything that is still on.", CliReports.History(new Dictionary<string, IReadOnlyList<JournalEntry>>(), []));
    }

    [Fact]
    public void Every_symptom_in_the_list_can_be_named_on_the_command_line()
    {
        var ids = CatalogLoader.LoadSymptoms().Select(s => s.Id).ToList();
        Assert.All(ids, id => Assert.Matches("^[a-z0-9-]+$", id));
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
