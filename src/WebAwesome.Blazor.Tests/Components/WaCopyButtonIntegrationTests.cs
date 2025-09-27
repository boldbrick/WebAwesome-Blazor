using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaCopyButton component focusing on validation
/// </summary>
public class WaCopyButtonIntegrationTests
{
    [Fact]
    public async Task CopyAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaCopyButton();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.CopyAsync());

        Assert.Contains("Cannot copy: component has not been rendered yet", exception.Message);
    }
}