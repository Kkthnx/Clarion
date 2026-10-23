using Clarion.Core.Catalog;
using Clarion.Core.Cleanup;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class CleanupAppNameTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "clarion-names-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try { Directory.Delete(_root, true); } catch (IOException) { }
    }

    [Fact]
    public void Every_row_that_waits_for_a_program_names_it_the_way_a_person_knows_it()
    {
        var rows = CatalogLoader.LoadCleanup().Where(t => t.Processes.Count > 0).ToList();

        Assert.NotEmpty(rows);
        Assert.All(rows, r => Assert.False(string.IsNullOrWhiteSpace(r.AppName), $"{r.Id} has no appName"));
    }

    [Fact]
    public void The_skip_message_uses_the_program_name_and_not_the_process_name()
    {
        Directory.CreateDirectory(_root);
        var platform = new FakeCleanupPlatform(_root);
        platform.Running.Add("Agent");
        var target = CatalogLoader.LoadCleanup().Single(t => t.Id == "launchers.battlenet");

        var result = new CleanupEngine(platform).Clean(target, false, false, null, default);

        Assert.True(result.Skipped);
        Assert.Equal(["Close Battle.net first."], result.Notes);
    }

    [Fact]
    public void A_row_without_a_name_falls_back_to_the_process_names()
    {
        var target = new CleanTarget
        {
            Id = "x",
            Group = "g",
            Name = "x",
            Summary = "s",
            Advice = "a",
            Benefit = "b",
            Risk = "r",
            RiskLevel = RiskLevel.Safe,
            Facts = ["one", "two"],
            Processes = ["one", "two"],
            Rules = [],
        };

        Assert.Equal("one, two", target.CloseLabel(["one", "two"]));
    }
}
