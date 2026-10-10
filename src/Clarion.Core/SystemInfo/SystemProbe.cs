using System.Globalization;
using System.Text.Json;
using Clarion.Core.Abstractions;
using Clarion.Core.Engine;
using Clarion.Core.Model;

namespace Clarion.Core.SystemInfo;

/// <summary>Collects facts about this PC and the Windows install, then judges whether the image looks customized.</summary>
public sealed class SystemProbe(IRegistryStore registry, IServiceStore services, IProcessRunner runner, ITaskStore? tasks = null, TimeProvider? clock = null)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(90);

    private const string CurrentVersion = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
    private const string Oem = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OEMInformation";
    private const string LabConfig = "SYSTEM\\Setup\\LabConfig";
    private const string UxSettings = "SOFTWARE\\Microsoft\\WindowsUpdate\\UX\\Settings";
    private const string UpdatePolicySettings = "SOFTWARE\\Microsoft\\WindowsUpdate\\UpdatePolicy\\Settings";
    private const string UpdatePolicyKey = "SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate";
    private const string ScanTask = "\\Microsoft\\Windows\\UpdateOrchestrator\\Schedule Scan";
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;

    // One PowerShell start collects everything slow, which keeps the page quick to open.
    internal const string Script = """
        $ErrorActionPreference = 'SilentlyContinue'
        function Try-Get([scriptblock]$s) { try { & $s } catch { $null } }
        $os = Get-CimInstance Win32_OperatingSystem
        $cs = Get-CimInstance Win32_ComputerSystem
        $bios = Get-CimInstance Win32_BIOS
        # Only the properties used are asked for. Win32_Processor with all of them also measures the live load, which took about a second.
        $cpu = @(Get-CimInstance Win32_Processor -Property Name,NumberOfCores,NumberOfLogicalProcessors)
        $gpu = @(Get-CimInstance Win32_VideoController -Property Name,DriverVersion | Where-Object { $_.Name })
        # Disks come straight from the storage provider, which is what Get-PhysicalDisk reads, without loading that module first (about a second).
        # Value names are from Microsoft's MSFT_PhysicalDisk reference. When the module is loaded the values already arrive as names.
        $busNames = @{ 1 = 'SCSI'; 2 = 'ATAPI'; 3 = 'ATA'; 4 = '1394'; 5 = 'SSA'; 6 = 'Fibre Channel'; 7 = 'USB'; 8 = 'RAID'; 9 = 'iSCSI'; 10 = 'SAS'; 11 = 'SATA'; 12 = 'SD'; 13 = 'MMC'; 15 = 'File Backed Virtual'; 16 = 'Storage Spaces'; 17 = 'NVMe' }
        $mediaNames = @{ 3 = 'HDD'; 4 = 'SSD'; 5 = 'SCM' }
        function Disk-Name($v, $map, $fallback) { if ($v -is [string]) { return $v }; if ($null -ne $v -and $map.ContainsKey([int]$v)) { return [string]$map[[int]$v] }; return $fallback }
        $disks = @(Get-CimInstance -Namespace root\Microsoft\Windows\Storage -ClassName MSFT_PhysicalDisk -Property FriendlyName,Size,MediaType,BusType | ForEach-Object { [pscustomobject]@{ Name = $_.FriendlyName; Bytes = [int64]$_.Size; Media = (Disk-Name $_.MediaType $mediaNames 'Unspecified'); Bus = (Disk-Name $_.BusType $busNames 'Unknown') } })
        $tpm = Try-Get { Get-Tpm }
        $secure = Try-Get { Confirm-SecureBootUEFI }
        $vbs = Try-Get { (Get-CimInstance -Namespace root\Microsoft\Windows\DeviceGuard -ClassName Win32_DeviceGuard).VirtualizationBasedSecurityStatus }
        $av = @(Try-Get { Get-CimInstance -Namespace root\SecurityCenter2 -ClassName AntiVirusProduct | ForEach-Object { $_.displayName } })
        $lic = Try-Get { Get-CimInstance SoftwareLicensingProduct -Property Name,ProductKeyChannel,LicenseStatus,ApplicationId,PartialProductKey -Filter "ApplicationId='55c92734-d682-4d71-983e-d6ec3f16059f' AND PartialProductKey IS NOT NULL" | Select-Object -First 1 }
        $store = [bool](Try-Get { Get-AppxPackage -AllUsers Microsoft.WindowsStore | Select-Object -First 1 })
        $edge = Test-Path "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
        $ghost = [bool]((Test-Path "$env:SystemDrive\Ghost Toolbox") -or (Test-Path "$env:PUBLIC\Desktop\Ghost Toolbox.lnk") -or (Test-Path "$env:USERPROFILE\Desktop\Ghost Toolbox.lnk"))
        $atlas = [bool]((Test-Path 'HKLM:\SOFTWARE\AtlasOS') -or (Test-Path "$env:windir\AtlasModules") -or (Test-Path "$env:windir\AtlasDesktop"))
        $revision = [bool](Test-Path "$env:ProgramFiles\Revision Tool")
        $wu = Try-Get { (New-Object -ComObject Microsoft.Update.AutoUpdate).Results }
        function Wu-Date($d) { if ($d) { try { ([datetime]$d).ToUniversalTime().ToString('o') } catch { $null } } }
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
          ghostToolbox = $ghost; atlasMarker = $atlas; revisionTool = $revision; lastCheck = (Wu-Date $wu.LastSearchSuccessDate); lastInstall = (Wu-Date $wu.LastInstallationSuccessDate)
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
        var updates = ReadUpdates(j, update);
        var inputs = new InstallInputs
        {
            GhostToolboxFound = Bool(j, "ghostToolbox") == true,
            AtlasMarkerFound = Bool(j, "atlasMarker") == true,
            RevisionToolFound = Bool(j, "revisionTool") == true,
            Tiny11SetupPattern = new[] { "BypassTPMCheck", "BypassSecureBootCheck", "BypassRAMCheck", "BypassCPUCheck", "BypassStorageCheck" }.All(n => Number(LabConfig, n) == 1)
                && Number("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OOBE", "BypassNRO") == 1,
            Tiny11CorePattern = (update is null or ServiceStartType.Disabled) && updates.UseUpdateServer && updates.AutoUpdateOff
                && updates.UpdateServer is { } server && UpdateState.IsThisPc(server),
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
            InstallCheck.Evaluate(inputs))
        {
            Updates = UpdateState.Evaluate(updates, _clock.GetUtcNow()),
        };
    }

    /// <summary>Reads the update settings from the registry, the services and the task list, plus the dates Windows keeps.</summary>
    private UpdateInputs ReadUpdates(JsonElement j, ServiceStartType? update)
    {
        var ends = new[] { "PauseUpdatesExpiryTime", "PauseFeatureUpdatesEndTime", "PauseQualityUpdatesEndTime" }
            .Select(n => Time(Text(Local(UxSettings, n)))).Where(t => t is not null).Select(t => t!.Value).ToList();
        var server = Text(Local(UpdatePolicyKey, "WUServer"));

        return new UpdateInputs
        {
            PauseEnds = ends.Count > 0 ? ends.Max() : null,
            PausedFeatureStatus = NumberOrNull(UpdatePolicySettings, "PausedFeatureStatus"),
            PausedQualityStatus = NumberOrNull(UpdatePolicySettings, "PausedQualityStatus"),
            WindowsUpdateServiceMissing = update is null,
            WindowsUpdateServiceDisabled = update == ServiceStartType.Disabled,
            OrchestratorMissing = services.GetStartType("UsoSvc") is null,
            MedicMissing = services.GetStartType("WaaSMedicSvc") is null,
            AccessBlockedByPolicy = Number(UpdatePolicyKey, "DisableWindowsUpdateAccess") == 1,
            NoConnectPolicy = Number(UpdatePolicyKey, "DoNotConnectToWindowsUpdateInternetLocations") == 1,
            AutoUpdateOff = Number(UpdatePolicyKey + "\\AU", "NoAutoUpdate") == 1,
            AutoUpdateOption = NumberOrNull(UpdatePolicyKey + "\\AU", "AUOptions"),
            UseUpdateServer = Number(UpdatePolicyKey + "\\AU", "UseWUServer") == 1,
            UpdateServer = server.Length > 0 ? server : null,
            SettingsPageHidden = Text(Local("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "SettingsPageVisibility"))
                .Contains("windowsupdate", StringComparison.OrdinalIgnoreCase),
            ScanTaskEnabled = tasks?.GetEnabled(ScanTask),
            LastCheck = Time(Str(j, "lastCheck")),
            LastInstall = Time(Str(j, "lastInstall")),
        };
    }

    /// <summary>A date from Windows, or null when it is missing or one of the placeholder dates it uses for "never".</summary>
    private static DateTimeOffset? Time(string text) =>
        DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var t) && t.Year >= 2000 ? t : null;

    private static RegistryTarget Local(string path, string name) => new(RegistryHive.LocalMachine, path, name);

    private string Text(RegistryTarget t) => registry.Read(t) is { Exists: true, Data: { } d } ? d.Value.Trim() : "";

    private long Number(string path, string name) =>
        registry.Read(Local(path, name)) is { Data: { } d } && long.TryParse(d.Value, out var n) ? n : 0;

    private int? NumberOrNull(string path, string name) =>
        registry.Read(Local(path, name)) is { Exists: true, Data: { } d } && int.TryParse(d.Value, out var n) ? n : null;

    private List<string> Branding(string caption)
    {
        var all = new List<string> { caption };
        all.AddRange(
        [
            Text(Local(CurrentVersion, "ProductName")), Text(Local(CurrentVersion, "RegisteredOwner")), Text(Local(CurrentVersion, "RegisteredOrganization")),
            Text(Local(CurrentVersion, "EditionSubVersion")),
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
