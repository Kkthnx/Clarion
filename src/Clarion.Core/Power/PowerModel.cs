using System.Text.RegularExpressions;

namespace Clarion.Core.Power;

public sealed record PowerPlan(string Guid, string Name, bool IsActive);

public interface IPowerStore
{
    IReadOnlyList<PowerPlan> List();
    void SetActive(string guid);

    /// <summary>Copies a built in plan from its template and returns the new plan GUID.</summary>
    string Duplicate(string templateGuid);

    void Rename(string guid, string name);

    /// <summary>True when hibernation is enabled, false when off, null when it cannot be read.</summary>
    bool? IsHibernationEnabled();

    void SetHibernation(bool enabled);
}

public static partial class PowerPlans
{
    public const string Balanced = "381b4222-f694-41f0-9685-ff5bb260df2e";
    public const string HighPerformance = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
    public const string PowerSaver = "a1841308-3541-4fab-bc81-f71556f20b4a";
    public const string UltimateTemplate = "e9a42b02-d5df-448d-aa00-03f14749eb61";
    public const string UltimateName = "Ultimate Performance";

    [GeneratedRegex(@"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\s+\((.*?)\)(\s*\*)?\s*$")]
    private static partial Regex LinePattern();

    [GeneratedRegex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")]
    private static partial Regex GuidPattern();

    public static bool IsGuid(string value) => GuidPattern().IsMatch(value);

    public static bool IsKnownAlias(string value) => value is "balanced" or "high-performance" or "power-saver" or "ultimate";

    public static bool IsValid(string value) => IsGuid(value) || IsKnownAlias(value);

    /// <summary>Reads the output of the plan list command. It does not depend on the Windows language.</summary>
    public static IReadOnlyList<PowerPlan> ParseList(string output)
    {
        var plans = new List<PowerPlan>();
        foreach (var raw in output.Split('\n'))
        {
            var m = LinePattern().Match(raw.TrimEnd('\r'));
            if (m.Success) plans.Add(new PowerPlan(m.Groups[1].Value.ToLowerInvariant(), m.Groups[2].Value, m.Groups[3].Success));
        }
        return plans;
    }

    /// <summary>Finds the plan an alias or GUID refers to, or null when this PC does not have it.</summary>
    public static PowerPlan? Resolve(string plan, IReadOnlyList<PowerPlan> plans)
    {
        if (IsGuid(plan)) return plans.FirstOrDefault(p => p.Guid.Equals(plan, StringComparison.OrdinalIgnoreCase));
        return plan switch
        {
            "balanced" => plans.FirstOrDefault(p => p.Guid == Balanced),
            "high-performance" => plans.FirstOrDefault(p => p.Guid == HighPerformance),
            "power-saver" => plans.FirstOrDefault(p => p.Guid == PowerSaver),
            "ultimate" => plans.FirstOrDefault(p => p.Name.Contains(UltimateName, StringComparison.OrdinalIgnoreCase)),
            _ => null,
        };
    }
}
