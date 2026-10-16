using System.Runtime.Versioning;
using Clarion.Core.Catalog;
using Clarion.Core.Model;
using Clarion.Core.WindowsServices;
using Clarion.Engine;

namespace Clarion.Tests;

public sealed class ServiceGuideTests
{
    private static readonly ServiceGuide Guide = ServiceGuide.LoadEmbedded();

    [Fact]
    public void The_guide_holds_all_of_Microsofts_table_with_unique_names()
    {
        Assert.Equal(255, Guide.Entries.Count);
        Assert.Equal(Guide.Entries.Count, Guide.Entries.Select(e => e.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(23, Guide.Entries.Count(e => e.PerUser));
        Assert.StartsWith("https://learn.microsoft.com/", Guide.Source, StringComparison.Ordinal);
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", Guide.Dated);
    }

    [Fact]
    public void The_counts_match_the_table_as_it_was_read()
    {
        var advice = Guide.Entries.GroupBy(e => e.Advice).ToDictionary(g => g.Key, g => g.Count());
        Assert.Equal(133, advice[ServiceAdvice.OkToDisable]);
        Assert.Equal(91, advice[ServiceAdvice.DontDisable]);
        Assert.Equal(20, advice[ServiceAdvice.NoGuidance]);
        Assert.Equal(7, advice[ServiceAdvice.AlreadyDisabled]);
        Assert.Equal(4, advice[ServiceAdvice.ShouldDisable]);

        var start = Guide.Entries.GroupBy(e => e.Default).ToDictionary(g => g.Key, g => g.Count());
        Assert.Equal(188, start[ServiceStartType.Manual]);
        Assert.Equal(57, start[ServiceStartType.Automatic]);
        Assert.Equal(3, start[ServiceStartType.AutomaticDelayed]);
        Assert.Equal(7, start[ServiceStartType.Disabled]);
    }

    [Fact]
    public void A_per_user_copy_is_found_under_its_template_and_nothing_else_is_guessed()
    {
        Assert.Equal("CDPUserSvc", Guide.Find("CDPUserSvc_4a2b3")?.Name);
        Assert.Equal("CDPUserSvc", Guide.TemplateName("CDPUserSvc_4a2b3"));
        Assert.Equal("Spooler", Guide.Find("spooler")?.Name);
        Assert.Null(Guide.Find("NoSuchService"));
        // A name that only looks like a copy is not one unless its template is a per-user service in the table.
        Assert.Null(Guide.Find("Spooler_4a2b3"));
        Assert.Equal("Whatever_abcd", Guide.TemplateName("Whatever_abcd"));
    }

    [Fact]
    public void Every_service_the_catalog_changes_is_one_Microsoft_says_can_be_switched_off()
    {
        var names = CatalogLoader.LoadEmbedded()
            .SelectMany(t => t.Apply.Concat(t.Undo).OfType<SetServiceStartType>().Select(s => (t.Id, s.Name)))
            .ToList();
        Assert.NotEmpty(names);
        var wrong = names
            .Where(x => Guide.Find(x.Name)?.Advice is not (ServiceAdvice.OkToDisable or ServiceAdvice.ShouldDisable))
            .Select(x => $"{x.Id} -> {x.Name}: {Guide.Find(x.Name)?.Advice.ToString() ?? "not in the table"}")
            .ToList();
        Assert.Empty(wrong);
    }

    [Fact]
    public void The_protected_list_is_stricter_than_Microsofts_table_in_exactly_these_places()
    {
        // Microsoft's table is for fixed function devices, so it calls these OK to disable. On a PC people use, switching them off
        // breaks sound, Windows Update, file sharing or networking, so .Clarion refuses them. Adding to this list is a decision.
        var stricter = CatalogLoader.ProtectedServices.Where(s => Guide.Find(s)?.Advice is ServiceAdvice.OkToDisable).Order(StringComparer.Ordinal).ToList();
        Assert.Equal(
            ["AudioEndpointBuilder", "AudioSrv", "BITS", "LanmanServer", "NcbService", "NlaSvc", "ShellHWDetection", "Themes", "VSS", "wuauserv"],
            stricter);
    }

    [Fact]
    public void The_words_have_no_dashes_and_every_value_has_some()
    {
        foreach (var a in Enum.GetValues<ServiceAdvice>())
        {
            Assert.DoesNotMatch("[–—]", ServiceGuide.Label(a) + ServiceGuide.Hint(a));
            Assert.NotEmpty(ServiceGuide.Label(a));
            Assert.NotEmpty(ServiceGuide.Hint(a));
        }
        foreach (var t in Enum.GetValues<ServiceStartType>()) Assert.NotEmpty(ServiceGuide.StartLabel(t));
    }

    private static ServiceSnapshot Snap(string name, ServiceStartType? start, bool running = false, string? display = null) =>
        new(name, display ?? name, start, running, false, null, false);

    private static Tweak Setting(string id, string name, params string[] services) => new()
    {
        Id = id,
        Category = "System",
        Name = name,
        Summary = "s",
        What = "w",
        Benefit = "b",
        Risk = "r",
        Evidence = Evidence.Situational,
        RiskLevel = RiskLevel.Low,
        Scope = TweakScope.Machine,
        Apply = [.. services.Select(s => (Operation)new SetServiceStartType(s, ServiceStartType.Disabled))],
    };

    [Fact]
    public void A_row_says_when_the_start_type_differs_from_the_table_and_never_when_the_service_is_not_listed()
    {
        var rows = ServiceRows.Build(
            [Snap("Spooler", ServiceStartType.Disabled), Snap("TrkWks", ServiceStartType.Automatic), Snap("VendorThing", ServiceStartType.Manual), Snap("Odd", null)],
            Guide, [], _ => false);
        Assert.True(rows.Single(r => r.Name == "Spooler").DiffersFromGuide);
        Assert.False(rows.Single(r => r.Name == "TrkWks").DiffersFromGuide);
        Assert.False(rows.Single(r => r.Name == "VendorThing").DiffersFromGuide);
        Assert.False(rows.Single(r => r.Name == "Odd").DiffersFromGuide);
        Assert.Null(rows.Single(r => r.Name == "VendorThing").Guide);
    }

    [Fact]
    public void A_service_is_linked_to_the_setting_that_changes_it_and_shows_whether_it_is_on()
    {
        var set = Setting("services.x", "Turn off X", "Spooler", "TrkWks");
        var rows = ServiceRows.Build([Snap("Spooler", ServiceStartType.Disabled), Snap("TrkWks", ServiceStartType.Automatic), Snap("Dnscache", ServiceStartType.Automatic)],
            Guide, [set], t => t.Id == "services.x");
        Assert.Equal("Turn off X", rows.Single(r => r.Name == "Spooler").Setting?.Name);
        Assert.True(rows.Single(r => r.Name == "TrkWks").SettingOn);
        Assert.Null(rows.Single(r => r.Name == "Dnscache").Setting);
        Assert.True(rows.Single(r => r.Name == "Dnscache").Protected);
        Assert.False(rows.Single(r => r.Name == "Spooler").Protected);
    }

    [Fact]
    public void The_filters_and_the_search_narrow_the_list_and_never_change_it()
    {
        var set = Setting("services.x", "Turn off printing", "Spooler");
        var all = ServiceRows.Build(
            [Snap("Spooler", ServiceStartType.Disabled, display: "Print Spooler"), Snap("AarSvc", ServiceStartType.Manual, running: true, display: "Agent Activation Runtime"),
             Snap("AxInstSV", ServiceStartType.Manual, display: "ActiveX Installer"), Snap("VendorThing", ServiceStartType.Manual)],
            Guide, [set], _ => false);

        Assert.Equal(4, ServiceRows.Filter(all, ServiceFilter.All, "").Count);
        Assert.Equal(["AarSvc"], ServiceRows.Filter(all, ServiceFilter.Running, "").Select(r => r.Name));
        Assert.Equal(["Spooler"], ServiceRows.Filter(all, ServiceFilter.DiffersFromGuide, "").Select(r => r.Name));
        Assert.Equal(["Spooler"], ServiceRows.Filter(all, ServiceFilter.HasSetting, "").Select(r => r.Name));
        Assert.Equal(["AarSvc"], ServiceRows.Filter(all, ServiceFilter.DontDisable, "").Select(r => r.Name));
        Assert.Equal(["VendorThing"], ServiceRows.Filter(all, ServiceFilter.NotListed, "").Select(r => r.Name));
        Assert.Equal(["AxInstSV", "Spooler"], ServiceRows.Filter(all, ServiceFilter.OkToDisable, "").Select(r => r.Name).Order());
        Assert.Equal(["Spooler"], ServiceRows.Filter(all, ServiceFilter.All, "print spool").Select(r => r.Name));
        Assert.Equal(["Spooler"], ServiceRows.Filter(all, ServiceFilter.All, "turn off printing").Select(r => r.Name));
        Assert.Empty(ServiceRows.Filter(all, ServiceFilter.All, "nothing like this"));
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void The_real_list_of_services_reads_this_PC_without_changing_it()
    {
        if (!OperatingSystem.IsWindows()) return;
        var list = new WindowsServiceLister().List();
        Assert.True(list.Count > 50, $"Only {list.Count} services were read");
        Assert.Equal(list.Count, list.Select(s => s.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        var log = Assert.Single(list, s => s.Name.Equals("EventLog", StringComparison.OrdinalIgnoreCase));
        Assert.True(log.Running);
        Assert.NotNull(log.Start);
        // Something real is known about the services Windows ships.
        Assert.True(list.Count(s => Guide.Find(s.Name) is not null) > 100);
        // A per-user copy is shown once, under its template's name, with no suffix left on its display name.
        Assert.DoesNotContain(list, s => System.Text.RegularExpressions.Regex.IsMatch(s.DisplayName, "_[0-9a-f]{4,8}$") && Guide.Find(s.Name)?.PerUser == true);
    }
}
