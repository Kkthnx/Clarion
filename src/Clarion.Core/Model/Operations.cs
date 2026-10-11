using System.Text.Json.Serialization;

namespace Clarion.Core.Model;

public enum ServiceStartType { Automatic, AutomaticDelayed, Manual, Disabled }

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SetRegistryValue), "registry.set")]
[JsonDerivedType(typeof(DeleteRegistryValue), "registry.delete")]
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
public abstract record Operation
{
    public abstract string Describe();
}

public sealed record SetRegistryValue(RegistryTarget Target, RegistryData Data) : Operation
{
    public override string Describe() => $"Set {Target} to {Data.Value} ({Data.Kind})";
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
/// <summary>Installs one package from winget's community source, pinned by its exact ID. Location is only for packages whose manifest requires an install folder.</summary>
public sealed record InstallWingetPackage(string Id, string? Location = null) : Operation
{
    public override string Describe() => Location is null ? $"Install {Id} with winget" : $"Install {Id} with winget into {Location}";
}

/// <summary>Uninstalls one package that winget installed or knows, pinned by its exact ID.</summary>
public sealed record UninstallWingetPackage(string Id) : Operation
{
    public override string Describe() => $"Uninstall {Id} with winget";
}
