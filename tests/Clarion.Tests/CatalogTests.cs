using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class CatalogTests
{
    [Fact]
    public void Embedded_catalog_loads_and_is_valid()
    {
        var tweaks = CatalogLoader.LoadEmbedded();
        Assert.NotEmpty(tweaks);
        Assert.Empty(CatalogLoader.Validate(tweaks));
    }

    [Fact]
    public void Validation_catches_common_mistakes()
    {
        const string json = """
        [{
          "id": "bad.one", "category": "x", "name": "n", "summary": "s", "what": "w", "benefit": "b", "risk": "r",
          "evidence": "Proven", "riskLevel": "Safe", "scope": "User",
          "apply": [
            { "type": "registry.set", "target": { "hive": "LocalMachine", "path": "HKLM\\Software\\X", "name": "V" }, "data": { "kind": "DWord", "value": "-1" } }
          ]
        }]
        """;
        var errors = CatalogLoader.Validate(CatalogLoader.Parse(json));
        Assert.Contains(errors, e => e.Contains("Proven evidence needs"));
        Assert.Contains(errors, e => e.Contains("relative to the hive"));
        Assert.Contains(errors, e => e.Contains("scope is not Machine"));
        Assert.Contains(errors, e => e.Contains("DWord value"));
    }

    [Fact]
    public void Operations_round_trip_through_json()
    {
        var tweak = CatalogLoader.LoadEmbedded().First(t => t.Id == "input.mouse-accel-off");
        Assert.Equal(3, tweak.Apply.Count);
        Assert.All(tweak.Apply, op => Assert.IsType<SetRegistryValue>(op));
    }
}
