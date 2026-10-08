using Clarion.Core.Catalog;
using Clarion.Core.Model;
using Clarion.Engine;

namespace Clarion.Tests;

/// <summary>Read-only checks against the real machine. Nothing is changed.</summary>
public sealed class WindowsReadOnlyTests(Xunit.Abstractions.ITestOutputHelper output)
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

    [Fact]
    public void Cleanup_platform_discovers_this_machine_and_scans_every_row_without_deleting()
    {
        if (!OperatingSystem.IsWindows()) return;
        var platform = new WindowsCleanupPlatform();

        Assert.NotEmpty(platform.RunningProcesses());
        Assert.Contains("Application", platform.EventLogNames());
        Assert.Contains("Security", platform.EventLogNames());
        Assert.True(platform.RecycleBinBytes() >= 0);
        output.WriteLine("steam libraries: " + string.Join(", ", platform.SteamLibraries()));
        output.WriteLine("wow folders: " + string.Join(", ", platform.WowVersionFolders()));

        var engine = new Clarion.Core.Cleanup.CleanupEngine(platform);
        long total = 0;
        foreach (var t in CatalogLoader.LoadCleanup())
        {
            var scan = engine.Scan(t);
            total += scan.Bytes;
            output.WriteLine($"{t.Id,-26} {scan.Bytes / 1048576.0,9:0.0} MB {scan.Files,7} files  running: {string.Join(",", scan.RunningApps)}");
        }
        output.WriteLine($"total {total / 1048576.0:0.0} MB");
        foreach (var rule in CatalogLoader.LoadCleanup().SelectMany(t => t.Rules))
        {
            var folder = rule switch
            {
                Clarion.Core.Cleanup.FolderRule f => platform.Expand(f.Path),
                Clarion.Core.Cleanup.FilePatternRule fp => platform.Expand(fp.Folder),
                _ => null,
            };
            if (folder is null || folder.Contains('*') || !Directory.Exists(folder)) continue;
            Assert.True(Clarion.Core.Cleanup.PathGuard.IsSafe(folder, platform), $"{folder} is on this PC but fails the safety check");
        }
    }
}