using Clarion.Core.Abstractions;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

public sealed class RegistryHandler(IRegistryStore store) : IOperationHandler
{
    public bool Handles(Operation op) => op is SetRegistryValue or DeleteRegistryValue;

    public bool IsApplicable(Operation op) => true;

    public bool IsSatisfied(Operation op)
    {
        switch (op)
        {
            case SetRegistryValue set:
                var now = store.Read(set.Target);
                return now.Exists && now.Data is not null
                       && now.Data.Kind == set.Data.Kind
                       && string.Equals(now.Data.Value, set.Data.Value, StringComparison.OrdinalIgnoreCase);
            case DeleteRegistryValue del:
                return !store.Read(del.Target).Exists;
            default:
                return false;
        }
    }

    public Operation CaptureUndo(Operation op)
    {
        var target = op switch
        {
            SetRegistryValue s => s.Target,
            DeleteRegistryValue d => d.Target,
            _ => throw new InvalidOperationException("Not a registry operation"),
        };
        var prior = store.Read(target);
        return prior.Exists && prior.Data is not null
            ? new SetRegistryValue(target, prior.Data)
            : new DeleteRegistryValue(target, prior.DeepestExistingKey);
    }

    public void Execute(Operation op)
    {
        switch (op)
        {
            case SetRegistryValue set:
                store.Write(set.Target, set.Data);
                break;
            case DeleteRegistryValue del:
                store.DeleteValue(del.Target);
                if (del.PruneTo is not null) store.PruneEmptyKeys(del.Target, del.PruneTo);
                break;
        }
    }
}

public sealed class ServiceHandler(IServiceStore store) : IOperationHandler
{
    public bool Handles(Operation op) => op is SetServiceStartType;

    public bool IsApplicable(Operation op) => op is SetServiceStartType s && store.GetStartType(s.Name) is not null;

    public bool IsSatisfied(Operation op) => op is SetServiceStartType s && store.GetStartType(s.Name) == s.StartType;

    public Operation CaptureUndo(Operation op)
    {
        var s = (SetServiceStartType)op;
        var current = store.GetStartType(s.Name) ?? throw new InvalidOperationException($"Service {s.Name} not found");
        return new SetServiceStartType(s.Name, current);
    }

    public void Execute(Operation op)
    {
        var s = (SetServiceStartType)op;
        store.SetStartType(s.Name, s.StartType);
    }
}

public sealed class TaskHandler(ITaskStore store) : IOperationHandler
{
    public bool Handles(Operation op) => op is SetTaskEnabled;

    public bool IsApplicable(Operation op) => op is SetTaskEnabled t && store.GetEnabled(t.Path) is not null;

    public bool IsSatisfied(Operation op) => op is SetTaskEnabled t && store.GetEnabled(t.Path) == t.Enabled;

    public Operation CaptureUndo(Operation op)
    {
        var t = (SetTaskEnabled)op;
        var current = store.GetEnabled(t.Path) ?? throw new InvalidOperationException($"Task {t.Path} not found");
        return new SetTaskEnabled(t.Path, current);
    }

    public void Execute(Operation op)
    {
        var t = (SetTaskEnabled)op;
        store.SetEnabled(t.Path, t.Enabled);
    }
}
