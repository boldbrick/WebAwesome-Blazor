using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaDetails component focusing on validation
/// </summary>
public class WaDetailsIntegrationTests
{
    [Fact]
    public async Task ShowAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaDetails();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ShowAsync());

        Assert.Contains("Cannot show details: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaDetails();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.HideAsync());

        Assert.Contains("Cannot hide details: component has not been rendered yet", exception.Message);
    }
}