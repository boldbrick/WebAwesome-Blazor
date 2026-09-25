using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Imperative method guard clauses of the WaZoomableFrame component: calling a zoom method before the first
/// render must throw. Its attributes and defaults (with-theme-sync, added in WA 3.4.0, included) are covered by
/// RenderedAttributeParityTests, its load/error events by EventCallbackBindingParityTests, both against the CEM.
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