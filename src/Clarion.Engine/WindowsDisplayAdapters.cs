using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Clarion.Core.Actions;

namespace Clarion.Engine;

/// <summary>
/// The graphics adapters, found through the Windows device setup functions. A restart is the request Device Manager makes when the properties
/// of a device change (DIF_PROPERTYCHANGE with DICS_PROPCHANGE), which stops the driver and starts it again. Unlike turning the device off and
/// on again, it is one request, so there is no moment where the adapter is off and the second half fails.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsDisplayAdapters : IDisplayAdapters
{
    // The setup class of display adapters, {4d36e968-e325-11ce-bfc1-08002be10318}. Microsoft lists it among the system-defined device setup classes.
    private static readonly Guid DisplayClass = new("4d36e968-e325-11ce-bfc1-08002be10318");

    private const int DigcfPresent = 0x2;
    private const int DifPropertyChange = 0x12;
    private const int DicsPropChange = 3, DicsFlagGlobal = 1;
    private const int SpdrpDeviceDesc = 0x0, SpdrpFriendlyName = 0xC;
    private const uint DnStarted = 0x8;
    private static readonly IntPtr InvalidHandle = new(-1);

    [StructLayout(LayoutKind.Sequential)]
    private struct DeviceInfoData
    {
        public int Size;
        public Guid ClassGuid;
        public int DevInst;
        public IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PropChangeParams
    {
        public int HeaderSize;
        public int InstallFunction;
        public int StateChange;
        public int Scope;
        public int HwProfile;
    }

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern IntPtr SetupDiGetClassDevsW(ref Guid classGuid, IntPtr enumerator, IntPtr parent, int flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiEnumDeviceInfo(IntPtr set, int index, ref DeviceInfoData data);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetupDiGetDeviceInstanceIdW(IntPtr set, ref DeviceInfoData data, StringBuilder? buffer, int size, out int required);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetupDiGetDeviceRegistryPropertyW(IntPtr set, ref DeviceInfoData data, int property, out int type, byte[]? buffer, int size, out int required);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiSetClassInstallParamsW(IntPtr set, ref DeviceInfoData data, ref PropChangeParams parameters, int size);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiCallClassInstaller(int function, IntPtr set, ref DeviceInfoData data);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);

    [DllImport("cfgmgr32.dll")]
    private static extern int CM_Get_DevNode_Status(out uint status, out uint problem, int devInst, int flags);

    public IReadOnlyList<DisplayAdapter> Present()
    {
        var found = new List<DisplayAdapter>();
        var classGuid = DisplayClass;
        var set = SetupDiGetClassDevsW(ref classGuid, IntPtr.Zero, IntPtr.Zero, DigcfPresent);
        if (set == InvalidHandle) throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            for (var index = 0; ; index++)
            {
                var data = NewData();
                if (!SetupDiEnumDeviceInfo(set, index, ref data)) break;
                var running = CM_Get_DevNode_Status(out var status, out var problem, data.DevInst, 0) == 0 && (status & DnStarted) != 0 && problem == 0;
                found.Add(new DisplayAdapter(InstanceId(set, ref data), Name(set, ref data), running));
            }
        }
        finally { SetupDiDestroyDeviceInfoList(set); }
        return found;
    }

    public string? Restart(DisplayAdapter adapter)
    {
        var classGuid = DisplayClass;
        var set = SetupDiGetClassDevsW(ref classGuid, IntPtr.Zero, IntPtr.Zero, DigcfPresent);
        if (set == InvalidHandle) return new Win32Exception(Marshal.GetLastWin32Error()).Message;
        try
        {
            for (var index = 0; ; index++)
            {
                var data = NewData();
                if (!SetupDiEnumDeviceInfo(set, index, ref data)) return "The adapter is no longer present.";
                if (!string.Equals(InstanceId(set, ref data), adapter.InstanceId, StringComparison.OrdinalIgnoreCase)) continue;

                var change = new PropChangeParams
                {
                    HeaderSize = 8,
                    InstallFunction = DifPropertyChange,
                    StateChange = DicsPropChange,
                    Scope = DicsFlagGlobal,
                    HwProfile = 0,
                };
                if (!SetupDiSetClassInstallParamsW(set, ref data, ref change, Marshal.SizeOf<PropChangeParams>()) ||
                    !SetupDiCallClassInstaller(DifPropertyChange, set, ref data))
                    return new Win32Exception(Marshal.GetLastWin32Error()).Message;
                return null;
            }
        }
        finally { SetupDiDestroyDeviceInfoList(set); }
    }

    private static DeviceInfoData NewData() => new() { Size = Marshal.SizeOf<DeviceInfoData>() };

    private static string InstanceId(IntPtr set, ref DeviceInfoData data)
    {
        var buffer = new StringBuilder(512);
        return SetupDiGetDeviceInstanceIdW(set, ref data, buffer, buffer.Capacity, out _) ? buffer.ToString() : "";
    }

    /// <summary>The friendly name when the device has one, otherwise its description.</summary>
    private static string Name(IntPtr set, ref DeviceInfoData data)
    {
        foreach (var property in new[] { SpdrpFriendlyName, SpdrpDeviceDesc })
        {
            var buffer = new byte[1024];
            if (SetupDiGetDeviceRegistryPropertyW(set, ref data, property, out _, buffer, buffer.Length, out var length) && length > 2)
                return Encoding.Unicode.GetString(buffer, 0, Math.Min(length, buffer.Length)).TrimEnd('\0');
        }
        return "Graphics adapter";
    }
}
