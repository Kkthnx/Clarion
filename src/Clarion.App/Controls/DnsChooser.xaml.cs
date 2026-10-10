using System.ComponentModel;
using Clarion.App.Services;
using Clarion.Core.Dns;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Controls;

/// <summary>
/// The DNS entries are one choice, since only one provider can be active. This shows them as a
/// dropdown and an encryption switch, and queues the matching setting underneath.
/// </summary>
public sealed partial class DnsChooser : UserControl
{
    private const string Prefix = "dns.";
    private const string EncryptedSuffix = "-encrypted";
    private const string DefaultKey = "";

    private List<TweakItem> _items = [];
    private bool _syncing;

    public DnsChooser()
    {
        InitializeComponent();
        Unloaded += (_, _) => Detach();
    }

    /// <summary>True when the setting belongs to the DNS choice shown here.</summary>
    public static bool IsDnsItem(TweakItem item) => item.Tweak.ExclusiveGroup == "dns-provider";

    public void Bind(IEnumerable<TweakItem> dnsItems)
    {
        Detach();
        _items = dnsItems.ToList();
        foreach (var i in _items) i.PropertyChanged += OnItemChanged;

        _syncing = true;
        ProviderBox.Items.Clear();
        ProviderBox.Items.Add(new ComboBoxItem { Content = "Your provider (default)", Tag = DefaultKey });
        foreach (var p in DnsProviders.All.Where(p => _items.Any(i => KeyOf(i) == p.Key)))
            ProviderBox.Items.Add(new ComboBoxItem { Content = p.Name, Tag = p.Key });
        _syncing = false;
        Refresh();
    }

    private void Detach()
    {
        foreach (var i in _items) i.PropertyChanged -= OnItemChanged;
    }

    private static string KeyOf(TweakItem i) => i.Id[Prefix.Length..].Replace(EncryptedSuffix, "");
    private static bool IsEncrypted(TweakItem i) => i.Id.EndsWith(EncryptedSuffix, StringComparison.Ordinal);

    private TweakItem? Find(string key, bool encrypted) =>
        _items.FirstOrDefault(i => KeyOf(i) == key && IsEncrypted(i) == encrypted);

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TweakItem.IsOn) or nameof(TweakItem.State) or nameof(TweakItem.IsChecking)) Refresh();
    }

    /// <summary>Shows what is chosen now, counting a queued change as chosen.</summary>
    private void Refresh()
    {
        _syncing = true;
        try
        {
            var chosen = _items.FirstOrDefault(i => i.IsOn && !i.IsApplied) ?? _items.FirstOrDefault(i => i.IsOn);
            var usable = _items.Any(i => i.CanToggle);
            ProviderBox.IsEnabled = usable;

            var key = chosen is null ? DefaultKey : KeyOf(chosen);
            ProviderBox.SelectedItem = ProviderBox.Items.Cast<ComboBoxItem>().FirstOrDefault(c => (string)c.Tag == key);

            var canEncrypt = key.Length > 0 && Find(key, true) is { CanToggle: true };
            EncryptSwitch.IsEnabled = canEncrypt;
            EncryptSwitch.IsOn = chosen is not null && IsEncrypted(chosen);

            var name = key.Length == 0 ? "Your provider" : DnsProviders.Find(key)?.Name ?? key;
            var pending = chosen is not null && chosen.IsPending;
            var detail = key.Length == 0 ? "" : EncryptSwitch.IsOn ? ", encrypted" : "";
            CurrentText.Text = !usable
                ? "No connected network adapter was found, so there is nothing to change."
                : pending ? $"Queued: {name}{detail}. Review and apply to use it."
                : _items.Any(i => i.IsPending && !i.IsOn) ? "Queued: go back to your provider DNS. Review and apply to use it."
                : $"In use: {name}{detail}";
            AdviceText.Text = chosen?.Tweak.Advice
                ?? "Your internet provider's DNS is used. Pick a public provider if it is slow or unreliable. Skip this on work or school networks.";
        }
        finally
        {
            _syncing = false;
        }
    }

    private void OnProviderChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncing || ProviderBox.SelectedItem is not ComboBoxItem { Tag: string key }) return;
        var keepEncrypted = EncryptSwitch.IsOn;
        Choose(key, keepEncrypted);
    }

    private string? _fastestKey;

    /// <summary>
    /// Times a lookup to each provider, on request. It sends three tiny lookups to each of the five providers, and only that, and it
    /// changes nothing. The answer is a rough guide to which is nearest, which is why it is described that way.
    /// </summary>
    private async void OnTestSpeed(object sender, RoutedEventArgs e)
    {
        SpeedButton.IsEnabled = false;
        SpeedRing.IsActive = true;
        SpeedStatus.Text = "Asking each provider three times";
        SpeedResults.Visibility = Visibility.Collapsed;
        UseFastest.Visibility = Visibility.Collapsed;
        _fastestKey = null;
        try
        {
            using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            var results = await DnsSpeed.MeasureAsync(new Clarion.Engine.UdpDnsTransport(), DnsProviders.All, limit.Token);
            ShowSpeed(results);
        }
        catch (OperationCanceledException)
        {
            SpeedStatus.Text = "That took too long and was stopped. Check the network and try again.";
        }
        finally
        {
            SpeedRing.IsActive = false;
            SpeedButton.IsEnabled = true;
        }
    }

    private void ShowSpeed(IReadOnlyList<DnsSpeedResult> results)
    {
        SpeedResults.Children.Clear();
        var best = results.FirstOrDefault(r => r.Milliseconds is not null);
        foreach (var r in results)
        {
            var row = new Grid { ColumnSpacing = 16 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var isBest = ReferenceEquals(r, best);
            row.Children.Add(new TextBlock { Text = r.Provider.Name, FontWeight = isBest ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal, Foreground = ThemeBrushes.Get(isBest ? "ClOkText" : "ClTextPrimary", ActualTheme) });
            var time = new TextBlock { Text = r.Milliseconds is { } ms ? $"{ms} ms" : "no answer", Foreground = ThemeBrushes.Get(r.Milliseconds is null ? "ClTextMuted" : isBest ? "ClOkText" : "ClTextSecondary", ActualTheme) };
            Grid.SetColumn(time, 1);
            row.Children.Add(time);
            SpeedResults.Children.Add(row);
        }
        SpeedResults.Visibility = Visibility.Visible;

        if (best is null)
        {
            SpeedStatus.Text = "No provider answered. This PC may be offline, or a firewall may block DNS to other servers.";
            return;
        }
        SpeedStatus.Text = $"Nearest from this PC: {best.Provider.Name}, {best.Milliseconds} ms. A rough guide, not a test of every site.";
        // Offering the one already chosen would be a button that changes nothing.
        var chosen = _items.FirstOrDefault(i => i.IsOn);
        var alreadyChosen = chosen is not null && KeyOf(chosen) == best.Provider.Key;
        if (!alreadyChosen && _items.Any(i => KeyOf(i) == best.Provider.Key && i.CanToggle))
        {
            _fastestKey = best.Provider.Key;
            UseFastest.Content = $"Choose {best.Provider.Name}";
            UseFastest.Visibility = Visibility.Visible;
        }
    }

    private void OnUseFastest(object sender, RoutedEventArgs e)
    {
        if (_fastestKey is null) return;
        Choose(_fastestKey, EncryptSwitch.IsOn);
    }

    private void OnEncryptToggled(object sender, RoutedEventArgs e)
    {
        if (_syncing || ProviderBox.SelectedItem is not ComboBoxItem { Tag: string key } || key.Length == 0) return;
        Choose(key, EncryptSwitch.IsOn);
    }

    private void Choose(string key, bool encrypted)
    {
        _syncing = true;
        try
        {
            if (key.Length == 0)
            {
                foreach (var i in _items.Where(i => i.IsOn)) i.IsOn = false;
            }
            else
            {
                var target = Find(key, encrypted && Find(key, true) is { CanToggle: true }) ?? Find(key, false);
                if (target is { CanToggle: true }) target.IsOn = true;
            }
        }
        finally
        {
            _syncing = false;
        }
        Refresh();
    }
}
