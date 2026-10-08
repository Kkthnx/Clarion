using System.Text.Json.Serialization;

namespace Clarion.Core.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SetRegistryValue), "registry.set")]
[JsonDerivedType(typeof(DeleteRegistryValue), "registry.delete")]
public abstract record Operation
{
    public abstract string Describe();
}

public sealed record SetRegistryValue(RegistryTarget Target, RegistryData Data) : Operation
{
    public override string Describe() => $"Set {Target} to {Data.Value} ({Data.Kind})";
}

public sealed record DeleteRegistryValue(RegistryTarget Target) : Operation
{
    public override string Describe() => $"Delete {Target}";
}
