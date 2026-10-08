using System.Globalization;
using System.Text.Json;
using Clarion.Core.Abstractions;
using Clarion.Core.Engine;
using Clarion.Core.Model;

namespace Clarion.Core.SystemInfo;

/// <summary>Collects facts about this PC and the Windows install, then judges whether the image looks customized.</summary>
public sealed class SystemProbe(IRegistryStore registry, IServiceStore services, IProcessRunner runner)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(90);

    private const string CurrentVersion = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
    private const string Oem = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OEMInformation";
    private const string LabConfig = "SYSTEM\\Setup\\LabConfig";

    // One PowerShell start collects everything slow, which keeps the page quick to open.
    internal const string Script = """
        $ErrorActionPreference = 'SilentlyContinue'
        function Try-Get([scriptblock]$s) { try { & $s } catch { $null } }
        $os = Get-CimInstance Win32_OperatingSystem
        $cs = Get-CimInstance Win32_ComputerSystem
        $bios = Get-CimInstance Win32_BIOS
        $cpu = @(Get-CimInstance Win32_Processor)
        $gpu = @(Get-CimInstance Win32_VideoController | Where-Object { $_.Name })
        $disks = @(Get-PhysicalDisk | ForEach-Object { [pscustomobject]@{ Name = $_.FriendlyName; Bytes = [int64]$_.Size; Media = [string]$_.MediaType; Bus = [string]$_.BusType } })
        $tpm = Try-Get { Get-Tpm }
        $secure = Try-Get { Confirm-SecureBootUEFI }
        $vbs = Try-Get { (Get-CimInstance -Namespace root\Microsoft\Windows\DeviceGuard -ClassName Win32_DeviceGuard).VirtualizationBasedSecurityStatus }
        $av = @(Try-Get { Get-CimInstance -Namespace root\SecurityCenter2 -ClassName AntiVirusProduct | ForEach-Object { $_.displayName } })
        $lic = Try-Get { Get-CimInstance SoftwareLicensingProduct -Filter "ApplicationId='55c92734-d682-4d71-983e-d6ec3f16059f' AND PartialProductKey IS NOT NULL" | Select-Object -First 1 }
        $store = [bool](Try-Get { Get-AppxPackage -AllUsers Microsoft.WindowsStore | Select-Object -First 1 })
        $edge = Test-Path "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
        $o = [ordered]@{
          osCaption = $os.Caption; osBuild = $os.BuildNumber; osArch = $os.OSArchitecture; installDate = $os.InstallDate.ToString('o'); boot = $os.LastBootUpTime.ToString('o')
          language = (Get-Culture).Name; domain = [bool]$cs.PartOfDomain; manufacturer = $cs.Manufacturer; model = $cs.Model
          biosVersion = $bios.SMBIOSBIOSVersion; biosDate = $(if ($bios.ReleaseDate) { $bios.ReleaseDate.ToString('yyyy-MM-dd') })
          cpu = ($cpu | Select-Object -First 1).Name; cores = [int](($cpu | Measure-Object NumberOfCores -Sum).Sum); threads = [int](($cpu | Measure-Object NumberOfLogicalProcessors -Sum).Sum)
          memBytes = [int64]$cs.TotalPhysicalMemory
          gpus = @($gpu | ForEach-Object { [pscustomobject]@{ Name = $_.Name; Driver = $_.DriverVersion } })
          disks = $disks
          tpmPresent = $(if ($tpm) { [bool]$tpm.TpmPresent }); tpmReady = $(if ($tpm) { [bool]$tpm.TpmReady })
          secureBoot = $secure; vbs = $vbs; av = $av
          licChannel = $lic.ProductKeyChannel; licStatus = $(if ($lic) { [int]$lic.LicenseStatus }); licName = $lic.Name
          store = $store; edge = $edge
        }
        ConvertTo-Json -InputObject $o -Depth 5 -Compress
        """;

    public SystemReport Read() => Build(PowerShellHost.Run(runner, Script, Timeout));

    /// <summary>Builds the report from the PowerShell answer plus registry and service reads.</summary>
    public SystemReport Build(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var j = doc.RootElement;

        var version = Text(Local(CurrentVersion, "DisplayVersion"));
        var ubr = Text(Local(CurrentVersion, "UBR"));

        var windows = new List<Fact>
        {
            new("Edition", Str(j, "osCaption").Replace("Microsoft ", "")),
            new("Version", $"{(version.Length > 0 ? version + ", " : "")}build {Str(j, "osBuild")}{(ubr.Length > 0 ? "." + ubr : "")}"),
            new("Architecture", Str(j, "osArch")),
            new("Installed", Date(j, "installDate")),
            new("Last restart", Uptime(j)),
            new("Activation", Activation(j)),
            new("Language", Str(j, "language")),
            new("Joined to a domain", Bool(j, "domain") == true ? "Yes" : "No"),
        };

        var hardware = new List<Fact>
        {
            new("Computer", $"{Str(j, "manufacturer")} {Str(j, "model")}".Trim()),
            new("BIOS", $"{Str(j, "biosVersion")} ({Str(j, "biosDate")})"),
            new("Processor", $"{Str(j, "cpu").Trim()}, {Int(j, "cores")} cores, {Int(j, "threads")} threads"),
            new("Memory", ByteSize.Format(Long(j, "memBytes"))),
        };
        foreach (var g in List(j, "gpus"))
            hardware.Add(new("Graphics", $"{Str(g, "Name")}, driver {Str(g, "Driver")}"));
        foreach (var d in List(j, "disks"))
            hardware.Add(new("Storage", $"{Str(d, "Name")}, {ByteSize.Format(Long(d, "Bytes"))}, {Str(d, "Media")} on {Str(d, "Bus")}"));

        var antivirus = string.Join(", ", List(j, "av").Select(a => a.GetString() ?? "").Where(a => a.Length > 0));
        var security = new List<Fact>
        {
            new("Secure Boot", Bool(j, "secureBoot") switch { true => "On", false => "Off", null => "Not available" }),
            new("TPM", Bool(j, "tpmPresent") switch { true => Bool(j, "tpmReady") == true ? "Present and ready" : "Present", false => "Not found", null => "Unknown" }),
            new("Virtualization based security", Int(j, "vbs") switch { 2 => "Running", 1 => "Enabled but not running", _ => "Off" }),
            new("Antivirus", antivirus.Length > 0 ? antivirus : "None reported"),
        };

        var update = services.GetStartType("wuauserv");
        var inputs = new InstallInputs
        {
            Branding = Branding(Str(j, "osCaption")),
            HardwareChecksBypassed = Bypassed(),
            DefenderServiceMissing = services.GetStartType("WinDefend") is null,
            UpdateServiceMissing = update is null,
            UpdateServiceDisabled = update == ServiceStartType.Disabled,
            MissingServices = InstallCheck.CoreServices.Where(s => services.GetStartType(s) is null).ToList(),
            StoreMissing = Bool(j, "store") is { } store ? !store : null,
            EdgeMissing = Bool(j, "edge") is { } edge ? !edge : null,
            LicenseChannel = Str(j, "licChannel"),
            Domain = Bool(j, "domain") == true,
        };

        return new SystemReport(
            [new("Windows", windows), new("Hardware", hardware), new("Security", security)],
            InstallCheck.Evaluate(inputs));
    }

    private static RegistryTarget Local(string path, string name) => new(RegistryHive.LocalMachine, path, name);

    private string Text(RegistryTarget t) => registry.Read(t) is { Exists: true, Data: { } d } ? d.Value.Trim() : "";

    private long Number(string path, string name) =>
        registry.Read(Local(path, name)) is { Data: { } d } && long.TryParse(d.Value, out var n) ? n : 0;

    private List<string> Branding(string caption)
    {
        var all = new List<string> { caption };
        all.AddRange(
        [
            Text(Local(CurrentVersion, "ProductName")), Text(Local(CurrentVersion, "RegisteredOwner")), Text(Local(CurrentVersion, "RegisteredOrganization")),
            Text(Local(Oem, "Manufacturer")), Text(Local(Oem, "Model")), Text(Local(Oem, "SupportURL")),
        ]);
        return all.Where(x => x.Length > 0).ToList();
    }

    private bool Bypassed() =>
        new[] { "BypassTPMCheck", "BypassSecureBootCheck", "BypassRAMCheck", "BypassCPUCheck", "BypassStorageCheck" }.Any(n => Number(LabConfig, n) == 1)
        || Number("SYSTEM\\Setup\\MoSetup", "AllowUpgradesWithUnsupportedTPMOrCPU") == 1;

    private static string Activation(JsonElement j)
    {
        if (!j.TryGetProperty("licStatus", out var st) || st.ValueKind != JsonValueKind.Number) return "Unknown";
        var state = st.GetInt32() switch { 1 => "Activated", 0 => "Not licensed", 5 => "Notification mode", _ => "Not activated" };
        var channel = Str(j, "licChannel");
        return channel.Length > 0 ? $"{state} ({channel})" : state;
    }

    private static string Uptime(JsonElement j) =>
        DateTimeOffset.TryParse(Str(j, "boot"), CultureInfo.InvariantCulture, DateTimeStyles.None, out var t)
            ? $"{t.LocalDateTime:g} ({Span(DateTimeOffset.Now - t)} ago)" : "Unknown";

    private static string Span(TimeSpan s) =>
        s.TotalDays >= 1 ? $"{(int)s.TotalDays} d {s.Hours} h" : s.TotalHours >= 1 ? $"{(int)s.TotalHours} h {s.Minutes} min" : $"{Math.Max(1, s.Minutes)} min";

    private static string Date(JsonElement j, string name) =>
        DateTimeOffset.TryParse(Str(j, name), CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) ? t.LocalDateTime.ToString("d") : "Unknown";

    private static string Str(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind is JsonValueKind.String or JsonValueKind.Number ? v.ToString() : "";

    private static bool? Bool(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind is JsonValueKind.True or JsonValueKind.False ? v.GetBoolean() : null;

    private static int Int(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;

    private static long Long(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt64() : 0;

    private static List<JsonElement> List(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v)
            ? v.ValueKind == JsonValueKind.Array ? v.EnumerateArray().ToList() : v.ValueKind == JsonValueKind.Null ? [] : [v]
            : [];
}
