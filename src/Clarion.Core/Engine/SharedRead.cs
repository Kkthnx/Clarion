namespace Clarion.Core.Engine;

/// <summary>
/// Lets callers that ask for the same slow read at the same time share one run of it. At startup the Home page and the image notes both
/// want the system report, and the probe takes several seconds, so two runs doubled the work. A read that failed is not kept, so the next
/// caller tries again. Asking for a refresh starts a new read even when an older one is still running.
/// </summary>
public sealed class SharedRead<T> where T : class
{
    private readonly Func<T> _read;
    private readonly object _gate = new();
    private Task<T>? _current;
    private int _generation;
    private T? _latest;

    public SharedRead(Func<T> read) => _read = read;

    /// <summary>The result of the newest read that finished, or null when none has yet.</summary>
    public T? Latest { get { lock (_gate) return _latest; } }

    public Task<T> GetAsync(bool refresh = false)
    {
        lock (_gate)
        {
            if (!refresh && _current is { IsFaulted: false, IsCanceled: false }) return _current;
            _current = Load(++_generation);
            return _current;
        }
    }

    private async Task<T> Load(int generation)
    {
        var result = await Task.Run(_read).ConfigureAwait(false);
        // A refresh started after this read may already have finished, so only the newest read is kept.
        lock (_gate)
        {
            if (generation == _generation) _latest = result;
        }
        return result;
    }
}
