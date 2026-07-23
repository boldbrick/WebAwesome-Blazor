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

        // Assert - Size, Radius, and ErrorCorrection keep their pre-existing non-nullable defaults
        var element = cut.Find("wa-qr-code");
        Assert.Equal("128", element.GetAttribute("size"));
        Assert.Equal("0", element.GetAttribute("radius"));
        Assert.Equal("M", element.GetAttribute("error-correction"));
    }
}
