using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

/// <summary>One tab: the name other pages ask for it by, the words on it, and the page it shows.</summary>
public sealed record TabSpec(string Tag, string Text, Type Page);

/// <summary>
/// A menu entry that holds more than one page, each as a tab. Every tab keeps the name it had as a menu entry, so a link that asks for
/// "verify" or "feedback" still lands on it.
/// </summary>
public sealed partial class TabsPage : Page
{
    /// <summary>The menu entries that hold tabs, by the entry's name. The first tab is the one a first visit shows.</summary>
    public static readonly IReadOnlyDictionary<string, TabSpec[]> Groups = new Dictionary<string, TabSpec[]>
    {
        ["repair"] =
        [
            new("repair", "Repair", typeof(RepairPage)),
            new("troubleshoot", "What broke", typeof(TroubleshootPage)),
            new("system", "System", typeof(SystemPage)),
        ],
        ["changes"] =
        [
            new("verify", "Verify", typeof(VerifyPage)),
            new("safety", "Safety", typeof(SafetyPage)),
        ],
        ["about"] =
        [
            new("about", "About", typeof(AboutPage)),
            new("whatsnew", "What's new", typeof(WhatsNewPage)),
            new("feedback", "Report a problem", typeof(FeedbackPage)),
        ],
    };

    // The tab shown last in each entry, so coming back lands where the person left it.
    private static readonly Dictionary<string, string> LastTab = [];

    /// <summary>The menu entry that holds this tab, or null for a name that is not a tab.</summary>
    public static string? GroupOf(string tag) => Groups.FirstOrDefault(g => g.Value.Any(t => t.Tag == tag)).Key;

    /// <summary>Asks for a tab before its entry is opened.</summary>
    public static void Select(string group, string tag) => LastTab[group] = tag;

    /// <summary>Raised with a tab's name each time one is shown, so the window knows which page is on screen.</summary>
    public static event EventHandler<string>? TabShown;

    private string _group = "";
    private bool _starting;

    public TabsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _group = e.Parameter as string ?? "";
        if (!Groups.TryGetValue(_group, out var tabs)) return;
        AutomationProperties.SetName(Tabs, _group switch { "repair" => "Repair pages", "changes" => "Changes pages", _ => "About pages" });

        _starting = true;
        Tabs.Items.Clear();
        foreach (var tab in tabs) Tabs.Items.Add(new SelectorBarItem { Text = tab.Text, Tag = tab.Tag });
        var wantedTag = LastTab.TryGetValue(_group, out var last) ? last : tabs[0].Tag;
        var wanted = Tabs.Items.FirstOrDefault(i => i.Tag as string == wantedTag) ?? Tabs.Items[0];
        Tabs.SelectedItem = wanted;
        _starting = false;
        Show((string)wanted.Tag);
    }

    private void OnTabChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (_starting || sender.SelectedItem?.Tag is not string tag) return;
        Show(tag);
    }

    private void Show(string tag)
    {
        var tab = Groups[_group].FirstOrDefault(t => t.Tag == tag);
        if (tab is null) return;
        LastTab[_group] = tag;
        Inner.Navigate(tab.Page);
        Inner.BackStack.Clear();
        TabShown?.Invoke(this, tag);
    }
}
