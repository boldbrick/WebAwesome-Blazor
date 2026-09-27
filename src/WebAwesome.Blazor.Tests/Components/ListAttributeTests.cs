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
/// The list-valued attributes render their items exactly as the element splits them, and refuse an item that would
/// split into two (a swatch with ';', a group-by id with a comma or a space, an attribute name with a space) or a
/// number the element cannot read (NaN, infinity), instead of silently sending a different list. The render-based
/// RenderedAttributeParityTests (h) checks the separators and the empty-list rule for every list parameter.
/// </summary>
public class ListAttributeTests : BunitContext
{
    public ListAttributeTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Numbers_RenderInvariant_InListOrder()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);

        var element = Render<WaIntersectionObserver>(p => p.Add(x => x.Threshold, new[] { 1, 0.5, -0.25 })).Find("wa-intersection-observer");

        Assert.Equal("1 0.5 -0.25", element.GetAttribute("threshold"));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void NonFiniteNumbers_AreRefused(double number)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Render<WaSparkline>(p => p.Add(x => x.Data, new[] { 1, number })));
    }

    [Fact]
    public void Swatches_AreSeparatedBySemicolons_AndMayContainCommas()
    {
        var element = Render<WaColorPicker>(p => p
            .Add(x => x.Swatches, new[] { "rgb(1, 2, 3)", "#ff0000" })
            .Add(x => x.ValueExpression, () => color)).Find("wa-color-picker");

        Assert.Equal("rgb(1, 2, 3);#ff0000", element.GetAttribute("swatches"));
        Assert.Throws<ArgumentException>(() => Render<WaColorPicker>(p => p
            .Add(x => x.Swatches, new[] { "#fff;#000" })
            .Add(x => x.ValueExpression, () => color)));
    }

    [Theory]
    [InlineData("region,country")]
    [InlineData("region country")]
    [InlineData(" ")]
    public void GroupByIds_ThatWouldSplit_AreRefused(string id)
    {
        Assert.Throws<ArgumentException>(() => Render<WaDataGrid>(p => p.Add(x => x.GroupBy, new[] { "team", id })));
    }

    [Fact]
    public void AttributeFilter_RendersOrdinalOrder_AndFallsBackToAttrWhenEmpty()
    {
        var filtered = Render<WaMutationObserver>(p => p.Add(x => x.AttributeFilter, new HashSet<string> { "title", "class", "Id" })).Find("wa-mutation-observer");
        var empty = Render<WaMutationObserver>(p => p
            .Add(x => x.AttributeFilter, new HashSet<string>())
            .Add(x => x.Attr, true)).Find("wa-mutation-observer");

        Assert.Equal("Id class title", filtered.GetAttribute("attr"));
        Assert.Equal("*", empty.GetAttribute("attr"));
        Assert.Throws<ArgumentException>(() => Render<WaMutationObserver>(p => p.Add(x => x.AttributeFilter, new HashSet<string> { "data-a data-b" })));
    }

    [Fact]
    public void FlipFallbackPlacements_KeepTheirOrder()
    {
        var element = Render<WaPopup>(p => p
            .Add(x => x.Flip, true)
            .Add(x => x.FlipFallbackPlacements, new[] { WaPlacement.RightEnd, WaPlacement.Bottom })).Find("wa-popup");

        Assert.Equal("right-end bottom", element.GetAttribute("flip-fallback-placements"));
    }

    #region ------ Internals ------

    // the bound field the color picker's ValueExpression points at
    private readonly string? color = null;

    #endregion
}
