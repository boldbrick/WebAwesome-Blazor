using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaDrawer component focusing on validation
/// </summary>
public class WaDrawerIntegrationTests
{
    [Fact]
    public async Task ShowAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaDrawer();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ShowAsync());

        Assert.Contains("Cannot show drawer: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaDrawer();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.HideAsync());

        Assert.Contains("Cannot hide drawer: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaDrawer();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.FocusAsync());

        Assert.Contains("Cannot focus drawer: component has not been rendered yet", exception.Message);
    }
}