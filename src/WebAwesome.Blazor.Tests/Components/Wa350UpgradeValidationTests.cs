using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Validation tests for the Web Awesome 3.5.0 upgrade: the new SSR hydration-hint attributes,
/// the color-picker placement, the copy-button default slot, the wa-rating form-control promotion,
/// and the four non-destructive upstream "breaking" changes.
/// </summary>
public class Wa350UpgradeValidationTests : BunitContext
{
    public Wa350UpgradeValidationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region ------ SSR hydration-hint attributes (rendered) ------

    [Fact]
    public void WaButton_WithStartAndWithEnd_DefaultOmitted_RenderWhenSet()
    {
        var off = Render<WaButton>();
        var button = off.Find("wa-button");
        Assert.False(button.HasAttribute("with-start"));
        Assert.False(button.HasAttribute("with-end"));

        var on = Render<WaButton>(p => p
            .Add(x => x.WithStart, true)
            .Add(x => x.WithEnd, true));
        var wired = on.Find("wa-button");
        Assert.True(wired.HasAttribute("with-start"));
        Assert.True(wired.HasAttribute("with-end"));
    }

    [Fact]
    public void WaDialog_WithFooter_RendersWhenSet()
    {
        Assert.False(Render<WaDialog>().Find("wa-dialog").HasAttribute("with-footer"));
        Assert.True(Render<WaDialog>(p => p.Add(x => x.WithFooter, true)).Find("wa-dialog").HasAttribute("with-footer"));
    }

    [Fact]
    public void WaDrawer_WithFooter_RendersWhenSet()
    {
        Assert.False(Render<WaDrawer>().Find("wa-drawer").HasAttribute("with-footer"));
        Assert.True(Render<WaDrawer>(p => p.Add(x => x.WithFooter, true)).Find("wa-drawer").HasAttribute("with-footer"));
    }

    [Fact]
    public void WaToastItem_WithIcon_RendersWhenSet()
    {
        Assert.False(Render<WaToastItem>().Find("wa-toast-item").HasAttribute("with-icon"));
        Assert.True(Render<WaToastItem>(p => p.Add(x => x.WithIcon, true)).Find("wa-toast-item").HasAttribute("with-icon"));
    }

    [Fact]
    public void WaCopyButton_ChildContent_RendersDefaultSlot()
    {
        var cut = Render<WaCopyButton>(p => p
            .Add(x => x.ChildContent, builder => builder.AddContent(0, "custom-trigger")));

        Assert.Contains("custom-trigger", cut.Find("wa-copy-button").TextContent);
    }

    #endregion

    #region ------ Form-control additions (property-level) ------

    [Fact]
    public void WaColorPicker_HasPlacementParameter()
    {
        var component = new WaColorPicker { Placement = WaPlacement.BottomStart };
        Assert.Equal(WaPlacement.BottomStart, component.Placement);
    }

    [Fact]
    public void WaSlider_WithHintAndWithLabel_DefaultToFalse_AndCanBeSet()
    {
        var component = new WaSlider();
        Assert.False(component.WithHint);
        Assert.False(component.WithLabel);

        component.WithHint = true;
        component.WithLabel = true;
        Assert.True(component.WithHint);
        Assert.True(component.WithLabel);
    }

    [Fact]
    public void WaTextArea_WithCount_DefaultsToFalse_AndCanBeSet()
    {
        var component = new WaTextArea();
        Assert.False(component.WithCount);

        component.WithCount = true;
        Assert.True(component.WithCount);
    }

    [Fact]
    public void WaRating_DefaultValue_DefaultsToZero_AndCanBeSet()
    {
        var component = new WaRating();
        Assert.Equal(0m, component.DefaultValue);

        component.DefaultValue = 3m;
        Assert.Equal(3m, component.DefaultValue);
    }

    #endregion

    #region ------ Non-destructive upstream "breaking" changes ------

    [Fact]
    public void WaRating_KeepsBlurAndFocusMethods_ThoughDroppedFromCem()
    {
        // WA 3.5.0 removed the documented blur()/focus() rating methods; the wrapper keeps
        // BlurAsync/FocusAsync since the native HTMLElement methods remain available
        var type = typeof(WaRating);
        Assert.NotNull(type.GetMethod("BlurAsync", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetMethod("FocusAsync", BindingFlags.Public | BindingFlags.Instance));
    }

    [Fact]
    public void WaTextArea_AutoCorrect_RemainsStringTyped()
    {
        // WA 3.5.0 widened the autocorrect JS property to boolean, but the attribute form is still
        // "off"/"on" - the wrapper keeps AutoCorrect as string?, unchanged
        var property = typeof(WaTextArea).GetProperty("AutoCorrect", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        Assert.Equal(typeof(string), property!.PropertyType);
    }

    #endregion
}
