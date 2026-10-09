using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

/// <summary>
/// Structural rules for the catalog, so they hold as it grows without anyone having to remember them.
/// </summary>
public sealed class CatalogDisciplineTests
{
    // Kept here on purpose, separate from CatalogLoader.ProtectedServices, so removing a name
    // from the production list also fails a test.
    private static readonly string[] ServiceDenylist =
    [
        "wuauserv", "UsoSvc", "WaaSMedicSvc", "BITS", "TrustedInstaller", "DoSvc",
        "DPS", "WdiServiceHost", "WdiSystemHost", "DusmSvc",
        "WinDefend", "WdNisSvc", "Sense", "SecurityHealthService", "wscsvc", "mpssvc", "BFE",
        "EventLog", "RpcSs", "RpcEptMapper", "DcomLaunch", "CryptSvc", "Dhcp", "Dnscache", "nsi",
        "Winmgmt", "Schedule", "gpsvc", "StateRepository", "TimeBrokerSvc", "ProfSvc", "UserManager",
        "NlaSvc", "netprofm", "LanmanWorkstation", "PlugPlay", "Power", "AudioSrv", "AudioEndpointBuilder",
        "KeyIso", "VSS",
    ];

    // The only services Clarion is allowed to change. Adding one means adding it here deliberately.
    private static readonly string[] ServiceAllowlist = ["DiagTrack", "dmwappushservice"];

    [Fact]
    public void No_tweak_touches_a_denylisted_service_in_apply_or_undo()
    {
        var hits = CatalogLoader.LoadEmbedded()
            .SelectMany(t => t.Apply.Concat(t.Undo).OfType<SetServiceStartType>().Select(s => (t.Id, s.Name)))
            .Where(x => ServiceDenylist.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .Select(x => $"{x.Id} -> {x.Name}")
            .ToList();
        Assert.Empty(hits);
    }

    [Fact]
    public void Only_allowlisted_services_are_ever_changed()
    {
        var unknown = CatalogLoader.LoadEmbedded()
            .SelectMany(t => t.Apply.Concat(t.Undo).OfType<SetServiceStartType>().Select(s => (t.Id, s.Name)))
            .Where(x => !ServiceAllowlist.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .Select(x => $"{x.Id} -> {x.Name}")
            .ToList();
        Assert.Empty(unknown);
    }

    [Fact]
    public void The_runtime_guard_covers_every_denylisted_service()
    {
        var missing = ServiceDenylist.Where(s => !CatalogLoader.ProtectedServices.Contains(s)).ToList();
        Assert.Empty(missing);
    }

    [Fact]
    public void Allowlisted_services_are_not_protected()
    {
        Assert.DoesNotContain(ServiceAllowlist, CatalogLoader.ProtectedServices.Contains);
    }

    [Fact]
    public void Validation_rejects_a_service_tweak_on_a_denylisted_service()
    {
        const string json = """
        [{
          "id": "bad.dps", "category": "x", "topic": "t", "name": "n", "summary": "s", "what": "w", "benefit": "b", "risk": "r",
          "evidence": "Situational", "riskLevel": "Low", "scope": "Machine",
          "apply": [ { "type": "service.start-type", "name": "DPS", "startType": "Disabled" } ]
        }]
        """;
        Assert.Contains(CatalogLoader.Validate(CatalogLoader.Parse(json)), e => e.Contains("protected service"));
    }

    [Fact]
    public void Every_tweak_records_the_build_it_was_last_checked_on()
    {
        var missing = CatalogLoader.LoadEmbedded().Where(t => t.LastVerifiedBuild <= 0).Select(t => t.Id).ToList();
        Assert.Empty(missing);
    }

    [Fact]
    public void No_tweak_is_stale()
    {
        var stale = CatalogLoader.LoadEmbedded()
            .Where(t => t.LastVerifiedBuild > 0 && t.LastVerifiedBuild < CatalogFreshness.OldestAcceptableBuild)
            .Select(t => $"{t.Id} (checked on {t.LastVerifiedBuild}, need {CatalogFreshness.OldestAcceptableBuild} or later)")
            .ToList();
        Assert.Empty(stale);
    }

    [Fact]
    public void No_tweak_claims_a_build_newer_than_the_catalog_has_reviewed()
    {
        var ahead = CatalogLoader.LoadEmbedded().Where(t => t.LastVerifiedBuild > CatalogFreshness.NewestKnownBuild).Select(t => t.Id).ToList();
        Assert.Empty(ahead);
    }

    [Fact]
    public void Tooltip_shows_the_last_checked_build()
    {
        var t = CatalogLoader.LoadEmbedded().First();
        Assert.Contains(TweakTooltip.Build(t).Notes, n => n.Contains($"build {t.LastVerifiedBuild}"));
    }
}
