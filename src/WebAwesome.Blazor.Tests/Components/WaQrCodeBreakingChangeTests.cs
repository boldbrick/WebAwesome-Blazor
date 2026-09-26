using Bunit;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Render-level validation for the WaQrCode Fill/Background breaking change in Web Awesome 3.2.0:
/// upstream changed both attribute defaults from "black"/"white" to '' (inherit from the current
/// theme), so the wrapper's Fill and Background parameters became nullable with a null default and
/// are only emitted when explicitly set.
/// </summary>
public class WaQrCodeBreakingChangeTests : BunitContext
{
    [Fact]
    public void FillAndBackground_DefaultToNullAndAreNotEmitted()
    {
        // Arrange & Act
        var cut = Render<WaQrCode>(parameters => parameters.Add(p => p.Value, "https://example.com"));

        // Assert
        var element = cut.Find("wa-qr-code");
        Assert.False(element.HasAttribute("fill"));
        Assert.False(element.HasAttribute("background"));
    }

    [Fact]
    public void Fill_WhenSet_IsEmitted()
    {
        // Arrange & Act
        var cut = Render<WaQrCode>(parameters => parameters
            .Add(p => p.Value, "https://example.com")
            .Add(p => p.Fill, "#ff0000"));

        // Assert
        Assert.Equal("#ff0000", cut.Find("wa-qr-code").GetAttribute("fill"));
    }

    [Fact]
    public void Background_WhenSet_IsEmitted()
    {
        // Arrange & Act
        var cut = Render<WaQrCode>(parameters => parameters
            .Add(p => p.Value, "https://example.com")
            .Add(p => p.Background, "transparent"));

        // Assert
        Assert.Equal("transparent", cut.Find("wa-qr-code").GetAttribute("background"));
    }

    [Fact]
    public void UnrelatedDefaults_AreUnaffectedByTheBreakingChange()
    {
        // Arrange & Act
        var cut = Render<WaQrCode>(parameters => parameters.Add(p => p.Value, "https://example.com"));

        // Assert - Size and Radius keep their non-nullable defaults, which are the element's own and render nothing; ErrorCorrection
        // is unset since 3.12.0, so the element's default (H) applies
        var element = cut.Find("wa-qr-code");
        Assert.False(element.HasAttribute("size"));
        Assert.False(element.HasAttribute("radius"));
        Assert.False(element.HasAttribute("error-correction"));
    }
}
