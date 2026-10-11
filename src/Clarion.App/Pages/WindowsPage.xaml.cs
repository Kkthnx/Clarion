using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

/// <summary>The five lists about Windows itself, as tabs of one page: Tweaks, Power, Network, Windows features and Updates.</summary>
public sealed partial class WindowsPage : Page
{
    public const string DefaultTab = "tweaks";

    /// <summary>The tab shown last, so coming back to the page lands where the person left it. Also how another page asks for one.</summary>
    public static string Tab { get; set; } = DefaultTab;

    private static readonly string[] TabNames = ["tweaks", "power", "network", "features", "updates"];

    /// <summary>True for the names of the five lists this page shows.</summary>
    public static bool IsTab(string tag) => TabNames.Contains(tag);

    private bool _starting;

    public WindowsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _starting = true;
        var wanted = Tabs.Items.OfType<SelectorBarItem>().FirstOrDefault(i => i.Tag as string == Tab) ?? (SelectorBarItem)Tabs.Items[0];
        Tabs.SelectedItem = wanted;
        _starting = false;
        Show((string)wanted.Tag);
    }

    private void OnTabChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (_starting || sender.SelectedItem?.Tag is not string tag) return;
        Tab = tag;
        Show(tag);
    }

    private void Show(string tag)
    {
        if (!MainWindow.Sections.TryGetValue(tag, out var args)) return;
        Inner.Navigate(typeof(TweakListPage), args with { Title = "" });
        Inner.BackStack.Clear();
    }
}
