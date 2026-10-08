using System.Collections.ObjectModel;
using Clarion.Core.Engine;
using Microsoft.UI.Xaml;

namespace Clarion.App.Services;

public enum LineState { Running, Done, Skipped, Failed }

/// <summary>One row in the activity panel.</summary>
public sealed class ActivityLine : UiObservableObject
{
    private LineState _state = LineState.Running;
    private string _detail = "";

    public ActivityLine(string text, string? tweakId = null)
    {
        Text = text;
        TweakId = tweakId;
    }

    public string Text { get; }
    public string? TweakId { get; }

    public LineState State
    {
        get => _state;
        set
        {
            if (!SetProperty(ref _state, value)) return;
            OnPropertyChanged(nameof(RunningVisibility));
            OnPropertyChanged(nameof(Glyph));
            OnPropertyChanged(nameof(GlyphBrush));
            OnPropertyChanged(nameof(GlyphVisibility));
        }
    }

    public string Detail
    {
        get => _detail;
        set
        {
            if (!SetProperty(ref _detail, value)) return;
            OnPropertyChanged(nameof(DetailVisibility));
        }
    }

    public Visibility RunningVisibility => State == LineState.Running ? Visibility.Visible : Visibility.Collapsed;
    public Visibility GlyphVisibility => State == LineState.Running ? Visibility.Collapsed : Visibility.Visible;
    public Visibility DetailVisibility => Detail.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

    public string Glyph => State switch
    {
        LineState.Done => "",
        LineState.Skipped => "",
        LineState.Failed => "",
        _ => "",
    };

    public Microsoft.UI.Xaml.Media.Brush GlyphBrush => (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources[State switch
    {
        LineState.Done => "ClOkBrush",
        LineState.Failed => "ClDangerBrush",
        _ => "ClTextMutedBrush",
    }];
}

/// <summary>
/// Live view of a run: a title, a progress count and one line per setting. Everything here is
/// changed on the UI thread, callers marshal before calling in.
/// </summary>
public sealed class ActivityLog : UiObservableObject
{
    private bool _isOpen;
    private bool _isRunning;
    private string _title = "";
    private string _progressText = "";
    private int _total;
    private int _value;

    public ObservableCollection<ActivityLine> Lines { get; } = [];

    public bool IsOpen { get => _isOpen; private set => SetProperty(ref _isOpen, value); }
    public bool IsRunning { get => _isRunning; private set => SetProperty(ref _isRunning, value); }
    public string Title { get => _title; private set => SetProperty(ref _title, value); }
    public string ProgressText { get => _progressText; private set => SetProperty(ref _progressText, value); }
    public int Total { get => _total; private set => SetProperty(ref _total, value); }
    public int Value { get => _value; private set => SetProperty(ref _value, value); }
    public bool Indeterminate => IsRunning && Total == 0;

    public void Start(string title)
    {
        Lines.Clear();
        Title = title;
        Total = 0;
        Value = 0;
        ProgressText = "Getting ready";
        IsRunning = true;
        IsOpen = true;
        OnPropertyChanged(nameof(Indeterminate));
    }

    public void Close() => IsOpen = false;

    /// <summary>A line about something other than one setting, such as making the restore point.</summary>
    public void Info(string text)
    {
        if (text.StartsWith("Applying ", StringComparison.Ordinal) || text.StartsWith("Reverting ", StringComparison.Ordinal)) return;
        Lines.Add(new ActivityLine(text));
    }

    public void Step(BatchStep step)
    {
        Total = step.Total;
        OnPropertyChanged(nameof(Indeterminate));
        if (step.State == BatchStepState.Running)
        {
            foreach (var l in Lines.Where(l => l.TweakId is null && l.State == LineState.Running)) l.State = LineState.Done;
            Lines.Add(new ActivityLine($"{(step.Reverting ? "Reverting" : "Applying")} {step.Name}", step.TweakId));
            ProgressText = $"{step.Index} of {step.Total}";
            return;
        }

        var line = Lines.LastOrDefault(l => l.TweakId == step.TweakId && l.State == LineState.Running);
        if (line is not null)
        {
            line.State = step.State switch
            {
                BatchStepState.Failed => LineState.Failed,
                BatchStepState.Skipped => LineState.Skipped,
                _ => LineState.Done,
            };
            line.Detail = step.State switch
            {
                BatchStepState.Failed => step.Error ?? "Did not finish",
                BatchStepState.Skipped => "Already in place",
                _ => "",
            };
        }
        Value = step.Index;
    }

    public void Finish(int done, int failed)
    {
        foreach (var l in Lines.Where(l => l.State == LineState.Running)) l.State = LineState.Done;
        IsRunning = false;
        Value = Total;
        OnPropertyChanged(nameof(Indeterminate));
        Title = failed == 0 ? "All done" : "Finished with problems";
        ProgressText = failed == 0 ? $"{done} change{(done == 1 ? "" : "s")} made" : $"{done} made, {failed} did not finish";
    }
}
