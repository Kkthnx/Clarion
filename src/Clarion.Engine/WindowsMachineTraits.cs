using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Clarion.Core.Model;
using Clarion.Core.SystemInfo;

namespace Clarion.Engine;

/// <summary>
/// Finds out whether this is a laptop, from the firmware chassis type and from whether Windows reports a battery, and whether it has two
/// graphics adapters. Nothing is written, nothing starts a process, and it takes a few milliseconds, so it is safe to run at every start.
/// </summary>
[SupportedOSPlatform("windows")]
public static class WindowsMachineTraits
{
    // The provider signature 'RSMB' that GetSystemFirmwareTable takes for the SMBIOS table.
    private const uint RawSmbios = 0x52534D42;

    // GetSystemPowerStatus reports 128 when the PC has no system battery and 255 when it cannot tell.
    private const byte NoBattery = 128, BatteryUnknown = 255;

    [StructLayout(LayoutKind.Sequential)]
    private struct PowerStatus
    {
        public byte AcLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint GetSystemFirmwareTable(uint provider, uint table, byte[]? buffer, uint size);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemPowerStatus(out PowerStatus status);

    public static MachineTraits Detect()
    {
        try
        {
            var table = ReadTable();
            var chassis = table is null ? [] : SmbiosTable.ChassisTypes(table);
            var maker = table is null ? "" : SmbiosTable.Maker(table);
            var battery = GetSystemPowerStatus(out var power) && power.BatteryFlag != NoBattery && power.BatteryFlag != BatteryUnknown;
            // Two adapters only matter on a portable PC, so a desktop never pays for listing them.
            var portable = battery || chassis.Any(MachineTraits.IsPortableChassis);
            var adapters = portable ? AdapterNames() : [];
            return MachineTraits.Classify(chassis, battery, adapters, maker);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // A PC that cannot be classified is treated as a desktop, which holds nothing back.
            return MachineTraits.Desktop;
        }
    }

    private static byte[]? ReadTable()
    {
        var size = GetSystemFirmwareTable(RawSmbios, 0, null, 0);
        if (size == 0) return null;
        var buffer = new byte[size];
        return GetSystemFirmwareTable(RawSmbios, 0, buffer, size) == size ? buffer : null;
    }

    private static IEnumerable<string> AdapterNames()
    {
        try { return new WindowsDisplayAdapters().Present().Select(a => a.Name).ToList(); }
        catch (Exception ex) when (ex is not OutOfMemoryException) { return []; }
    }
}
