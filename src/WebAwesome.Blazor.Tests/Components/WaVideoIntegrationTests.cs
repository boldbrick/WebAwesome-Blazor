using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaVideo player introduced in Web Awesome 3.7.0.
/// </summary>
public class WaVideoIntegrationTests : BunitContext
{
    public WaVideoIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        var cut = Render<WaVideo>();

        var element = cut.Find("wa-video");
        Assert.False(element.HasAttribute("controls"));
        Assert.False(element.HasAttribute("preload"));
        Assert.False(element.HasAttribute("src"));
        Assert.False(element.HasAttribute("volume"));
        Assert.False(element.HasAttribute("playing"));
        Assert.False(element.HasAttribute("muted"));
        Assert.False(element.HasAttribute("autoplay"));
    }

    [Fact]
    public void Parameters_WhenSet_RenderExpectedAttributes()
    {
        var cut = Render<WaVideo>(parameters => parameters
            .Add(p => p.Controls, WaVideoControls.Full)
            .Add(p => p.Preload, WaVideoPreload.Auto)
            .Add(p => p.Src, "movie.mp4")
            .Add(p => p.Poster, "poster.jpg")
            .Add(p => p.Title, "My Video")
            .Add(p => p.Muted, true)
            .Add(p => p.Autoplay, true)
            .Add(p => p.Volume, 0.5));

        var element = cut.Find("wa-video");
        Assert.Equal("full", element.GetAttribute("controls"));
        Assert.Equal("auto", element.GetAttribute("preload"));
        Assert.Equal("movie.mp4", element.GetAttribute("src"));
        Assert.Equal("poster.jpg", element.GetAttribute("poster"));
        Assert.Equal("My Video", element.GetAttribute("title"));
        Assert.True(element.HasAttribute("muted"));
        Assert.True(element.HasAttribute("autoplay"));
        // volume must use invariant formatting (decimal point, never a locale comma)
        Assert.Equal("0.5", element.GetAttribute("volume"));
    }

    [Fact]
    public void PosterIconName_RendersWaIconIntoSlot_WhenNoFragment()
    {
        var cut = Render<WaVideo>(parameters => parameters
            .Add(p => p.PosterIconName, "circle-play"));

        var icon = cut.Find("wa-icon[slot='poster-icon']");
        Assert.Equal("circle-play", icon.GetAttribute("name"));
    }

    [Fact]
    public void PosterIcon_FragmentWins_OverIconName()
    {
        var cut = Render<WaVideo>(parameters => parameters
            .Add(p => p.PosterIconName, "circle-play")
            .Add(p => p.PosterIcon, builder => builder.AddContent(0, "custom")));

        Assert.Equal("custom", cut.Find("span[slot='poster-icon']").TextContent);
        Assert.Empty(cut.FindAll("wa-icon[slot='poster-icon']"));
    }

    [Fact]
    public void NativeMediaEvents_WhenWired_ReceiveDomEvents()
    {
        var play = 0;
        var pause = 0;
        var ended = 0;
        var timeUpdate = 0;
        var volumeChange = 0;
        var loadedMetadata = 0;
        var error = 0;
        var cut = Render<WaVideo>(parameters => parameters
            .Add(p => p.OnPlay, () => play++)
            .Add(p => p.OnPause, () => pause++)
            .Add(p => p.OnEnded, () => ended++)
            .Add(p => p.OnTimeUpdate, () => timeUpdate++)
            .Add(p => p.OnVolumeChange, () => volumeChange++)
            .Add(p => p.OnLoadedMetadata, () => loadedMetadata++)
            .Add(p => p.OnError, () => error++));

        var element = cut.Find("wa-video");
        element.TriggerEvent("onplay", new EventArgs());
        element.TriggerEvent("onpause", new EventArgs());
        element.TriggerEvent("onended", new EventArgs());
        element.TriggerEvent("ontimeupdate", new EventArgs());
        element.TriggerEvent("onvolumechange", new EventArgs());
        element.TriggerEvent("onloadedmetadata", new EventArgs());
        element.TriggerEvent("onerror", new EventArgs());

        Assert.Equal(1, play);
        Assert.Equal(1, pause);
        Assert.Equal(1, ended);
        Assert.Equal(1, timeUpdate);
        Assert.Equal(1, volumeChange);
        Assert.Equal(1, loadedMetadata);
        Assert.Equal(1, error);
    }

    [Fact]
    public async Task PlayAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaVideo();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.PlayAsync());
        Assert.Contains("Cannot invoke 'play': component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GetStateAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaVideo();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.GetStateAsync());
        Assert.Contains("Cannot get state: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task PlayAsync_WithRenderedElement_InvokesPlayOnElement()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaVideo>();

        await cut.InvokeAsync(() => cut.Instance.PlayAsync());

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("play", invocation.Arguments[1]);
    }

    [Fact]
    public async Task SeekAsync_WithRenderedElement_InvokesSeekWithTime()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaVideo>();

        await cut.InvokeAsync(() => cut.Instance.SeekAsync(12.5));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("seek", invocation.Arguments[1]);
    }

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
}
