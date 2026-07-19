using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaMutationObserver component focusing on validation
/// </summary>
public class WaMutationObserverIntegrationTests
{
    [Fact]
    public async Task DisconnectAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaMutationObserver();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.DisconnectAsync());

        Assert.Contains("Cannot disconnect observer: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ReconnectAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaMutationObserver();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ReconnectAsync());

        Assert.Contains("Cannot reconnect observer: component has not been rendered yet", exception.Message);
    }
}