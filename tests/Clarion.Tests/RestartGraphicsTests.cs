using Clarion.Core.Actions;
using Clarion.Core.Catalog;
using Clarion.Core.Model;
using Clarion.Engine;

namespace Clarion.Tests;

public sealed class RestartGraphicsTests
{
    private sealed class FakeAdapters(params DisplayAdapter[] present) : IDisplayAdapters
    {
        public List<string> Restarted { get; } = [];
        public HashSet<string> Failing { get; } = [];

        public IReadOnlyList<DisplayAdapter> Present() => present;

        public string? Restart(DisplayAdapter adapter)
        {
            Restarted.Add(adapter.InstanceId);
            return Failing.Contains(adapter.InstanceId) ? "Access is denied." : null;
        }
    }

    private sealed class NoTools : IStreamingRunner
    {
        public Task<int> RunAsync(string systemTool, string args, bool utf16, Action<string> onLine, CancellationToken cancel) =>
            throw new InvalidOperationException("No tool should run.");
    }

    private static ActionDef Job() => CatalogLoader.LoadActions().Single(a => a.Id == "repair.restart-graphics");

    private static DisplayAdapter Adapter(string id, bool running = true) => new(id, "Adapter " + id, running);

    [Fact]
    public async Task Only_adapters_whose_driver_is_running_are_restarted()
    {
        var adapters = new FakeAdapters(Adapter("A"), Adapter("B", running: false), Adapter("C"));
        var result = await new ActionRunner(new NoTools(), null, adapters).RunAsync(Job(), _ => { }, default);

        Assert.True(result.Success);
        Assert.Equal(["A", "C"], adapters.Restarted);
    }

    [Fact]
    public async Task One_adapter_failing_does_not_stop_the_next_and_the_job_says_which_one_failed()
    {
        var adapters = new FakeAdapters(Adapter("A"), Adapter("B"));
        adapters.Failing.Add("A");
        var result = await new ActionRunner(new NoTools(), null, adapters).RunAsync(Job(), _ => { }, default);

        Assert.False(result.Success);
        Assert.Contains("Adapter A did not restart", result.Error);
        Assert.Equal(["A", "B"], adapters.Restarted);
    }

    [Fact]
    public async Task With_no_running_adapter_the_job_fails_and_restarts_nothing()
    {
        var adapters = new FakeAdapters(Adapter("A", running: false));
        var result = await new ActionRunner(new NoTools(), null, adapters).RunAsync(Job(), _ => { }, default);

        Assert.False(result.Success);
        Assert.Empty(adapters.Restarted);
    }

    [Fact]
    public async Task Without_a_way_to_restart_adapters_the_job_fails_instead_of_doing_nothing()
    {
        var result = await new ActionRunner(new NoTools()).RunAsync(Job(), _ => { }, default);
        Assert.False(result.Success);
    }

    [Fact]
    public void The_job_is_marked_medium_risk_and_the_loader_refuses_a_lower_mark()
    {
        var job = Job();
        Assert.True(job.RiskLevel >= RiskLevel.Medium);
        Assert.Empty(CatalogLoader.ValidateActions([job]));
        Assert.Contains(CatalogLoader.ValidateActions([job with { RiskLevel = RiskLevel.Low }]), e => e.Contains("Medium"));
    }

    [Fact]
    public void The_real_list_of_adapters_can_be_read_and_names_each_one()
    {
        if (!OperatingSystem.IsWindows()) return;
        // Reading only. This test never restarts a driver.
        foreach (var adapter in new WindowsDisplayAdapters().Present())
        {
            Assert.False(string.IsNullOrWhiteSpace(adapter.InstanceId));
            Assert.False(string.IsNullOrWhiteSpace(adapter.Name));
        }
    }
}
