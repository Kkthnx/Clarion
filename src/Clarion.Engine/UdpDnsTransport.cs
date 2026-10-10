using System.Diagnostics;
using System.Net.Sockets;
using Clarion.Core.Dns;

namespace Clarion.Engine;

/// <summary>Sends one DNS query over UDP to port 53 and times the reply.</summary>
public sealed class UdpDnsTransport : IDnsTransport
{
    public async Task<TimeSpan?> RoundTripAsync(string server, byte[] query, TimeSpan timeout, CancellationToken cancel)
    {
        try
        {
            using var udp = new UdpClient();
            udp.Connect(server, 53);
            using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancel);
            limit.CancelAfter(timeout);
            var clock = Stopwatch.StartNew();
            await udp.SendAsync(query, limit.Token);
            var reply = await udp.ReceiveAsync(limit.Token);
            clock.Stop();
            return DnsSpeed.IsAnswerTo(query, reply.Buffer) ? clock.Elapsed : null;
        }
        catch (OperationCanceledException) when (!cancel.IsCancellationRequested) { return null; }
        catch (SocketException) { return null; }
    }
}
