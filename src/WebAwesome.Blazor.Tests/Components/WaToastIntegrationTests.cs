using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaToast stack introduced in Web Awesome 3.3.0.
/// </summary>
public class WaToastIntegrationTests : BunitContext
{
    public WaToastIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DefaultRender_OmitsPlacement()
    {
        var cut = Render<WaToast>();

        Assert.False(cut.Find("wa-toast").HasAttribute("placement"));
    }

    [Theory]
    [InlineData(WaToastPlacement.TopStart, "top-start")]
    [InlineData(WaToastPlacement.TopCenter, "top-center")]
    [InlineData(WaToastPlacement.TopEnd, "top-end")]
    [InlineData(WaToastPlacement.BottomStart, "bottom-start")]
    [InlineData(WaToastPlacement.BottomCenter, "bottom-center")]
    [InlineData(WaToastPlacement.BottomEnd, "bottom-end")]
    public void Placement_MapsToHtmlValue(WaToastPlacement placement, string expected)
    {
        var cut = Render<WaToast>(parameters => parameters.Add(p => p.Placement, placement));

        Assert.Equal(expected, cut.Find("wa-toast").GetAttribute("placement"));
    }

    [Fact]
    public void ChildContent_RendersToastItems()
    {
        var cut = Render<WaToast>(parameters => parameters
            .AddChildContent<WaToastItem>(itemParams => itemParams
                .AddChildContent("Saved!")));

        Assert.NotNull(cut.Find("wa-toast wa-toast-item"));
        Assert.Contains("Saved!", cut.Markup);
    }

    [Fact]
    public async Task CreateAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaToast();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.CreateAsync("hi"));
        Assert.Contains("Cannot create a toast before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithRenderedElement_InvokesCreateOnElement()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaToast>();

        await cut.InvokeAsync(() => cut.Instance.CreateAsync("Hello world"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("create", invocation.Arguments[1]);
    }

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
}
