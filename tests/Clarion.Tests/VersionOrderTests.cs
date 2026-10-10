using Clarion.Core.Updates;

namespace Clarion.Tests;

public sealed class VersionOrderTests
{
    [Theory]
    [InlineData("0.1.0-beta.10", "0.1.0-beta.9")]
    [InlineData("0.1.0-beta.11", "0.1.0-beta.10")]
    [InlineData("0.1.0-beta.100", "0.1.0-beta.99")]
    [InlineData("0.1.0", "0.1.0-beta.10")]
    [InlineData("0.2.0-beta.1", "0.1.0-beta.10")]
    public void A_version_with_a_longer_number_still_sorts_after_the_shorter_one(string newer, string older)
    {
        // As text, "beta.10" comes before "beta.9". Semantic versioning compares the numbers, and the update check and What's new rely on that.
        Assert.True(ReleaseVersion.Parse(newer)! > ReleaseVersion.Parse(older)!);
        Assert.True(ReleaseVersion.Parse(older)! < ReleaseVersion.Parse(newer)!);
    }

    [Fact]
    public void The_update_check_offers_beta_10_to_someone_on_beta_9_and_not_the_other_way_round()
    {
        static ReleaseInfo Rel(string tag) => new(tag, tag, "https://github.com/Kkthnx/Clarion/releases/tag/" + tag, true, false, null);
        var releases = new[] { Rel("v0.1.0-beta.9"), Rel("v0.1.0-beta.10"), Rel("v0.1.0-beta.8") };

        Assert.Equal(["v0.1.0-beta.10"], UpdateCheck.NewerThan("0.1.0-beta.9", releases).Select(r => r.Tag));
        Assert.Empty(UpdateCheck.NewerThan("0.1.0-beta.10", releases));
    }
}
