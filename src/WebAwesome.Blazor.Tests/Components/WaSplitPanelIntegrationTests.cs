using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaSplitPanel component focusing on validation
/// </summary>
public class WaSplitPanelIntegrationTests
{
    [Fact]
    public async Task GetPositionAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaSplitPanel();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.GetPositionAsync());

        Assert.Contains("Cannot get position: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GetPositionInPixelsAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaSplitPanel();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.GetPositionInPixelsAsync());

        Assert.Contains("Cannot get position in pixels: component has not been rendered yet", exception.Message);
    }
}