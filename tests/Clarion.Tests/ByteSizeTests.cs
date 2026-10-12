using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class ByteSizeTests
{
    [Theory]
    [InlineData(0, "0 bytes")]
    [InlineData(1, "1 byte")]
    [InlineData(2, "2 bytes")]
    [InlineData(1023, "1023 bytes")]
    public void Small_counts_are_in_bytes_with_the_right_plural(long bytes, string expected) => Assert.Equal(expected, ByteSize.Format(bytes));

    [Fact]
    public void Larger_counts_use_the_largest_unit_that_fits()
    {
        using var _ = new CultureScope("en-US");
        Assert.Equal("1.0 KB", ByteSize.Format(1024));
        Assert.Equal("1.5 MB", ByteSize.Format(3L << 19));
        Assert.Equal("2.0 GB", ByteSize.Format(2L << 30));
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly System.Globalization.CultureInfo _old = System.Globalization.CultureInfo.CurrentCulture;
        public CultureScope(string name) => System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(name);
        public void Dispose() => System.Globalization.CultureInfo.CurrentCulture = _old;
    }
}
