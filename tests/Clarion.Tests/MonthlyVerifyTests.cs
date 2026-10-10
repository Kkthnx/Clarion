using System.Xml.Linq;
using Clarion.Core.Scheduling;

namespace Clarion.Tests;

public sealed class FakeTaskStore : IScheduledTaskStore
{
    public Dictionary<string, string> Tasks { get; } = new(StringComparer.OrdinalIgnoreCase);
    public bool Exists(string name) => Tasks.ContainsKey(name);
    public void Create(string name, string xml) => Tasks[name] = xml;
    public void Delete(string name) => Tasks.Remove(name);
    public string? ReadXml(string name) => Tasks.TryGetValue(name, out var xml) ? xml : null;
}

public sealed class MonthlyTaskLocationTests
{
    private const string Exe = @"C:\Program Files\Clarion\Clarion.exe";

    [Fact]
    public void The_program_the_task_starts_is_read_back_from_its_definition()
    {
        var store = new FakeTaskStore();
        var schedule = new MonthlyVerifySchedule(store);
        schedule.Enable(Exe, @"PC\me", new DateTime(2026, 10, 14));

        Assert.Equal(Exe, schedule.CurrentCommand());
        Assert.False(schedule.PointsElsewhere(Exe));
        Assert.False(schedule.PointsElsewhere(@"c:\program files\clarion\CLARION.EXE"));
    }

    [Fact]
    public void A_task_made_from_another_folder_is_noticed()
    {
        var store = new FakeTaskStore();
        var schedule = new MonthlyVerifySchedule(store);
        schedule.Enable(@"D:\Old place\Clarion.exe", @"PC\me", new DateTime(2026, 10, 14));

        Assert.True(schedule.PointsElsewhere(Exe));
    }

    [Fact]
    public void No_task_or_an_unreadable_one_is_not_reported_as_moved()
    {
        var store = new FakeTaskStore();
        var schedule = new MonthlyVerifySchedule(store);
        Assert.False(schedule.PointsElsewhere(Exe));

        store.Tasks[MonthlyVerifySchedule.TaskName] = "not xml at all";
        Assert.False(schedule.PointsElsewhere(Exe));
    }

    [Fact]
    public void Paths_with_letters_outside_ascii_are_not_compared_so_a_console_code_page_cannot_cause_a_false_alarm()
    {
        var store = new FakeTaskStore();
        var schedule = new MonthlyVerifySchedule(store);
        schedule.Enable(@"C:\Users\José\Clarion\Clarion.exe", @"PC\José", new DateTime(2026, 10, 14));

        Assert.False(schedule.PointsElsewhere(@"C:\Users\Jos?\Clarion\Clarion.exe"));
    }

    [Fact]
    public void Enabling_again_from_this_copy_repairs_the_task()
    {
        var store = new FakeTaskStore();
        var schedule = new MonthlyVerifySchedule(store);
        schedule.Enable(@"D:\Old place\Clarion.exe", @"PC\me", new DateTime(2026, 10, 14));
        schedule.Enable(Exe, @"PC\me", new DateTime(2026, 10, 14));

        Assert.False(schedule.PointsElsewhere(Exe));
        Assert.Single(store.Tasks);
    }
}

public sealed class MonthlyVerifyTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-monthly-" + Guid.NewGuid().ToString("N"));
    private static readonly DateTime Day = new(2026, 10, 14);

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static XDocument Parse(string xml) => XDocument.Parse(xml.Replace("encoding=\"UTF-16\"", "encoding=\"utf-8\""));

    private static IEnumerable<XElement> All(XDocument d, string name) => d.Descendants().Where(e => e.Name.LocalName == name);

    [Fact]
    public void The_task_runs_on_the_second_wednesday_of_every_month()
    {
        var doc = Parse(MonthlyVerifySchedule.BuildXml(@"C:\Program Files\Clarion\Clarion.exe", @"PC\me", Day));

        Assert.Equal("2", All(doc, "Week").Single().Value);
        Assert.Single(All(doc, "Wednesday"));
        Assert.Equal(12, All(doc, "Months").Single().Elements().Count());
        Assert.Equal("2026-10-14T09:00:00", All(doc, "StartBoundary").Single().Value);
    }

    [Fact]
    public void The_task_runs_the_read_only_check_and_saves_the_result()
    {
        var doc = Parse(MonthlyVerifySchedule.BuildXml(@"C:\Program Files\Clarion\Clarion.exe", @"PC\me", Day));

        Assert.Equal(@"C:\Program Files\Clarion\Clarion.exe", All(doc, "Command").Single().Value);
        Assert.Equal("--verify --save", All(doc, "Arguments").Single().Value);
    }

    [Fact]
    public void The_task_runs_as_the_person_who_turned_it_on_without_asking_again_and_catches_up_after_the_pc_was_off()
    {
        var doc = Parse(MonthlyVerifySchedule.BuildXml(@"C:\x\Clarion.exe", @"PC\me", Day));

        Assert.Equal(@"PC\me", All(doc, "UserId").Single().Value);
        Assert.Equal("InteractiveToken", All(doc, "LogonType").Single().Value);
        Assert.Equal("HighestAvailable", All(doc, "RunLevel").Single().Value);
        Assert.Equal("true", All(doc, "StartWhenAvailable").Single().Value);
    }

    [Fact]
    public void A_path_or_user_with_special_characters_is_escaped_and_cannot_change_the_task()
    {
        var xml = MonthlyVerifySchedule.BuildXml(@"C:\Tom & Jerry\<Clarion>.exe", @"PC\a<b>&c", Day);

        var doc = Parse(xml);   // would throw if the text broke the document

        Assert.Equal(@"C:\Tom & Jerry\<Clarion>.exe", All(doc, "Command").Single().Value);
        Assert.Equal(@"PC\a<b>&c", All(doc, "UserId").Single().Value);
        Assert.Single(All(doc, "Exec"));
    }

    [Fact]
    public void Turning_it_on_creates_one_task_and_turning_it_on_again_replaces_it()
    {
        var store = new FakeTaskStore();
        var schedule = new MonthlyVerifySchedule(store);
        Assert.False(schedule.IsEnabled());

        schedule.Enable(@"C:\a\Clarion.exe", @"PC\me", Day);
        schedule.Enable(@"C:\b\Clarion.exe", @"PC\me", Day);

        Assert.True(schedule.IsEnabled());
        var only = Assert.Single(store.Tasks);
        Assert.Equal(MonthlyVerifySchedule.TaskName, only.Key);
        Assert.Contains(@"C:\b\Clarion.exe", only.Value);
    }

    [Fact]
    public void Turning_it_off_removes_the_task_and_turning_it_off_when_there_is_none_is_fine()
    {
        var store = new FakeTaskStore();
        var schedule = new MonthlyVerifySchedule(store);
        schedule.Enable(@"C:\a\Clarion.exe", @"PC\me", Day);

        schedule.Disable();
        schedule.Disable();

        Assert.False(schedule.IsEnabled());
        Assert.Empty(store.Tasks);
    }

    // ---- the saved result ----

    [Fact]
    public void A_saved_result_comes_back_and_can_be_marked_as_seen()
    {
        var store = new ScheduledVerifyStore(Path.Combine(_dir, "scheduled-verify.json"));
        var when = new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);
        store.Save(new ScheduledVerifyResult(when, 26200, 50, ["a", "b"], ["app.x"], 1));

        var loaded = store.Load()!;
        Assert.Equal(3, loaded.Attention);
        Assert.Equal(when, loaded.At);
        Assert.False(loaded.Seen);

        store.MarkSeen();
        Assert.True(store.Load()!.Seen);
    }

    [Fact]
    public void No_result_or_a_damaged_one_loads_as_nothing()
    {
        var path = Path.Combine(_dir, "scheduled-verify.json");
        var store = new ScheduledVerifyStore(path);
        Assert.Null(store.Load());

        Directory.CreateDirectory(_dir);
        File.WriteAllText(path, "{ not json");
        Assert.Null(store.Load());
        store.MarkSeen();   // must not throw
    }
}
