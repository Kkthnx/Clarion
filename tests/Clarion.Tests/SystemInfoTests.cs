using Clarion.Core.Model;
using Clarion.Core.SystemInfo;

namespace Clarion.Tests;

public sealed class SystemInfoTests
{
    private const string Sample = """
        {"osCaption":"Microsoft Windows 11 Pro","osBuild":"26200","osArch":"64-bit","installDate":"2025-03-22T02:15:33.0000000-04:00","boot":"2026-10-08T15:03:38.5000000-04:00","language":"en-US","domain":false,"manufacturer":"Example Co.","model":"Board 1","biosVersion":"F1","biosDate":"2026-06-29","cpu":"Example 8-Core Processor   ","cores":8,"threads":16,"memBytes":33944879104,"gpus":[{"Name":"Example GPU","Driver":"1.2.3"}],"disks":[{"Name":"Disk A","Bytes":1000204886016,"Media":"SSD","Bus":"NVMe"}],"tpmPresent":true,"tpmReady":true,"secureBoot":true,"vbs":0,"av":["Windows Defender"],"licChannel":"Retail","licStatus":1,"licName":"Windows(R), Professional edition","store":true,"edge":true}
        """;

    private static readonly string[] AllServices = ["wuauserv", "WinDefend", .. InstallCheck.CoreServices];

    private static (SystemProbe Probe, FakeRegistry Registry, FakeServices Services) Make(bool withServices = true)
    {
        var registry = new FakeRegistry();
        var services = new FakeServices();
        if (withServices) foreach (var s in AllServices) services.Set(s, ServiceStartType.Manual);
        return (new SystemProbe(registry, services, new FakeProcessRunner()), registry, services);
    }

    private static RegistryTarget Local(string path, string name) => new(RegistryHive.LocalMachine, path, name);

    [Fact]
    public void A_standard_install_reads_as_standard_and_lists_the_facts()
    {
        var (probe, _, _) = Make();
        var report = probe.Build(Sample);

        Assert.Equal(InstallLevel.Standard, report.Install.Level);
        Assert.Empty(report.Install.Signals);
        var text = report.ToText();
        Assert.Contains("Windows 11 Pro", text);
        Assert.Contains("Example GPU", text);
        Assert.Contains("Disk A", text);
        Assert.Contains("Present and ready", text);
        Assert.Contains("Activated (Retail)", text);
    }

    [Fact]
    public void A_named_custom_image_is_flagged_as_likely()
    {
        var (probe, registry, _) = Make();
        registry.Write(Local("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OEMInformation", "Manufacturer"),
            new RegistryData(RegistryKind.String, "Ghost Spectre Edition"));

        var report = probe.Build(Sample);

        Assert.Equal(InstallLevel.Likely, report.Install.Level);
        Assert.Contains(report.Install.Signals, s => s.Text.Contains("Ghost Spectre"));
    }

    [Fact]
    public void A_stripped_image_with_no_name_is_still_caught_by_what_is_missing()
    {
        var (probe, registry, services) = Make(withServices: false);
        services.Set("BITS", ServiceStartType.Manual);
        registry.Write(Local("SYSTEM\\Setup\\LabConfig", "BypassTPMCheck"), new RegistryData(RegistryKind.DWord, "1"));

        var report = probe.Build(Sample.Replace("\"store\":true", "\"store\":false"));

        Assert.Equal(InstallLevel.Likely, report.Install.Level);
        Assert.Contains(report.Install.Signals, s => s.Text.Contains("Windows Update service is not installed"));
        Assert.Contains(report.Install.Signals, s => s.Text.Contains("skip the hardware checks"));
    }

    [Fact]
    public void One_missing_browser_alone_is_not_enough_to_raise_a_flag()
    {
        var (probe, _, _) = Make();
        var report = probe.Build(Sample.Replace("\"edge\":true", "\"edge\":false"));
        Assert.Equal(InstallLevel.Standard, report.Install.Level);
        Assert.Single(report.Install.Signals);
    }

    [Fact]
    public void Missing_or_odd_data_never_crashes_the_report()
    {
        var (probe, _, _) = Make();
        var report = probe.Build("{}");
        Assert.NotEmpty(report.Groups);
        Assert.Contains("Unknown", report.ToText());
        Assert.ThrowsAny<System.Text.Json.JsonException>(() => probe.Build("not json"));
    }

    [Fact]
    public void Volume_license_on_a_home_pc_is_a_weak_hint_but_not_on_a_domain()
    {
        var plain = InstallCheck.Evaluate(new InstallInputs { LicenseChannel = "Volume:GVLK" });
        var joined = InstallCheck.Evaluate(new InstallInputs { LicenseChannel = "Volume:GVLK", Domain = true });
        Assert.Single(plain.Signals);
        Assert.Empty(joined.Signals);
        Assert.Equal(InstallLevel.Standard, plain.Level);
    }

    private static readonly string[] FiveChecks = ["BypassTPMCheck", "BypassSecureBootCheck", "BypassRAMCheck", "BypassCPUCheck", "BypassStorageCheck"];

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Fact]
    public void The_Ghost_Toolbox_names_Ghost_Spectre_even_when_the_system_information_does_not()
    {
        var (probe, _, _) = Make();
        var report = probe.Build(Sample.Replace("\"store\":true", "\"store\":true,\"ghostToolbox\":true"));

        Assert.Equal(InstallLevel.Likely, report.Install.Level);
        Assert.Equal("Ghost Spectre", report.Install.ImageName);
        Assert.True(report.Install.ImageNameIsFirm);
        Assert.Equal("This looks like Ghost Spectre, a customized Windows image", report.Install.Headline);
        Assert.Contains(report.Install.Signals, s => s.Text.Contains("Ghost Toolbox"));
    }

    [Fact]
    public void Tiny11_is_only_guessed_from_the_full_pattern_and_the_guess_says_so()
    {
        var (probe, registry, _) = Make();
        foreach (var n in FiveChecks) registry.Write(Local("SYSTEM\\Setup\\LabConfig", n), new RegistryData(RegistryKind.DWord, "1"));
        registry.Write(Local("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OOBE", "BypassNRO"), new RegistryData(RegistryKind.DWord, "1"));

        var report = probe.Build(Sample);

        Assert.Equal("tiny11", report.Install.ImageName);
        Assert.False(report.Install.ImageNameIsFirm);
        Assert.Equal("possibly tiny11", report.Install.ImageLabel);
        Assert.Contains("possibly tiny11", report.Install.Headline);
    }

    [Fact]
    public void Three_of_the_five_bypass_values_is_not_the_tiny11_pattern()
    {
        // Read from a real Ghost Spectre PC, which skips the TPM, Secure Boot and CPU checks only.
        var (probe, registry, _) = Make();
        foreach (var n in new[] { "BypassTPMCheck", "BypassSecureBootCheck", "BypassCPUCheck" })
            registry.Write(Local("SYSTEM\\Setup\\LabConfig", n), new RegistryData(RegistryKind.DWord, "1"));

        var report = probe.Build(Sample);

        Assert.Null(report.Install.ImageName);
        Assert.Contains(report.Install.Signals, s => s.Text.Contains("skip the hardware checks"));
    }

    [Fact]
    public void Windows_Update_turned_off_and_pointed_at_this_PC_looks_like_tiny11_Core()
    {
        var (probe, registry, services) = Make();
        services.Set("wuauserv", ServiceStartType.Disabled);
        const string policy = "SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate";
        registry.Write(Local(policy, "WUServer"), new RegistryData(RegistryKind.String, "localhost"));
        registry.Write(Local(policy + "\\AU", "UseWUServer"), new RegistryData(RegistryKind.DWord, "1"));
        registry.Write(Local(policy + "\\AU", "NoAutoUpdate"), new RegistryData(RegistryKind.DWord, "1"));

        var report = probe.Build(Sample);

        Assert.Equal("tiny11 Core", report.Install.ImageName);
        Assert.Equal(UpdateLevel.Held, report.Updates!.Level);
        Assert.Contains("Windows Update is held back", report.ToText());
    }

    [Fact]
    public void The_report_reads_the_pause_values_and_the_dates_windows_keeps()
    {
        var (probe, registry, _) = Make();
        const string ux = "SOFTWARE\\Microsoft\\WindowsUpdate\\UX\\Settings";
        const string status = "SOFTWARE\\Microsoft\\WindowsUpdate\\UpdatePolicy\\Settings";
        foreach (var n in new[] { "PauseUpdatesExpiryTime", "PauseFeatureUpdatesEndTime", "PauseQualityUpdatesEndTime" })
            registry.Write(Local(ux, n), new RegistryData(RegistryKind.String, "2077-01-01T10:38:56Z"));
        registry.Write(Local(status, "PausedFeatureStatus"), new RegistryData(RegistryKind.DWord, "2"));
        registry.Write(Local(status, "PausedQualityStatus"), new RegistryData(RegistryKind.DWord, "2"));
        var json = Sample.Replace("\"store\":true", "\"store\":true,\"lastCheck\":\"2026-10-10T18:23:38.0000000Z\",\"lastInstall\":\"2026-10-10T19:51:11.0000000Z\"");
        var probeWithClock = new SystemProbe(registry, FakeServicesWith(AllServices), new FakeProcessRunner(), null, new FixedClock(new DateTimeOffset(2026, 10, 10, 21, 0, 0, TimeSpan.Zero)));

        var report = probeWithClock.Build(json);

        Assert.Equal(UpdateLevel.Working, report.Updates!.Level);
        Assert.Contains(report.Updates.Findings, f => f.Text.Contains("reports it as ended"));
        Assert.Contains(report.Updates.Findings, f => f.Kind == UpdateFindingKind.Good && f.Text.Contains("installed an update"));
        Assert.Equal(new DateTimeOffset(2077, 1, 1, 10, 38, 56, TimeSpan.Zero), report.Updates.Inputs!.PauseEnds);
    }

    [Fact]
    public void Windows_placeholder_dates_for_never_are_not_read_as_real()
    {
        var (probe, _, _) = Make();
        var report = probe.Build(Sample.Replace("\"store\":true", "\"store\":true,\"lastCheck\":\"1899-12-30T05:00:00.0000000Z\",\"lastInstall\":null"));
        Assert.Null(report.Updates!.Inputs!.LastCheck);
        Assert.Null(report.Updates.Inputs.LastInstall);
    }

    [Fact]
    public void A_disabled_scan_task_is_read_from_the_task_store()
    {
        var registry = new FakeRegistry();
        var tasks = new FakeTasks();
        tasks.Set("\\Microsoft\\Windows\\UpdateOrchestrator\\Schedule Scan", false);
        var probe = new SystemProbe(registry, FakeServicesWith(AllServices), new FakeProcessRunner(), tasks);

        var report = probe.Build(Sample);

        Assert.Equal(false, report.Updates!.Inputs!.ScanTaskEnabled);
        Assert.Equal(UpdateLevel.Limited, report.Updates.Level);
    }

    private static FakeServices FakeServicesWith(IEnumerable<string> names)
    {
        var s = new FakeServices();
        foreach (var n in names) s.Set(n, ServiceStartType.Manual);
        return s;
    }
}
