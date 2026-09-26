using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The sticky attribute rule on the owner's examples: a parameter left at its default renders nothing, but once its
/// attribute was rendered, returning to the default (or to null) renders the element default instead of removing the
/// attribute, which would make Lit set the element property to null (a slider's max 0, an input's type null). Exempt:
/// boolean attributes (a removed Lit boolean reads false, the default) and attributes without a literal default. The
/// browser half is sticky-attributes.spec.js; RenderedAttributeParityTests (i) checks every wrapper parameter.
/// </summary>
public class StickyAttributeTests : BunitContext
{
    public StickyAttributeTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void SliderMax_BackToTheDefault_RendersTheDefault()
    {
        var cut = Render<WaSlider>(p => p.Add(x => x.ValueExpression, () => sliderValue));
        Assert.False(cut.Find("wa-slider").HasAttribute("max"));

        cut.Render(p => p.Add(x => x.Max, 50m));
        Assert.Equal("50", cut.Find("wa-slider").GetAttribute("max"));

        cut.Render(p => p.Add(x => x.Max, WaSlider.DefaultMax));
        Assert.Equal("100", cut.Find("wa-slider").GetAttribute("max"));
    }

    [Fact]
    public void InputType_BackToText_RendersText()
    {
        var cut = Render<WaInput>(p => p.Add(x => x.ValueExpression, () => textValue));
        Assert.False(cut.Find("wa-input").HasAttribute("type"));

        cut.Render(p => p.Add(x => x.Type, WaInputType.Password));
        Assert.Equal("password", cut.Find("wa-input").GetAttribute("type"));

        cut.Render(p => p.Add(x => x.Type, WaInput.DefaultType));
        Assert.Equal("text", cut.Find("wa-input").GetAttribute("type"));
    }

    [Fact]
    public void NullableDistance_BackToNull_RendersTheElementDefault()
    {
        var cut = Render<WaTooltip>(p => p.Add(x => x.For, "anchor"));
        Assert.False(cut.Find("wa-tooltip").HasAttribute("distance"));

        cut.Render(p => p.Add(x => x.Distance, 20));
        Assert.Equal("20", cut.Find("wa-tooltip").GetAttribute("distance"));

        cut.Render(p => p.Add(x => x.Distance, (int?)null));
        Assert.Equal("8", cut.Find("wa-tooltip").GetAttribute("distance"));
    }

    [Fact]
    public void ExplicitDefault_OnTheFirstRender_RendersNothing()
    {
        var cut = Render<WaSlider>(p => p
            .Add(x => x.ValueExpression, () => sliderValue)
            .Add(x => x.Max, WaSlider.DefaultMax));

        Assert.False(cut.Find("wa-slider").HasAttribute("max"));
    }

    [Fact]
    public void BooleanAndDefaultlessAttributes_AreRemoved()
    {
        var cut = Render<WaInput>(p => p
            .Add(x => x.ValueExpression, () => textValue)
            .Add(x => x.Pill, true)
            .Add(x => x.Pattern, "[0-9]+"));

        cut.Render(p => p
            .Add(x => x.Pill, false)
            .Add(x => x.Pattern, (string?)null));

        var element = cut.Find("wa-input");
        Assert.False(element.HasAttribute("pill"));
        Assert.False(element.HasAttribute("pattern"));
    }

    #region ------ Internals ------

    // the bound fields the ValueExpressions point at
    private readonly decimal? sliderValue = null;
    private readonly string? textValue = null;

    #endregion
}
