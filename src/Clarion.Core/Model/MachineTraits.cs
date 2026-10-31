namespace Clarion.Core.Model;

/// <summary>What kind of PC this is, as far as that changes which settings are a good idea.</summary>
public sealed record MachineTraits
{
    /// <summary>A desktop, or a PC nothing is known about. Nothing is held back on it.</summary>
    public static MachineTraits Desktop { get; } = new();

    /// <summary>True for a laptop, notebook, convertible, detachable, tablet or handheld, which runs on a battery and has a small cooling budget.</summary>
    public bool IsPortable { get; init; }

    /// <summary>The PC reports a battery.</summary>
    public bool HasBattery { get; init; }

    /// <summary>A portable PC with two real graphics adapters, an integrated one for the desktop and a discrete one for games.</summary>
    public bool HasHybridGraphics { get; init; }

    /// <summary>The maker named in the firmware, such as ASUS or Lenovo. Empty when it says nothing.</summary>
    public string Maker { get; init; } = "";

    /// <summary>
    /// The SMBIOS chassis types that are carried around. Microsoft's Win32_SystemEnclosure reference lists them as Portable (8), Laptop (9),
    /// Notebook (10), Hand Held (11), Sub Notebook (14), Tablet (30), Convertible (31) and Detachable (32).
    /// </summary>
    private static readonly int[] PortableChassis = [8, 9, 10, 11, 14, 30, 31, 32];

    public static bool IsPortableChassis(int chassisType) => PortableChassis.Contains(chassisType);

    /// <summary>
    /// Works out the traits from what the firmware and Windows report. Either a portable chassis or a battery makes a PC portable, since
    /// some gaming laptops report an unhelpful chassis such as Other (1) and a desktop never reports a system battery.
    /// </summary>
    public static MachineTraits Classify(IEnumerable<int> chassisTypes, bool hasBattery, IEnumerable<string> adapterNames, string maker)
    {
        var portable = hasBattery || chassisTypes.Any(IsPortableChassis);
        var realAdapters = adapterNames.Count(n => !IsVirtualAdapter(n));
        return new MachineTraits
        {
            IsPortable = portable,
            HasBattery = hasBattery,
            HasHybridGraphics = portable && realAdapters >= 2,
            Maker = maker.Trim(),
        };
    }

    /// <summary>Adapters that are not a graphics chip: the fallback driver, remote sessions and virtual displays.</summary>
    internal static bool IsVirtualAdapter(string name) =>
        new[] { "Basic Display", "Basic Render", "Remote Display", "Virtual", "Indirect Display", "Parsec", "Citrix", "Hyper-V Video" }
            .Any(v => name.Contains(v, StringComparison.OrdinalIgnoreCase));
}
