using Clarion.Core.Abstractions;
using Clarion.Core.Model;
using Clarion.Core.Power;

namespace Clarion.Core.Engine;

/// <summary>Switches the active power plan and the hibernation setting, and remembers the previous choice.</summary>
public sealed class PowerHandler(IPowerStore store) : IOperationHandler
{
    public bool Handles(Operation op) => op is SetPowerPlan or SetHibernation;

    public bool IsApplicable(Operation op) => op switch
    {
        SetPowerPlan p => p.Plan == "ultimate" || PowerPlans.Resolve(p.Plan, store.List()) is not null,
        SetHibernation => store.IsHibernationEnabled() is not null,
        _ => false,
    };

    public bool IsSatisfied(Operation op)
    {
        switch (op)
        {
            case SetPowerPlan p:
                var plans = store.List();
                var target = PowerPlans.Resolve(p.Plan, plans);
                var active = plans.FirstOrDefault(x => x.IsActive);
                return target is not null && active is not null && active.Guid == target.Guid;
            case SetHibernation h:
                return store.IsHibernationEnabled() == h.Enabled;
            default:
                return false;
        }
    }

    public Operation CaptureUndo(Operation op) => op switch
    {
        SetPowerPlan => new SetPowerPlan(store.List().FirstOrDefault(x => x.IsActive)?.Guid
                                         ?? throw new InvalidOperationException("No active power plan found")),
        SetHibernation => new SetHibernation(store.IsHibernationEnabled() ?? false),
        _ => throw new InvalidOperationException("Not a power operation"),
    };

    public void Execute(Operation op)
    {
        switch (op)
        {
            case SetPowerPlan p:
                if (!PowerPlans.IsValid(p.Plan)) throw new InvalidOperationException($"Not a valid power plan: {p.Plan}");
                var found = PowerPlans.Resolve(p.Plan, store.List());
                if (found is null && p.Plan == "ultimate")
                {
                    var created = store.Duplicate(PowerPlans.UltimateTemplate);
                    store.Rename(created, PowerPlans.UltimateName);
                    store.SetActive(created);
                }
                else if (found is not null)
                {
                    store.SetActive(found.Guid);
                }
                else
                {
                    throw new InvalidOperationException("That power plan is not on this PC.");
                }
                break;
            case SetHibernation h:
                store.SetHibernation(h.Enabled);
                break;
        }
    }
}
