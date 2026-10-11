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
        // A web page is opened for the signed-in user, not with Clarion's administrator rights, so the browser is not elevated.
        if (arguments is null && Uri.TryCreate(target, UriKind.Absolute, out var link) && link.Scheme is "http" or "https" && OpenUnelevated(link)) return true;
        try
        {
            // A program named without a folder (notepad.exe, rstrui.exe) is looked for beside Clarion first, and Clarion runs with
            // administrator rights, so those names are turned into the copy in the Windows folder before anything starts.
            var file = target.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? Clarion.Core.Engine.SystemTools.Resolve(target, Environment.SystemDirectory)
                : target;
            var info = new ProcessStartInfo(file) { UseShellExecute = true };
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

    private static bool OpenUnelevated(Uri link)
    {
        try
        {
            // rundll32 with url.dll hands the address to whatever is registered for it, which then runs with the shell's token.
            var rundll = Path.Combine(Environment.SystemDirectory, "rundll32.exe");
            return Clarion.Engine.UnelevatedLauncher.Start(rundll, $"url.dll,FileProtocolHandler {link.AbsoluteUri}") != 0;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException)
        {
            Log.Write($"Could not open {link.Host} without elevation: {ex.Message}");
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
