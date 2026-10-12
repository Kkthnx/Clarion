using System.Runtime.Versioning;
using Clarion.Core.Dns;
using Clarion.Engine;

namespace Clarion.Tests;

[SupportedOSPlatform("windows")]
public sealed class DnsInputTests
{
    [Theory]
    [InlineData("1.1.1.1")]
    [InlineData("8.8.4.4")]
    [InlineData("255.255.255.255")]
    [InlineData("0.0.0.0")]
    public void Plain_dotted_addresses_are_accepted(string address) => Assert.True(DnsProviders.IsIpv4(address));

    [Theory]
    [InlineData("")]
    [InlineData("1.1.1")]
    [InlineData("1.1.1.1.1")]
    [InlineData("256.1.1.1")]
    [InlineData("1.1.1.1\n")]
    [InlineData("1.1.1.1\n'; calc; '")]
    [InlineData(" 1.1.1.1")]
    [InlineData("1.1.1.1'")]
    public void Anything_that_is_not_a_plain_address_is_refused(string address) => Assert.False(DnsProviders.IsIpv4(address));

    [Fact]
    public void Digits_from_other_scripts_are_refused_instead_of_throwing()
    {
        // Arabic-Indic digits match \d in .NET, and int.Parse then throws on them.
        Assert.False(DnsProviders.IsIpv4("١.١.١.١"));
    }

    [Fact]
    public void An_encrypted_dns_template_with_a_quote_in_it_is_never_run()
    {
        var runner = new FakeProcessRunner();
        var store = new WindowsDnsStore(runner);

        Assert.Throws<ArgumentException>(() =>
            store.UpsertDoh(new DohRegistration("1.1.1.1", "https://dns.example/q'; calc; '", true, false)));

        Assert.Empty(runner.Calls);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{\"something\":1}")]
    public void Output_that_is_not_the_expected_json_reads_as_no_adapters_instead_of_throwing(string output)
    {
        var runner = new FakeProcessRunner { Respond = _ => new Clarion.Core.Abstractions.ProcessResult(0, output, "") };
        var store = new WindowsDnsStore(runner);

        Assert.Empty(store.GetAdapters());
        Assert.Null(store.Find(3));
        Assert.Empty(store.GetDohRegistrations());
    }
}
