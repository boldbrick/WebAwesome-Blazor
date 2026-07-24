using Bunit;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaIcon Canvas parameter added in Web Awesome 3.10.0.
/// </summary>
public class WaIconCanvasIntegrationTests : BunitContext
{
    [Fact]
    public void DefaultRender_OmitsCanvas()
    {
        // Arrange & Act
        var cut = Render<WaIcon>(parameters => parameters.Add(p => p.Name, "star"));

        // Assert - Canvas is nullable and omitted when unset (unset renders as the fixed box upstream)
        Assert.False(cut.Find("wa-icon").HasAttribute("canvas"));
    }

    [Theory]
    [InlineData(WaIconCanvas.Fixed, "fixed")]
    [InlineData(WaIconCanvas.Auto, "auto")]
    [InlineData(WaIconCanvas.Square, "square")]
    [InlineData(WaIconCanvas.Roomy, "roomy")]
    public void Canvas_MapsToHtmlValue(WaIconCanvas canvas, string expected)
    {
        // Arrange & Act
        var cut = Render<WaIcon>(parameters => parameters
            .Add(p => p.Name, "star")
            .Add(p => p.Canvas, canvas));

        // Assert
        Assert.Equal(expected, cut.Find("wa-icon").GetAttribute("canvas"));
    }
}
