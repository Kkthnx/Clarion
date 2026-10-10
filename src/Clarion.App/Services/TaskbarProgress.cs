using System.Runtime.InteropServices;

namespace Clarion.App.Services;

/// <summary>What the taskbar button shows: nothing, a moving bar, a bar that is waiting, a stopped (yellow) bar, or a failed (red) bar.</summary>
public enum TaskbarState { None = 0, Indeterminate = 1, Normal = 2, Error = 4, Paused = 8 }

/// <summary>
/// Shows progress on the app's taskbar button, so a long run can be followed from another window. Uses the Windows taskbar
/// list interface (ITaskbarList3). Nothing here may stop a run, so every failure is swallowed and the bar is simply not shown.
/// </summary>
public sealed class TaskbarProgress
{
    private static readonly Guid ClsidTaskbarList = new("56FDF344-FD6D-11d0-958A-006097C9A090");

    // The methods are declared in the order Windows lays them out, and only as far as the ones used. Reordering breaks the calls.
    [ComImport, Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList3
    {
        // ITaskbarList
        [PreserveSig] int HrInit();
        [PreserveSig] int AddTab(IntPtr hwnd);
        [PreserveSig] int DeleteTab(IntPtr hwnd);
        [PreserveSig] int ActivateTab(IntPtr hwnd);
        [PreserveSig] int SetActiveAlt(IntPtr hwnd);
        // ITaskbarList2
        [PreserveSig] int MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fullscreen);
        // ITaskbarList3
        [PreserveSig] int SetProgressValue(IntPtr hwnd, ulong completed, ulong total);
        [PreserveSig] int SetProgressState(IntPtr hwnd, int flags);
    }

    private readonly IntPtr _hwnd;
    private ITaskbarList3? _list;
    private bool _failed;

    public TaskbarProgress(IntPtr hwnd) => _hwnd = hwnd;

    /// <summary>Sets what the button shows. For Normal and Error, completed and total place the bar.</summary>
    public void Set(TaskbarState state, int completed = 0, int total = 0)
    {
        if (_failed || _hwnd == IntPtr.Zero) return;
        try
        {
            _list ??= Create();
            if (_list is null) return;
            if (state is TaskbarState.Normal or TaskbarState.Error or TaskbarState.Paused && total > 0)
                _list.SetProgressValue(_hwnd, (ulong)Math.Clamp(completed, 0, total), (ulong)total);
            _list.SetProgressState(_hwnd, (int)state);
        }
        catch (Exception ex) when (ex is COMException or InvalidCastException or NotSupportedException or InvalidOperationException)
        {
            _failed = true;
            Log.Write($"Taskbar progress is not available: {ex.Message}");
        }
    }

    private ITaskbarList3? Create()
    {
        var type = Type.GetTypeFromCLSID(ClsidTaskbarList);
        if (type is null || Activator.CreateInstance(type) is not ITaskbarList3 list)
        {
            _failed = true;
            return null;
        }
        if (list.HrInit() != 0)
        {
            _failed = true;
            return null;
        }
        return list;
    }
}
