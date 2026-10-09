using System.Text.Json;
using System.Text.RegularExpressions;

namespace Clarion.Core.Dns;

public sealed record AdapterDns(int IfIndex, string Alias, IReadOnlyList<string> Servers);

public sealed record DohRegistration(string Address, string Template, bool AutoUpgrade, bool AllowFallbackToUdp);

public sealed record DnsProvider(string Key, string Name, IReadOnlyList<string> Servers, string DohTemplate);

public interface IDnsStore
{
    /// <summary>Physical network adapters that are connected, with their IPv4 DNS servers. An empty list means automatic.</summary>
    IReadOnlyList<AdapterDns> GetAdapters();

    /// <summary>One adapter by its current index, connected or not. Null when no adapter has that index now.</summary>
    AdapterDns? Find(int ifIndex);

    void SetServers(int ifIndex, IReadOnlyList<string> servers);

    /// <summary>Goes back to the servers the network hands out.</summary>
    void ResetServers(int ifIndex);

    IReadOnlyList<DohRegistration> GetDohRegistrations();

    /// <summary>Adds the registration, or updates it when the address is already known to Windows.</summary>
    void UpsertDoh(DohRegistration registration);

    void RemoveDoh(string address);
}

public static partial class DnsProviders
{
    public static readonly IReadOnlyList<DnsProvider> All =
    [
        new("cloudflare", "Cloudflare", ["1.1.1.1", "1.0.0.1"], "https://cloudflare-dns.com/dns-query"),
        new("google", "Google", ["8.8.8.8", "8.8.4.4"], "https://dns.google/dns-query"),
        new("quad9", "Quad9", ["9.9.9.9", "149.112.112.112"], "https://dns.quad9.net/dns-query"),
        new("opendns", "OpenDNS", ["208.67.222.222", "208.67.220.220"], "https://doh.opendns.com/dns-query"),
        new("adguard", "AdGuard DNS", ["94.140.14.14", "94.140.15.15"], "https://dns.adguard-dns.com/dns-query"),
    ];

    public static DnsProvider? Find(string key) => All.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

    [GeneratedRegex(@"^\d{1,3}(\.\d{1,3}){3}$")]
    private static partial Regex Ipv4();

    public static bool IsIpv4(string s) => Ipv4().IsMatch(s) && s.Split('.').All(p => int.Parse(p) <= 255);

    public static bool SameServers(IReadOnlyList<string> a, IReadOnlyList<string> b) =>
        a.Count == b.Count && a.Order().SequenceEqual(b.Order(), StringComparer.OrdinalIgnoreCase);

    public static string Serialize(IReadOnlyList<AdapterDns> adapters) => JsonSerializer.Serialize(adapters);
}
