using System.Runtime.InteropServices;

namespace Clarion.Engine;

/// <summary>
/// Starts a program with the token of the running Windows shell, which is not elevated, even when Clarion itself is. A browser opened
/// from an elevated program would otherwise run elevated too, with its sandbox weakened, just to show a web page. This is the method
/// Microsoft's Raymond Chen describes for launching an unelevated program from an elevated one.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public static class UnelevatedLauncher
{
    private const uint TokenDuplicate = 0x0002;
    private const uint TokenAllAccess = 0x000F01FF;
    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const int SecurityImpersonation = 2;
    private const int TokenPrimary = 1;

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inherit, uint processId);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool DuplicateTokenEx(IntPtr existing, uint access, IntPtr attributes, int level, int type, out IntPtr duplicate);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcessWithTokenW(IntPtr token, uint logonFlags, string? application, string commandLine, uint creationFlags,
        IntPtr environment, string? directory, ref StartupInfo startup, out ProcessInformation process);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        public int cb;
        public string? reserved;
        public string? desktop;
        public string? title;
        public int x, y, xSize, ySize, xCountChars, yCountChars, fillAttribute, flags;
        public short showWindow, reserved2Size;
        public IntPtr reserved2, stdInput, stdOutput, stdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr process, thread;
        public int processId, threadId;
    }

    /// <summary>True when a window of the shell was found, which is what the token comes from.</summary>
    public static bool IsAvailable => GetShellWindow() != IntPtr.Zero;

    /// <summary>
    /// Starts the program, given by full path and with its command line, as the signed-in user's normal shell would.
    /// Returns the new process id, or 0 when it could not be done, and then nothing was started.
    /// </summary>
    public static int Start(string fullPath, string arguments)
    {
        var shell = GetShellWindow();
        if (shell == IntPtr.Zero) return 0;
        GetWindowThreadProcessId(shell, out var shellProcessId);
        if (shellProcessId == 0) return 0;

        IntPtr process = IntPtr.Zero, shellToken = IntPtr.Zero, token = IntPtr.Zero;
        try
        {
            process = OpenProcess(ProcessQueryLimitedInformation, false, shellProcessId);
            if (process == IntPtr.Zero || !OpenProcessToken(process, TokenDuplicate, out shellToken)) return 0;
            if (!DuplicateTokenEx(shellToken, TokenAllAccess, IntPtr.Zero, SecurityImpersonation, TokenPrimary, out token)) return 0;

            var startup = new StartupInfo { cb = Marshal.SizeOf<StartupInfo>() };
            var commandLine = $"\"{fullPath}\" {arguments}";
            if (!CreateProcessWithTokenW(token, 0, null, commandLine, 0, IntPtr.Zero, null, ref startup, out var started)) return 0;
            CloseHandle(started.thread);
            CloseHandle(started.process);
            return started.processId;
        }
        finally
        {
            if (token != IntPtr.Zero) CloseHandle(token);
            if (shellToken != IntPtr.Zero) CloseHandle(shellToken);
            if (process != IntPtr.Zero) CloseHandle(process);
        }
    }
}
