using System.Xml.Linq;

namespace Clarion.Tests;

/// <summary>
/// A control that takes input but has no text of its own is read out by a screen reader as just its type unless it is given a name.
/// This reads the page files and lists the ones that have none. It found nothing by hand-checking until a person ran a screen reader
/// sweep, so it runs with every build now.
/// </summary>
public sealed class XamlAccessibilityTests
{
    private static readonly HashSet<string> InputControls =
    [
        "ComboBox", "ToggleSwitch", "TextBox", "PasswordBox", "AutoSuggestBox", "CheckBox", "RadioButton", "ToggleButton", "Slider",
        "CalendarDatePicker", "DatePicker", "NumberBox", "RatingControl",
    ];

    private static string AppFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "src", "Clarion.App");
    }

    private static IEnumerable<string> PageFiles() =>
        Directory.EnumerateFiles(AppFolder(), "*.xaml", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

    private static bool HasName(XElement e)
    {
        bool Has(string attribute) => e.Attributes().Any(a => a.Name.LocalName == attribute && a.Value.Trim().Length > 0);
        if (Has("AutomationProperties.Name") || Has("AutomationProperties.LabeledBy") || Has("Header")) return true;
        // A switch whose on and off text says what it is, such as "Preview only", names itself, and so does a box with a caption.
        if (Has("OnContent") && Has("OffContent")) return true;
        return Has("Content") || (e.Nodes().OfType<XText>().Any(t => t.Value.Trim().Length > 0));
    }

    [Fact]
    public void Every_page_file_is_well_formed_xml()
    {
        var files = PageFiles().ToList();
        Assert.NotEmpty(files);
        foreach (var file in files) XDocument.Load(file);
    }

    [Fact]
    public void Every_input_control_has_a_name_a_screen_reader_can_read()
    {
        var unnamed = new List<string>();
        foreach (var file in PageFiles())
        {
            var doc = XDocument.Load(file, LoadOptions.SetLineInfo);
            foreach (var e in doc.Descendants().Where(e => InputControls.Contains(e.Name.LocalName)))
            {
                if (HasName(e)) continue;
                var line = ((System.Xml.IXmlLineInfo)e).LineNumber;
                unnamed.Add($"{Path.GetRelativePath(AppFolder(), file)}:{line} {e.Name.LocalName}");
            }
        }
        Assert.True(unnamed.Count == 0, "Input controls with no accessible name (add AutomationProperties.Name):\n" + string.Join("\n", unnamed));
    }
}
