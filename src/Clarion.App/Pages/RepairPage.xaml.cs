using Clarion.Core.Engine;
using Clarion.Core.Model;
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

        foreach (var tool in WindowsTools.All)
        {
            var button = new Button { Content = tool.Name, Tag = tool };
            ToolTipService.SetToolTip(button, tool.Summary);
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(button, tool.Name);
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetHelpText(button, tool.Summary);
            button.Click += OnOpenTool;
            ToolButtons.Children.Add(button);
        }
    }

    private async void OnOpenTool(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not WindowsTool tool) return;
        if (!Shell.OpenTool(tool))
            await Dialogs.Say(XamlRoot, $"{tool.Name} did not open", "Windows could not start it. The log has the reason.");
    }

    private async void OnRun(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ActionItem item) return;
        if (_running || _app.IsBusy) return;

        if (_app.PendingCount > 0)
        {
            await Dialogs.Say(XamlRoot, "Apply or clear your queued changes first", "Repair jobs and queued settings should not run together.");
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

        if (item.Def.RestorePoint && !await MakeRestorePointAsync(item)) return;
        await RunWithLogAsync(item);
    }

    /// <summary>Makes the restore point before the job. If it fails the person decides: turn on System Protection and retry, go on without one, or stop.</summary>
    private async Task<bool> MakeRestorePointAsync(ActionItem item)
    {
        var turnOn = false;
        while (true)
        {
            _app.Status = "Creating a restore point";
            var rp = await Task.Run(() => _app.Runtime.RestorePoints.CreateEnsuring($".Clarion {item.Name}", turnOn, Log.Write));
            if (rp.Success) return true;

            var dialog = new ContentDialog
            {
                Title = "No restore point",
                Content = rp.Error + (turnOn
                    ? "\n\nSystem Protection could not be turned on, or the restore point still failed. A policy on this PC can block it."
                    : "\n\nSystem Protection is often off on a new Windows install. .Clarion can turn it on for your system drive and try again. Restore points keep some disk space."),
                PrimaryButtonText = turnOn ? "" : "Turn on System Protection and retry",
                SecondaryButtonText = "Run without one",
                CloseButtonText = "Cancel",
                DefaultButton = turnOn ? ContentDialogButton.Close : ContentDialogButton.Primary,
                XamlRoot = XamlRoot,
            };
            var choice = await dialog.ShowAsync();
            if (choice == ContentDialogResult.Primary) { turnOn = true; continue; }
            return choice == ContentDialogResult.Secondary;
        }
    }

    private async Task RunWithLogAsync(ActionItem item)
    {
        _running = true;
        _app.RepairRunning = true;
        using var log = new LogDialog(XamlRoot, item.Name);
        log.Start();

        Clarion.Core.Actions.ActionResult result;
        try
        {
            log.SetStatus("Running");
            result = await _app.Runtime.Actions.RunAsync(item.Def, line => { log.Append(line); Log.Write($"repair {item.Def.Id}: {line}"); }, log.Stop);
        }
        catch (Exception ex)
        {
            // Anything the runner did not expect still ends the dialog cleanly. Otherwise it stays open with no way to close it.
            Log.Write($"repair {item.Def.Id}: {ex}");
            result = new Clarion.Core.Actions.ActionResult(false, ex.Message, 0, TimeSpan.Zero);
        }
        finally
        {
            // The job is over here, whatever the dialog does next, so closing the window is no longer held back.
            _app.RepairRunning = false;
        }

        var status = result.Success
            ? $"Finished in {result.Duration.TotalSeconds:0} seconds.{(result.BytesFreed > 0 ? $" Freed {ByteSize.Format(result.BytesFreed)}." : "")}{(item.Def.NeedsReboot ? " Restart to finish." : "")}"
            : result.Cancelled ? "Stopped." : $"Did not finish. {result.Error}";
        await log.FinishAsync(status);
        _running = false;
    }
}
