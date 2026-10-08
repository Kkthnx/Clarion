using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class TooltipTests
{
    [Fact]
    public void Every_catalog_item_has_advice_and_facts()
    {
        foreach (var t in CatalogLoader.LoadEmbedded())
        {
            Assert.False(string.IsNullOrWhiteSpace(t.Advice), t.Id);
            Assert.InRange(t.Facts.Count, 2, 6);
        }
    }

    [Fact]
    public void Tooltip_carries_recommendation_facts_and_notes()
    {
        var t = CatalogLoader.LoadEmbedded().First(x => x.Id == "input.mouse-accel-off");
        var tip = TweakTooltip.Build(t);

        Assert.Equal("Only if it fits you", tip.RecommendationLabel);
        Assert.Contains(tip.Notes, n => n.Contains("Sign out"));
        Assert.Contains(tip.Notes, n => n.Contains("your account only"));
        var text = tip.ToPlainText();
        Assert.Contains("Evidence:", text);
        Assert.Contains("Benefit:", text);
        Assert.Contains("- ", text);
    }

    [Fact]
    public void Edition_and_machine_scope_notes_appear()
    {
        var t = CatalogLoader.LoadEmbedded().First(x => x.Id == "debloat.consumer-features-policy");
        var tip = TweakTooltip.Build(t);
        Assert.Contains(tip.Notes, n => n.Contains("Enterprise"));
        Assert.Contains(tip.Notes, n => n.Contains("administrator"));
    }

    [Fact]
    public void Validation_rejects_missing_advice_and_risky_recommended()
    {
        const string json = """
        [{
          "id": "bad.tip", "category": "x", "name": "n", "summary": "s", "what": "w", "benefit": "b", "risk": "r",
          "evidence": "Cosmetic", "riskLevel": "High", "scope": "User", "recommendation": "Recommended",
          "apply": [ { "type": "registry.set", "target": { "hive": "CurrentUser", "path": "Software\\X", "name": "V" }, "data": { "kind": "DWord", "value": "1" } } ]
        }]
        """;
        var errors = CatalogLoader.Validate(CatalogLoader.Parse(json));
        Assert.Contains(errors, e => e.Contains("advice is empty"));
        Assert.Contains(errors, e => e.Contains("facts"));
        Assert.Contains(errors, e => e.Contains("cannot be marked Recommended"));
    }
}
