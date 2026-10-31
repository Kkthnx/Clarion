using System.Text;

namespace Clarion.App.Services;

/// <summary>
/// A way to see whether pages are given back after the person leaves them. Off unless CLARION_LEAK_PROBE=1 is set, and then it keeps a weak
/// reference to each page shown, and after a full collection says how many of each kind are still alive. A page that is left behind for
/// good is a page that something long lived still holds on to, such as an event on the window or on <see cref="AppServices"/>.
/// </summary>
internal static class LeakProbe
{
    public static bool Enabled { get; } = Environment.GetEnvironmentVariable("CLARION_LEAK_PROBE") == "1";

    private static readonly List<(string Type, WeakReference Page)> Seen = [];

    public static void Track(object? page)
    {
        if (!Enabled || page is null) return;
        lock (Seen) Seen.Add((page.GetType().Name, new WeakReference(page)));
    }

    private static readonly List<(string Label, WeakReference Owner)> Samples = [];

    /// <summary>
    /// Watches how many handlers are on an object's PropertyChanged. A row that outlives its page keeps one handler for every page that
    /// bound to it and was not told to stop, and each handler keeps the elements of that page alive.
    /// </summary>
    public static void Watch(string label, object? owner)
    {
        if (!Enabled || owner is null) return;
        lock (Samples) Samples.Add((label, new WeakReference(owner)));
    }

    private static int Handlers(object owner)
    {
        for (var type = owner.GetType(); type is not null; type = type.BaseType)
            if (type.GetField("PropertyChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly)?.GetValue(owner) is Delegate d)
                return d.GetInvocationList().Length;
        return 0;
    }

    /// <summary>Collects everything that can be collected, then lists the pages that survived, with the managed memory that is left.</summary>
    public static string Report()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
        var sb = new StringBuilder();
        lock (Seen)
        {
            var alive = Seen.Where(s => s.Page.IsAlive).GroupBy(s => s.Type).OrderByDescending(g => g.Count()).ToList();
            sb.Append($"Leak probe: {Seen.Count} pages shown, {alive.Sum(g => g.Count())} still alive, managed heap {GC.GetTotalMemory(false) / 1024 / 1024} MB.");
            foreach (var g in alive) sb.Append($" {g.Key}={g.Count()}");
        }
        lock (Samples)
            foreach (var (label, owner) in Samples)
                if (owner.Target is { } target) sb.Append($" [{label} handlers={Handlers(target)}]");
        return sb.ToString();
    }
}
