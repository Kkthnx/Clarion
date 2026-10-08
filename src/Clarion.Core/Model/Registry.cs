namespace Clarion.Core.Model;

/// <summary>A registry value location. An empty Name means the default value of the key.</summary>
public sealed record RegistryTarget(RegistryHive Hive, string Path, string Name)
{
    public override string ToString() =>
        $"{(Hive == RegistryHive.CurrentUser ? "HKCU" : "HKLM")}\\{Path}\\{(Name.Length == 0 ? "(Default)" : Name)}";
}

/// <summary>Value data stored as text. DWord and QWord are decimal, Binary is hex.</summary>
public sealed record RegistryData(RegistryKind Kind, string Value);

/// <summary>
/// What a value looked like before a change. DeepestExistingKey is the longest
/// key path that existed, so a revert can remove keys the change created.
/// </summary>
public sealed record RegistrySnapshot(bool Exists, RegistryData? Data, string DeepestExistingKey);
