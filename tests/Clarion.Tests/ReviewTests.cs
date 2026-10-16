using Clarion.Core.Journal;
using Clarion.Core.Layout;
using Clarion.Core.Model;
using Clarion.Core.Reports;

namespace Clarion.Tests;

public sealed class ReviewTests
{
    [Theory]
    [InlineData(true, false, true, ReviewKind.InstallApps)]
    [InlineData(true, false, false, ReviewKind.UninstallApps)]
    [InlineData(false, true, true, ReviewKind.RemoveWindowsApps)]
    [InlineData(false, true, false, ReviewKind.Revert)]
    [InlineData(false, false, true, ReviewKind.TurnOn)]
    [InlineData(false, false, false, ReviewKind.Revert)]
    public void A_change_is_sorted_by_what_it_does(bool installs, bool removes, bool on, ReviewKind expected) =>
        Assert.Equal(expected, ReviewGroups.Of(installs, removes, on));

    [Fact]
    public void A_short_queue_stays_one_plain_list_whatever_it_holds()
    {
        var kinds = new[] { ReviewKind.TurnOn, ReviewKind.InstallApps, ReviewKind.Revert };
        Assert.False(ReviewGroups.ShouldGroup(kinds));
    }

    [Fact]
    public void A_long_queue_of_one_kind_needs_no_sections()
    {
        var kinds = Enumerable.Repeat(ReviewKind.TurnOn, 20).ToList();
        Assert.False(ReviewGroups.ShouldGroup(kinds));
    }

    [Fact]
    public void A_long_mixed_queue_is_sorted_into_sections()
    {
        var kinds = Enumerable.Repeat(ReviewKind.TurnOn, 5).Append(ReviewKind.InstallApps).ToList();
        Assert.True(ReviewGroups.ShouldGroup(kinds));
    }

    [Fact]
    public void Every_kind_has_a_place_in_the_order_and_a_title_with_its_count()
    {
        Assert.Equal(Enum.GetValues<ReviewKind>().Order(), ReviewGroups.Order.Order());
        foreach (var kind in ReviewGroups.Order) Assert.EndsWith("(3)", ReviewGroups.Title(kind, 3));
    }

    private static JournalEntry Entry(string id, DateTimeOffset when, string name)
    {
        var set = new SetRegistryValue(new RegistryTarget(RegistryHive.LocalMachine, "Software\\Test", name), new RegistryData(RegistryKind.DWord, "1"));
        var back = new SetRegistryValue(new RegistryTarget(RegistryHive.LocalMachine, "Software\\Test", name), new RegistryData(RegistryKind.DWord, "0"));
        return new JournalEntry(Guid.NewGuid(), id, JournalAction.Apply, when, set, back);
    }

    [Fact]
    public void A_revert_preview_lists_the_recorded_undo_steps_newest_first_like_the_engine()
    {
        var t = DateTimeOffset.Now;
        var preview = RevertPreview.For([Entry("a", t, "First"), Entry("a", t.AddMinutes(1), "Second")], []);

        Assert.True(preview.FromHistory);
        Assert.Collection(preview.Lines,
            l => Assert.Contains("Second", l),
            l => Assert.Contains("First", l));
    }

    [Fact]
    public void A_revert_preview_with_no_history_shows_the_default_undo()
    {
        var fallback = new SetRegistryValue(new RegistryTarget(RegistryHive.CurrentUser, "Software\\Test", "D"), new RegistryData(RegistryKind.DWord, "5"));
        var preview = RevertPreview.For([], [fallback]);

        Assert.False(preview.FromHistory);
        Assert.Equal(["Set HKCU\\Software\\Test\\D to 5 (DWord)"], preview.Lines);
    }

}
