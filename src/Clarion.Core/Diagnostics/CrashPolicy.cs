namespace Clarion.Core.Diagnostics;

public enum CrashAction
{
    /// <summary>An error while a change is running. The run ends with its own read of Windows, so nothing more is done here.</summary>
    Continue,

    /// <summary>An unexpected error while idle. The queue is emptied and every setting is read again, so nothing stale is acted on.</summary>
    SafeState,

    /// <summary>Errors keep coming. The state cannot be trusted, so the program ends.</summary>
    Terminate,
}

/// <summary>
/// Decides what to do with an error nothing caught. A handler that only logs and carries on hides a state that may no longer be
/// right, and one that ends the program on the first odd error would lose queued work over nothing. So the first few are met with a
/// reset to a known state, and a run of them ends the program.
/// </summary>
public sealed class CrashPolicy(TimeProvider? clock = null, int limit = 5, TimeSpan? window = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;
    private readonly TimeSpan _window = window ?? TimeSpan.FromMinutes(1);
    private readonly Queue<DateTimeOffset> _recent = new();
    private readonly object _gate = new();

    public CrashAction Next(bool changingThisPc)
    {
        lock (_gate)
        {
            var now = _clock.GetUtcNow();
            while (_recent.Count > 0 && now - _recent.Peek() > _window) _recent.Dequeue();
            _recent.Enqueue(now);
            if (_recent.Count > limit) return CrashAction.Terminate;
            return changingThisPc ? CrashAction.Continue : CrashAction.SafeState;
        }
    }
}
