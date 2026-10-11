namespace Clarion.Core.Drift;

/// <summary>
/// Says what is worth trying when a setting keeps being changed back. It uses only what Clarion recorded: the Windows build at each time
/// the setting was found changed back. The build number changes with a Windows version update and not with the monthly fixes, so it
/// separates "an update did this" from "something running on this PC did this". It makes no claim about which program or policy it is.
/// </summary>
public static class RepeatAdvice
{
    /// <param name="buildsWhenChangedBack">The build at each separate time the setting was found changed back, oldest first.</param>
    /// <returns>A short plain paragraph, or null when it has not happened twice.</returns>
    public static string? For(IReadOnlyList<int> buildsWhenChangedBack)
    {
        if (buildsWhenChangedBack.Count < 2) return null;

        var sameEveryTime = buildsWhenChangedBack.Distinct().Count() == 1;
        if (sameEveryTime)
            return "It came back each time on the same Windows version, so a version update did not do it. Something that keeps running on this PC did, such as a policy from a workplace or school, another tweak tool or a scheduled task. Putting it back again may not last until that is found.";

        var newerEachTime = buildsWhenChangedBack.Zip(buildsWhenChangedBack.Skip(1), (earlier, later) => later > earlier).All(x => x);
        if (newerEachTime)
            return "Each time it came back, Windows was on a newer version than the time before. Putting it back again is the right fix, and Verify will say so if it comes back without an update.";

        return "Some of these followed a Windows version change and some did not, so more than one thing is changing it. If it keeps coming back without an update, look for another tweak tool, a workplace or school policy or a scheduled task.";
    }
}
