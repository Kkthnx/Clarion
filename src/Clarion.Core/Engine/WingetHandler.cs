using Clarion.Core.Abstractions;
using Clarion.Core.Apps;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

/// <summary>Installs and uninstalls packages through winget, so apps get the same journal, Revert and Verify as settings do.</summary>
public sealed class WingetHandler(IWingetStore store) : IOperationHandler
{
    public bool Handles(Operation op) => op is InstallWingetPackage or UninstallWingetPackage;

    /// <summary>Without winget there is nothing to do, so the app shows as unavailable and not as switched off.</summary>
    public bool IsApplicable(Operation op) => store.IsAvailable;

    public void Invalidate() => store.Invalidate();

    public void Warm(IReadOnlyList<Operation> operations)
    {
        if (store.IsAvailable && operations.Any(Handles)) store.InstalledIds();
    }

    public bool IsSatisfied(Operation op) => op switch
    {
        InstallWingetPackage i => store.InstalledIds().Contains(i.Id),
        UninstallWingetPackage u => !store.InstalledIds().Contains(u.Id),
        _ => false,
    };

    public string? WhyBroken(Operation op) =>
        op is InstallWingetPackage i ? $"{i.Id} is no longer installed" : null;

    public Operation CaptureUndo(Operation op) => op switch
    {
        InstallWingetPackage i => new UninstallWingetPackage(i.Id),
        UninstallWingetPackage u => new InstallWingetPackage(u.Id),
        _ => throw new InvalidOperationException("Not a winget operation"),
    };

    public void Execute(Operation op)
    {
        switch (op)
        {
            case InstallWingetPackage install:
                // The licence terms are accepted by a person on the review screen, never here on their behalf.
                if (!store.AcceptPackageAgreements)
                    throw new InvalidOperationException("The licence terms were not accepted, so nothing was installed.");
                Finish(store.Install(install.Id));
                break;
            case UninstallWingetPackage uninstall:
                var outcome = store.Uninstall(uninstall.Id);
                // Already gone, for example removed by hand since it was installed, is the state that was asked for.
                if (outcome.ExitCode == WingetCodes.NoPackagesFound) store.Invalidate();
                else Finish(outcome);
                break;
        }
    }

    private void Finish(WingetOutcome outcome)
    {
        store.Invalidate();
        if (!outcome.Succeeded) throw new InvalidOperationException(outcome.Message);
    }
}
