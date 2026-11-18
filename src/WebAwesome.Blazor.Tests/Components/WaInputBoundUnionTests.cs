#if NET11_0_OR_GREATER
using System;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// WaInput's min and max on net11.0, the C# 15 union WaInputBound (CEM number | string, passed to the native input
/// unchanged): every case (string, decimal, int, long, ulong, DateOnly, TimeOnly, DateTime) renders the same attribute
/// text as the matching typed accessor of net9.0 and net10.0 (WaInputBoundsTests; int, which has no accessor, like
/// long), culture-free, an integer constant binds the int case, and null or a default union renders nothing.
/// </summary>
public class WaInputBoundUnionTests : BunitContext
{
    public WaInputBoundUnionTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// The value of each case and the attribute text it must render (the texts of WaInputBoundsTests.Accessors, plus int).
    /// </summary>
    public static TheoryData<object, string> Cases => new()
    {
        { "2026-01-02", "2026-01-02" },
        { -1234.5m, "-1234.5" },
        { -2_000_000_000, "-2000000000" },
        { -9_000_000_000L, "-9000000000" },
        { 18_000_000_000_000_000_000UL, "18000000000000000000" },
        { new DateOnly(2026, 3, 4), "2026-03-04" },
        { new TimeOnly(13, 4), "13:04" },
        { new TimeOnly(13, 4, 5), "13:04:05" },
        { new DateTime(2026, 3, 4, 13, 4, 0), "2026-03-04T13:04" },
        { new DateTime(2026, 3, 4, 13, 4, 5, DateTimeKind.Utc), "2026-03-04T13:04:05" },
        { new DateTime(2026, 3, 4, 13, 4, 5, 250), "2026-03-04T13:04:05.250" },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void EveryCase_RendersTheNativeForm_InAnyCulture(object value, string expected)
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var bound = ToBound(value);

        var element = RenderInput(bound, bound);

        Assert.Equal(expected, element.GetAttribute("min"));
        Assert.Equal(expected, element.GetAttribute("max"));
    }

    [Fact]
    public void IntegerConstant_BindsTheIntCase()
    {
        WaInputBound bound = 10;

        Assert.Equal(10, Assert.IsType<int>(bound.Value));
    }

    [Fact]
    public void NullBounds_RenderNothing()
    {
        var element = RenderInput(null, null);

        Assert.False(element.HasAttribute("min"));
        Assert.False(element.HasAttribute("max"));
    }

    [Fact]
    public void DefaultUnion_RendersNothing()
    {
        var element = RenderInput(default(WaInputBound), default(WaInputBound));

        Assert.False(element.HasAttribute("min"));
        Assert.False(element.HasAttribute("max"));
    }

    [Fact]
    public void MinAndMax_TakeDifferentCases()
    {
        var element = RenderInput(new DateOnly(2026, 1, 1), "2026-12-31");

        Assert.Equal("2026-01-01", element.GetAttribute("min"));
        Assert.Equal("2026-12-31", element.GetAttribute("max"));
    }

    #region ------ Internals ------

    // the bound field the input's ValueExpression points at
    private readonly string? text = null;

    // the union of a case value, through the implicit conversion a consumer uses
    private static WaInputBound ToBound(object value) => value switch
    {
        string raw => raw,
        decimal number => number,
        int whole => whole,
        long integer => integer,
        ulong unsigned => unsigned,
        DateOnly date => date,
        TimeOnly time => time,
        DateTime dateTime => dateTime,
        _ => throw new ArgumentException($"No WaInputBound case takes a {value.GetType().Name}.", nameof(value))
    };

    private AngleSharp.Dom.IElement RenderInput(WaInputBound? min, WaInputBound? max)
    {
        var cut = Render<WaInput>(p => p
            .Add(x => x.ValueExpression, () => text)
            .Add(x => x.Min, min)
            .Add(x => x.Max, max));

        return cut.Find("wa-input");
    }

    #endregion
}
#endif
