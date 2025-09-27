using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaPopover component focusing on validation
/// </summary>
public class WaPopoverIntegrationTests
{
    [Fact]
    public async Task ShowAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaPopover();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ShowAsync());

        Assert.Contains("Cannot show popover: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaPopover();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.HideAsync());

        Assert.Contains("Cannot hide popover: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task RepositionAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaPopover();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.RepositionAsync());

        Assert.Contains("Cannot reposition popover: component has not been rendered yet", exception.Message);
    }
}