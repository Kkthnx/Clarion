using Clarion.Core.Abstractions;
using Clarion.Core.Model;
using Clarion.Core.Startup;

namespace Clarion.Core.Engine;

/// <summary>
/// Turns startup entries on and off through the mark under StartupApproved. The undo is the mark exactly as it was, or no mark at all,
/// which Windows reads as on, so putting a change back leaves nothing different from before.
/// </summary>
public sealed class StartupHandler(IRegistryStore store, TimeProvider? clock = null) : IOperationHandler
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;

    public bool Handles(Operation op) => op is SetStartupEnabled;

    public bool IsApplicable(Operation op) => true;

    private static RegistryTarget TargetOf(SetStartupEnabled s) =>
        new(StartupApproval.IsPerUser(s.Origin) ? RegistryHive.CurrentUser : RegistryHive.LocalMachine, StartupApproval.KeyOf(s.Origin), s.Name);

    public bool IsSatisfied(Operation op)
    {
        var step = (SetStartupEnabled)op;
        return IsEnabledNow(step) == step.Enabled;
    }

    private bool IsEnabledNow(SetStartupEnabled step)
    {
        var now = store.Read(TargetOf(step));
        if (!now.Exists || now.Data is not { Kind: RegistryKind.Binary } data) return true;
        try { return StartupApproval.IsEnabled(Convert.FromHexString(data.Value)); }
        catch (FormatException) { return true; }
    }

    public string? WhyBroken(Operation op) =>
        op is SetStartupEnabled { Enabled: false } s ? $"{s.Name} is set to start with Windows again. A program can turn its own entry back on." : null;

    public Operation CaptureUndo(Operation op)
    {
        var target = TargetOf((SetStartupEnabled)op);
        var prior = store.Read(target);
        if (!prior.Exists) return new DeleteRegistryValue(target, prior.DeepestExistingKey);
        if (prior.Data is not { Kind: RegistryKind.Binary } data)
            throw new NotSupportedException($"{target.Name} holds a kind of value that .Clarion cannot put back, so it was left as it is.");
        return new SetRegistryValue(target, data);
    }

    public void Execute(Operation op)
    {
        var step = (SetStartupEnabled)op;
        var hex = step.Enabled ? StartupApproval.EnabledHex : StartupApproval.DisabledHex(_clock.GetUtcNow());
        store.Write(TargetOf(step), new RegistryData(RegistryKind.Binary, hex));
    }
}
