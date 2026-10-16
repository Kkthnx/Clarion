using System.Text;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Services;

/// <summary>
/// A dialog that shows what a long job prints while it runs, with a spinner, a one line status and a Stop button, and then the result with a Close
/// button. A job can print hundreds of lines a second (a system file check does), so lines are collected from any thread and shown a few times a
/// second, not once each. Repair jobs and app updates use it, so they look and behave the same.
/// </summary>
public sealed class LogDialog : IDisposable
{
    private readonly StringBuilder _text = new();
    private readonly object _gate = new();
    private readonly CancellationTokenSource _stop = new();
    private readonly TextBlock _log;
    private readonly TextBlock _status;
    private readonly ScrollViewer _scroller;
    private readonly ProgressRing _ring;
    private readonly ContentDialog _dialog;
    private readonly DispatcherQueueTimer _timer;
    private bool _dirty;
    private bool _finished;
    private string? _pendingStatus;
    private Task? _shown;

    public LogDialog(XamlRoot root, string title, string firstStatus = "Starting")
    {
        _log = new TextBlock
        {
            IsTextSelectionEnabled = true,
            TextWrapping = TextWrapping.NoWrap,
            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Cascadia Mono"),
            FontSize = 12,
        };
        _scroller = new ScrollViewer
        {
            Content = _log,
            Width = 640,
            Height = 300,
            Padding = new Thickness(8),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        _ring = new ProgressRing { IsActive = true, Width = 20, Height = 20 };
        _status = new TextBlock { Text = firstStatus, VerticalAlignment = VerticalAlignment.Center };
        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Children = { _ring, _status } };
        var content = new StackPanel { Spacing = 12, Children = { header, _scroller } };

        _dialog = new ContentDialog { Title = title, Content = content, PrimaryButtonText = "Stop", XamlRoot = root };
        _dialog.PrimaryButtonClick += (_, a) =>
        {
            // The dialog stays open until the job has really stopped and reported.
            a.Cancel = true;
            _status.Text = "Stopping";
            _stop.Cancel();
        };

        // Escape closes a dialog even when it has no Close button. While the job runs that would leave it going with no window and no Stop.
        _dialog.Closing += (_, a) => { if (!_finished) a.Cancel = true; };

        _timer = _dialog.DispatcherQueue.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(150);
        _timer.Tick += (_, _) => Flush();
    }

    /// <summary>Cancelled when the person presses Stop.</summary>
    public CancellationToken Stop => _stop.Token;

    /// <summary>Opens the dialog. Call <see cref="FinishAsync"/> when the job is over.</summary>
    public void Start()
    {
        _shown = _dialog.ShowAsync().AsTask();
        _timer.Start();
    }

    /// <summary>Adds a line, from any thread. A status, when given, replaces the one line status above the log.</summary>
    public void Append(string line, string? status = null)
    {
        lock (_gate)
        {
            _text.AppendLine(line);
            _dirty = true;
            if (status is not null) _pendingStatus = status;
        }
    }

    /// <summary>Changes the status line. Call it from the window's thread.</summary>
    public void SetStatus(string status) => _status.Text = status;

    private void Flush()
    {
        string? text = null, status = null;
        lock (_gate)
        {
            if (_dirty) { text = _text.ToString(); _dirty = false; }
            status = _pendingStatus;
            _pendingStatus = null;
        }
        if (status is not null) _status.Text = status;
        if (text is null) return;
        _log.Text = text;
        _scroller.UpdateLayout();
        _scroller.ChangeView(null, _scroller.ScrollableHeight, null, true);
    }

    /// <summary>Shows the result, turns Stop into Close, and returns when the person has closed the dialog.</summary>
    public async Task FinishAsync(string status, params string[] extraLines)
    {
        _timer.Stop();
        foreach (var line in extraLines) Append(line);
        Flush();
        _ring.IsActive = false;
        _status.Text = status;
        _dialog.PrimaryButtonText = "";
        _dialog.CloseButtonText = "Close";
        _finished = true;
        if (_shown is not null) await _shown;
    }

    public void Dispose() => _stop.Dispose();
}
