namespace Clarion.Core.Apps;

/// <summary>How one app's update ended.</summary>
public sealed record AppUpdateResult(AppUpdate Update, bool Succeeded, bool Skipped, string Message);

/// <summary>
/// Updates the apps a person chose, one at a time, in the order given. A stop request ends the run at once: it tells winget to stop the installer
/// that is running now, and the apps after it are reported as not started. An update that fails does not stop the next one. The update is not
/// recorded for Revert, because winget keeps no copy of the old version, and the result says so in the window and the log.
/// </summary>
public static class AppUpdater
{
    public static IReadOnlyList<AppUpdateResult> Run(
        IWingetStore store, IEnumerable<AppUpdate> updates, bool acceptPackageAgreements, Action<string> say, CancellationToken stop)
    {
        var results = new List<AppUpdateResult>();
        using var cancel = stop.Register(store.CancelRunning);
        foreach (var update in updates)
        {
            if (stop.IsCancellationRequested)
            {
                results.Add(new AppUpdateResult(update, false, true, "Not started, because you stopped the run."));
                continue;
            }

            say($"Updating {update.Id} from {update.Current} to {update.Available}");
            WingetOutcome outcome;
            try
            {
                outcome = store.Upgrade(update.Id, acceptPackageAgreements);
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or ArgumentException)
            {
                // A package ID that cannot be put on a command line, or winget gone missing mid run, ends this app and not the others.
                store.Invalidate();
                results.Add(new AppUpdateResult(update, false, false, ex.Message));
                say($"{update.Id}: {ex.Message}");
                continue;
            }
            store.Invalidate();

            var message = WingetCodes.ExplainUpgrade(outcome.ExitCode);
            var ok = WingetCodes.IsUpgradeSuccess(outcome.ExitCode);
            results.Add(new AppUpdateResult(update, ok, false, ok ? message : $"{message}{InstallerCode(outcome)}"));
            say($"{update.Id}: {results[^1].Message}");
        }
        return results;
    }

    // The installer's own exit code, which winget prints in English only, is what a search for the error needs.
    private static string InstallerCode(WingetOutcome outcome)
    {
        var message = outcome.Message;
        var sentence = WingetCodes.Explain(outcome.ExitCode);
        return message.Length > sentence.Length && message.StartsWith(sentence, StringComparison.Ordinal) ? message[sentence.Length..] : "";
    }

    /// <summary>One line for the end of a run, such as "3 updated, 1 failed".</summary>
    public static string Summary(IReadOnlyList<AppUpdateResult> results)
    {
        var done = results.Count(r => r.Succeeded);
        var failed = results.Count(r => !r.Succeeded && !r.Skipped);
        var skipped = results.Count(r => r.Skipped);
        var parts = new List<string> { $"{done} updated" };
        if (failed > 0) parts.Add($"{failed} failed");
        if (skipped > 0) parts.Add($"{skipped} not started");
        return string.Join(", ", parts);
    }
}
