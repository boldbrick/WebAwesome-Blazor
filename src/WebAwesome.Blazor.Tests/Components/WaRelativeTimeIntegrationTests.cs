using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaRelativeTime component focusing on validation
/// </summary>
public class WaRelativeTimeIntegrationTests
{
    [Fact]
    public async Task UpdateAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaRelativeTime();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.UpdateAsync());

        Assert.Contains("Cannot update relative time: component has not been rendered yet", exception.Message);
    }
}