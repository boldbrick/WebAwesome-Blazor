using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaInclude component focusing on validation
/// </summary>
public class WaIncludeIntegrationTests
{
    [Fact]
    public async Task ReloadAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaInclude();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ReloadAsync());

        Assert.Contains("Cannot reload content: component has not been rendered yet", exception.Message);
    }
}