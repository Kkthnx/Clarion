namespace Clarion.Core.Layout;

/// <summary>Decides whether a setting answers what was typed in a search box.</summary>
public static class SearchMatch
{
    /// <summary>
    /// Every word typed has to be found somewhere in the texts of the setting, in any order, so "accel mouse" finds the mouse acceleration
    /// setting. Case is ignored. Nothing typed matches everything.
    /// </summary>
    public static bool Matches(string query, IReadOnlyList<string> texts)
    {
        var words = query.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return words.All(word => texts.Any(text => text.Contains(word, StringComparison.CurrentCultureIgnoreCase)));
    }

    /// <summary>The sentence shown in place of the list when nothing is left, saying why and what to try.</summary>
    public static string EmptyMessage(string query, bool filtered, bool hidesRiskier)
    {
        var trimmed = query.Trim();
        if (trimmed.Length > 0)
            return $"Nothing matches \"{trimmed}\". Try fewer or shorter words{(filtered ? ", or choose All settings" : "")}.{(hidesRiskier ? " Some settings are only shown in Expert mode, which you can turn on in Settings." : "")}";
        if (filtered) return "No settings match this filter. Choose All settings to see them again.";
        return hidesRiskier ? "Nothing to show here. Some settings are only shown in Expert mode, which you can turn on in Settings." : "Nothing to show here.";
    }
}
