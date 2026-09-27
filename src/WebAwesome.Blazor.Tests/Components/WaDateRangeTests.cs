using System;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The WaDateRange value type: structural equality, and the culture-free conversion of Web Awesome's from/to wire
/// form, mirroring the element (formatRange writes a half-filled range as its one date; parseRange reads a single
/// date as the start and orders a complete range), strict about each date (exactly yyyy-MM-dd, a real day).
/// </summary>
public class WaDateRangeTests
{
    [Fact]
    public void Equality_IsStructural()
    {
        Assert.Equal(new WaDateRange(May1, May7), new WaDateRange(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 7)));
        Assert.NotEqual(new WaDateRange(May1, May7), new WaDateRange(May7, May1));
        Assert.NotEqual(new WaDateRange(May1, null), new WaDateRange(null, May1));
        Assert.Equal(default, new WaDateRange(null, null));
    }

    [Fact]
    public void IsEmpty_AndIsComplete_ReflectTheEnds()
    {
        Assert.True(default(WaDateRange).IsEmpty);
        Assert.False(default(WaDateRange).IsComplete);
        Assert.False(new WaDateRange(May1, null).IsEmpty);
        Assert.False(new WaDateRange(May1, null).IsComplete);
        Assert.True(new WaDateRange(May1, May7).IsComplete);
    }

    [Theory]
    [InlineData(true, true, "2026-05-01/2026-05-07")]
    [InlineData(true, false, "2026-05-01")]
    [InlineData(false, true, "2026-05-07")]
    [InlineData(false, false, "")]
    public void ToString_WritesTheWireForm_InAnyCulture(bool withFrom, bool withTo, string expected)
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);

        var range = new WaDateRange(withFrom ? May1 : null, withTo ? May7 : null);

        Assert.Equal(expected, range.ToString());
    }

    [Fact]
    public void ToString_ZeroPadsTheYear()
    {
        Assert.Equal("0987-01-02/0987-01-03", new WaDateRange(new DateOnly(987, 1, 2), new DateOnly(987, 1, 3)).ToString());
    }

    [Fact]
    public void TryParse_CompleteRange()
    {
        Assert.True(WaDateRange.TryParse("2026-05-01/2026-05-07", out var range));
        Assert.Equal(new WaDateRange(May1, May7), range);
    }

    [Fact]
    public void TryParse_SingleDate_IsAHalfFilledRange()
    {
        Assert.True(WaDateRange.TryParse("2026-05-01", out var range));
        Assert.Equal(new WaDateRange(May1, null), range);
    }

    [Fact]
    public void TryParse_ReversedRange_IsOrdered()
    {
        Assert.True(WaDateRange.TryParse("2026-05-07/2026-05-01", out var range));
        Assert.Equal(new WaDateRange(May1, May7), range);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("2026-05-01/")]
    [InlineData("/2026-05-07")]
    [InlineData("2026-05-01/2026-05-07/2026-05-09")]
    [InlineData("01.05.2026/07.05.2026")]
    [InlineData("2026-5-1/2026-5-7")]
    [InlineData("2026-02-30/2026-03-01")]
    [InlineData("2026-05-01 - 2026-05-07")]
    public void TryParse_RejectsWhatIsNotTheWireForm(string text)
    {
        Assert.False(WaDateRange.TryParse(text, out var range));
        Assert.Equal(default, range);
    }

    [Fact]
    public void TryParse_Null_Fails()
    {
        Assert.False(WaDateRange.TryParse(null, out _));
    }

    [Fact]
    public void Parse_ThrowsOnInvalidText()
    {
        Assert.Throws<FormatException>(() => WaDateRange.Parse("2026-05-01/garbage"));
        Assert.Throws<ArgumentNullException>(() => WaDateRange.Parse(null!));
        Assert.Equal(new WaDateRange(May1, May7), WaDateRange.Parse("2026-05-01/2026-05-07"));
    }

    [Theory]
    [InlineData("2026-05-01/2026-05-07")]
    [InlineData("2026-05-01")]
    public void ToString_RoundTripsThroughTryParse(string text)
    {
        Assert.True(WaDateRange.TryParse(text, out var range));
        Assert.Equal(text, range.ToString());
    }

    #region ------ Internals ------

    private static readonly DateOnly May1 = new(2026, 5, 1);
    private static readonly DateOnly May7 = new(2026, 5, 7);

    #endregion
}
