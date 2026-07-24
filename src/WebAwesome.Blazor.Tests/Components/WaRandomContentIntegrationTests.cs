using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaRandomContent component added in Web Awesome 3.10.0.
/// </summary>
public class WaRandomContentIntegrationTests : BunitContext
{
    public WaRandomContentIntegrationTests()
    {
        // WaRandomContent injects WebAwesomeJSInterop (for RandomizeAsync); register it so the
        // component can render inside the bUnit context
        Services.AddScoped<WebAwesomeJSInterop>();
    }

    [Fact]
    public void DefaultRender_EmitsElementWithDefaultAttributes()
    {
        // Arrange & Act
        var cut = Render<WaRandomContent>();

        // Assert - non-nullable defaults are always emitted (items=1, mode=unique, animation=none, interval=3000)
        var element = cut.Find("wa-random-content");
        Assert.Equal("1", element.GetAttribute("items"));
        Assert.Equal("unique", element.GetAttribute("mode"));
        Assert.Equal("none", element.GetAttribute("animation"));
        Assert.Equal("3000", element.GetAttribute("autoplay-interval"));
        // autoplay is a bool attribute, false by default -> Blazor omits it
        Assert.False(element.HasAttribute("autoplay"));
    }

    [Theory]
    [InlineData(WaRandomContentMode.Random, "random")]
    [InlineData(WaRandomContentMode.Unique, "unique")]
    [InlineData(WaRandomContentMode.Sequence, "sequence")]
    public void Mode_MapsToHtmlValue(WaRandomContentMode mode, string expected)
    {
        // Arrange & Act
        var cut = Render<WaRandomContent>(parameters => parameters.Add(p => p.Mode, mode));

        // Assert
        Assert.Equal(expected, cut.Find("wa-random-content").GetAttribute("mode"));
    }

    [Theory]
    [InlineData(WaRandomContentAnimation.None, "none")]
    [InlineData(WaRandomContentAnimation.Fade, "fade")]
    [InlineData(WaRandomContentAnimation.FadeUp, "fade-up")]
    [InlineData(WaRandomContentAnimation.FadeDown, "fade-down")]
    [InlineData(WaRandomContentAnimation.FadeLeft, "fade-left")]
    [InlineData(WaRandomContentAnimation.FadeRight, "fade-right")]
    public void Animation_MapsToKebabCaseHtmlValue(WaRandomContentAnimation animation, string expected)
    {
        // Arrange & Act
        var cut = Render<WaRandomContent>(parameters => parameters.Add(p => p.Animation, animation));

        // Assert
        Assert.Equal(expected, cut.Find("wa-random-content").GetAttribute("animation"));
    }

    [Fact]
    public void ItemsAndAutoplay_RenderConfiguredValues()
    {
        // Arrange & Act
        var cut = Render<WaRandomContent>(parameters => parameters
            .Add(p => p.Items, 3)
            .Add(p => p.Autoplay, true)
            .Add(p => p.AutoplayInterval, 5000));

        // Assert
        var element = cut.Find("wa-random-content");
        Assert.Equal("3", element.GetAttribute("items"));
        // autoplay is a bool attribute, true -> Blazor renders it present (empty value)
        Assert.True(element.HasAttribute("autoplay"));
        Assert.Equal("5000", element.GetAttribute("autoplay-interval"));
    }

    [Fact]
    public void ChildContent_RendersInsidePool()
    {
        // Arrange & Act
        var cut = Render<WaRandomContent>(parameters => parameters
            .AddChildContent("<span class=\"pool-item\">one</span><span class=\"pool-item\">two</span>"));

        // Assert
        Assert.Equal(2, cut.FindAll("wa-random-content .pool-item").Count);
    }

    [Fact]
    public void OnContentChange_WhenSet_RendersWithoutError()
    {
        // Arrange & Act - binding the callback must render the element cleanly
        var cut = Render<WaRandomContent>(parameters => parameters
            .Add(p => p.OnContentChange, EventCallback.Factory.Create<WaContentChangeEventArgs>(this, _ => { })));

        // Assert
        Assert.NotNull(cut.Find("wa-random-content"));
    }
}
