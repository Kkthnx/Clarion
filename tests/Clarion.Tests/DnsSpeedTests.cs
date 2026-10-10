using Clarion.Core.Dns;

namespace Clarion.Tests;

public sealed class DnsSpeedTests
{
    private sealed class FakeTransport(Func<string, int, TimeSpan?> answer) : IDnsTransport
    {
        private readonly Dictionary<string, int> _calls = new();
        public List<string> Servers { get; } = [];

        public Task<TimeSpan?> RoundTripAsync(string server, byte[] query, TimeSpan timeout, CancellationToken cancel)
        {
            Servers.Add(server);
            _calls[server] = _calls.GetValueOrDefault(server) + 1;
            return Task.FromResult(answer(server, _calls[server]));
        }
    }

    [Fact]
    public void The_query_is_a_standard_lookup_for_an_address_record()
    {
        var packet = DnsSpeed.BuildQuery("example.com", 0x1234);

        Assert.Equal(
            [0x12, 0x34, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
             7, (byte)'e', (byte)'x', (byte)'a', (byte)'m', (byte)'p', (byte)'l', (byte)'e', 3, (byte)'c', (byte)'o', (byte)'m', 0,
             0x00, 0x01, 0x00, 0x01],
            packet);
        Assert.Throws<ArgumentException>(() => DnsSpeed.BuildQuery(new string('a', 64) + ".com", 1));
    }

    [Fact]
    public void Only_a_matching_answer_counts_and_a_failure_or_refusal_does_not()
    {
        var query = DnsSpeed.BuildQuery("example.com", 0xABCD);
        byte[] Reply(byte id1, byte id2, byte flags, byte code) => [id1, id2, flags, code, 0, 1, 0, 1, 0, 0, 0, 0];

        Assert.True(DnsSpeed.IsAnswerTo(query, Reply(0xAB, 0xCD, 0x81, 0x80)));      // success
        Assert.True(DnsSpeed.IsAnswerTo(query, Reply(0xAB, 0xCD, 0x81, 0x83)));      // no such name still proves it answered
        Assert.False(DnsSpeed.IsAnswerTo(query, Reply(0xAB, 0xCD, 0x81, 0x82)));     // server failure
        Assert.False(DnsSpeed.IsAnswerTo(query, Reply(0xAB, 0xCD, 0x81, 0x85)));     // refused
        Assert.False(DnsSpeed.IsAnswerTo(query, Reply(0x00, 0x01, 0x81, 0x80)));     // someone else's answer
        Assert.False(DnsSpeed.IsAnswerTo(query, Reply(0xAB, 0xCD, 0x01, 0x00)));     // a query, not an answer
        Assert.False(DnsSpeed.IsAnswerTo(query, [1, 2, 3]));
    }

    [Fact]
    public async Task The_middle_time_decides_so_one_slow_reply_does_not()
    {
        // Google answers in 10, 200 and 12 ms. Cloudflare answers in 20 ms each time. The middle time puts Google first.
        var transport = new FakeTransport((server, call) => server switch
        {
            "8.8.8.8" => TimeSpan.FromMilliseconds(call == 2 ? 200 : call == 1 ? 10 : 12),
            "1.1.1.1" => TimeSpan.FromMilliseconds(20),
            _ => null,
        });

        var result = await DnsSpeed.MeasureAsync(transport, DnsProviders.All.Where(p => p.Key is "google" or "cloudflare"), default);

        Assert.Equal(["google", "cloudflare"], result.Select(r => r.Provider.Key).ToArray());
        Assert.Equal(12, result[0].Milliseconds);
        Assert.Equal(20, result[1].Milliseconds);
    }

    [Fact]
    public async Task A_provider_that_never_answers_is_listed_last_with_no_time()
    {
        var transport = new FakeTransport((server, _) => server == "9.9.9.9" ? null : TimeSpan.FromMilliseconds(30));

        var result = await DnsSpeed.MeasureAsync(transport, DnsProviders.All, default);

        Assert.Equal("quad9", result[^1].Provider.Key);
        Assert.Null(result[^1].Milliseconds);
        Assert.All(result.Take(result.Count - 1), r => Assert.Equal(30, r.Milliseconds));
    }

    [Fact]
    public async Task One_late_answer_still_gives_a_time_and_each_provider_is_asked_the_same_number_of_times()
    {
        var transport = new FakeTransport((server, call) => server == "1.1.1.1" && call != 3 ? null : TimeSpan.FromMilliseconds(40));

        var result = await DnsSpeed.MeasureAsync(transport, DnsProviders.All.Where(p => p.Key is "cloudflare" or "google"), default);

        Assert.All(result, r => Assert.Equal(40, r.Milliseconds));
        Assert.Equal(3, transport.Servers.Count(s => s == "1.1.1.1"));
        Assert.Equal(3, transport.Servers.Count(s => s == "8.8.8.8"));
    }

    [Fact]
    public async Task Stopping_stops_the_measuring()
    {
        using var stop = new CancellationTokenSource();
        stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => DnsSpeed.MeasureAsync(new FakeTransport((_, _) => TimeSpan.Zero), DnsProviders.All, stop.Token));
    }

    [Fact]
    public async Task The_real_transport_gets_a_usable_answer_from_a_public_resolver_or_reports_none_without_throwing()
    {
        // Needs the network, so it only checks the shape: a time of zero or more, or null when the PC is offline.
        var transport = new Clarion.Engine.UdpDnsTransport();
        var elapsed = await transport.RoundTripAsync("1.1.1.1", DnsSpeed.BuildQuery("example.com", 7), TimeSpan.FromSeconds(3), default);
        Assert.True(elapsed is null || elapsed.Value >= TimeSpan.Zero);
    }
}
