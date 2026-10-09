using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Core.Appx;

/// <summary>
/// The "also stop it for new accounts" form of an app removal. Removing the provisioned package
/// keeps Windows from installing the app for accounts created later. It needs administrator rights,
/// and Revert cannot undo it: the app can be put back for you, but only the Store reinstalls it for others.
/// </summary>
public static class DeprovisionVariant
{
    /// <summary>True for settings made only of app removals that do not already stop new accounts.</summary>
    public static bool CanApply(Tweak tweak) =>
        tweak.Apply.Count > 0 &&
        tweak.Apply.All(o => o is RemoveAppxPackage) &&
        tweak.Apply.OfType<RemoveAppxPackage>().Any(r => !r.Deprovision);

    public static Tweak Of(Tweak tweak) => tweak with
    {
        Scope = TweakScope.Machine,
        Apply = tweak.Apply.Select(o => o is RemoveAppxPackage r ? r with { Deprovision = true } : o).ToList(),
    };

    /// <summary>True when the recorded steps of a setting included stopping new accounts from getting the app.</summary>
    public static bool WasRecorded(IEnumerable<JournalEntry> recorded) =>
        recorded.Any(e => e.Operation is RemoveAppxPackage { Deprovision: true });
}
