using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Validation tests for the Web Awesome 3.5.0 upgrade: the new SSR hydration-hint attributes, the copy-button
/// default slot, and the non-destructive upstream "breaking" changes the wrapper absorbs. The form-control
/// attributes 3.5.0 added (wa-color-picker placement, wa-slider with-hint/with-label, wa-textarea with-count,
/// wa-rating default-value) are covered, defaults included, by the CEM-driven RenderedAttributeParityTests.
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
