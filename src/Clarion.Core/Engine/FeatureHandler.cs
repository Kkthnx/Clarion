using Clarion.Core.Abstractions;
using Clarion.Core.Features;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

/// <summary>Turns Windows optional features and capabilities on or off, and remembers how to put them back.</summary>
public sealed class FeatureHandler(IFeatureStore store) : IOperationHandler
{
    public bool Handles(Operation op) => op is SetWindowsFeature or SetWindowsCapability;

    public bool IsApplicable(Operation op) => op switch
    {
        SetWindowsFeature f => store.IsFeatureEnabled(f.Name) is not null,
        SetWindowsCapability c => store.IsCapabilityInstalled(c.Name) is not null,
        _ => false,
    };

    public bool IsSatisfied(Operation op) => op switch
    {
        SetWindowsFeature f => store.IsFeatureEnabled(f.Name) == f.Enabled,
        SetWindowsCapability c => store.IsCapabilityInstalled(c.Name) == c.Installed,
        _ => false,
    };

    public Operation CaptureUndo(Operation op) => op switch
    {
        SetWindowsFeature f => new SetWindowsFeature(f.Name, store.IsFeatureEnabled(f.Name) ?? false),
        SetWindowsCapability c => new SetWindowsCapability(c.Name, store.IsCapabilityInstalled(c.Name) ?? false),
        _ => throw new InvalidOperationException("Not a feature operation"),
    };

    public void Execute(Operation op)
    {
        switch (op)
        {
            case SetWindowsFeature f:
                if (!FeatureRules.IsValidName(f.Name)) throw new InvalidOperationException($"Not a valid feature name: {f.Name}");
                if (!f.Enabled && !FeatureRules.CanDisable(f.Name)) throw new InvalidOperationException($"{f.Name} is protected and will not be turned off.");
                store.SetFeature(f.Name, f.Enabled);
                break;
            case SetWindowsCapability c:
                if (!FeatureRules.IsValidName(c.Name)) throw new InvalidOperationException($"Not a valid capability name: {c.Name}");
                store.SetCapability(c.Name, c.Installed);
                break;
        }
    }
}
