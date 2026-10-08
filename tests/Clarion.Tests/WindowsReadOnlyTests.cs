using Clarion.Core.Catalog;
using Clarion.Core.Model;
using Clarion.Engine;

namespace Clarion.Tests;

/// <summary>Read-only checks against the real machine. Nothing is changed.</summary>
public sealed class WindowsReadOnlyTests
{
    [Fact]
    public void Service_store_reads_real_services_and_hides_drivers()
    {
        if (!OperatingSystem.IsWindows()) return;
        var store = new WindowsServiceStore(new WindowsProcessRunner());
        Assert.NotNull(store.GetStartType("Dnscache"));
        Assert.Null(store.GetStartType("NoSuchServiceClarion"));
        Assert.Null(store.GetStartType("Tcpip"));
    }

    [Fact]
    public void Task_store_reads_without_throwing()
    {
        if (!OperatingSystem.IsWindows()) return;
        var store = new WindowsTaskStore();
        Assert.Null(store.GetEnabled(@"\Clarion\NoSuchTask"));
        _ = store.GetEnabled(@"\Microsoft\Windows\Application Experience\ProgramDataUpdater");
    }

    [Fact]
    public void Machine_profile_is_detected()
    {
        if (!OperatingSystem.IsWindows()) return;
        var profile = WindowsMachine.Detect();
        Assert.True(profile.Build >= 10240);
        Assert.NotEqual("Unknown", profile.Edition);
    }

    [Fact]
    public void Validation_rejects_protected_services_and_bad_task_paths()
    {
        const string json = """
        [{
          "id": "bad.svc", "category": "x", "name": "n", "summary": "s", "what": "w", "benefit": "b", "risk": "r",
          "evidence": "Cosmetic", "riskLevel": "Safe", "scope": "Machine",
          "apply": [
            { "type": "service.start-type", "name": "WinDefend", "startType": "Disabled" },
            { "type": "task.set-enabled", "path": "NoSlash", "enabled": false }
          ]
        }]
        """;
        var errors = CatalogLoader.Validate(CatalogLoader.Parse(json));
        Assert.Contains(errors, e => e.Contains("protected service"));
        Assert.Contains(errors, e => e.Contains("start with a backslash"));
    }

    [Fact]
    public void Appx_inventory_runs_for_real_and_parses()
    {
        if (!OperatingSystem.IsWindows()) return;
        var snap = new WindowsAppxStore(new WindowsProcessRunner()).GetSnapshot();
        Assert.NotEmpty(snap.Installed);
        Assert.True(snap.Installed.All(p => p.Name.Length > 0 && p.FamilyName.Length > 0));
        Assert.Contains(snap.Installed, p => p.SignatureKind.Length > 0);
    }
}