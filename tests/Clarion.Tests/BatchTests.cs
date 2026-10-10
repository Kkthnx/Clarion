using Clarion.Core.Abstractions;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class BatchTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-batch-" + Guid.NewGuid().ToString("N"));
    private readonly FakeRegistry _reg = new();
    private readonly FakeProcessRunner _proc = new();
    private static readonly MachineProfile Win11 = new(26100, "Professional");

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static readonly RegistryTarget Freq = new(RegistryHive.LocalMachine,
        "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SystemRestore", "SystemRestorePointCreationFrequency");

    private static Tweak UserTweak(string id = "u.one", TweakScope scope = TweakScope.User, int minBuild = 0)
    {
        var hive = scope == TweakScope.User ? RegistryHive.CurrentUser : RegistryHive.LocalMachine;
        return new Tweak
        {
            Id = id, Category = "T", Name = id, Summary = "s", What = "w", Benefit = "b", Risk = "r",
            Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe, Scope = scope,
            Requires = new Requirements { MinBuild = minBuild },
            Apply = [new SetRegistryValue(new RegistryTarget(hive, "Software\\T", id), new RegistryData(RegistryKind.DWord, "1"))],
        };
    }

    private BatchRunner Runner(bool elevated = true)
    {
        var engine = new TweakEngine([new RegistryHandler(_reg)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
        return new BatchRunner(engine, new RestorePointService(_reg, _proc), () => elevated);
    }

    [Fact]
    public void Restore_point_runs_before_changes_and_frequency_value_is_put_back()
    {
        _reg.Write(Freq, new RegistryData(RegistryKind.DWord, "1440"));
        var seenDuring = "";
        _proc.Respond = _ => { seenDuring = _reg.Read(Freq).Data!.Value; return new ProcessResult(0, "", ""); };

        var result = Runner().Apply([UserTweak()], Win11, new BatchOptions());

        Assert.True(result.AllSucceeded);
        Assert.Equal("0", seenDuring);
        Assert.Equal("1440", _reg.Read(Freq).Data!.Value);
        Assert.Contains("Checkpoint-Computer", PowerShellHost.Decode(Assert.Single(_proc.Calls)));
    }

    [Fact]
    public void Frequency_value_is_removed_again_if_it_did_not_exist()
    {
        Runner().Apply([UserTweak()], Win11, new BatchOptions());
        Assert.False(_reg.Read(Freq).Exists);
    }

    private bool _protectionOn;

    private void ProtectionStartsOff()
    {
        _protectionOn = false;
        _proc.Respond = args =>
        {
            var script = PowerShellHost.Decode(args);
            if (script.Contains("Enable-ComputerRestore")) { _protectionOn = true; return new ProcessResult(0, "", ""); }
            return _protectionOn ? new ProcessResult(0, "", "") : new ProcessResult(1, "", "System Restore is turned off");
        };
    }

    private int CallsTo(string cmdlet) => _proc.Calls.Count(c => PowerShellHost.Decode(c.Split(' ', 2)[1]).Contains(cmdlet));

    [Fact]
    public void System_protection_is_turned_on_and_the_restore_point_retried_when_allowed()
    {
        ProtectionStartsOff();
        var tweak = UserTweak();

        var result = Runner().Apply([tweak], Win11, new BatchOptions { TurnOnSystemProtection = true });

        Assert.True(result.AllSucceeded);
        Assert.Null(result.Blocked);
        Assert.Equal(1, CallsTo("Enable-ComputerRestore"));
        Assert.Equal(2, CallsTo("Checkpoint-Computer"));
    }

    [Fact]
    public void System_protection_is_left_alone_unless_the_person_allowed_it()
    {
        ProtectionStartsOff();

        var result = Runner().Apply([UserTweak()], Win11, new BatchOptions());

        Assert.NotNull(result.Blocked);
        Assert.Equal(0, CallsTo("Enable-ComputerRestore"));
    }

    [Fact]
    public void Nothing_is_changed_and_both_errors_are_shown_when_protection_cannot_be_turned_on()
    {
        _proc.Respond = args => PowerShellHost.Decode(args).Contains("Enable-ComputerRestore")
            ? new ProcessResult(1, "", "Blocked by group policy")
            : new ProcessResult(1, "", "System Restore is turned off");

        var result = Runner().Apply([UserTweak()], Win11, new BatchOptions { TurnOnSystemProtection = true });

        Assert.Contains("turned off", result.Blocked);
        Assert.Contains("Blocked by group policy", result.Blocked);
        Assert.False(_reg.Read(new RegistryTarget(RegistryHive.CurrentUser, "Software\\T", "u.one")).Exists);
    }

    [Fact]
    public void Protection_is_not_touched_when_the_restore_point_works_the_first_time()
    {
        var result = Runner().Apply([UserTweak()], Win11, new BatchOptions { TurnOnSystemProtection = true });

        Assert.True(result.AllSucceeded);
        Assert.Equal(0, CallsTo("Enable-ComputerRestore"));
    }

    [Fact]
    public void A_failed_setting_does_not_stop_the_ones_after_it_and_is_reported_on_its_own()
    {
        _reg.FailWrite = t => t.Name == "u.two";

        var result = Runner().Apply([UserTweak("u.one"), UserTweak("u.two"), UserTweak("u.three")], Win11, new BatchOptions { CreateRestorePoint = false });

        Assert.False(result.AllSucceeded);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal([true, false, true], result.Items.Select(i => i.Result.Success));
        Assert.True(_reg.Read(new RegistryTarget(RegistryHive.CurrentUser, "Software\\T", "u.three")).Exists);
        Assert.False(_reg.Read(new RegistryTarget(RegistryHive.CurrentUser, "Software\\T", "u.two")).Exists);
    }

    [Fact]
    public void Failed_restore_point_blocks_the_batch_unless_user_accepts_risk()
    {
        _proc.Respond = _ => new ProcessResult(1, "", "System Protection is off");
        var tweak = UserTweak();

        var blocked = Runner().Apply([tweak], Win11, new BatchOptions());
        Assert.NotNull(blocked.Blocked);
        Assert.DoesNotContain(blocked.Items, i => !i.Skipped);
        Assert.False(_reg.Read(tweak.Apply.OfType<SetRegistryValue>().First().Target).Exists);

        var allowed = Runner().Apply([tweak], Win11, new BatchOptions { ContinueWithoutRestorePoint = true });
        Assert.True(allowed.AllSucceeded);
    }

    [Fact]
    public void Unsupported_and_unelevated_tweaks_are_skipped_without_a_restore_point()
    {
        var old = UserTweak("old", minBuild: 99999);
        var machine = UserTweak("machine", TweakScope.Machine);

        var result = Runner(elevated: false).Apply([old, machine], Win11, new BatchOptions());

        Assert.Empty(_proc.Calls);
        Assert.All(result.Items, i => Assert.True(i.Skipped && !i.Result.Success));
    }

    [Fact]
    public void Already_applied_tweaks_need_no_restore_point()
    {
        var tweak = UserTweak();
        Runner().Apply([tweak], Win11, new BatchOptions { CreateRestorePoint = false });
        _proc.Calls.Clear();

        var result = Runner().Apply([tweak], Win11, new BatchOptions());

        Assert.Empty(_proc.Calls);
        Assert.True(result.AllSucceeded);
    }

    [Fact]
    public void Restore_point_description_cannot_inject_commands()
    {
        var svc = new RestorePointService(_reg, _proc);
        svc.Create("x'; Remove-Item C:\\ -Recurse; '");
        var call = PowerShellHost.Decode(Assert.Single(_proc.Calls));
        Assert.DoesNotContain("'x'", call);
        Assert.DoesNotContain(";", call.Replace("Checkpoint-Computer", "").Replace("$ErrorActionPreference='Stop'; [Console]::OutputEncoding=[Text.Encoding]::UTF8; ", ""));
    }

    [Fact]
    public void A_run_that_only_reverts_still_makes_the_restore_point_the_dialog_offered()
    {
        var tweak = UserTweak();
        var runner = Runner();
        runner.Apply([tweak], Win11, new BatchOptions { CreateRestorePoint = false });
        _proc.Calls.Clear();

        var result = runner.Execute([], [tweak], Win11, new BatchOptions());

        Assert.True(result.AllSucceeded);
        Assert.Single(_proc.Calls);
        Assert.Contains("Checkpoint-Computer", PowerShellHost.Decode(_proc.Calls[0]));
    }

    [Fact]
    public void A_run_that_only_reverts_changes_nothing_when_the_restore_point_fails_and_was_not_waived()
    {
        var tweak = UserTweak();
        var runner = Runner();
        runner.Apply([tweak], Win11, new BatchOptions { CreateRestorePoint = false });
        _proc.Respond = _ => new ProcessResult(1, "", "System Protection is off");

        var result = runner.Execute([], [tweak], Win11, new BatchOptions());

        Assert.NotNull(result.Blocked);
        Assert.Contains("nothing was changed", result.Blocked);
        Assert.Equal(TweakState.Applied, runner.Engine.Detect(tweak));
    }

    [Fact]
    public void One_restore_point_covers_the_apply_and_the_revert_of_the_same_run()
    {
        var back = UserTweak("u.back");
        var fresh = UserTweak("u.fresh");
        var runner = Runner();
        runner.Apply([back], Win11, new BatchOptions { CreateRestorePoint = false });
        _proc.Calls.Clear();

        var result = runner.Execute([fresh], [back], Win11, new BatchOptions());

        Assert.True(result.AllSucceeded);
        Assert.Single(_proc.Calls);
    }

    [Fact]
    public void A_restore_point_that_cannot_start_powershell_is_a_failure_and_the_frequency_value_is_put_back()
    {
        _reg.Write(Freq, new RegistryData(RegistryKind.DWord, "1440"));
        _proc.Respond = _ => throw new System.ComponentModel.Win32Exception(5, "Access is denied");
        var svc = new RestorePointService(_reg, _proc);

        var made = svc.Create("Clarion test");
        var enabled = svc.EnableProtection("C:\\");

        Assert.False(made.Success);
        Assert.Contains("could not be started", made.Error);
        Assert.False(enabled.Success);
        Assert.Equal("1440", _reg.Read(Freq).Data!.Value);
    }

    [Fact]
    public void Revert_batch_restores_values()
    {
        var tweak = UserTweak();
        var runner = Runner();
        runner.Apply([tweak], Win11, new BatchOptions { CreateRestorePoint = false });
        var result = runner.Revert([tweak]);
        Assert.True(result.AllSucceeded);
        Assert.False(_reg.Read(tweak.Apply.OfType<SetRegistryValue>().First().Target).Exists);
    }

    [Fact]
    public void Steps_report_each_setting_across_apply_and_revert_with_a_shared_count()
    {
        var already = UserTweak("u.done");
        var fresh = UserTweak("u.fresh");
        var back = UserTweak("u.back");
        var runner = Runner();
        runner.Apply([already, back], Win11, new BatchOptions { CreateRestorePoint = false });

        var steps = new List<BatchStep>();
        var result = runner.Execute([already, fresh], [back], Win11, new BatchOptions { CreateRestorePoint = false, Step = steps.Add });

        Assert.True(result.AllSucceeded);
        Assert.All(steps, s => Assert.Equal(3, s.Total));
        Assert.Equal([1, 1, 2, 2, 3, 3], steps.Select(s => s.Index));
        Assert.Equal(
            [BatchStepState.Running, BatchStepState.Skipped, BatchStepState.Running, BatchStepState.Done, BatchStepState.Running, BatchStepState.Done],
            steps.Select(s => s.State));
        Assert.True(steps[^1].Reverting);
    }
}
