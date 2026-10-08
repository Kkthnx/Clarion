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
