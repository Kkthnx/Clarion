using Clarion.Core.Catalog;

namespace Clarion.Tests;

public sealed class CatalogValueChecksTests
{
    private static IReadOnlyList<string> Check(string operation) => CatalogLoader.Validate(CatalogLoader.Parse($$"""
        [{
          "id": "bad.one", "category": "x", "name": "n", "summary": "s", "what": "w", "benefit": "b", "risk": "r",
          "evidence": "Cosmetic", "riskLevel": "Safe", "scope": "Machine", "topic": "t",
          "apply": [ {{operation}} ]
        }]
        """));

    [Theory]
    [InlineData("ABC")]
    [InlineData("0G")]
    public void A_binary_value_must_be_whole_bytes_in_hex(string value) =>
        Assert.Contains(Check($$"""{ "type": "registry.set", "target": { "hive": "LocalMachine", "path": "Software", "name": "V" }, "data": { "kind": "Binary", "value": "{{value}}" } }"""),
            e => e.Contains("Binary value"));

    [Fact]
    public void A_good_binary_value_passes() =>
        Assert.DoesNotContain(Check("""{ "type": "registry.set", "target": { "hive": "LocalMachine", "path": "Software", "name": "V" }, "data": { "kind": "Binary", "value": "00ff" } }"""),
            e => e.Contains("Binary value"));

    [Theory]
    [InlineData("Bad Name")]
    [InlineData("a/b")]
    public void A_service_name_must_be_a_plain_name(string name) =>
        Assert.Contains(Check($$"""{ "type": "service.start-type", "name": "{{name}}", "startType": "Manual" }"""), e => e.Contains("not a valid service name"));
}
