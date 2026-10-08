using System.Runtime.InteropServices;

namespace Clarion.App.Services;

/// <summary>
/// Classic Windows file dialogs. The newer pickers do not work in a program that runs as administrator,
/// and Clarion always does.
/// </summary>
public static class FileDialogs
{
    private const int MaxPath = 1024;
    private const int OfnPathMustExist = 0x800, OfnFileMustExist = 0x1000, OfnOverwritePrompt = 0x2, OfnNoChangeDir = 0x8, OfnExplorer = 0x80000;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OpenFileName
    {
        public int StructSize;
        public IntPtr Owner;
        public IntPtr Instance;
        public string Filter;
        public string? CustomFilter;
        public int MaxCustomFilter;
        public int FilterIndex;
        public IntPtr File;
        public int MaxFile;
        public string? FileTitle;
        public int MaxFileTitle;
        public string? InitialDir;
        public string? Title;
        public int Flags;
        public short FileOffset;
        public short FileExtension;
        public string? DefExt;
        public IntPtr CustData;
        public IntPtr Hook;
        public string? TemplateName;
        public IntPtr Reserved;
        public int ReservedInt;
        public int FlagsEx;
    }

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetSaveFileName(ref OpenFileName ofn);

    public static string? Open(IntPtr owner, string title, string filter) => Show(owner, title, filter, null, save: false);

    public static string? Save(IntPtr owner, string title, string filter, string suggestedName) => Show(owner, title, filter, suggestedName, save: true);

    private static string? Show(IntPtr owner, string title, string filter, string? suggestedName, bool save)
    {
        var buffer = Marshal.AllocHGlobal(MaxPath * 2);
        try
        {
            var initial = suggestedName ?? "";
            var chars = (initial + "\0").ToCharArray();
            Marshal.Copy(chars, 0, buffer, Math.Min(chars.Length, MaxPath - 1));
            if (chars.Length >= MaxPath) Marshal.WriteInt16(buffer, (MaxPath - 1) * 2, 0);

            var ofn = new OpenFileName
            {
                StructSize = Marshal.SizeOf<OpenFileName>(), Owner = owner,
                Filter = filter.Replace('|', '\0') + "\0\0", FilterIndex = 1, File = buffer, MaxFile = MaxPath,
                Title = title, DefExt = "json",
                Flags = OfnExplorer | OfnNoChangeDir | OfnPathMustExist | (save ? OfnOverwritePrompt : OfnFileMustExist),
            };
            var ok = save ? GetSaveFileName(ref ofn) : GetOpenFileName(ref ofn);
            return ok ? Marshal.PtrToStringUni(buffer) : null;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
