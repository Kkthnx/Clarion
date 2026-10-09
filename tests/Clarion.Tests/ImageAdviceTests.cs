using Clarion.Core.Catalog;
using Clarion.Core.Model;
using Clarion.Core.SystemInfo;

namespace Clarion.Tests;

public sealed class ImageAdviceTests
{
    private static Tweak Of(string topic, params Operation[] ops) => new()
    {
        Id = "t." + topic, Category = "Test", Topic = topic, Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe,
        Apply = ops.Length > 0 ? ops : [new SetRegistryValue(new RegistryTarget(RegistryHive.CurrentUser, "Software\\T", "x"), new RegistryData(RegistryKind.DWord, "0"))],
    };

    private static InstallVerdict Verdict(InstallInputs inputs) => InstallCheck.Evaluate(inputs);

    [Theory]
    [InlineData("Windows 11 Pro Ghost Spectre Superlite 25H2", "Ghost Spectre")]
    [InlineData("tiny11 core", "Tiny11")]
    [InlineData("Windows 10 ReviOS 24.12", "ReviOS")]
    [InlineData("AtlasOS 0.4", "AtlasOS")]
    public void A_known_image_named_in_the_system_information_is_reported_by_name(string branding, string expected) =>
        Assert.Equal(expected, Verdict(new InstallInputs { Branding = [branding] }).ImageName);

    [Fact]
    public void A_standard_install_names_no_image_and_gets_no_notes()
    {
        var verdict = Verdict(new InstallInputs { Branding = ["Windows 11 Pro"], StoreMissing = false, EdgeMissing = false });

        Assert.Null(verdict.ImageName);
        Assert.Empty(ImageAdvice.Notes(CatalogLoader.LoadEmbedded(), verdict));
    }

    [Fact]
    public void Edge_settings_are_noted_when_edge_is_not_installed_and_other_settings_are_not()
    {
        var verdict = Verdict(new InstallInputs { EdgeMissing = true });

        Assert.Contains("Edge is not installed", ImageAdvice.NoteFor(Of("Microsoft Edge"), verdict));
        Assert.Null(ImageAdvice.NoteFor(Of("Search"), verdict));
    }

    [Fact]
    public void Defender_sharing_settings_are_noted_when_the_defender_service_is_missing()
    {
        var verdict = Verdict(new InstallInputs { DefenderServiceMissing = true });

        Assert.Contains("Defender service is not installed", ImageAdvice.NoteFor(Of("Defender sharing"), verdict));
    }

    [Fact]
    public void Update_settings_are_noted_when_the_update_service_is_missing_or_off_with_different_words()
    {
        var missing = ImageAdvice.NoteFor(Of("Updates"), Verdict(new InstallInputs { UpdateServiceMissing = true }));
        var off = ImageAdvice.NoteFor(Of("Updates"), Verdict(new InstallInputs { UpdateServiceDisabled = true }));

        Assert.Contains("not installed", missing);
        Assert.Contains("turned off", off);
    }

    [Fact]
    public void App_removals_carry_a_store_note_only_when_the_store_is_gone()
    {
        var app = Of("Apps", new RemoveAppxPackage("Contoso.Widget"));

        Assert.Contains("Store is not installed", ImageAdvice.NoteFor(app, Verdict(new InstallInputs { StoreMissing = true })));
        Assert.Null(ImageAdvice.NoteFor(app, Verdict(new InstallInputs { StoreMissing = false })));
        Assert.Null(ImageAdvice.NoteFor(app, Verdict(new InstallInputs())));
    }

    [Fact]
    public void Nothing_is_noted_before_the_system_has_been_read()
    {
        Assert.Null(ImageAdvice.NoteFor(Of("Microsoft Edge"), null));
        Assert.Empty(ImageAdvice.Notes(CatalogLoader.LoadEmbedded(), null));
    }

    [Fact]
    public void On_a_stripped_image_the_catalog_gets_notes_for_the_edge_defender_update_and_app_settings()
    {
        var verdict = Verdict(new InstallInputs
        {
            Branding = ["Ghost Spectre"], EdgeMissing = true, DefenderServiceMissing = true, UpdateServiceDisabled = true, StoreMissing = true,
        });

        var notes = ImageAdvice.Notes(CatalogLoader.LoadEmbedded(), verdict);

        Assert.Contains(notes.Keys, id => id.StartsWith("edge.", StringComparison.Ordinal));
        Assert.Contains(notes.Keys, id => id.StartsWith("defender.", StringComparison.Ordinal));
        Assert.Contains(notes.Keys, id => id.StartsWith("updates.", StringComparison.Ordinal));
        Assert.Contains(notes.Keys, id => id.StartsWith("debloat.app.", StringComparison.Ordinal));
        Assert.DoesNotContain(notes.Keys, id => id.StartsWith("dns.", StringComparison.Ordinal));
    }
}
