using Clarion.Core.Abstractions;
using Clarion.Core.Appx;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

/// <summary>Removes and restores app packages. Refuses anything protected or owned by the system.</summary>
public sealed class AppxHandler(IAppxStore store) : IOperationHandler
{
    public bool Handles(Operation op) => op is RemoveAppxPackage or RestoreAppxPackage;

    public bool IsApplicable(Operation op) => true;

    public bool IsSatisfied(Operation op)
    {
        var snap = store.GetSnapshot();
        switch (op)
        {
            case RemoveAppxPackage r:
                var gone = r.AllUsers ? !snap.IsInstalledForAnyone(r.Name) : !snap.IsInstalledForCurrentUser(r.Name);
                return gone && (!r.Deprovision || (snap.ProvisionedKnown && !snap.IsProvisioned(r.Name)));
            case RestoreAppxPackage s:
                return s.FamilyName is null || snap.IsInstalledForCurrentUser(s.Name);
            default:
                return false;
        }
    }

    public Operation CaptureUndo(Operation op) => op switch
    {
        RemoveAppxPackage r => new RestoreAppxPackage(r.Name, store.GetSnapshot().Find(r.Name)?.FamilyName),
        RestoreAppxPackage s => new RemoveAppxPackage(s.Name, AllUsers: false, Deprovision: false),
        _ => throw new InvalidOperationException("Not an app operation"),
    };

    public void Execute(Operation op)
    {
        switch (op)
        {
            case RemoveAppxPackage r:
                Guard(r.Name);
                store.Remove(r.Name, r.AllUsers, r.Deprovision);
                break;
            case RestoreAppxPackage s when s.FamilyName is not null:
                store.Restore(s.FamilyName);
                break;
        }
    }

    private void Guard(string name)
    {
        if (!AppxSafety.IsValidName(name)) throw new InvalidOperationException($"Not a valid package name: {name}");
        if (AppxSafety.IsProtected(name)) throw new InvalidOperationException($"{name} is protected and will not be removed.");
        var found = store.GetSnapshot().Find(name);
        if (found is not null && AppxSafety.IsSystemOwned(found))
            throw new InvalidOperationException($"{name} is owned by Windows and will not be removed.");
    }
}
