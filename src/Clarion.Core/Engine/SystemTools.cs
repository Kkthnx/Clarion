namespace Clarion.Core.Engine;

/// <summary>
/// Finds a Windows tool by its full path. A program started by name alone is looked for first in the folder the running program came
/// from, then in the current folder, and only then in the Windows folders, and Clarion runs with administrator rights. A file with the
/// same name placed next to a portable copy would otherwise run with those rights. Microsoft's CreateProcess page describes the order.
/// </summary>
public static class SystemTools
{
    public static string Resolve(string fileName, string systemDirectory, Func<string, bool>? exists = null)
    {
        // A name that already carries a folder was chosen on purpose, so it is left alone.
        if (fileName.Contains('\\') || fileName.Contains('/') || Path.IsPathRooted(fileName)) return fileName;
        exists ??= File.Exists;

        // Windows PowerShell 5.1 is the only one Clarion's scripts are written for. It lives in its own folder, and PowerShell 7 is not it.
        if (fileName.Equals("powershell.exe", StringComparison.OrdinalIgnoreCase))
        {
            var powershell = Path.Combine(systemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
            if (exists(powershell)) return powershell;
        }

        var path = Path.Combine(systemDirectory, fileName);
        return exists(path) ? path : fileName;
    }
}
