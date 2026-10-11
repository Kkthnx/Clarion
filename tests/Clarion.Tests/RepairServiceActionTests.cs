using Clarion.Core.Actions;
using Clarion.Core.Catalog;

namespace Clarion.Tests;

public sealed class RepairServiceActionTests
{
    [Theory]
    [InlineData("repair.restart-audio", "audiosrv")]
    [InlineData("repair.restart-audio", "AudioEndpointBuilder")]
    [InlineData("repair.restart-bluetooth", "bthserv")]
    public void A_service_that_is_stopped_is_always_started_again(string actionId, string service)
    {
        var action = CatalogLoader.LoadActions().Single(a => a.Id == actionId);
        Assert.Contains(action.Steps.OfType<RunProcess>(), s => s.Args.StartsWith($"stop {service}", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(action.Always.OfType<RunProcess>(), s => s.Args == $"start {service}");
    }

    [Fact]
    public void Resync_starts_the_time_service_then_asks_for_a_resync()
    {
        var steps = CatalogLoader.LoadActions().Single(a => a.Id == "repair.resync-clock").Steps.OfType<RunProcess>().ToList();
        Assert.Equal("start w32time", steps[0].Args);
        Assert.Equal("w32tm.exe", steps[1].File);
        Assert.Equal("/resync", steps[1].Args);
    }

    [Fact]
    public void Every_repair_action_names_a_source()
    {
        foreach (var a in CatalogLoader.LoadActions().Where(a => a.Id.StartsWith("repair.restart-", StringComparison.Ordinal) || a.Id == "repair.resync-clock"))
            Assert.NotEmpty(a.Sources);
    }
}
