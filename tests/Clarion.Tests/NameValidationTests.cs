using Clarion.Core.Appx;
using Clarion.Core.Features;
using Clarion.Core.Power;

namespace Clarion.Tests;

/// <summary>A name that ends in a line break must not pass for a plain one, because it ends up in a command line.</summary>
public sealed class NameValidationTests
{
    [Fact]
    public void A_trailing_line_break_makes_a_name_invalid()
    {
        Assert.True(AppxSafety.IsValidName("Microsoft.BingNews"));
        Assert.False(AppxSafety.IsValidName("Microsoft.BingNews\n"));
        Assert.True(FeatureRules.IsValidName("Printing-PrintToPDFServices-Features"));
        Assert.False(FeatureRules.IsValidName("Printing-PrintToPDFServices-Features\n"));
        Assert.True(PowerPlans.IsValid("381b4222-f694-41f0-9685-ff5bb260df2e"));
        Assert.False(PowerPlans.IsValid("381b4222-f694-41f0-9685-ff5bb260df2e\n"));
    }
}
