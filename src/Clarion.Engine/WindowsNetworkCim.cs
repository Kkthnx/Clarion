using System.Management;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Clarion.Core.Dns;
using Microsoft.Win32;

namespace Clarion.Engine;

/// <summary>
/// Reads the network adapters and the encrypted DNS entries from the management classes that Get-NetAdapter and
/// Get-DnsClientDohServerAddress are built on, in this process. Those two cmdlets took 0.8 and 0.4 seconds on the PC this was built on,
/// after PowerShell had started, and the classes answer in about 15 milliseconds. Every method answers null when the classes cannot be
/// read, and the caller then asks PowerShell as before. Nothing is written.
/// </summary>
[SupportedOSPlatform("windows")]
internal static partial class WindowsNetworkCim
{
    private const string Scope = "root\\StandardCimv2";
    private const string InterfacesKey = "SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\";

    // IF_OPER_STATUS in the IP Helper reference: 1 is up. Get-NetAdapter reports Up for the same value.
    private const uint OperationalUp = 1;

    [GeneratedRegex("[,\\s]+")]
    private static partial Regex Separators();

    /// <summary>
    /// The connected hardware adapters, or with <paramref name="ifIndex"/> the one adapter with that index in any state. Only the servers
    /// set by hand are listed. The ones a router hands out are left out, since writing those back would turn an automatic adapter into a
    /// fixed one. An empty server list means automatic.
    /// </summary>
    public static List<AdapterDns>? Adapters(int? ifIndex = null)
    {
        try
        {
            var where = ifIndex is { } i ? $" WHERE InterfaceIndex = {i}" : "";
            using var searcher = new ManagementObjectSearcher(Scope,
                $"SELECT InterfaceIndex, Name, InterfaceGuid, HardwareInterface, InterfaceOperationalStatus FROM MSFT_NetAdapter{where}");
            var found = new List<AdapterDns>();
            foreach (ManagementBaseObject adapter in searcher.Get())
            {
                using (adapter)
                {
                    if (ifIndex is null && !(Convert.ToBoolean(adapter["HardwareInterface"]) && Convert.ToUInt32(adapter["InterfaceOperationalStatus"]) == OperationalUp))
                        continue;
                    found.Add(new AdapterDns(Convert.ToInt32(adapter["InterfaceIndex"]), adapter["Name"] as string ?? "", StaticServers(adapter["InterfaceGuid"] as string)));
                }
            }
            return found;
        }
        catch (Exception ex) when (ex is ManagementException or System.Runtime.InteropServices.COMException or InvalidCastException or FormatException)
        {
            return null;
        }
    }

    public static List<DohRegistration>? DohRegistrations()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(Scope,
                "SELECT ServerAddress, DohTemplate, AutoUpgrade, AllowFallbackToUdp FROM MSFT_DNSClientDohServerAddress");
            var found = new List<DohRegistration>();
            foreach (ManagementBaseObject entry in searcher.Get())
            {
                using (entry)
                {
                    var address = entry["ServerAddress"] as string ?? "";
                    if (!DnsProviders.IsIpv4(address)) continue;
                    found.Add(new DohRegistration(address, entry["DohTemplate"] as string ?? "", Convert.ToBoolean(entry["AutoUpgrade"]), Convert.ToBoolean(entry["AllowFallbackToUdp"])));
                }
            }
            return found;
        }
        catch (Exception ex) when (ex is ManagementException or System.Runtime.InteropServices.COMException or InvalidCastException or FormatException)
        {
            return null;
        }
    }

    private static List<string> StaticServers(string? interfaceGuid)
    {
        if (string.IsNullOrEmpty(interfaceGuid)) return [];
        using var key = Registry.LocalMachine.OpenSubKey(InterfacesKey + interfaceGuid);
        return key?.GetValue("NameServer") is string { Length: > 0 } text
            ? Separators().Split(text).Where(DnsProviders.IsIpv4).ToList()
            : [];
    }
}
