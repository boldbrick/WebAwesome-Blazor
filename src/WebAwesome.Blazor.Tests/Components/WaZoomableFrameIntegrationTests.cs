using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaZoomableFrame component focusing on validation
/// </summary>
public class WaZoomableFrameIntegrationTests
{
    [Fact]
    public async Task SetZoomAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaZoomableFrame();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.SetZoomAsync(1.5));

        Assert.Contains("Cannot set zoom: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ResetAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaZoomableFrame();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ResetAsync());

        Assert.Contains("Cannot reset zoom: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ZoomInAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaZoomableFrame();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ZoomInAsync());

        Assert.Contains("Cannot zoom in: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ZoomOutAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaZoomableFrame();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ZoomOutAsync());

        Assert.Contains("Cannot zoom out: component has not been rendered yet", exception.Message);
    }
}