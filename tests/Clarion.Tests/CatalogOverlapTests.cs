using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class CatalogOverlapTests
{
    [Fact]
    public void Two_settings_that_write_the_same_registry_value_are_alternatives_in_one_exclusive_group()
    {
        var overlaps = CatalogLoader.LoadEmbedded()
            .SelectMany(t => t.Apply.OfType<SetRegistryValue>().Select(op => (Target: op.Target.ToString(), Tweak: t)))
            .GroupBy(x => x.Target, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Select(x => x.Tweak.Id).Distinct().Count() > 1)
            .Where(g => g.Select(x => x.Tweak.ExclusiveGroup).Distinct().Count() != 1 || g.First().Tweak.ExclusiveGroup is null)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(x => x.Tweak.Id).Distinct())}")
            .ToList();

        Assert.True(overlaps.Count == 0, "These settings write the same value without being exclusive alternatives:\n" + string.Join("\n", overlaps));
    }
}
