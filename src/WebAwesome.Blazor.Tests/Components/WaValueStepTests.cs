using System;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The WaValueStep value type (a positive number or "any", the number | 'any' step of wa-input, wa-number-input and
/// wa-time-input): implicit conversion from the numeric literals Razor produces, the positive-and-finite invariant, the
/// culture-free wire form, and structural equality with default being Any.
/// </summary>
public class WaValueStepTests
{
    [Fact]
    public void Numbers_ConvertImplicitly()
    {
        WaValueStep fromInt = 5;
        WaValueStep fromLong = 7L;
        WaValueStep fromDouble = 0.5;
        WaValueStep fromDecimal = 2.25m;

        Assert.Equal(5m, fromInt.Number);
        Assert.Equal(7m, fromLong.Number);
        Assert.Equal(0.5m, fromDouble.Number);
        Assert.Equal(2.25m, fromDecimal.Number);
        Assert.False(fromInt.IsAny);
    }

    [Fact]
    public void Any_IsTheDefault_AndHasNoNumber()
    {
        Assert.True(WaValueStep.Any.IsAny);
        Assert.Null(WaValueStep.Any.Number);
        Assert.Equal(WaValueStep.Any, default);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(1e30)]
    public void NonPositiveOrNonFiniteNumbers_AreRejected(double step)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => (WaValueStep)step);
    }

    [Fact]
    public void NonPositiveDecimals_AreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaValueStep(0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => (WaValueStep)(-3));
    }

    [Theory]
    [InlineData(0.5, "0.5")]
    [InlineData(60, "60")]
    [InlineData(1234.5, "1234.5")]
    public void ToString_IsTheInvariantWireForm_InAnyCulture(double step, string expected)
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);

        Assert.Equal(expected, ((WaValueStep)step).ToString());
        Assert.Equal("any", WaValueStep.Any.ToString());
    }

    [Fact]
    public void Equality_IsStructural()
    {
        Assert.Equal((WaValueStep)1.5, new WaValueStep(1.5m));
        Assert.NotEqual((WaValueStep)1, WaValueStep.Any);
        Assert.NotEqual((WaValueStep)1, (WaValueStep)2);
    }
}
