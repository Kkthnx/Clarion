using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class TweakEngineTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-tests-" + Guid.NewGuid().ToString("N"));
    private readonly FakeRegistry _reg = new();
    private readonly FakeServices _svc = new();
    private readonly FakeTasks _tasks = new();
    private const string TaskPath = @"\A\B";
    private readonly ChangeJournal _journal;
    private readonly TweakEngine _engine;

    public TweakEngineTests()
    {
        _journal = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"));
        _engine = new TweakEngine([new RegistryHandler(_reg), new ServiceHandler(_svc), new TaskHandler(_tasks)], _journal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static RegistryTarget T(string name, string path = "Software\\Test\\A") => new(RegistryHive.CurrentUser, path, name);

    private static Tweak Make(params Operation[] ops) => new()
    {
        Id = "t.one", Category = "Test", Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe, Apply = ops,
    };

    private static SetRegistryValue Dword(RegistryTarget t, string v) => new(t, new RegistryData(RegistryKind.DWord, v));

    [Fact]
    public void Apply_then_revert_restores_prior_value()
    {
        _reg.Write(T("X"), new RegistryData(RegistryKind.DWord, "7"));
        var tweak = Make(Dword(T("X"), "0"));

        Assert.Equal(TweakState.NotApplied, _engine.Detect(tweak));
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Equal(TweakState.Applied, _engine.Detect(tweak));

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Equal("7", _reg.Read(T("X")).Data!.Value);
    }

    [Fact]
    public void Revert_removes_value_and_keys_the_tweak_created()
    {
        var target = T("", "Software\\Test\\New\\Deep");
        var tweak = Make(new SetRegistryValue(target, new RegistryData(RegistryKind.String, "")));

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(_reg.HasKey(RegistryHive.CurrentUser, "Software\\Test\\New\\Deep"));

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.False(_reg.Read(target).Exists);
        Assert.False(_reg.HasKey(RegistryHive.CurrentUser, "Software\\Test\\New"));
    }

    [Fact]
    public void The_record_of_a_step_is_written_before_the_step_is_run()
    {
        // If the process died during the write of B, B's record has to exist already. A change with no record could never be reverted.
        var recordsWhileWritingB = -1;
        _reg.FailWrite = t =>
        {
            if (t.Name == "B") recordsWhileWritingB = _journal.OutstandingFor("t.one").Count;
            return false;
        };

        Assert.True(_engine.Apply(Make(Dword(T("A"), "0"), Dword(T("B"), "0")), Guid.NewGuid()).Success);

        Assert.Equal(2, recordsWhileWritingB);
    }

    [Fact]
    public void A_step_that_fails_is_written_down_as_undone_and_leaves_nothing_listed_as_on()
    {
        _reg.Write(T("A"), new RegistryData(RegistryKind.DWord, "1"));
        _reg.FailWrite = t => t.Name == "B";

        var result = _engine.Apply(Make(Dword(T("A"), "0"), Dword(T("B"), "0")), Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("1", _reg.Read(T("A")).Data!.Value);
        Assert.Empty(_journal.OutstandingFor("t.one"));
    }

    [Fact]
    public void When_going_back_also_fails_the_message_says_the_setting_is_partly_applied_and_the_history_still_lists_it()
    {
        _reg.Write(T("A"), new RegistryData(RegistryKind.DWord, "1"));
        var writesToA = 0;
        // B cannot be written, and then the write that puts A back fails as well.
        _reg.FailWrite = t => t.Name == "B" || (t.Name == "A" && ++writesToA == 2);

        var result = _engine.Apply(Make(Dword(T("A"), "0"), Dword(T("B"), "0")), Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains("partly applied", result.Error);
        Assert.Contains("Going back also failed for 1 earlier step", result.Error);
        // A is still changed. The history lists the whole run as on, A and the step that failed (which may or may not have changed
        // anything), so Revert has something to work from and a half undone setting is not mistaken for a finished one.
        Assert.Equal("0", _reg.Read(T("A")).Data!.Value);
        Assert.Equal(2, _journal.OutstandingFor("t.one").Count);

        _reg.FailWrite = null;
        Assert.True(_engine.Revert(Make(Dword(T("A"), "0"), Dword(T("B"), "0")), Guid.NewGuid()).Success);
        Assert.Equal("1", _reg.Read(T("A")).Data!.Value);
        Assert.Empty(_journal.OutstandingFor("t.one"));
    }

    [Fact]
    public void Failed_step_rolls_back_earlier_steps()
    {
        _reg.Write(T("A"), new RegistryData(RegistryKind.DWord, "1"));
        _reg.FailWrite = t => t.Name == "B";
        var tweak = Make(Dword(T("A"), "0"), Dword(T("B"), "0"));

        var result = _engine.Apply(tweak, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("1", _reg.Read(T("A")).Data!.Value);
        Assert.False(_reg.Read(T("B")).Exists);
    }

    [Fact]
    public void Applying_twice_does_not_overwrite_the_recorded_prior_state()
    {
        _reg.Write(T("X"), new RegistryData(RegistryKind.DWord, "5"));
        var tweak = Make(Dword(T("X"), "0"));

        _engine.Apply(tweak, Guid.NewGuid());
        _engine.Apply(tweak, Guid.NewGuid());
        _engine.Revert(tweak, Guid.NewGuid());

        Assert.Equal("5", _reg.Read(T("X")).Data!.Value);
    }

    [Fact]
    public void Detect_reports_partial()
    {
        _reg.Write(T("A"), new RegistryData(RegistryKind.DWord, "0"));
        var tweak = Make(Dword(T("A"), "0"), Dword(T("B"), "0"));
        Assert.Equal(TweakState.Partial, _engine.Detect(tweak));
    }

    [Fact]
    public void Delete_operation_is_applied_and_reverted()
    {
        _reg.Write(T("X"), new RegistryData(RegistryKind.String, "keep"));
        var tweak = Make(new DeleteRegistryValue(T("X")));

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.False(_reg.Read(T("X")).Exists);
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Equal("keep", _reg.Read(T("X")).Data!.Value);
    }

    [Fact]
    public void Revert_with_no_history_uses_catalog_undo_or_fails()
    {
        var plain = Make(Dword(T("X"), "0"));
        Assert.False(_engine.Revert(plain, Guid.NewGuid()).Success);

        var withUndo = plain with { Undo = [Dword(T("X"), "1")] };
        Assert.True(_engine.Revert(withUndo, Guid.NewGuid()).Success);
        Assert.Equal("1", _reg.Read(T("X")).Data!.Value);
    }

    [Fact]
    public void Journal_survives_a_new_instance()
    {
        var tweak = Make(Dword(T("X"), "0"));
        _engine.Apply(tweak, Guid.NewGuid());

        var reopened = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"));
        Assert.Single(reopened.OutstandingFor("t.one"));
    }

    [Fact]
    public void Machine_profile_gates_by_build_and_edition()
    {
        var req = new Requirements { MinBuild = 22000, Editions = ["Professional"] };
        Assert.True(new MachineProfile(26100, "Professional").Supports(req));
        Assert.False(new MachineProfile(19045, "Professional").Supports(req));
        Assert.False(new MachineProfile(26100, "Core").Supports(req));
    }

    [Fact]
    public void Service_and_task_steps_apply_and_revert()
    {
        _svc.Set("DiagTrack", ServiceStartType.Automatic);
        _tasks.Set(TaskPath, true);
        var tweak = Make(new SetServiceStartType("DiagTrack", ServiceStartType.Disabled), new SetTaskEnabled(TaskPath, false));

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Equal(ServiceStartType.Disabled, _svc.GetStartType("DiagTrack"));
        Assert.False(_tasks.GetEnabled(TaskPath));

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Equal(ServiceStartType.Automatic, _svc.GetStartType("DiagTrack"));
        Assert.True(_tasks.GetEnabled(TaskPath));
    }

    [Fact]
    public void Missing_service_is_skipped_and_all_missing_is_unavailable()
    {
        _svc.Set("Here", ServiceStartType.Manual);
        var mixed = Make(new SetServiceStartType("Gone", ServiceStartType.Disabled), new SetServiceStartType("Here", ServiceStartType.Disabled));
        Assert.True(_engine.Apply(mixed, Guid.NewGuid()).Success);
        Assert.Equal(TweakState.Applied, _engine.Detect(mixed));

        var none = Make(new SetServiceStartType("Gone", ServiceStartType.Disabled));
        Assert.Equal(TweakState.Unavailable, _engine.Detect(none));
        Assert.False(_engine.Apply(none, Guid.NewGuid()).Success);
    }
}
