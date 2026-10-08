using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class PresetTests
{
    [Fact]
    public void Every_preset_entry_exists_and_has_no_duplicates()
    {
        var ids = CatalogLoader.LoadEmbedded().Select(t => t.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var presets = CatalogLoader.LoadPresets();
        Assert.True(presets.Count >= 4);
        foreach (var (name, list) in presets)
        {
            Assert.All(list, id => Assert.True(ids.Contains(id), $"{name} references unknown tweak {id}"));
            Assert.Equal(list.Count, list.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }
    }

    [Fact]
    public void Presets_never_include_high_risk_or_unproven_tweaks()
    {
        var byId = CatalogLoader.LoadEmbedded().ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
        foreach (var id in CatalogLoader.LoadPresets().Values.SelectMany(v => v))
        {
            Assert.NotEqual(RiskLevel.High, byId[id].RiskLevel);
            Assert.NotEqual(Evidence.Unproven, byId[id].Evidence);
        }
    }

    [Fact]
    public void Minimal_is_a_subset_of_standard_and_standard_of_advanced()
    {
        var p = CatalogLoader.LoadPresets();
        Assert.Subset(p["standard"].ToHashSet(), p["minimal"].ToHashSet());
        Assert.Subset(p["advanced"].ToHashSet(), p["standard"].ToHashSet());
    }
}
