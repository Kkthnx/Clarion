using System.Runtime.Versioning;
using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Model;
using Clarion.Engine;

namespace Clarion.Tests;

[SupportedOSPlatform("windows")]
public sealed class ServiceNameTests
{
    [Theory]
    [InlineData("DiagTrack")]
    [InlineData("dmwappushservice")]
    [InlineData("WSearch")]
    [InlineData("Fax")]
    [InlineData("edgeupdate")]
    [InlineData("MapsBroker")]
    [InlineData("Some.Service-Name_2")]
    public void Real_service_names_are_accepted(string name) => Assert.True(ServiceNames.IsValid(name));

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("DiagTrack binPath= C:\\evil.exe")]
    [InlineData("DiagTrack\nstart= auto")]
    [InlineData("a/b")]
    [InlineData("..\\Tcpip")]
    [InlineData("name\"quoted")]
    [InlineData("a&b")]
    public void Anything_that_could_add_arguments_or_climb_out_of_the_services_key_is_refused(string name) =>
        Assert.False(ServiceNames.IsValid(name));

    [Fact]
    public void A_start_type_change_with_a_crafted_name_never_reaches_sc()
    {
        var runner = new FakeProcessRunner();
        var store = new WindowsServiceStore(runner);

        Assert.Throws<ArgumentException>(() => store.SetStartType("DiagTrack binPath= C:\\evil.exe", ServiceStartType.Disabled));

        Assert.Empty(runner.Calls);
    }

    [Fact]
    public void Every_service_the_catalog_changes_has_an_accepted_name()
    {
        var names = CatalogLoader.LoadEmbedded()
            .SelectMany(t => t.Apply.Concat(t.Undo))
            .OfType<SetServiceStartType>()
            .Select(s => s.Name)
            .Distinct()
            .ToList();

        Assert.NotEmpty(names);
        Assert.All(names, n => Assert.True(ServiceNames.IsValid(n), n));
    }

    [Fact]
    public void Reading_a_crafted_name_finds_nothing()
    {
        Assert.Null(new WindowsServiceStore(new FakeProcessRunner()).GetStartType("..\\..\\Tcpip"));
    }
}
