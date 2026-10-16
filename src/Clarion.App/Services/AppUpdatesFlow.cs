using Clarion.Core.Apps;
using Clarion.Core.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Services;

/// <summary>
/// Check for app updates: ask winget which installed apps have a newer version, let the person pick which to update, update those one at a
/// time with a live log and a Stop button, and say what happened. Nothing is ticked at first, because an update changes a program that may
/// be open and cannot be undone from here.
/// </summary>
public static class AppUpdatesFlow
{
    public static async Task RunAsync(XamlRoot root)
    {
        var app = AppServices.Instance;
        if (app.IsBusy || app.RepairRunning)
        {
            await Dialogs.Say(root, "Wait for the current job to finish", "An update and another change should not run at the same time.");
            return;
        }
        if (!app.Runtime.Winget.IsAvailable)
        {
            await Dialogs.Say(root, "winget was not found for this account", "Apps are updated with winget, which comes with App Installer. Install or repair it from Microsoft, then try again.");
            return;
        }
        if (!app.IsElevated)
        {
            var ask = new ContentDialog
            {
                Title = "Administrator rights are needed to update apps",
                Content = "Most installers change machine wide files. Restart .Clarion as administrator to continue.",
                PrimaryButtonText = "Restart as administrator",
                CloseButtonText = "Not now",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = root,
            };
            if (await ask.ShowAsync() == ContentDialogResult.Primary) ((App)Application.Current).RestartAsAdministrator();
            return;
        }

        var byPackage = app.Items
            .Where(i => i.InstallsApps)
            .Select(i => (Item: i, Package: i.Tweak.Apply.OfType<InstallWingetPackage>().First().Id))
            .ToDictionary(p => p.Package, p => p.Item, StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<AppUpdate> updates;
        var waiting = new ContentDialog
        {
            Title = "Asking winget",
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Children = { new ProgressRing { IsActive = true, Width = 24, Height = 24 }, new TextBlock { Text = "Which of your apps have a newer version? This can take a few seconds.", VerticalAlignment = VerticalAlignment.Center } },
            },
            XamlRoot = root,
        };
        var waitShown = waiting.ShowAsync();
        try
        {
            updates = await Task.Run(() => app.Runtime.Winget.AvailableUpdates(byPackage.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase)));
        }
        catch (InvalidOperationException ex)
        {
            Log.Write($"Could not read the updates: {ex.Message}");
            waiting.Hide();
            await waitShown;
            await Dialogs.Say(root, "winget could not say what is out of date", ex.Message);
            return;
        }
        waiting.Hide();
        await waitShown;

        if (updates.Count == 0)
        {
            await Dialogs.Say(root, "Your apps are up to date",
                "Every app from this page that winget can read the version of is on its newest version. winget does not list an app whose installed version it cannot read, or one that is pinned.");
            return;
        }

        var chosen = await PickAsync(root, updates, byPackage);
        if (chosen is null) return;
        await UpdateAsync(root, app, chosen.Value.Updates, chosen.Value.AcceptTerms);
    }

    private static async Task<(IReadOnlyList<AppUpdate> Updates, bool AcceptTerms)?> PickAsync(
        XamlRoot root, IReadOnlyList<AppUpdate> updates, Dictionary<string, TweakItem> byPackage)
    {
        var boxes = new List<(CheckBox Box, AppUpdate Update)>();
        var list = new StackPanel { Spacing = 4 };
        foreach (var update in updates)
        {
            var item = byPackage[update.Id];
            var row = new Grid { ColumnSpacing = 8 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var box = new CheckBox { Content = $"{item.Name}   {update.Current} → {update.Available}", IsChecked = false };
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(box, $"Update {item.Name} from {update.Current} to {update.Available}");
            row.Children.Add(box);
            if (item.SourceLinks.FirstOrDefault(l => l.Link.AbsoluteUri.Contains("winget-pkgs", StringComparison.Ordinal)) is { } record)
            {
                var link = new HyperlinkButton { Content = "Package record", FontSize = 12, Padding = new Thickness(4, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center };
                link.Click += (_, _) => Shell.Open(record.Link.AbsoluteUri);
                Grid.SetColumn(link, 1);
                row.Children.Add(link);
            }
            boxes.Add((box, update));
            list.Children.Add(row);
        }

        var all = new HyperlinkButton { Content = "Select all", Padding = new Thickness(0) };
        var none = new HyperlinkButton { Content = "Select none", Padding = new Thickness(0) };
        var agree = new CheckBox { Content = "I accept the licence terms of an app whose new version asks for them again", IsChecked = false };
        var note = new TextBlock
        {
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            FontSize = 12,
            Opacity = 0.8,
            Text = "An update cannot be undone from .Clarion, because winget keeps no copy of the old version. Close an app before you update it, or its installer may wait for you. An app whose version winget cannot read, or that is pinned in winget, is not listed.",
        };
        var body = new StackPanel { Spacing = 12, MaxWidth = 560 };
        body.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { all, none } });
        body.Children.Add(new ScrollViewer { MaxHeight = 320, Content = list });
        body.Children.Add(agree);
        body.Children.Add(note);

        var dialog = new ContentDialog
        {
            Title = $"{updates.Count} {(updates.Count == 1 ? "update" : "updates")} available",
            Content = body,
            PrimaryButtonText = "Update selected",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            IsPrimaryButtonEnabled = false,
            XamlRoot = root,
        };
        void Refresh()
        {
            var count = boxes.Count(b => b.Box.IsChecked == true);
            dialog.IsPrimaryButtonEnabled = count > 0;
            dialog.PrimaryButtonText = count == 0 ? "Update selected" : $"Update {count}";
        }
        foreach (var (box, _) in boxes) { box.Checked += (_, _) => Refresh(); box.Unchecked += (_, _) => Refresh(); }
        all.Click += (_, _) => { foreach (var (box, _) in boxes) box.IsChecked = true; };
        none.Click += (_, _) => { foreach (var (box, _) in boxes) box.IsChecked = false; };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return null;
        return (boxes.Where(b => b.Box.IsChecked == true).Select(b => b.Update).ToList(), agree.IsChecked == true);
    }

    private static async Task UpdateAsync(XamlRoot root, AppServices app, IReadOnlyList<AppUpdate> updates, bool acceptTerms)
    {
        using var log = new LogDialog(root, $"Updating {updates.Count} {(updates.Count == 1 ? "app" : "apps")}");
        log.Start();

        IReadOnlyList<AppUpdateResult> results;
        app.RepairRunning = true;
        try
        {
            results = await Task.Run(() => AppUpdater.Run(app.Runtime.Winget, updates, acceptTerms,
                line => { Log.Write($"update: {line}"); log.Append(line, line.StartsWith("Updating ", StringComparison.Ordinal) ? line : null); }, log.Stop));
        }
        finally { app.RepairRunning = false; }

        var summary = AppUpdater.Summary(results) + ".";
        var extra = new List<string> { "", summary };
        if (results.Any(r => !r.Succeeded && !r.Skipped && r.Message.Contains("licence terms", StringComparison.Ordinal)))
            extra.Add("Run the update again with the licence box ticked for the apps that asked for their terms again.");
        if (app.Runtime.Winget.TakeRestartNeeded()) extra.Add("Restart Windows to finish some of these.");
        await log.FinishAsync(summary, extra.ToArray());
    }
}
