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
}
