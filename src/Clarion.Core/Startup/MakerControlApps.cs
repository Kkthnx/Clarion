namespace Clarion.Core.Startup;

/// <summary>A control app that a laptop or PC maker ships, with what the maker says it does.</summary>
public sealed record MakerControlApp(string Maker, string Product, string[] Names, string Handles, string Source);

/// <summary>
/// Finds the startup entries of the maker's own control software, such as the app behind a gaming laptop's fan modes and Fn keys.
/// Turning such an entry off is usually safe and is better than uninstalling the app, but on some models a feature or a hotkey then
/// waits for the app to be started by hand, so the Startup tab says so before it is switched off. An entry is recognised by its own name,
/// its command or the description in its file, never by the maker alone, since a maker also ships promotions that are fine to stop.
/// </summary>
public static class MakerControlApps
{
    // What each app handles is taken from the maker's own pages, linked in Source. The names are the product names the makers use.
    public static readonly IReadOnlyList<MakerControlApp> All =
    [
        new("ASUS", "Armoury Crate", ["Armoury Crate", "ArmouryCrate"],
            "the performance and fan modes, the keyboard lighting and some of the keys on ROG, TUF and other ASUS laptops",
            "https://www.asus.com/us/support/faq/1043747/"),
        new("Lenovo", "Lenovo Vantage and Legion Zone", ["Lenovo Vantage", "LenovoVantage", "Legion Zone", "LegionZone"],
            "the performance modes that Fn+Q switches between on Legion laptops, and other Lenovo settings",
            "https://download.lenovo.com/pccbbs/pubs/loq_x90/ug/html_en/en/intro_operation_mode_legion.html"),
        new("MSI", "MSI Center", ["MSI Center", "MSI_Center", "Dragon Center", "DragonCenter"],
            "the User Scenario modes and the fan speed settings on MSI laptops",
            "https://us.msi.com/faq/2155"),
        new("Dell", "Alienware Command Center", ["Alienware Command Center", "AlienwareCommandCenter"],
            "the thermal modes, the fan speed and the lighting on Alienware laptops",
            "https://www.dell.com/support/kbdoc/en-us/000198980/alienware-x15-r2-and-x17-r2-guide-to-thermal-controls-in-operating-modes-in-alienware-command-center"),
        new("HP", "HP System Event Utility", ["HP System Event Utility", "HPSystemEventUtility"],
            "the programmable special function keys, such as the OMEN key, on supported HP notebooks",
            "https://ftp.hp.com/pub/softpaq/sp91501-92000/sp91504.html"),
        new("Acer", "NitroSense and PredatorSense", ["NitroSense", "PredatorSense"],
            "the fan modes, the power plan and the hotkeys on Acer Nitro and Predator laptops",
            "https://community.acer.com/en/kb/articles/16481-acer-nitrosense-software"),
    ];

    public static MakerControlApp? Find(StartupEntry entry)
    {
        var text = $"{entry.Name} {entry.Command} {entry.Program.Description} {entry.Program.Path}";
        return All.FirstOrDefault(a => a.Names.Any(n => text.Contains(n, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>The sentence the Startup tab shows under the entry.</summary>
    public static string Warning(MakerControlApp app) =>
        $"{app.Product} is {app.Maker}'s own control software. It handles {app.Handles}. Turning it off here is safer than uninstalling it, but on some models those features or keys then wait until you start it by hand. If something stops working, turn it back on.";
}
