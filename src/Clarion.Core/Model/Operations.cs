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
    public override string Describe() => $"Restore app {Name}";
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