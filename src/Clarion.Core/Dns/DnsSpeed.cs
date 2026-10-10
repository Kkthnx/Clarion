namespace Clarion.Core.Dns;

/// <summary>How long one provider took to answer, or null when it did not answer at all.</summary>
public sealed record DnsSpeedResult(DnsProvider Provider, int? Milliseconds);

/// <summary>Sends one DNS query and says how long the answer took. Null means no usable answer in time.</summary>
public interface IDnsTransport
{
    Task<TimeSpan?> RoundTripAsync(string server, byte[] query, TimeSpan timeout, CancellationToken cancel);
}

/// <summary>
/// Measures which DNS provider answers fastest from this PC. It sends a real, tiny lookup to each provider and times the answer, a few times
/// with different names, and takes the middle time so one slow reply does not decide it. A provider's second address is tried only when the first does not answer. Every provider is asked at the same moment, so
/// one that never answers costs one wait per name, not one wait per provider. It is a rough guide to how
/// near each provider is, not a test of how fast every kind of name resolves. The only requests are these lookups, and only when asked.
/// </summary>
public static class DnsSpeed
{
    /// <summary>Well known names, so the lookups are ordinary ones. A different name each round keeps a resolver's cache from answering all three.</summary>
    public static readonly IReadOnlyList<string> Names = ["example.com", "www.microsoft.com", "www.wikipedia.org"];

    /// <summary>A standard query for the address record of one name: header, then the name as length-prefixed parts, then type A, class IN.</summary>
    public static byte[] BuildQuery(string name, ushort id)
    {
        var bytes = new List<byte> { (byte)(id >> 8), (byte)id, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        foreach (var label in name.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (label.Length is 0 or > 63) throw new ArgumentException($"Not a usable name: {name}", nameof(name));
            bytes.Add((byte)label.Length);
            bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(label));
        }
        bytes.AddRange([0x00, 0x00, 0x01, 0x00, 0x01]);
        return bytes.ToArray();
    }

    /// <summary>True when the packet is a reply to this query: same id, marked as an answer, and not a server failure or refusal.</summary>
    public static bool IsAnswerTo(byte[] query, byte[] response)
    {
        if (query.Length < 12 || response.Length < 12) return false;
        if (response[0] != query[0] || response[1] != query[1]) return false;
        var isResponse = (response[2] & 0x80) != 0;
        var code = response[3] & 0x0F;
        // 0 is success and 3 is "no such name", and either proves the resolver answered. 2 and 5 are a failure and a refusal.
        return isResponse && code is 0 or 3;
    }

    public static async Task<IReadOnlyList<DnsSpeedResult>> MeasureAsync(
        IDnsTransport transport, IEnumerable<DnsProvider> providers, CancellationToken cancel, TimeSpan? timeout = null, int rounds = 3)
    {
        var list = providers.ToList();
        var limit = timeout ?? TimeSpan.FromMilliseconds(1500);
        var samples = list.ToDictionary(p => p.Key, _ => new List<double>());
        var id = (ushort)Random.Shared.Next(1, ushort.MaxValue);

        // One name at a time, every provider at once, so each provider is asked the same thing at the same moment.
        for (var round = 0; round < rounds; round++)
        {
            var query = BuildQuery(Names[round % Names.Count], unchecked((ushort)(id + round)));
            var asked = list.Select(p => AskAsync(transport, p, query, limit, cancel)).ToList();
            var answers = await Task.WhenAll(asked);
            for (var i = 0; i < list.Count; i++)
                if (answers[i] is { } ms) samples[list[i].Key].Add(ms);
        }

        return list.Select(p => new DnsSpeedResult(p, samples[p.Key].Count == 0 ? null : (int)Math.Round(Median(samples[p.Key]))))
            .OrderBy(r => r.Milliseconds is null ? 1 : 0).ThenBy(r => r.Milliseconds).ThenBy(r => r.Provider.Name, StringComparer.Ordinal).ToList();
    }

    /// <summary>The time of the first of the provider's addresses that answers, or null when none does.</summary>
    private static async Task<double?> AskAsync(IDnsTransport transport, DnsProvider provider, byte[] query, TimeSpan limit, CancellationToken cancel)
    {
        foreach (var server in provider.Servers)
        {
            cancel.ThrowIfCancellationRequested();
            if (await transport.RoundTripAsync(server, query, limit, cancel) is { } elapsed) return elapsed.TotalMilliseconds;
        }
        return null;
    }

    private static double Median(List<double> values)
    {
        var sorted = values.Order().ToList();
        var middle = sorted.Count / 2;
        return sorted.Count % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }
}
