using Bunit;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaIcon Animation, Flip, and Rotate parameters added in Web Awesome 3.2.0.
/// </summary>
public class WaIconAnimationIntegrationTests : BunitContext
{
    [Fact]
    public void DefaultRender_OmitsAnimationAndFlipButAlwaysEmitsRotate()
    {
        // Arrange & Act
        var cut = Render<WaIcon>(parameters => parameters.Add(p => p.Name, "star"));

        // Assert - rotate always renders (int, default 0); animation/flip are nullable and omitted when unset
        var element = cut.Find("wa-icon");
        Assert.False(element.HasAttribute("animation"));
        Assert.False(element.HasAttribute("flip"));
        Assert.Equal("0", element.GetAttribute("rotate"));
    }

    [Theory]
    [InlineData(WaIconAnimation.Beat, "beat")]
    [InlineData(WaIconAnimation.Fade, "fade")]
    [InlineData(WaIconAnimation.BeatFade, "beat-fade")]
    [InlineData(WaIconAnimation.Bounce, "bounce")]
    [InlineData(WaIconAnimation.Flip, "flip")]
    [InlineData(WaIconAnimation.Shake, "shake")]
    [InlineData(WaIconAnimation.Spin, "spin")]
    [InlineData(WaIconAnimation.SpinPulse, "spin-pulse")]
    [InlineData(WaIconAnimation.SpinReverse, "spin-reverse")]
    public void Animation_MapsToKebabCaseHtmlValue(WaIconAnimation animation, string expected)
    {
        // Arrange & Act
        var cut = Render<WaIcon>(parameters => parameters.Add(p => p.Animation, animation));

        // Assert
        Assert.Equal(expected, cut.Find("wa-icon").GetAttribute("animation"));
    }

    [Theory]
    [InlineData(WaFlip.X, "x")]
    [InlineData(WaFlip.Y, "y")]
    [InlineData(WaFlip.Both, "both")]
    public void Flip_MapsToHtmlValue(WaFlip flip, string expected)
    {
        // Arrange & Act
        var cut = Render<WaIcon>(parameters => parameters.Add(p => p.Flip, flip));

        // Assert
        Assert.Equal(expected, cut.Find("wa-icon").GetAttribute("flip"));
    }

    [Fact]
    public void Rotate_WhenSet_RendersNonZeroDegreeValue()
    {
        // Arrange & Act
        var cut = Render<WaIcon>(parameters => parameters.Add(p => p.Rotate, 90));

        // Assert
        Assert.Equal("90", cut.Find("wa-icon").GetAttribute("rotate"));
    }

    [Fact]
    public void AnimationAndFlipAndRotate_CanBeCombined()
    {
        // Arrange & Act
        var cut = Render<WaIcon>(parameters => parameters
            .Add(p => p.Animation, WaIconAnimation.SpinReverse)
            .Add(p => p.Flip, WaFlip.Both)
            .Add(p => p.Rotate, 180));

        // Assert
        var element = cut.Find("wa-icon");
        Assert.Equal("spin-reverse", element.GetAttribute("animation"));
        Assert.Equal("both", element.GetAttribute("flip"));
        Assert.Equal("180", element.GetAttribute("rotate"));
    }
}
