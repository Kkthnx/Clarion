namespace Clarion.Core.SystemInfo;

/// <summary>Where the PC gets its power right now.</summary>
/// <param name="HasBattery">Windows reports a system battery.</param>
/// <param name="OnBattery">The charger is not connected.</param>
/// <param name="Charging">The battery is taking charge.</param>
/// <param name="Percent">The charge left, 0 to 100, or null when Windows cannot say.</param>
public sealed record PowerSource(bool HasBattery, bool OnBattery, bool Charging, int? Percent)
{
    /// <summary>A desktop, or a PC that cannot say. Nothing is warned about.</summary>
    public static PowerSource None { get; } = new(false, false, false, null);
}

/// <summary>
/// Decides when a long job should wait for the charger. Updates, repairs and installs run for minutes, and a laptop that loses power in the
/// middle of one can be left with a half finished update or a damaged system file. The warning is advice, and the person can start anyway.
/// </summary>
public static class BatteryGuard
{
    /// <summary>The charge at or below which a long job on battery power is worth a warning.</summary>
    public const int LowPercent = 30;

    /// <summary>A job shorter than this is over before the battery matters, so it is never warned about.</summary>
    public const int ShortestWorthWarning = 5;

    /// <summary>
    /// The warning for starting <paramref name="job"/> now, or null when the power is fine or the job is short. <paramref name="minutes"/> is
    /// the time the job is known to take, or null when it only is known to take a while, such as an install.
    /// </summary>
    public static string? Warning(PowerSource power, string job, int? minutes = null)
    {
        if (!power.HasBattery || !power.OnBattery || power.Charging) return null;
        if (power.Percent is not { } percent || percent > LowPercent) return null;
        if (minutes is < ShortestWorthWarning) return null;
        var length = minutes is { } m ? $"takes about {m} minutes" : "can take several minutes";
        return $"{job} {length}, and a laptop that runs out of power part way can be left half changed. " +
               $"The battery is at {percent}% and the charger is not connected. Plug it in first, or start anyway.";
    }
}
