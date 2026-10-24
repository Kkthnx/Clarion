using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Clarion.Core.Nvidia;

namespace Clarion.Engine;

/// <summary>
/// Reads and writes the NVIDIA driver's global profile through nvapi64.dll, the library NVIDIA's driver puts in System32 and documents in
/// its public NVAPI repository (github.com/NVIDIA/nvapi, MIT licence). The function numbers are the ones in that repository's
/// nvapi_interface.h, and the structure layouts follow nvapi.h. No third party program is involved.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed unsafe class NvapiDriverStore : INvidiaDriverStore
{
    private const int NvapiOk = 0;
    private const int SettingNotFound = -160;
    private const int DwordType = 0;
    private const uint StructVersion1 = 1;

    private readonly object _gate = new();
    private bool _tried;
    private bool _available;

    private delegate* unmanaged[Cdecl]<IntPtr*, int> _createSession;
    private delegate* unmanaged[Cdecl]<IntPtr, int> _destroySession;
    private delegate* unmanaged[Cdecl]<IntPtr, int> _loadSettings;
    private delegate* unmanaged[Cdecl]<IntPtr, int> _saveSettings;
    private delegate* unmanaged[Cdecl]<IntPtr, IntPtr*, int> _currentGlobalProfile;
    private delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, Setting*, int> _getSetting;
    private delegate* unmanaged[Cdecl]<IntPtr, IntPtr, Setting*, int> _setSetting;
    private delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, int> _restoreDefault;
    private delegate* unmanaged[Cdecl]<int, byte*, int> _errorMessage;

    public bool IsAvailable()
    {
        lock (_gate) return Load();
    }

    public uint? Read(uint id)
    {
        lock (_gate)
        {
            RequireLoaded();
            return InSession(save: false, (session, profile) =>
            {
                var setting = NewSetting();
                var status = _getSetting(session, profile, id, &setting);
                if (status == SettingNotFound) return (uint?)null;
                Check(status, "read the setting");
                return setting.CurrentValue;
            });
        }
    }

    public IReadOnlyDictionary<uint, uint?> ReadMany(IEnumerable<uint> ids)
    {
        var wanted = ids.Distinct().ToList();
        lock (_gate)
        {
            RequireLoaded();
            return InSession(save: false, (session, profile) =>
            {
                var found = new Dictionary<uint, uint?>();
                foreach (var id in wanted)
                {
                    var setting = NewSetting();
                    var status = _getSetting(session, profile, id, &setting);
                    if (status == SettingNotFound) { found[id] = null; continue; }
                    Check(status, "read the setting");
                    found[id] = setting.CurrentValue;
                }
                return found;
            });
        }
    }

    public void Write(uint id, uint value)
    {
        lock (_gate)
        {
            RequireLoaded();
            InSession(save: true, (session, profile) =>
            {
                var setting = NewSetting();
                setting.Id = id;
                setting.Type = DwordType;
                setting.CurrentValue = value;
                Check(_setSetting(session, profile, &setting), "write the setting");
                return 0;
            });
        }
    }

    public void Reset(uint id)
    {
        lock (_gate)
        {
            RequireLoaded();
            InSession(save: true, (session, profile) =>
            {
                var status = _restoreDefault(session, profile, id);
                // A value that is not stored is already at the default, which is what was asked for.
                if (status != SettingNotFound) Check(status, "put the setting back to its default");
                return 0;
            });
        }
    }

    private T InSession<T>(bool save, Func<IntPtr, IntPtr, T> work)
    {
        IntPtr session;
        Check(_createSession(&session), "open a session with the NVIDIA driver settings");
        try
        {
            Check(_loadSettings(session), "load the NVIDIA driver settings");
            IntPtr profile;
            Check(_currentGlobalProfile(session, &profile), "find the global NVIDIA profile");
            var result = work(session, profile);
            if (save) Check(_saveSettings(session), "save the NVIDIA driver settings");
            return result;
        }
        finally
        {
            _destroySession(session);
        }
    }

    private void RequireLoaded()
    {
        if (!Load()) throw new InvalidOperationException("There is no NVIDIA graphics driver on this PC.");
    }

    private bool Load()
    {
        if (_tried) return _available;
        _tried = true;
        try
        {
            // The library is taken from System32 only, where the driver installs it, and never from beside the program.
            var path = Path.Combine(Environment.SystemDirectory, "nvapi64.dll");
            if (!File.Exists(path)) return false;
            // The setting structure must match NVIDIA's layout byte for byte, or the driver would read and write the wrong places.
            if (sizeof(Setting) != 12320) return false;
            var library = NativeLibrary.Load(path);
            var query = (delegate* unmanaged[Cdecl]<uint, IntPtr>)NativeLibrary.GetExport(library, "nvapi_QueryInterface");

            var initialize = (delegate* unmanaged[Cdecl]<int>)query(0x0150e828);
            if (initialize == null || initialize() != NvapiOk) return false;

            _createSession = (delegate* unmanaged[Cdecl]<IntPtr*, int>)query(0x0694d52e);
            _destroySession = (delegate* unmanaged[Cdecl]<IntPtr, int>)query(0xdad9cff8);
            _loadSettings = (delegate* unmanaged[Cdecl]<IntPtr, int>)query(0x375dbd6b);
            _saveSettings = (delegate* unmanaged[Cdecl]<IntPtr, int>)query(0xfcbc7e14);
            _currentGlobalProfile = (delegate* unmanaged[Cdecl]<IntPtr, IntPtr*, int>)query(0x617bff9f);
            _getSetting = (delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, Setting*, int>)query(0x73bf8338);
            _setSetting = (delegate* unmanaged[Cdecl]<IntPtr, IntPtr, Setting*, int>)query(0x577dd202);
            _restoreDefault = (delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, int>)query(0x53f0381e);
            _errorMessage = (delegate* unmanaged[Cdecl]<int, byte*, int>)query(0x6c2d048c);

            _available = _createSession != null && _destroySession != null && _loadSettings != null && _saveSettings != null
                         && _currentGlobalProfile != null && _getSetting != null && _setSetting != null && _restoreDefault != null;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or FileNotFoundException)
        {
            _available = false;
        }
        return _available;
    }

    private void Check(int status, string what)
    {
        if (status == NvapiOk) return;
        var text = "";
        if (_errorMessage != null)
        {
            var buffer = stackalloc byte[64];
            if (_errorMessage(status, buffer) == NvapiOk) text = Marshal.PtrToStringAnsi((IntPtr)buffer) ?? "";
        }
        throw new InvalidOperationException($"The NVIDIA driver would not {what}{(text.Length > 0 ? $": {text}" : "")} (code {status}).");
    }

    private static Setting NewSetting() => new() { Version = (uint)(sizeof(Setting) | (int)(StructVersion1 << 16)) };

    // NVDRS_SETTING_V1 from nvapi.h, packed to 4 bytes. The two unions are as large as the binary form, 4 bytes of length and 4096 of data.
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct Setting
    {
        public uint Version;
        public fixed ushort Name[2048];
        public uint Id;
        public int Type;
        public int Location;
        public uint IsCurrentPredefined;
        public uint IsPredefinedValid;
        public fixed byte PredefinedValue[4100];
        public fixed byte Current[4100];

        public uint CurrentValue
        {
            get { fixed (byte* p = Current) return *(uint*)p; }
            set { fixed (byte* p = Current) *(uint*)p = value; }
        }
    }
}
