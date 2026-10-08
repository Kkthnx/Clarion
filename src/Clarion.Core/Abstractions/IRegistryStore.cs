using Clarion.Core.Model;

namespace Clarion.Core.Abstractions;

public interface IRegistryStore
{
    RegistrySnapshot Read(RegistryTarget target);
    void Write(RegistryTarget target, RegistryData data);
    void DeleteValue(RegistryTarget target);

    /// <summary>
    /// Removes empty keys from the target key upward, stopping at
    /// (and keeping) the given ancestor path.
    /// </summary>
    void PruneEmptyKeys(RegistryTarget target, string keepAncestor);
}
