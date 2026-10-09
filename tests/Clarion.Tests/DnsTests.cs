using Clarion.Core.Catalog;
using Clarion.Core.Dns;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class FakeDnsStore : IDnsStore
{
    public List<AdapterDns> Adapters { get; } = [];
    public List<DohRegistration> Doh { get; } = [];
    public List<string> Calls { get; } = [];

    /// <summary>Adapters that exist but are not connected, such as Wi-Fi switched off. They are not listed as connected.</summary>
    public List<AdapterDns> Down { get; } = [];

    public IReadOnlyList<AdapterDns> GetAdapters() => Adapters.ToList();
    public AdapterDns? Find(int ifIndex) => Adapters.Concat(Down).FirstOrDefault(a => a.IfIndex == ifIndex);

    private void Update(int ifIndex, IReadOnlyList<string> servers)
    {
        var list = Adapters.Any(a => a.IfIndex == ifIndex) ? Adapters : Down.Any(a => a.IfIndex == ifIndex) ? Down : throw new InvalidOperationException($"No adapter {ifIndex}");
        var i = list.FindIndex(a => a.IfIndex == ifIndex);
        list[i] = list[i] with { Servers = servers.ToList() };
    }
    public IReadOnlyList<DohRegistration> GetDohRegistrations() => Doh.ToList();

    public void SetServers(int ifIndex, IReadOnlyList<string> servers)
    {
        Calls.Add($"set {ifIndex} {string.Join(",", servers)}");
        Update(ifIndex, servers);
    }

    public void ResetServers(int ifIndex)
    {
        Calls.Add($"reset {ifIndex}");
        Update(ifIndex, []);
    }

    public void UpsertDoh(DohRegistration r)
    {
        Calls.Add($"doh {r.Address} {r.AutoUpgrade}");
        Doh.RemoveAll(x => x.Address == r.Address);
        Doh.Add(r);
    }

    public void RemoveDoh(string address)
    {
        Calls.Add($"remove-doh {address}");
        Doh.RemoveAll(x => x.Address == address);
    }
}

public sealed class DnsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-dns-" + Guid.NewGuid().ToString("N"));
    private readonly FakeDnsStore _store = new();
    private readonly TweakEngine _engine;

    public DnsTests()
    {
        _store.Adapters.Add(new AdapterDns(8, "Ethernet", ["192.168.1.1"]));
        _store.Adapters.Add(new AdapterDns(12, "Wi-Fi", []));
        _engine = new TweakEngine([new DnsHandler(_store)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static Tweak Make(string provider, bool encrypted) => new()
    {
        Id = "dns.t", Category = "Network", Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Situational, RiskLevel = RiskLevel.Low, Scope = TweakScope.Machine, Apply = [new SetDnsProvider(provider, encrypted)],
    };

    [Fact]
    public void Apply_sets_every_connected_adapter_and_revert_restores_each_one_exactly()
    {
        var tweak = Make("cloudflare", false);
        Assert.Equal(TweakState.NotApplied, _engine.Detect(tweak));

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Equal(TweakState.Applied, _engine.Detect(tweak));
        Assert.All(_store.Adapters, a => Assert.Equal(["1.1.1.1", "1.0.0.1"], a.Servers));

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Equal(["192.168.1.1"], _store.Adapters[0].Servers);
        Assert.Empty(_store.Adapters[1].Servers);
        Assert.Contains("reset 12", _store.Calls);
    }

    [Fact]
    public void Encrypted_mode_adds_missing_entries_and_restores_flags_of_existing_ones()
    {
        _store.Doh.Add(new DohRegistration("1.1.1.1", "https://cloudflare-dns.com/dns-query", AutoUpgrade: false, AllowFallbackToUdp: true));
        var tweak = Make("cloudflare", true);

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.All(["1.1.1.1", "1.0.0.1"], ip => Assert.Contains(_store.Doh, d => d.Address == ip && d.AutoUpgrade && !d.AllowFallbackToUdp));

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        var kept = Assert.Single(_store.Doh);
        Assert.Equal("1.1.1.1", kept.Address);
        Assert.False(kept.AutoUpgrade);
        Assert.True(kept.AllowFallbackToUdp);
    }

    [Fact]
    public void Switching_from_one_provider_to_another_and_back_keeps_the_original_servers()
    {
        var a = Make("google", false);
        var b = Make("quad9", false) with { Id = "dns.u" };

        _engine.Apply(a, Guid.NewGuid());
        _engine.Apply(b, Guid.NewGuid());
        Assert.Equal(["9.9.9.9", "149.112.112.112"], _store.Adapters[0].Servers);

        _engine.Revert(b, Guid.NewGuid());
        Assert.Equal(["8.8.8.8", "8.8.4.4"], _store.Adapters[0].Servers);
        _engine.Revert(a, Guid.NewGuid());
        Assert.Equal(["192.168.1.1"], _store.Adapters[0].Servers);
    }

    [Fact]
    public void Revert_skips_an_adapter_that_is_gone_and_still_restores_the_others()
    {
        var tweak = Make("cloudflare", false);
        _engine.Apply(tweak, Guid.NewGuid());

        _store.Adapters.RemoveAll(a => a.IfIndex == 12);
        _engine.Invalidate();

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Equal(["192.168.1.1"], _store.Adapters.Single().Servers);
        Assert.DoesNotContain("reset 12", _store.Calls);
    }

    [Fact]
    public void Revert_restores_an_adapter_that_is_present_but_no_longer_connected()
    {
        var tweak = Make("cloudflare", false);
        _engine.Apply(tweak, Guid.NewGuid());

        var wifi = _store.Adapters.Single(a => a.IfIndex == 12);
        _store.Adapters.Remove(wifi);
        _store.Down.Add(wifi);
        _engine.Invalidate();

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Empty(_store.Down.Single().Servers);
        Assert.Contains("reset 12", _store.Calls);
    }

    [Fact]
    public void Revert_works_when_no_adapter_is_connected_at_all()
    {
        var tweak = Make("cloudflare", false);
        _engine.Apply(tweak, Guid.NewGuid());

        _store.Down.AddRange(_store.Adapters);
        _store.Adapters.Clear();
        _engine.Invalidate();

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Equal(["192.168.1.1"], _store.Down.Single(a => a.IfIndex == 8).Servers);
    }

    [Fact]
    public void Revert_leaves_alone_a_different_adapter_that_reuses_the_same_index()
    {
        var tweak = Make("cloudflare", false);
        _engine.Apply(tweak, Guid.NewGuid());

        _store.Adapters.RemoveAll(a => a.IfIndex == 12);
        _store.Adapters.Add(new AdapterDns(12, "Work VPN", ["10.0.0.53"]));
        _engine.Invalidate();

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Equal(["10.0.0.53"], _store.Adapters.Single(a => a.IfIndex == 12).Servers);
        Assert.DoesNotContain("reset 12", _store.Calls);
        Assert.Equal(1, _store.Calls.Count(c => c.StartsWith("set 12 ", StringComparison.Ordinal)));
    }

    [Fact]
    public void No_connected_adapter_makes_the_setting_unavailable()
    {
        _store.Adapters.Clear();
        Assert.Equal(TweakState.Unavailable, _engine.Detect(Make("google", false)));
    }

    [Theory]
    [InlineData("1.1.1.1", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("256.1.1.1", false)]
    [InlineData("1.1.1", false)]
    [InlineData("1.1.1.1; calc", false)]
    [InlineData("", false)]
    public void Only_plain_ipv4_addresses_pass_the_check(string value, bool valid) => Assert.Equal(valid, DnsProviders.IsIpv4(value));

    [Fact]
    public void Server_comparison_ignores_order()
    {
        Assert.True(DnsProviders.SameServers(["1.1.1.1", "1.0.0.1"], ["1.0.0.1", "1.1.1.1"]));
        Assert.False(DnsProviders.SameServers(["1.1.1.1"], ["1.1.1.1", "1.0.0.1"]));
    }

    [Fact]
    public void Catalog_dns_entries_are_valid_exclusive_and_use_known_providers()
    {
        var dns = CatalogLoader.LoadEmbedded().Where(t => t.Topic == "DNS").ToList();
        Assert.Equal(10, dns.Count);
        Assert.All(dns, t => Assert.Equal("dns-provider", t.ExclusiveGroup));
        Assert.All(dns.Where(t => t.Id.EndsWith("-encrypted", StringComparison.Ordinal)), t => Assert.True(t.Requires.MinBuild >= 22000));
        Assert.Empty(CatalogLoader.Validate(CatalogLoader.LoadEmbedded()));
        Assert.All(dns.SelectMany(t => t.Apply).OfType<SetDnsProvider>(), o => Assert.NotNull(DnsProviders.Find(o.Provider)));
    }

    [Fact]
    public void Every_provider_has_two_ipv4_servers_and_an_https_template()
    {
        Assert.All(DnsProviders.All, p =>
        {
            Assert.Equal(2, p.Servers.Count);
            Assert.All(p.Servers, s => Assert.True(DnsProviders.IsIpv4(s)));
            Assert.StartsWith("https://", p.DohTemplate);
        });
    }
}
