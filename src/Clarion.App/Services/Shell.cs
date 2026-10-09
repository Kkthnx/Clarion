using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Clarion.App.Services;

/// <summary>Small helpers for handing something to Windows: opening a file or program, and finding the window of a running copy.</summary>
public static class Shell
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? className, string windowName);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int command);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    private const int Restore = 9;

    /// <summary>Opens a file, folder, address or program. Returns false, and logs why, when Windows cannot.</summary>
    public static bool Open(string target, string? arguments = null)
    {
        try
        {
            var info = new ProcessStartInfo(target) { UseShellExecute = true };
            if (arguments is not null) info.Arguments = arguments;
            // The handle is not needed. Without this the process object would hold it until the next collection.
            using var started = Process.Start(info);
            return true;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            Log.Write($"Could not open {target}: {ex.Message}");
            return false;
        }
    }

    /// <summary>Brings the window of a copy that is already running to the front. Returns false when it cannot be found.</summary>
    public static bool BringExistingToFront(string windowTitle)
    {
        var hwnd = FindWindow(null, windowTitle);
        if (hwnd == IntPtr.Zero) return false;
        if (IsIconic(hwnd)) ShowWindow(hwnd, Restore);
        return SetForegroundWindow(hwnd);
    }
}
