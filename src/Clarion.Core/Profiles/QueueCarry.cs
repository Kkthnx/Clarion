namespace Clarion.Core.Profiles;

/// <summary>
/// Keeps the queue a person built across the restart that asks for administrator rights. The new window starts empty, so someone who
/// picked a starting point, pressed Review and was told to restart would otherwise have to choose all over again. The file is in the
/// person's own folder, like the flag for starting without the prompt, and holds only setting names in the setup file format, which is
/// checked when it is read. It is only ever read back into the queue, which the person still reviews and applies themselves.
/// </summary>
public sealed class QueueCarry(string path, TimeProvider? clock = null)
{
    /// <summary>A carried queue older than this is ignored, so one left behind by a restart that never happened cannot turn up much later.</summary>
    public static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(10);

    private readonly TimeProvider _clock = clock ?? TimeProvider.System;

    public static string DefaultPath => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Clarion", "carried-queue.json");

    /// <summary>Writes the names down. With none, any older file is removed. False when nothing was written.</summary>
    public bool Save(IEnumerable<string> tweakIds, string version = "carried")
    {
        var ids = tweakIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        Discard();
        if (ids.Count == 0) return false;
        try
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            File.WriteAllText(path, ProfileFile.Serialize(ids, version));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>The names that were carried, read once. The file is removed whatever it held, and a bad or old file gives nothing.</summary>
    public IReadOnlyList<string> Take()
    {
        try
        {
            if (!File.Exists(path)) return [];
            var age = _clock.GetUtcNow() - new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero);
            var load = ProfileFile.Load(path);
            Discard();
            return age <= MaxAge && age >= TimeSpan.FromMinutes(-1) && load.Profile is { } profile ? profile.Tweaks : [];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    public void Discard()
    {
        try { File.Delete(path); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
}
