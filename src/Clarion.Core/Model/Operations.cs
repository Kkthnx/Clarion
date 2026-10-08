using System.Text.Json.Serialization;

namespace Clarion.Core.Model;

public enum ServiceStartType { Automatic, AutomaticDelayed, Manual, Disabled }

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SetRegistryValue), "registry.set")]
[JsonDerivedType(typeof(DeleteRegistryValue), "registry.delete")]
[JsonDerivedType(typeof(SetServiceStartType), "service.start-type")]
[JsonDerivedType(typeof(SetTaskEnabled), "task.set-enabled")]
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
