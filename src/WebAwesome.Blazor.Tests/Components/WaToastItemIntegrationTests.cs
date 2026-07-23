using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaToastItem notification introduced in Web Awesome 3.3.0.
/// </summary>
public class WaToastItemIntegrationTests : BunitContext
{
    public WaToastItemIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        var cut = Render<WaToastItem>();

        var element = cut.Find("wa-toast-item");
        Assert.False(element.HasAttribute("duration"));
        Assert.False(element.HasAttribute("size"));
        Assert.False(element.HasAttribute("variant"));
    }

    [Fact]
    public void Parameters_WhenSet_RenderExpectedAttributes()
    {
        var cut = Render<WaToastItem>(parameters => parameters
            .Add(p => p.Duration, 0)
            .Add(p => p.Size, WaSize.Large)
            .Add(p => p.Variant, WaVariant.Success));

        var element = cut.Find("wa-toast-item");
        Assert.Equal("0", element.GetAttribute("duration"));
        Assert.Equal("large", element.GetAttribute("size"));
        Assert.Equal("success", element.GetAttribute("variant"));
    }

    [Fact]
    public void IconContent_RendersIntoIconSlot()
    {
        var cut = Render<WaToastItem>(parameters => parameters
            .Add(p => p.IconContent, builder => builder.AddContent(0, "bell")));

        Assert.Equal("bell", cut.Find("span[slot='icon']").TextContent);
    }

    [Fact]
    public void IconName_RendersWaIconIntoIconSlot_WhenNoFragment()
    {
        var cut = Render<WaToastItem>(parameters => parameters
            .Add(p => p.IconName, "circle-check"));

        var icon = cut.Find("wa-icon[slot='icon']");
        Assert.Equal("circle-check", icon.GetAttribute("name"));
    }

    [Fact]
    public void Events_WhenWired_ReceiveDomEvents()
    {
        var showCount = 0;
        var afterShowCount = 0;
        var hideCount = 0;
        var afterHideCount = 0;
        var cut = Render<WaToastItem>(parameters => parameters
            .Add(p => p.OnShow, () => showCount++)
            .Add(p => p.OnAfterShow, () => afterShowCount++)
            .Add(p => p.OnHide, () => hideCount++)
            .Add(p => p.OnAfterHide, () => afterHideCount++));

        var element = cut.Find("wa-toast-item");
        element.TriggerEvent("onwa-show", new EventArgs());
        element.TriggerEvent("onwa-after-show", new EventArgs());
        element.TriggerEvent("onwa-hide", new EventArgs());
        element.TriggerEvent("onwa-after-hide", new EventArgs());

        Assert.Equal(1, showCount);
        Assert.Equal(1, afterShowCount);
        Assert.Equal(1, hideCount);
        Assert.Equal(1, afterHideCount);
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaToastItem();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.HideAsync());
        Assert.Contains("Cannot hide the toast item before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task HideAsync_WithRenderedElement_InvokesHideOnElement()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaToastItem>();

        await cut.InvokeAsync(() => cut.Instance.HideAsync());

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("hide", invocation.Arguments[1]);
    }

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
}
