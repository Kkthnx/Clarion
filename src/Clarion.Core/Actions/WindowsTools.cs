namespace Clarion.Core.Actions;

/// <summary>One classic Windows tool that has a button on the Repair page: a name, the program that hosts it and the file it opens.</summary>
public sealed record WindowsTool(string Name, string Summary, string Host, string File)
{
    /// <summary>The command line for the tool. Both the host program and the file are in the Windows system folder, named by full path.</summary>
    public (string Program, string Arguments) CommandLine(string systemDirectory) =>
        (Path.Combine(systemDirectory, Host), $"\"{Path.Combine(systemDirectory, File)}\"");
}

/// <summary>
/// Shortcuts to the old Control Panel items and consoles that Windows 11 Settings still sends people to. Each one only opens a window that ships
/// with Windows. Nothing is changed by opening it, and the tool asks for administrator rights itself if it needs them.
/// </summary>
public static class WindowsTools
{
    public static readonly IReadOnlyList<WindowsTool> All =
    [
        new("Programs and Features", "Uninstall or repair a program, and turn Windows features on or off.", "control.exe", "appwiz.cpl"),
        new("Network connections", "Every network adapter, with its properties and its enable and disable switch.", "control.exe", "ncpa.cpl"),
        new("Sound", "Playback and recording devices, with the old per device properties.", "control.exe", "mmsys.cpl"),
        new("Power options", "The power plans and what each one does.", "control.exe", "powercfg.cpl"),
        new("Date and time", "The clock, the time zone and the internet time server.", "control.exe", "timedate.cpl"),
        new("Region", "Formats, location and the language for programs without Unicode.", "control.exe", "intl.cpl"),
        new("System properties", "Computer name, performance options, startup and recovery, and System Protection.", "control.exe", "sysdm.cpl"),
        new("Device Manager", "Every device and its driver, with the option to roll a driver back.", "mmc.exe", "devmgmt.msc"),
        new("Disk Management", "Drives, partitions and volumes.", "mmc.exe", "diskmgmt.msc"),
        new("Event Viewer", "The Windows logs, where a crash or a failed update leaves its reason.", "mmc.exe", "eventvwr.msc"),
    ];
}
