namespace Clarion.Core.Settings;

/// <summary>
/// Whether this person wants Clarion to start without asking for administrator rights. It is a one file flag, and it lives in the
/// person's own folder, not in the shared data folder, because that folder can only be written by administrators and the person who
/// most wants this is the one who is not one. Nothing here leaves the PC.
/// </summary>
public sealed class LaunchPreference(string path)
{
    public bool StartLimited
    {
        get
        {
            try { return File.Exists(path); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return false; }
        }
    }

    /// <summary>Remembers the choice. False when the file could not be written or removed, so the caller can say so.</summary>
    public bool SetStartLimited(bool on)
    {
        try
        {
            if (on)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, "Clarion starts without asking for administrator rights while this file exists.\n");
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
            return StartLimited == on;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
