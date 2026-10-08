using Clarion.Core.Abstractions;
using Clarion.Core.Dns;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

/// <summary>
/// Points every connected physical adapter at a public DNS provider, optionally with encryption,
/// and remembers exactly what each adapter and each encryption entry looked like before.
/// </summary>
public sealed class DnsHandler(IDnsStore store) : IOperationHandler
{
    private readonly object _gate = new();
    private IReadOnlyList<AdapterDns>? _adapters;
    private IReadOnlyList<DohRegistration>? _doh;

    public bool Handles(Operation op) => op is SetDnsProvider or RestoreDns;

    // Each lookup starts PowerShell, so the answers are kept until something is written or Invalidate is called.
    private IReadOnlyList<AdapterDns> Adapters()
    {
        lock (_gate) return _adapters ??= store.GetAdapters();
    }

    private IReadOnlyList<DohRegistration> Doh()
    {
        lock (_gate) return _doh ??= store.GetDohRegistrations();
    }

    public void Invalidate()
    {
        lock (_gate) { _adapters = null; _doh = null; }
    }

    public void Warm(IReadOnlyList<Operation> operations)
    {
        if (!operations.Any(Handles)) return;
        Parallel.Invoke(() => Adapters(), () => Doh());
    }

    public bool IsApplicable(Operation op) => Adapters().Count > 0;

    public bool IsSatisfied(Operation op)
    {
        var adapters = Adapters();
        if (adapters.Count == 0) return false;
        var doh = Doh();
        switch (op)
        {
            case SetDnsProvider s:
                var provider = DnsProviders.Find(s.Provider);
                if (provider is null) return false;
                if (!adapters.All(a => DnsProviders.SameServers(a.Servers, provider.Servers))) return false;
                return !s.Encrypted || provider.Servers.All(ip => doh.Any(r => r.Address == ip && r.AutoUpgrade));
            case RestoreDns r:
                return r.Adapters.All(prev => adapters.FirstOrDefault(a => a.IfIndex == prev.IfIndex) is { } now
                                              && DnsProviders.SameServers(now.Servers, prev.Servers))
                       && r.RemoveDohFor.All(ip => doh.All(x => x.Address != ip))
                       && r.RestoreDoh.All(prev => doh.Any(x => x.Address == prev.Address && x.AutoUpgrade == prev.AutoUpgrade));
            default:
                return false;
        }
    }

    public Operation CaptureUndo(Operation op)
    {
        var adapters = Adapters();
        var remove = new List<string>();
        var restore = new List<DohRegistration>();
        if (op is SetDnsProvider { Encrypted: true } s && DnsProviders.Find(s.Provider) is { } p)
        {
            var existing = Doh();
            foreach (var ip in p.Servers)
            {
                var found = existing.FirstOrDefault(x => x.Address == ip);
                if (found is null) remove.Add(ip);
                else restore.Add(found);
            }
        }
        return new RestoreDns(adapters, remove, restore);
    }

    public void Execute(Operation op)
    {
        try { Run(op); }
        finally { Invalidate(); }
    }

    private void Run(Operation op)
    {
        switch (op)
        {
            case SetDnsProvider s:
                var provider = DnsProviders.Find(s.Provider) ?? throw new InvalidOperationException($"Unknown DNS provider {s.Provider}");
                if (s.Encrypted)
                {
                    foreach (var ip in provider.Servers) store.UpsertDoh(new DohRegistration(ip, provider.DohTemplate, AutoUpgrade: true, AllowFallbackToUdp: false));
                }
                foreach (var a in store.GetAdapters()) store.SetServers(a.IfIndex, provider.Servers);
                break;
            case RestoreDns r:
                foreach (var prev in r.Adapters)
                {
                    if (prev.Servers.Count == 0) store.ResetServers(prev.IfIndex);
                    else store.SetServers(prev.IfIndex, prev.Servers);
                }
                foreach (var prev in r.RestoreDoh) store.UpsertDoh(prev);
                foreach (var ip in r.RemoveDohFor) store.RemoveDoh(ip);
                break;
        }
    }
}
