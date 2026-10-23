using System.Text.Json.Serialization;

namespace Clarion.Core.Model;

public enum ServiceStartType { Automatic, AutomaticDelayed, Manual, Disabled }

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SetRegistryValue), "registry.set")]
[JsonDerivedType(typeof(DeleteRegistryValue), "registry.delete")]
[JsonDerivedType(typeof(SetRegistryListEntry), "registry.list-entry")]
[JsonDerivedType(typeof(SetServiceStartType), "service.start-type")]
[JsonDerivedType(typeof(SetTaskEnabled), "task.set-enabled")]
[JsonDerivedType(typeof(RemoveAppxPackage), "appx.remove")]
[JsonDerivedType(typeof(RestoreAppxPackage), "appx.restore")]
[JsonDerivedType(typeof(SetWindowsFeature), "feature.set")]
[JsonDerivedType(typeof(SetWindowsCapability), "capability.set")]
[JsonDerivedType(typeof(SetPowerPlan), "power.plan")]
[JsonDerivedType(typeof(SetHibernation), "power.hibernation")]
[JsonDerivedType(typeof(SetDnsProvider), "dns.provider")]
[JsonDerivedType(typeof(RestoreDns), "dns.restore")]
[JsonDerivedType(typeof(InstallWingetPackage), "winget.install")]
[JsonDerivedType(typeof(UninstallWingetPackage), "winget.uninstall")]
[JsonDerivedType(typeof(SetNvidiaSetting), "nvidia.set")]
[JsonDerivedType(typeof(ResetNvidiaSetting), "nvidia.reset")]
public abstract record Operation
{
    public abstract string Describe();
}

/// <summary>
/// Sets a value. When Mask is set, the value holds flags and only the bits of the mask are this step's business: they are checked and
/// written, and every other bit is left as Windows or the person has it.
/// </summary>
public sealed record SetRegistryValue(RegistryTarget Target, RegistryData Data, string? Mask = null) : Operation
{
    public override string Describe() => Mask is null
        ? $"Set {Target} to {Data.Value} ({Data.Kind})"
        : $"Turn {RegistryBits.Describe(Data.Value, Mask)} in {Target} ({Data.Kind})";
}

/// <summary>Deletes a value. When PruneTo is set, empty keys below that path are removed too.</summary>
public sealed record DeleteRegistryValue(RegistryTarget Target, string? PruneTo = null) : Operation
{
    public override string Describe() => $"Delete {Target}";
}

public sealed record SetServiceStartType(string Name, ServiceStartType StartType) : Operation
{
    public override string Describe() => $"Set service {Name} start type to {StartType}";
}

public sealed record SetTaskEnabled(string Path, bool Enabled) : Operation
{
    public override string Describe() => $"{(Enabled ? "Enable" : "Disable")} scheduled task {Path}";
}

/// <summary>Removes an app package. AllUsers removes it for every account, Deprovision stops new accounts from getting it.</summary>
public sealed record RemoveAppxPackage(string Name, bool AllUsers = false, bool Deprovision = false) : Operation
{
    public override string Describe() =>
        $"Remove app {Name}{(AllUsers ? " for all accounts" : " for this account")}{(Deprovision ? " and from new accounts" : "")}";
}

/// <summary>Registers a removed app again. A null FamilyName means there was nothing to restore.</summary>
public sealed record RestoreAppxPackage(string Name, string? FamilyName) : Operation
{
    public override string Describe() => FamilyName is null
        ? $"Restore app {Name} (it was not installed for this account when it was removed, so there is nothing to put back)"
        : $"Restore app {Name}";
}
/// <summary>Turns a Windows optional feature on or off. Turning on also enables the features it depends on.</summary>
public sealed record SetWindowsFeature(string Name, bool Enabled) : Operation
{
    public override string Describe() => $"{(Enabled ? "Turn on" : "Turn off")} Windows feature {Name}";
}

/// <summary>Installs or removes a Windows capability such as the SSH server.</summary>
public sealed record SetWindowsCapability(string Name, bool Installed) : Operation
{
    public override string Describe() => $"{(Installed ? "Install" : "Remove")} Windows capability {Name}";
}
/// <summary>Makes a power plan active. Plan is balanced, high-performance, power-saver, ultimate, or a plan GUID.</summary>
public sealed record SetPowerPlan(string Plan) : Operation
{
    public override string Describe() => $"Use power plan {Plan}";
}

public sealed record SetHibernation(bool Enabled) : Operation
{
    public override string Describe() => Enabled ? "Turn hibernation on" : "Turn hibernation off and delete the hibernation file";
}
/// <summary>Uses a public DNS provider on every connected adapter. Encrypted also registers DNS over HTTPS for it.</summary>
public sealed record SetDnsProvider(string Provider, bool Encrypted = false) : Operation
{
    public override string Describe() => $"Use {Provider} DNS{(Encrypted ? " with encryption" : "")} on connected network adapters";
}

/// <summary>Puts each adapter back to the servers it had, and removes encryption entries Clarion added.</summary>
public sealed record RestoreDns(IReadOnlyList<Dns.AdapterDns> Adapters, IReadOnlyList<string> RemoveDohFor, IReadOnlyList<Dns.DohRegistration> RestoreDoh) : Operation
{
    public override string Describe() => "Put the previous DNS servers back";
}
/// <summary>
/// Installs one package from winget's community source, pinned by its exact ID. Location is only for packages whose manifest requires an install
/// folder. Scope is null for winget's own choice, which for these packages is machine wide, or "user" to install for the account that runs
/// .Clarion only, and is only ever "user" for a package whose installer winget says supports it.
/// </summary>
public sealed record InstallWingetPackage(string Id, string? Location = null, string? Scope = null) : Operation
{
    public override string Describe() =>
        (Location is null ? $"Install {Id} with winget" : $"Install {Id} with winget into {Location}") + (Scope == "user" ? " for this account only" : "");
}

/// <summary>Uninstalls one package that winget installed or knows, pinned by its exact ID. Scope is "user" for a package that was installed for one account.</summary>
public sealed record UninstallWingetPackage(string Id, string? Location = null, string? Scope = null) : Operation
{
    public override string Describe() => Scope == "user" ? $"Uninstall {Id} with winget, from this account" : $"Uninstall {Id} with winget";
}

/// <summary>
/// Sets one value in the NVIDIA graphics driver's global profile, which is what NVIDIA's own control panel and app edit. Only the
/// settings listed in <see cref="Nvidia.NvidiaSettings"/> may be set, each with the values NVIDIA documents for it.
/// </summary>
public sealed record SetNvidiaSetting(uint Id, uint Value) : Operation
{
    public override string Describe() => Nvidia.NvidiaSettings.Find(Id) is { } s
        ? $"Set the NVIDIA driver setting \"{s.Name}\" to {s.Describe(Value)}"
        : $"Set NVIDIA driver setting 0x{Id:X8} to {Value}";
}

/// <summary>Takes a value out of the NVIDIA driver's global profile, so the driver's own default applies again.</summary>
public sealed record ResetNvidiaSetting(uint Id) : Operation
{
    public override string Describe() => Nvidia.NvidiaSettings.Find(Id) is { } s
        ? $"Put the NVIDIA driver setting \"{s.Name}\" back to the driver's default"
        : $"Put NVIDIA driver setting 0x{Id:X8} back to the driver's default";
}

/// <summary>
/// Sets one Name=Value entry in a text value that holds a list of such entries separated by semicolons, such as the graphics
/// preferences Windows keeps in DirectXUserGlobalSettings. The other entries in the list are left as they are.
/// </summary>
public sealed record SetRegistryListEntry(RegistryTarget Target, string Key, string Value) : Operation
{
    public override string Describe() => $"In {Target}, set the entry {Key} to {Value}";
}
