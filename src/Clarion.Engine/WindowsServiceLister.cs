using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Clarion.Core.Model;
using Clarion.Core.WindowsServices;
using Microsoft.Win32;

namespace Clarion.Engine;

/// <summary>
/// Lists every Windows service with the service control manager's own enumeration, which also says which process hosts each one. The
/// start type is read from the service's registry key, the same way the settings read it. Nothing is changed. A per-user service has
/// one copy for each signed in user, so those copies are shown once, under the name of the template they come from.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed partial class WindowsServiceLister : IServiceLister
{
    private const string Root = @"SYSTEM\CurrentControlSet\Services\";
    private const uint ManagerConnect = 0x1;
    private const uint ManagerEnumerate = 0x4;
    private const uint ServiceQueryConfig = 0x1;
    private const uint ServiceWin32 = 0x30;
    private const uint StateAll = 3;
    private const uint StateRunning = 4;
    private const uint ConfigTriggerInfo = 8;
    private const int ErrorMoreData = 234;

    [GeneratedRegex("^(?<template>.+)_[0-9a-fA-F]{4,8}$")]
    private static partial Regex InstanceName();

    [StructLayout(LayoutKind.Sequential)]
    private struct StatusProcess
    {
        public uint ServiceType, CurrentState, ControlsAccepted, Win32ExitCode, ServiceSpecificExitCode, CheckPoint, WaitHint, ProcessId, ServiceFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct EnumEntry
    {
        public IntPtr ServiceName;
        public IntPtr DisplayName;
        public StatusProcess Status;
    }

    [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr OpenSCManager(string? machine, string? database, uint access);

    [DllImport("advapi32.dll", EntryPoint = "OpenServiceW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr OpenService(IntPtr manager, string name, uint access);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool CloseServiceHandle(IntPtr handle);

    [DllImport("advapi32.dll", EntryPoint = "EnumServicesStatusExW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool EnumServicesStatusEx(IntPtr manager, int infoLevel, uint type, uint state, IntPtr buffer, uint size,
        out uint needed, out uint returned, ref uint resume, string? group);

    [DllImport("advapi32.dll", EntryPoint = "QueryServiceConfig2W", SetLastError = true)]
    private static extern bool QueryServiceConfig2(IntPtr service, uint level, IntPtr buffer, uint size, out uint needed);

    private sealed record Raw(string Name, string Display, bool Running, int ProcessId);

    public IReadOnlyList<ServiceSnapshot> List()
    {
        var manager = OpenSCManager(null, null, ManagerConnect | ManagerEnumerate);
        if (manager == IntPtr.Zero) throw new InvalidOperationException($"The service list could not be opened (error {Marshal.GetLastWin32Error()}).");
        try
        {
            var raw = Enumerate(manager);
            // Services that share one process cannot be told apart by its memory, so that is only shown for a service that has its own.
            var sharing = raw.Where(r => r.Running && r.ProcessId > 0).GroupBy(r => r.ProcessId).ToDictionary(g => g.Key, g => g.Count());

            var result = new Dictionary<string, ServiceSnapshot>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in raw)
            {
                var template = TemplateOf(r.Name);
                var instance = !string.Equals(template, r.Name, StringComparison.OrdinalIgnoreCase);
                var shared = r.Running && r.ProcessId > 0 && sharing[r.ProcessId] > 1;
                // A copy's display name ends in the same suffix as its service name, which is no use to a reader.
                var display = instance ? InstanceName().Replace(r.Display, "${template}") : r.Display;
                var snapshot = new ServiceSnapshot(template, display, ReadStart(template), r.Running, HasTriggers(manager, r.Name),
                    r.Running && !shared && r.ProcessId > 0 ? WorkingSet(r.ProcessId) : null, shared);

                // Several copies of a per-user service collapse to one row, which is running if any copy is.
                if (instance && result.TryGetValue(template, out var seen))
                    result[template] = seen with { Running = seen.Running || snapshot.Running, MemoryBytes = null, SharedHost = seen.SharedHost || snapshot.SharedHost };
                else
                    result[template] = snapshot;
            }
            return result.Values.ToList();
        }
        finally
        {
            CloseServiceHandle(manager);
        }
    }

    private static List<Raw> Enumerate(IntPtr manager)
    {
        var list = new List<Raw>();
        uint resume = 0;
        var size = 64u * 1024;
        var buffer = Marshal.AllocHGlobal((int)size);
        try
        {
            while (true)
            {
                var ok = EnumServicesStatusEx(manager, 0, ServiceWin32, StateAll, buffer, size, out var needed, out var returned, ref resume, null);
                var error = Marshal.GetLastWin32Error();
                if (!ok && error != ErrorMoreData) throw new InvalidOperationException($"The service list could not be read (error {error}).");

                var step = Marshal.SizeOf<EnumEntry>();
                for (var i = 0; i < returned; i++)
                {
                    var entry = Marshal.PtrToStructure<EnumEntry>(buffer + i * step);
                    list.Add(new Raw(Marshal.PtrToStringUni(entry.ServiceName) ?? "", Marshal.PtrToStringUni(entry.DisplayName) ?? "",
                        entry.Status.CurrentState == StateRunning, (int)entry.Status.ProcessId));
                }
                if (ok) return list;

                // More came than fit: the next call carries on from where this one stopped, with room for at least what was asked for.
                if (needed > size)
                {
                    Marshal.FreeHGlobal(buffer);
                    size = needed + 4096;
                    buffer = Marshal.AllocHGlobal((int)size);
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>The service a per-user copy comes from, when the template has a key of its own. Other names are returned as they are.</summary>
    private static string TemplateOf(string name)
    {
        var m = InstanceName().Match(name);
        if (!m.Success) return name;
        using var key = Registry.LocalMachine.OpenSubKey(Root + m.Groups["template"].Value);
        return key is null ? name : m.Groups["template"].Value;
    }

    private static ServiceStartType? ReadStart(string name)
    {
        // Same rules as the settings use: a driver, or anything that starts at boot or with the system, is not a service to show a start type for.
        using var key = Registry.LocalMachine.OpenSubKey(Root + name);
        if (key?.GetValue("Start") is not int start) return null;
        return start switch
        {
            2 => key.GetValue("DelayedAutostart") is int d && d == 1 ? ServiceStartType.AutomaticDelayed : ServiceStartType.Automatic,
            3 => ServiceStartType.Manual,
            4 => ServiceStartType.Disabled,
            _ => null,
        };
    }

    private static long? WorkingSet(int processId)
    {
        try
        {
            using var p = Process.GetProcessById(processId);
            return p.WorkingSet64;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    /// <summary>True when Windows can start the service on an event, such as a device arriving, so Manual does not mean it never runs.</summary>
    private static bool HasTriggers(IntPtr manager, string name)
    {
        var service = OpenService(manager, name, ServiceQueryConfig);
        if (service == IntPtr.Zero) return false;
        try
        {
            QueryServiceConfig2(service, ConfigTriggerInfo, IntPtr.Zero, 0, out var needed);
            if (needed < 4 || needed > 64 * 1024) return false;
            var buffer = Marshal.AllocHGlobal((int)needed);
            try
            {
                // The structure starts with the number of triggers.
                return QueryServiceConfig2(service, ConfigTriggerInfo, buffer, needed, out _) && Marshal.ReadInt32(buffer) > 0;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            CloseServiceHandle(service);
        }
    }
}
