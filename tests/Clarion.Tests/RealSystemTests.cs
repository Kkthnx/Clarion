using System.Diagnostics;
using System.Runtime.Versioning;
using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Engine;
using Xunit.Abstractions;

namespace Clarion.Tests;

/// <summary>
/// Runs against the real machine with throwaway objects it creates and removes itself.
/// Skipped unless CLARION_REAL_TESTS=1. Needs an elevated shell.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RealSystemTests(ITestOutputHelper output) : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-real-sys-" + Guid.NewGuid().ToString("N"));

    private static bool Enabled =>
        OperatingSystem.IsWindows() && Environment.GetEnvironmentVariable("CLARION_REAL_TESTS") == "1" && WindowsMachine.IsElevated();

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static Tweak Make(params Operation[] ops) => new()
    {
        Id = "real.sys", Category = "T", Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe, Scope = TweakScope.Machine, Apply = ops,
    };

    [Fact]
    public void Startup_state_read_is_fast_enough()
    {
        if (!Enabled) return;
        var runtime = EngineFactory.Create(_dir);
        var tweaks = CatalogLoader.LoadEmbedded().Where(t => runtime.Profile.Supports(t.Requires)).ToList();

        var sw = Stopwatch.StartNew();
        runtime.Engine.WarmUp(tweaks);
        var warm = sw.ElapsedMilliseconds;
        foreach (var t in tweaks) runtime.Engine.Detect(t);
        var total = sw.ElapsedMilliseconds;

        output.WriteLine($"{tweaks.Count} tweaks, warm-up {warm} ms, total {total} ms");
        Assert.True(total < 15000, $"Reading state took {total} ms");
    }

    [Fact]
    public void Real_service_start_type_changes_and_reverts()
    {
        if (!Enabled) return;
        const string name = "ClarionTestSvc";
        var runner = new WindowsProcessRunner();
        runner.Run("sc.exe", $"delete {name}", TimeSpan.FromSeconds(20));
        var create = runner.Run("sc.exe", $"create {name} binPath= \"C:\\Windows\\System32\\cmd.exe\" start= demand", TimeSpan.FromSeconds(20));
        Assert.Equal(0, create.ExitCode);
        try
        {
            var store = new WindowsServiceStore(runner);
            var engine = new TweakEngine([new ServiceHandler(store)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
            var tweak = Make(new SetServiceStartType(name, ServiceStartType.Disabled));

            Assert.Equal(ServiceStartType.Manual, store.GetStartType(name));
            Assert.True(engine.Apply(tweak, Guid.NewGuid()).Success);
            Assert.Equal(ServiceStartType.Disabled, store.GetStartType(name));
            Assert.True(engine.Revert(tweak, Guid.NewGuid()).Success);
            Assert.Equal(ServiceStartType.Manual, store.GetStartType(name));
        }
        finally
        {
            runner.Run("sc.exe", $"delete {name}", TimeSpan.FromSeconds(20));
        }
    }

    [Fact]
    public void Real_scheduled_task_disables_and_reverts()
    {
        if (!Enabled) return;
        const string path = "\\ClarionTest\\Probe";
        var runner = new WindowsProcessRunner();
        runner.Run("schtasks.exe", $"/delete /tn \"{path}\" /f", TimeSpan.FromSeconds(20));
        var create = runner.Run("schtasks.exe", $"/create /tn \"{path}\" /tr \"cmd.exe /c exit\" /sc once /st 23:59 /f", TimeSpan.FromSeconds(20));
        Assert.Equal(0, create.ExitCode);
        try
        {
            var store = new WindowsTaskStore();
            var engine = new TweakEngine([new TaskHandler(store)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
            var tweak = Make(new SetTaskEnabled(path, false));

            Assert.True(store.GetEnabled(path));
            Assert.True(engine.Apply(tweak, Guid.NewGuid()).Success);
            Assert.False(store.GetEnabled(path));
            Assert.True(engine.Revert(tweak, Guid.NewGuid()).Success);
            Assert.True(store.GetEnabled(path));
        }
        finally
        {
            runner.Run("schtasks.exe", $"/delete /tn \"{path}\" /f", TimeSpan.FromSeconds(20));
            runner.Run("schtasks.exe", "/delete /tn \"\\ClarionTest\" /f", TimeSpan.FromSeconds(20));
        }
    }

    [Fact]
    public void Real_user_tweak_apply_and_revert_restores_exact_prior_value()
    {
        if (!Enabled) return;
        var runtime = EngineFactory.Create(_dir);
        var tweak = CatalogLoader.LoadEmbedded().First(t => t.Id == "input.menu-show-delay-zero");
        var store = new WindowsRegistryStore();
        var target = ((SetRegistryValue)tweak.Apply[0]).Target;
        var before = store.Read(target);

        try
        {
            var res = runtime.Runner.Execute([tweak], [], runtime.Profile, new BatchOptions { CreateRestorePoint = false });
            Assert.True(res.AllSucceeded);
            Assert.Equal("0", store.Read(target).Data!.Value);

            var back = runtime.Runner.Execute([], [tweak], runtime.Profile, new BatchOptions { CreateRestorePoint = false });
            Assert.True(back.AllSucceeded);
        }
        finally
        {
            if (before.Exists && before.Data is not null) store.Write(target, before.Data);
        }
        Assert.Equal(before, store.Read(target));
    }

    [Fact]
    public void Real_restore_point_is_created_and_frequency_value_restored()
    {
        if (!Enabled || Environment.GetEnvironmentVariable("CLARION_REAL_RESTORE") != "1") return;
        var registry = new WindowsRegistryStore();
        var freq = new RegistryTarget(Clarion.Core.Model.RegistryHive.LocalMachine,
            "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SystemRestore", "SystemRestorePointCreationFrequency");
        var before = registry.Read(freq);

        var result = new RestorePointService(registry, new WindowsProcessRunner()).Create("Clarion test restore point");
        output.WriteLine($"restore point: {(result.Success ? "ok" : result.Error)}");

        Assert.Equal(before.Exists, registry.Read(freq).Exists);
        if (before.Data is not null) Assert.Equal(before.Data, registry.Read(freq).Data);
    }

    [Fact]
    public void Real_power_plans_list_duplicate_rename_and_clean_up()
    {
        if (!Enabled) return;
        var runner = new WindowsProcessRunner();
        var store = new WindowsPowerStore(runner);

        var plans = store.List();
        Assert.NotEmpty(plans);
        Assert.Single(plans, p => p.IsActive);
        Assert.NotNull(store.IsHibernationEnabled());

        var created = store.Duplicate(Clarion.Core.Power.PowerPlans.Balanced);
        try
        {
            store.Rename(created, "Clarion Probe Plan");
            Assert.Contains(store.List(), p => p.Guid == created && p.Name == "Clarion Probe Plan" && !p.IsActive);
        }
        finally
        {
            runner.Run("powercfg.exe", $"/delete {created}", TimeSpan.FromSeconds(20));
        }
        Assert.DoesNotContain(store.List(), p => p.Guid == created);
    }

    [Fact]
    public void Real_dns_provider_switch_and_exact_restore()
    {
        if (!Enabled) return;
        var runner = new WindowsProcessRunner();
        var store = new WindowsDnsStore(runner);
        var before = store.GetAdapters();
        if (before.Count == 0) return;
        var dohBefore = store.GetDohRegistrations();

        var engine = new TweakEngine([new DnsHandler(store)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
        var tweak = Make(new SetDnsProvider("google", Encrypted: true));
        try
        {
            var applied = engine.Apply(tweak, Guid.NewGuid());
            output.WriteLine($"apply: {applied.Success} {applied.Error}");
            output.WriteLine("after apply: " + string.Join("; ", store.GetAdapters().Select(a => string.Join(",", a.Servers))));
            Assert.True(applied.Success, applied.Error);
            Assert.All(store.GetAdapters(), a => Assert.Equal(["8.8.8.8", "8.8.4.4"], a.Servers.Order(StringComparer.Ordinal).Reverse().ToArray()));
            Assert.Equal(TweakState.Applied, engine.Detect(tweak));
            var reverted = engine.Revert(tweak, Guid.NewGuid());
            output.WriteLine($"revert: {reverted.Success} {reverted.Error}");
            output.WriteLine("after revert: " + string.Join("; ", store.GetAdapters().Select(a => string.Join(",", a.Servers))));
            Assert.True(reverted.Success, reverted.Error);
        }
        finally
        {
            foreach (var a in before)
            {
                if (a.Servers.Count == 0) store.ResetServers(a.IfIndex); else store.SetServers(a.IfIndex, a.Servers);
            }
            foreach (var d in dohBefore) store.UpsertDoh(d);
        }

        foreach (var a in before)
            Assert.Equal(a.Servers, store.GetAdapters().Single(x => x.IfIndex == a.IfIndex).Servers);
        foreach (var d in dohBefore)
            Assert.Equal(d, store.GetDohRegistrations().Single(x => x.Address == d.Address));
    }
}