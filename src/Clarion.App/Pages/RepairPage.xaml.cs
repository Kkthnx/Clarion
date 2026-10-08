using System.Text;
using Clarion.App.Controls;
using Clarion.App.Services;
using Clarion.Core.Actions;
using Clarion.Core.Catalog;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Clarion.App.Pages;

public sealed record RepairGroup(string Name, IReadOnlyList<ActionItem> Items);

public sealed partial class RepairPage : Page
{
    private readonly AppServices _app = AppServices.Instance;
    private bool _running;

    public RepairPage()
    {
        InitializeComponent();
        var items = CatalogLoader.LoadActions().Select(a => new ActionItem(a)).ToList();
        var order = new[] { "Repair", "Network", "Updates" };
        Groups.ItemsSource = items.GroupBy(i => i.Category)
            .OrderBy(g => Array.IndexOf(order, g.Key) is var ix && ix < 0 ? 99 : ix)
            .Select(g => new RepairGroup(g.Key, g.ToList())).ToList();
    }

    private async void OnRun(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ActionItem item) return;
        if (_running || _app.IsBusy) return;

        if (_app.PendingCount > 0)
        {
            await Info("Apply or clear your queued changes first", "Repair jobs and queued settings should not run together.");
            return;
        }

        var body = new StackPanel { Spacing = 8, MaxWidth = 520 };
        body.Children.Add(new TextBlock { Text = item.Def.What, TextWrapping = TextWrapping.Wrap });
        body.Children.Add(new TextBlock { Text = $"{item.DurationText}. You can stop it at any time.", TextWrapping = TextWrapping.Wrap, Opacity = 0.8 });
        if (item.Def.RestorePoint) body.Children.Add(new TextBlock { Text = "A restore point is made first.", TextWrapping = TextWrapping.Wrap, Opacity = 0.8 });
        if (item.Def.NeedsReboot) body.Children.Add(new TextBlock { Text = "A restart finishes the job.", TextWrapping = TextWrapping.Wrap, Opacity = 0.8 });
        if (item.Def.RiskLevel >= Clarion.Core.Model.RiskLevel.Medium)
            body.Children.Add(new TextBlock { Text = item.Def.Risk, TextWrapping = TextWrapping.Wrap, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });

        var confirm = new ContentDialog
        {
            Title = $"Run: {item.Name}?",
            Content = body,
            PrimaryButtonText = "Run",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };
        if (await confirm.ShowAsync() != ContentDialogResult.Primary) return;

        await RunWithLogAsync(item);
    }

    private async Task RunWithLogAsync(ActionItem item)
    {
        _running = true;
        var cts = new CancellationTokenSource();
        var builder = new StringBuilder();
        var dirty = false;

        var logBox = new TextBlock
        {
            IsTextSelectionEnabled = true, TextWrapping = TextWrapping.NoWrap,
            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Cascadia Mono"), FontSize = 12,
        };
        var scroller = new ScrollViewer
        {
            Content = logBox, Width = 640, Height = 300, Padding = new Thickness(8),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        var ring = new ProgressRing { IsActive = true, Width = 20, Height = 20 };
        var status = new TextBlock { Text = "Starting", VerticalAlignment = VerticalAlignment.Center };
        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        header.Children.Add(ring);
        header.Children.Add(status);
        var content = new StackPanel { Spacing = 12 };
        content.Children.Add(header);
        content.Children.Add(scroller);

        var dialog = new ContentDialog { Title = item.Name, Content = content, PrimaryButtonText = "Stop", XamlRoot = XamlRoot };
        dialog.PrimaryButtonClick += (_, a) =>
        {
            a.Cancel = true;
            status.Text = "Stopping";
            cts.Cancel();
        };
        var shown = dialog.ShowAsync();

        void Append(string line)
        {
            lock (builder) { builder.AppendLine(line); dirty = true; }
            Log.Write($"repair {item.Def.Id}: {line}");
        }

        using var timerStop = new CancellationTokenSource();
        var refresh = Task.Run(async () =>
        {
            while (!timerStop.IsCancellationRequested)
            {
                await Task.Delay(150);
                string? text = null;
                lock (builder) { if (dirty) { text = builder.ToString(); dirty = false; } }
                if (text is null) continue;
                DispatcherQueue.TryEnqueue(() =>
                {
                    logBox.Text = text;
                    scroller.UpdateLayout();
                    scroller.ChangeView(null, scroller.ScrollableHeight, null, true);
                    status.Text = "Running";
                });
            }
        });

        Clarion.Core.Actions.ActionResult result;
        try
        {
            if (item.Def.RestorePoint)
            {
                status.Text = "Creating a restore point";
                var rp = await Task.Run(() => _app.Runtime.RestorePoints.Create($"Clarion {item.Name}"));
                Append(rp.Success ? "Restore point created." : $"Restore point failed: {rp.Error}. Continuing.");
            }
            status.Text = "Running";
            result = await _app.Runtime.Actions.RunAsync(item.Def, Append, cts.Token);
        }
        finally
        {
            timerStop.Cancel();
            await refresh;
        }

        string finalText;
        lock (builder) finalText = builder.ToString();
        logBox.Text = finalText;
        scroller.UpdateLayout();
        scroller.ChangeView(null, scroller.ScrollableHeight, null, true);
        ring.IsActive = false;
        status.Text = result.Success
            ? $"Finished in {result.Duration.TotalSeconds:0} seconds.{(result.BytesFreed > 0 ? $" Freed {FolderCleaner.Format(result.BytesFreed)}." : "")}{(item.Def.NeedsReboot ? " Restart to finish." : "")}"
            : result.Cancelled ? "Stopped." : $"Did not finish. {result.Error}";
        dialog.PrimaryButtonText = "";
        dialog.CloseButtonText = "Close";
        await shown;
        _running = false;
    }

    private async Task Info(string title, string message) =>
        await new ContentDialog { Title = title, Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot }.ShowAsync();

    private void OnCardTapped(object sender, TappedRoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { Tag: ActionItem }) return;
        if ((sender as FrameworkElement)?.Tag is not ActionItem item) return;
        Detail.Item = item;
        Detail.Visibility = Visibility.Visible;
        EmptyDetail.Visibility = Visibility.Collapsed;
    }

    private void OnTipOpened(object sender, RoutedEventArgs e)
    {
        if (sender is not ToolTip tip || tip.Content is TweakDetail) return;
        if (tip.Tag is not ActionItem item) return;
        tip.Content = new TweakDetail { Item = item, ShowTechnical = false, Width = 400 };
    }
}
