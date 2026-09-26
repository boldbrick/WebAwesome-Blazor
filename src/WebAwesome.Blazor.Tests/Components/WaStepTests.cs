using System;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The WaStep value type (a positive number or "any", the number | 'any' step of wa-input, wa-number-input and
/// wa-time-input): implicit conversion from the numeric literals Razor produces, the positive-and-finite invariant, the
/// culture-free wire form, and structural equality with default being Any.
/// </summary>
public class WaStepTests
{
    [Fact]
    public void Numbers_ConvertImplicitly()
    {
        WaStep fromInt = 5;
        WaStep fromLong = 7L;
        WaStep fromDouble = 0.5;
        WaStep fromDecimal = 2.25m;

        Assert.Equal(5m, fromInt.Number);
        Assert.Equal(7m, fromLong.Number);
        Assert.Equal(0.5m, fromDouble.Number);
        Assert.Equal(2.25m, fromDecimal.Number);
        Assert.False(fromInt.IsAny);
    }

    [Fact]
    public void Any_IsTheDefault_AndHasNoNumber()
    {
        Assert.True(WaStep.Any.IsAny);
        Assert.Null(WaStep.Any.Number);
        Assert.Equal(WaStep.Any, default);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(1e30)]
    public void NonPositiveOrNonFiniteNumbers_AreRejected(double step)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => (WaStep)step);
    }

    [Fact]
    public void NonPositiveDecimals_AreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaStep(0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => (WaStep)(-3));
    }

    [Theory]
    [InlineData(0.5, "0.5")]
    [InlineData(60, "60")]
    [InlineData(1234.5, "1234.5")]
    public void ToString_IsTheInvariantWireForm_InAnyCulture(double step, string expected)
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);

        Assert.Equal(expected, ((WaStep)step).ToString());
        Assert.Equal("any", WaStep.Any.ToString());
    }

    [Fact]
    public void Equality_IsStructural()
    {
        Assert.Equal((WaStep)1.5, new WaStep(1.5m));
        Assert.NotEqual((WaStep)1, WaStep.Any);
        Assert.NotEqual((WaStep)1, (WaStep)2);
    }
}
