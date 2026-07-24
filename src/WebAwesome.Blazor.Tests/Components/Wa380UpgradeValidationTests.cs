using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Validates the additive attribute changes that Web Awesome 3.8.0 brought to existing wrappers
/// (WaCard header/footer action SSR flags, WaFileInput capture, WaQrCode centered-image attributes)
/// and the behavioral realignment of WaDrawer's light-dismiss default. Renders each wrapper via bUnit
/// and asserts the emitted markup, complementing the object-level enhancement tests.
/// </summary>
public class Wa380UpgradeValidationTests : BunitContext
{
    public Wa380UpgradeValidationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region ------ WaCard ------

    [Fact]
    public void WaCard_WithHeaderActions_EmitsAttribute()
    {
        var cut = Render<WaCard>(p => p.Add(c => c.WithHeaderActions, true));
        Assert.True(cut.Find("wa-card").HasAttribute("with-header-actions"));
    }

    [Fact]
    public void WaCard_WithFooterActions_EmitsAttribute()
    {
        var cut = Render<WaCard>(p => p.Add(c => c.WithFooterActions, true));
        Assert.True(cut.Find("wa-card").HasAttribute("with-footer-actions"));
    }

    [Fact]
    public void WaCard_FooterActionsContent_ImpliesWithFooterActions()
    {
        var cut = Render<WaCard>(p => p.Add(c => c.FooterActionsContent, (RenderFragment)(b => b.AddContent(0, "x"))));
        Assert.True(cut.Find("wa-card").HasAttribute("with-footer-actions"));
    }

    #endregion

    #region ------ WaFileInput ------

    [Fact]
    public void WaFileInput_Capture_EmitsAttribute()
    {
        var cut = Render<WaFileInput>(p => p.Add(c => c.Capture, WaCaptureMode.Environment));
        Assert.Equal("environment", cut.Find("wa-file-input").GetAttribute("capture"));
    }

    [Fact]
    public void WaFileInput_Capture_DefaultsToUnset()
    {
        var cut = Render<WaFileInput>();
        Assert.False(cut.Find("wa-file-input").HasAttribute("capture"));
    }

    #endregion

    #region ------ WaQrCode ------

    [Fact]
    public void WaQrCode_ImageAttributes_Emit()
    {
        var cut = Render<WaQrCode>(p => p
            .Add(c => c.Value, "https://webawesome.com/")
            .Add(c => c.Image, "/logo.png")
            .Add(c => c.ImageBackground, "white")
            .Add(c => c.ImageCoverage, 0.3m)
            .Add(c => c.ImagePadding, 4));

        var element = cut.Find("wa-qr-code");
        Assert.Equal("/logo.png", element.GetAttribute("image"));
        Assert.Equal("white", element.GetAttribute("image-background"));
        Assert.Equal("0.3", element.GetAttribute("image-coverage"));
        Assert.Equal("4", element.GetAttribute("image-padding"));
    }

    [Fact]
    public void WaQrCode_ImageAttributes_DefaultToUnset()
    {
        var cut = Render<WaQrCode>(p => p.Add(c => c.Value, "https://webawesome.com/"));
        var element = cut.Find("wa-qr-code");
        Assert.False(element.HasAttribute("image"));
        Assert.False(element.HasAttribute("image-coverage"));
        Assert.False(element.HasAttribute("image-padding"));
    }

    #endregion

    #region ------ WaDrawer light-dismiss realignment ------

    [Fact]
    public void WaDrawer_LightDismiss_DefaultsToFalse_AndOmitsAttribute()
    {
        // WA 3.8.0 changed the upstream light-dismiss default from true to false, aligning it with the
        // wrapper's long-standing false default; the attribute is only emitted when explicitly enabled.
        var cut = Render<WaDrawer>();
        Assert.False(cut.Find("wa-drawer").HasAttribute("light-dismiss"));
    }

    [Fact]
    public void WaDrawer_LightDismiss_WhenEnabled_EmitsAttribute()
    {
        var cut = Render<WaDrawer>(p => p.Add(c => c.LightDismiss, true));
        Assert.True(cut.Find("wa-drawer").HasAttribute("light-dismiss"));
    }

    #endregion
}
