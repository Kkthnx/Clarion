using Clarion.Core.Model;

namespace Clarion.Core.SystemInfo;

/// <summary>
/// Notes for settings that behave differently on a stripped or rebranded Windows image. They are built
/// from what Clarion can see on this PC (a missing service, Store or Edge), not from a list of what a
/// given image is claimed to remove, because those lists change between versions and builds.
/// Settings whose target is already gone are handled by the engine, which shows them as having nothing to change.
/// </summary>
public static class ImageAdvice
{
    public static string? NoteFor(Tweak tweak, InstallVerdict? verdict)
    {
        if (verdict?.Inputs is not { } seen) return null;

        if (tweak.Topic == "Microsoft Edge" && seen.EdgeMissing == true)
            return "Microsoft Edge is not installed on this PC, so there is nothing for this to change.";

        if (tweak.Topic == "Defender sharing" && seen.DefenderServiceMissing)
            return "The Microsoft Defender service is not installed on this PC, so there is nothing for this to change.";

        if (tweak.Topic == "Updates" && seen.UpdateServiceMissing)
            return "The Windows Update service is not installed on this PC, so this would have no effect.";

        if (tweak.Topic == "Updates" && seen.UpdateServiceDisabled)
            return "The Windows Update service is turned off on this PC, so this has no effect until it is on.";

        if (seen.StoreMissing == true && tweak.Apply.Any(o => o is RemoveAppxPackage))
            return "The Microsoft Store is not installed on this PC. Revert can bring this app back only if Windows still has its files.";

        return null;
    }

    /// <summary>The settings that have a note on this PC, keyed by id.</summary>
    public static IReadOnlyDictionary<string, string> Notes(IEnumerable<Tweak> catalog, InstallVerdict? verdict)
    {
        var notes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in catalog)
            if (NoteFor(t, verdict) is { } note) notes[t.Id] = note;
        return notes;
    }
}
