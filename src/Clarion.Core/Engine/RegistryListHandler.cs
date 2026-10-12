using Clarion.Core.Abstractions;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

/// <summary>
/// Sets one entry of a text value that holds a list of Name=Value entries. The undo is the whole text as it was, or no value when there
/// was none, so putting it back also puts back every entry that something else had written.
/// </summary>
public sealed class RegistryListHandler(IRegistryStore store) : IOperationHandler
{
    public bool Handles(Operation op) => op is SetRegistryListEntry;

    public bool IsApplicable(Operation op) => true;

    public bool IsSatisfied(Operation op)
    {
        var entry = (SetRegistryListEntry)op;
        var now = store.Read(entry.Target);
        return now.Exists && now.Data is { Kind: RegistryKind.String } data
               && string.Equals(RegistryList.Get(data.Value, entry.Key), entry.Value, StringComparison.OrdinalIgnoreCase);
    }

    public Operation CaptureUndo(Operation op)
    {
        var entry = (SetRegistryListEntry)op;
        var prior = store.Read(entry.Target);
        if (!prior.Exists) return new DeleteRegistryValue(entry.Target, prior.DeepestExistingKey);
        if (prior.Data is not { Kind: RegistryKind.String } data)
            throw new NotSupportedException($"{entry.Target.Name} holds a kind of value that .Clarion cannot put back, so it was left as it is.");
        return new SetRegistryValue(entry.Target, data);
    }

    public void Execute(Operation op)
    {
        var entry = (SetRegistryListEntry)op;
        var now = store.Read(entry.Target);
        if (now.Exists && now.Data is not { Kind: RegistryKind.String })
            throw new NotSupportedException($"{entry.Target.Name} holds a kind of value that .Clarion cannot change safely, so it was left as it is.");
        var text = RegistryList.Set(now.Data?.Value, entry.Key, entry.Value);
        store.Write(entry.Target, new RegistryData(RegistryKind.String, text));
    }
}
