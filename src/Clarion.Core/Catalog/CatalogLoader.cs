using System.Text.Json;
using System.Text.Json.Serialization;
using Clarion.Core.Model;

namespace Clarion.Core.Catalog;

public static class CatalogLoader
{
    /// <summary>Services that must never be changed, whatever a catalog entry says.</summary>
    public static readonly IReadOnlySet<string> ProtectedServices = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "WinDefend", "WdNisSvc", "Sense", "SecurityHealthService", "wscsvc", "mpssvc", "BFE",
        "wuauserv", "UsoSvc", "WaaSMedicSvc", "BITS", "TrustedInstaller", "EventLog", "RpcSs",
        "RpcEptMapper", "DcomLaunch", "LSM", "SamSs", "CryptSvc", "Dhcp", "Dnscache", "nsi",
        "ProfSvc", "UserManager", "Winmgmt", "Schedule", "gpsvc", "StateRepository", "TimeBrokerSvc",
        "DPS", "WdiServiceHost", "WdiSystemHost", "DusmSvc", "NcbService", "netprofm", "NlaSvc", "LanmanWorkstation",
        "LanmanServer", "PlugPlay", "Power", "AudioSrv", "AudioEndpointBuilder", "DoSvc", "ShellHWDetection",
        "SENS", "Themes", "CoreMessagingRegistrar", "SystemEventsBroker", "BrokerInfrastructure", "KeyIso", "vds", "VSS",
    };

    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Reads every embedded catalog file whose resource name contains the marker, in name order.</summary>
    private static List<T> LoadEmbeddedFiles<T>(string marker)
    {
        var asm = typeof(CatalogLoader).Assembly;
        var all = new List<T>();
        foreach (var name in asm.GetManifestResourceNames().Where(n => n.Contains(marker, StringComparison.Ordinal) && n.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).Order())
        {
            using var stream = asm.GetManifestResourceStream(name)!;
            all.AddRange(JsonSerializer.Deserialize<List<T>>(stream, Options) ?? throw new InvalidDataException($"Empty catalog file {name}"));
        }
        return all;
    }

    public static IReadOnlyList<Tweak> LoadEmbedded() => LoadEmbeddedFiles<Tweak>(".Catalog.Data.");

    public static IReadOnlyList<Actions.ActionDef> LoadActions() => LoadEmbeddedFiles<Actions.ActionDef>(".Catalog.Actions.");

    public static IReadOnlyList<Cleanup.CleanTarget> LoadCleanup() => LoadEmbeddedFiles<Cleanup.CleanTarget>(".Catalog.Cleanup.");

    /// <summary>Named groups of tweak ids, such as minimal or gaming.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> LoadPresets()
    {
        var asm = typeof(CatalogLoader).Assembly;
        var name = asm.GetManifestResourceNames().Single(n => n.EndsWith(".Catalog.Presets.presets.json", StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(name)!;
        var raw = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(stream, Options) ?? [];
        return raw.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<string>)kv.Value, StringComparer.OrdinalIgnoreCase);
    }


    /// <summary>The rules every catalog item must follow, whether it is a setting, a repair job or a cleanup row.</summary>
    private static void CheckCommon(string id, string advice, IReadOnlyList<string> facts, Recommendation recommendation, RiskLevel risk, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(advice)) errors.Add($"{id}: advice is empty.");
        if (facts.Count < 2 || facts.Count > 6) errors.Add($"{id}: needs 2 to 6 facts for the tooltip.");
        foreach (var fact in facts)
        {
            if (string.IsNullOrWhiteSpace(fact) || fact.Length > 140) errors.Add($"{id}: each fact must be 1 to 140 characters.");
        }
        if (recommendation == Recommendation.Recommended && risk >= RiskLevel.Medium)
            errors.Add($"{id}: a Medium or High risk item cannot be marked Recommended.");
    }

    private static readonly string[] CleanupTokens = ["%LOCALAPPDATA%", "%APPDATA%", "%LOCALLOW%", "%PROGRAMDATA%", "%SystemRoot%", "%TEMP%", "%SystemDrive%"];

    public static IReadOnlyList<string> ValidateCleanup(IEnumerable<Cleanup.CleanTarget> targets)
    {
        var errors = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in targets)
        {
            if (!seen.Add(t.Id)) errors.Add($"{t.Id}: duplicate id.");
            CheckCommon(t.Id, t.Advice, t.Facts, t.Recommendation, t.RiskLevel, errors);
            if (t.Rules.Count == 0) errors.Add($"{t.Id}: no rules.");
            if (t.Irreversible && t.DefaultOn) errors.Add($"{t.Id}: an irreversible row cannot start ticked.");
            if (t.Irreversible && t.RiskLevel < RiskLevel.Medium) errors.Add($"{t.Id}: an irreversible row must be Medium risk or higher.");
            foreach (var rule in t.Rules)
            {
                var path = rule switch { Cleanup.FolderRule f => f.Path, Cleanup.FilePatternRule p => p.Folder, _ => null };
                if (path is null) continue;
                if (path.Contains("..", StringComparison.Ordinal)) errors.Add($"{t.Id}: path must not contain .. : {path}");
                if (!CleanupTokens.Any(tok => path.StartsWith(tok, StringComparison.OrdinalIgnoreCase)))
                    errors.Add($"{t.Id}: path must start with a known folder token: {path}");
                if (!path.Equals("%TEMP%", StringComparison.OrdinalIgnoreCase) && CleanupTokens.Any(tok => path.Equals(tok, StringComparison.OrdinalIgnoreCase)))
                    errors.Add($"{t.Id}: path must go below the folder token: {path}");
                if (rule is Cleanup.FilePatternRule fp && (fp.Pattern.Contains('\\') || fp.Pattern.Contains('/')))
                    errors.Add($"{t.Id}: pattern must be a file name pattern: {fp.Pattern}");
            }
        }
        return errors;
    }

    public static IReadOnlyList<Actions.ActionDef> ParseActions(string json) =>
        JsonSerializer.Deserialize<List<Actions.ActionDef>>(json, Options) ?? [];

    public static IReadOnlyList<string> ValidateActions(IEnumerable<Actions.ActionDef> actions)
    {
        var errors = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var a in actions)
        {
            if (!seen.Add(a.Id)) errors.Add($"{a.Id}: duplicate id.");
            CheckCommon(a.Id, a.Advice, a.Facts, a.Recommendation, a.RiskLevel, errors);
            if (a.Steps.Count == 0) errors.Add($"{a.Id}: no steps.");
            if (a.EstimatedMinutes < 1) errors.Add($"{a.Id}: estimated minutes must be at least 1.");
            foreach (var step in a.Steps.Concat(a.Always))
            {
                switch (step)
                {
                    case Actions.RunProcess p:
                        if (!Actions.ActionRules.IsAllowedTool(p.File)) errors.Add($"{a.Id}: {p.File} is not an allowed tool.");
                        break;
                    case Actions.CleanFolders c:
                        foreach (var f in c.Folders)
                            if (!Actions.ActionRules.IsAllowedFolder(f)) errors.Add($"{a.Id}: {f} is not an allowed folder.");
                        break;
                    case Actions.RenameFolder r:
                        if (!Actions.ActionRules.IsAllowedFolder(r.Folder)) errors.Add($"{a.Id}: {r.Folder} is not an allowed folder.");
                        if (!Actions.ActionRules.IsPlainName(r.NewName)) errors.Add($"{a.Id}: {r.NewName} is not a plain folder name.");
                        break;
                    case Actions.DeleteFolder d:
                        if (!Actions.ActionRules.IsAllowedFolder(d.Folder)) errors.Add($"{a.Id}: {d.Folder} is not an allowed folder.");
                        break;
                }
            }
        }
        return errors;
    }

    public static IReadOnlyList<Tweak> Parse(string json) =>
        JsonSerializer.Deserialize<List<Tweak>>(json, Options) ?? [];

    /// <summary>Returns a list of problems. An empty list means the catalog is valid.</summary>
    public static IReadOnlyList<string> Validate(IEnumerable<Tweak> tweaks)
    {
        var errors = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in tweaks)
        {
            if (string.IsNullOrWhiteSpace(t.Id)) { errors.Add("A tweak has no id."); continue; }
            if (!seen.Add(t.Id)) errors.Add($"{t.Id}: duplicate id.");

            foreach (var (label, text) in new[] { ("name", t.Name), ("summary", t.Summary), ("what", t.What), ("benefit", t.Benefit), ("risk", t.Risk) })
            {
                if (string.IsNullOrWhiteSpace(text)) errors.Add($"{t.Id}: {label} is empty.");
            }

            if (string.IsNullOrWhiteSpace(t.Topic)) errors.Add($"{t.Id}: topic is empty.");
            CheckCommon(t.Id, t.Advice, t.Facts, t.Recommendation, t.RiskLevel, errors);
            if (t.Apply.Count == 0) errors.Add($"{t.Id}: no apply operations.");
            if (t.Evidence is Evidence.Proven && t.Sources.Count == 0) errors.Add($"{t.Id}: Proven evidence needs at least one source.");

            foreach (var op in t.Apply.Concat(t.Undo))
            {
                switch (op)
                {
                    case SetRegistryValue s:
                        CheckRegistry(t, s.Target, errors);
                        if (s.Data.Kind is RegistryKind.DWord && !uint.TryParse(s.Data.Value, out _))
                            errors.Add($"{t.Id}: DWord value is not a valid unsigned number: {s.Data.Value}");
                        if (s.Data.Kind is RegistryKind.QWord && !ulong.TryParse(s.Data.Value, out _))
                            errors.Add($"{t.Id}: QWord value is not a valid unsigned number: {s.Data.Value}");
                        break;
                    case DeleteRegistryValue d:
                        CheckRegistry(t, d.Target, errors);
                        break;
                    case SetServiceStartType svc:
                        if (t.Scope != TweakScope.Machine) errors.Add($"{t.Id}: service changes need Machine scope.");
                        if (string.IsNullOrWhiteSpace(svc.Name)) errors.Add($"{t.Id}: service name is empty.");
                        if (ProtectedServices.Contains(svc.Name)) errors.Add($"{t.Id}: {svc.Name} is a protected service.");
                        break;
                    case SetTaskEnabled task:
                        if (t.Scope != TweakScope.Machine) errors.Add($"{t.Id}: task changes need Machine scope.");
                        if (!task.Path.StartsWith('\\')) errors.Add($"{t.Id}: task path must start with a backslash: {task.Path}");
                        break;
                    case RemoveAppxPackage app:
                        if (!Appx.AppxSafety.IsValidName(app.Name)) errors.Add($"{t.Id}: invalid package name {app.Name}.");
                        else if (Appx.AppxSafety.IsProtected(app.Name)) errors.Add($"{t.Id}: {app.Name} is a protected package.");
                        if ((app.AllUsers || app.Deprovision) && t.Scope != TweakScope.Machine)
                            errors.Add($"{t.Id}: all account app removal needs Machine scope.");
                        break;
                    case RestoreAppxPackage:
                        break;
                    case SetPowerPlan plan:
                        if (t.Scope != TweakScope.Machine) errors.Add($"{t.Id}: power changes need Machine scope.");
                        if (!Power.PowerPlans.IsValid(plan.Plan)) errors.Add($"{t.Id}: unknown power plan {plan.Plan}.");
                        break;
                    case SetHibernation:
                        if (t.Scope != TweakScope.Machine) errors.Add($"{t.Id}: power changes need Machine scope.");
                        break;
                    case SetDnsProvider dns:
                        if (t.Scope != TweakScope.Machine) errors.Add($"{t.Id}: DNS changes need Machine scope.");
                        if (Dns.DnsProviders.Find(dns.Provider) is null) errors.Add($"{t.Id}: unknown DNS provider {dns.Provider}.");
                        break;
                    case RestoreDns:
                        break;
                    case SetWindowsFeature feat:
                        if (t.Scope != TweakScope.Machine) errors.Add($"{t.Id}: feature changes need Machine scope.");
                        if (!Features.FeatureRules.IsValidName(feat.Name)) errors.Add($"{t.Id}: invalid feature name {feat.Name}.");
                        else if (!feat.Enabled && !Features.FeatureRules.CanDisable(feat.Name)) errors.Add($"{t.Id}: {feat.Name} is a protected feature.");
                        break;
                    case SetWindowsCapability cap:
                        if (t.Scope != TweakScope.Machine) errors.Add($"{t.Id}: capability changes need Machine scope.");
                        if (!Features.FeatureRules.IsValidName(cap.Name)) errors.Add($"{t.Id}: invalid capability name {cap.Name}.");
                        break;
                    default:
                        errors.Add($"{t.Id}: unknown operation type.");
                        break;
                }
            }
        }
        return errors;
    }

    private static void CheckRegistry(Tweak t, RegistryTarget target, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(target.Path) || target.Path.StartsWith('\\') || target.Path.StartsWith("HK", StringComparison.OrdinalIgnoreCase))
            errors.Add($"{t.Id}: registry path must be relative to the hive: {target.Path}");
        if (target.Hive == RegistryHive.LocalMachine && t.Scope != TweakScope.Machine)
            errors.Add($"{t.Id}: touches HKLM but scope is not Machine.");
    }
}
