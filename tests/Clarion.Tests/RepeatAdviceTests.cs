using Clarion.Core.Drift;

namespace Clarion.Tests;

public sealed class RepeatAdviceTests : IDisposable
{
    private static readonly DateTimeOffset Day1 = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-advice-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch (IOException) { }
    }

    private DriftHistory History() => new(Path.Combine(_dir, "drift-history.jsonl"));

    [Fact]
    public void A_setting_changed_back_once_gets_no_advice()
    {
        Assert.Null(RepeatAdvice.For([26100]));
        Assert.Null(RepeatAdvice.For([]));
    }

    [Fact]
    public void Repeats_on_one_windows_version_point_away_from_updates()
    {
        var advice = RepeatAdvice.For([26100, 26100, 26100]);

        Assert.NotNull(advice);
        Assert.Contains("same Windows version", advice);
        Assert.Contains("did not do it", advice);
    }

    [Fact]
    public void Repeats_after_each_version_change_point_to_updates()
    {
        var advice = RepeatAdvice.For([26100, 26200, 26300]);

        Assert.NotNull(advice);
        Assert.Contains("newer version", advice);
    }

    [Fact]
    public void A_mix_of_both_says_more_than_one_thing_is_at_work()
    {
        var advice = RepeatAdvice.For([26100, 26100, 26200]);

        Assert.NotNull(advice);
        Assert.Contains("more than one thing", advice);
    }

    [Fact]
    public void The_history_records_the_build_at_each_separate_time_a_setting_was_changed_back()
    {
        var history = History();
        history.Record(Day1, 26100, ["a"]);                  // first time, build 26100
        history.Record(Day1.AddDays(1), 26100, ["a"]);       // same problem, not counted again
        history.Record(Day1.AddDays(2), 26100, []);          // put back
        history.Record(Day1.AddDays(3), 26200, ["a", "b"]);  // a again after an update, b for the first time

        var builds = history.BuildsWhenChangedBack();

        Assert.Equal([26100, 26200], builds["a"]);
        Assert.Equal([26200], builds["b"]);
        Assert.Equal(2, history.TimesChangedBack()["a"]);
    }

    [Fact]
    public void A_report_gives_advice_only_for_settings_that_repeat()
    {
        var report = new DriftReport(Day1, 26100, null, 2, [], [])
        {
            BuildsWhenChangedBack = new Dictionary<string, IReadOnlyList<int>>
            {
                ["repeats"] = [26100, 26100],
                ["once"] = [26100],
            },
        };

        Assert.NotNull(report.AdviceFor("repeats"));
        Assert.Null(report.AdviceFor("once"));
        Assert.Null(report.AdviceFor("never"));
    }
}
