using Clarion.Core.Model;
using Clarion.Core.SystemInfo;

namespace Clarion.Tests;

public sealed class StorageOrderTests
{
    private static string Json(string disks) => """
        {"osCaption":"Microsoft Windows 11 Pro","osBuild":"26200","osArch":"64-bit","installDate":"2025-03-22T02:15:33.0000000-04:00","boot":"2026-10-08T15:03:38.5000000-04:00","language":"en-US","domain":false,"manufacturer":"Example Co.","model":"Board 1","biosVersion":"F1","biosDate":"2026-06-29","cpu":"Example CPU","cores":8,"threads":16,"memBytes":33944879104,"gpus":[],"disks":
        """ + disks + """
        ,"tpmPresent":true,"tpmReady":true,"secureBoot":true,"vbs":0,"av":[],"licChannel":"Retail","licStatus":1,"licName":"Windows","store":true,"edge":true}
        """;

    private static List<string> Storage(string disks)
    {
        var probe = new SystemProbe(new FakeRegistry(), new FakeServices(), new FakeProcessRunner());
        var report = probe.Build(Json(disks));
        return report.Groups.First(g => g.Name == "Hardware").Facts.Where(f => f.Label == "Storage").Select(f => f.Value).ToList();
    }

    private const string Hdd = """{"Name":"Big HDD","Bytes":1000204886016,"Media":"HDD","Bus":"SATA"}""";
    private const string Nvme = """{"Name":"Boot NVMe","Bytes":512110190592,"Media":"SSD","Bus":"NVMe","Boot":true}""";
    private const string Usb = """{"Name":"Stick","Bytes":250000000000,"Media":"SSD","Bus":"USB"}""";

    [Fact]
    public void The_disk_windows_starts_from_comes_first_and_is_marked()
    {
        var disks = Storage($"[{Usb},{Hdd},{Nvme}]");

        Assert.StartsWith("Boot NVMe", disks[0]);
        Assert.EndsWith("Windows drive", disks[0]);
        Assert.DoesNotContain("Windows drive", disks[1]);
        Assert.DoesNotContain("Windows drive", disks[2]);
    }

    [Fact]
    public void Without_a_boot_flag_internal_disks_come_before_usb_and_larger_before_smaller()
    {
        var older = Storage($"[{Usb},{Nvme.Replace(",\"Boot\":true", "")},{Hdd}]");

        Assert.StartsWith("Big HDD", older[0]);
        Assert.StartsWith("Boot NVMe", older[1]);
        Assert.StartsWith("Stick", older[2]);
        Assert.All(older, d => Assert.DoesNotContain("Windows drive", d));
    }

    [Fact]
    public void A_single_disk_reads_as_before()
    {
        var disks = Storage($"[{Hdd}]");

        Assert.Equal(["Big HDD, 931.5 GB, HDD on SATA"], disks);
    }
}
