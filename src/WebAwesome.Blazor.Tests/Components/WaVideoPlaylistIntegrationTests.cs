using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaVideoPlaylist component introduced in Web Awesome 3.7.0.
/// </summary>
public class WaVideoPlaylistIntegrationTests : BunitContext
{
    public WaVideoPlaylistIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        var cut = Render<WaVideoPlaylist>();

        var element = cut.Find("wa-video-playlist");
        Assert.False(element.HasAttribute("controls"));
        Assert.False(element.HasAttribute("icon-library"));
    }

    [Fact]
    public void Parameters_WhenSet_RenderExpectedAttributes()
    {
        var cut = Render<WaVideoPlaylist>(parameters => parameters
            .Add(p => p.Controls, WaVideoControls.Standard)
            .Add(p => p.IconLibrary, "system"));

        var element = cut.Find("wa-video-playlist");
        Assert.Equal("standard", element.GetAttribute("controls"));
        Assert.Equal("system", element.GetAttribute("icon-library"));
    }

    [Fact]
    public void OnVideoChange_WhenWired_ReceivesEvent()
    {
        WaVideoChangeEventArgs? received = null;
        var cut = Render<WaVideoPlaylist>(parameters => parameters
            .Add(p => p.OnVideoChange, args => received = args));

        var payload = new WaVideoChangeEventArgs { PreviousIndex = 0, CurrentIndex = 2, VideoTitle = "Clip C" };
        cut.Find("wa-video-playlist").TriggerEvent("onwa-video-change", payload);

        Assert.NotNull(received);
        Assert.Equal(0, received!.PreviousIndex);
        Assert.Equal(2, received.CurrentIndex);
        Assert.Equal("Clip C", received.VideoTitle);
    }

    [Fact]
    public async Task NextAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaVideoPlaylist();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.NextAsync());
        Assert.Contains("Cannot invoke 'next': component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GoToAsync_WithRenderedElement_InvokesGoToOnElement()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaVideoPlaylist>();

        await cut.InvokeAsync(() => cut.Instance.GoToAsync(1));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("goTo", invocation.Arguments[1]);
    }

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
}
