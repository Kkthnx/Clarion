using Clarion.Core.Abstractions;
using Clarion.Core.Apps;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

/// <summary>Installs and uninstalls packages through winget, so apps get the same journal, Revert and Verify as settings do.</summary>
/// <param name="store">winget.</param>
/// <param name="wait">How an uninstall waits for an app that is still going. Tests pass a no-op.</param>
public sealed class WingetHandler(IWingetStore store, Action<TimeSpan>? wait = null) : IOperationHandler
{
    private readonly Action<TimeSpan> _wait = wait ?? Thread.Sleep;

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
        // The folder and the scope travel with the pair: reinstalling a package whose record requires a folder fails without it, and a copy
        // installed for one account has to be removed, and put back, for that account.
        InstallWingetPackage i => new UninstallWingetPackage(i.Id, i.Location, i.Scope),
        UninstallWingetPackage u => new InstallWingetPackage(u.Id, u.Location, u.Scope),
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
                Finish(store.Install(install.Id, install.Location, install.Scope));
                break;
            case UninstallWingetPackage uninstall:
                var outcome = store.Uninstall(uninstall.Id, uninstall.Scope);
                // Already gone, for example removed by hand since it was installed, is the state that was asked for.
                if (outcome.ExitCode == WingetCodes.NoPackagesFound) store.Invalidate();
                else
                {
                    Finish(outcome);
                    EnsureGone(uninstall.Id);
                }
                break;
        }
    }

    /// <summary>
    /// winget uses an app's quiet uninstall command only when the app registered one, and otherwise runs its ordinary, interactive uninstaller and
    /// reports success as soon as that starts (winget-cli, UninstallFlow.cpp). So "uninstalled" from winget can mean a wizard is open and waiting. The
    /// app is looked for again after a short wait, because some uninstallers finish a moment later, and if it is still there the person is told.
    /// </summary>
    private void EnsureGone(string id)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try { if (!store.InstalledIds().Contains(id)) return; }
            catch (InvalidOperationException) { return; }   // winget cannot say, so the check after the step decides
            if (attempt == 2) break;
            _wait(TimeSpan.FromSeconds(4));
            store.Invalidate();
        }
        throw new InvalidOperationException(
            "winget said it uninstalled it, but it is still installed. This app's uninstaller cannot run silently, so it may be waiting for you in its own window. Finish it there, then press Re-check.");
    }

    private void Finish(WingetOutcome outcome)
    {
        store.Invalidate();
        if (!outcome.Succeeded) throw new InvalidOperationException(outcome.Message);
    }
}
