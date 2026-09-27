using System;
using System.Collections.Generic;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// WaInput's min and max (CEM number | string, passed to the native input unchanged): the raw string and each typed
/// accessor (decimal, long, ulong, DateOnly, TimeOnly, DateTime) render the form the native input reads, culture-free,
/// and setting two accessors of one bound throws. Until C# 15 union types (net11.0) replace them.
/// </summary>
public class WaInputBoundsTests : BunitContext
{
    public WaInputBoundsTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Each accessor with a value and the attribute text it must render.
    /// </summary>
    public static TheoryData<string, object, string> Accessors => new()
    {
        { "", "2026-01-02", "2026-01-02" },
        { "Decimal", -1234.5m, "-1234.5" },
        { "Long", -9_000_000_000L, "-9000000000" },
        { "ULong", 18_000_000_000_000_000_000UL, "18000000000000000000" },
        { "Date", new DateOnly(2026, 3, 4), "2026-03-04" },
        { "Time", new TimeOnly(13, 4), "13:04" },
        { "Time", new TimeOnly(13, 4, 5), "13:04:05" },
        { "DateTime", new DateTime(2026, 3, 4, 13, 4, 0), "2026-03-04T13:04" },
        { "DateTime", new DateTime(2026, 3, 4, 13, 4, 5, DateTimeKind.Utc), "2026-03-04T13:04:05" },
        { "DateTime", new DateTime(2026, 3, 4, 13, 4, 5, 250), "2026-03-04T13:04:05.250" },
    };

    [Theory]
    [MemberData(nameof(Accessors))]
    public void EveryAccessor_RendersTheNativeForm_InAnyCulture(string suffix, object value, string expected)
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);

        var element = RenderInput(new Dictionary<string, object?> { [MinName + suffix] = value, [MaxName + suffix] = value });

        Assert.Equal(expected, element.GetAttribute("min"));
        Assert.Equal(expected, element.GetAttribute("max"));
    }

    [Theory]
    [InlineData(MinName)]
    [InlineData(MaxName)]
    public void TwoAccessorsOfOneBound_Throw(string bound)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => RenderInput(new Dictionary<string, object?>
        {
            [bound + "Decimal"] = 1m,
            [bound + "Date"] = new DateOnly(2026, 1, 1)
        }));

        Assert.Contains($"{bound}Decimal and {bound}Date", exception.Message);
    }

    [Fact]
    public void OneAccessorPerBound_IsFine()
    {
        var element = RenderInput(new Dictionary<string, object?> { ["MinDate"] = new DateOnly(2026, 1, 1), ["Max"] = "2026-12-31" });

        Assert.Equal("2026-01-01", element.GetAttribute("min"));
        Assert.Equal("2026-12-31", element.GetAttribute("max"));
    }

    #region ------ Internals ------

    private const string MinName = "Min";
    private const string MaxName = "Max";

    // the bound field the input's ValueExpression points at
    private readonly string? text = null;

    private AngleSharp.Dom.IElement RenderInput(IReadOnlyDictionary<string, object?> bounds)
    {
        var cut = Render<WaInput>(p =>
        {
            p.Add(x => x.ValueExpression, () => text);
            foreach (var (name, value) in bounds)
                p.TryAdd(name, value);
        });

        return cut.Find("wa-input");
    }

    #endregion
}
