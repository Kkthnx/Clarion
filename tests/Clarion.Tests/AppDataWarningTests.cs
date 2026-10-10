using Clarion.Core.Appx;
using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class AppDataWarningTests
{
    [Fact]
    public void The_notice_says_the_data_goes_and_that_revert_does_not_bring_it_back()
    {
        Assert.Contains("data", AppRemovalNotice.Text);
        Assert.Contains("Revert reinstalls the app, not that data", AppRemovalNotice.Text);
        Assert.StartsWith("https://learn.microsoft.com/", AppRemovalNotice.Source);
    }

    [Fact]
    public void Removing_Sticky_Notes_is_not_called_low_risk_and_does_not_promise_that_notes_sync()
    {
        // Microsoft says an app's data is removed with the app. Sticky Notes keeps unsynced notes only in the app, so the catalog must not
        // call this Low risk or state as fact that the notes are safe elsewhere.
        var tweak = CatalogLoader.LoadEmbedded().Single(t => t.Id == "debloat.app.microsoftstickynotes");

        Assert.True(tweak.RiskLevel >= RiskLevel.Medium);
        Assert.Contains("not synced", tweak.Risk);
        Assert.DoesNotContain(tweak.Facts, f => f.Equals("Notes sync with your Microsoft account", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(AppRemovalNotice.Source, tweak.Sources);
    }

    [Fact]
    public void Every_app_removal_in_the_catalog_is_something_the_notice_applies_to()
    {
        var removals = CatalogLoader.LoadEmbedded().Where(t => t.Apply.Any(o => o is RemoveAppxPackage)).ToList();

        Assert.NotEmpty(removals);
        Assert.All(removals, t => Assert.All(t.Apply, o => Assert.IsType<RemoveAppxPackage>(o)));
    }
}
