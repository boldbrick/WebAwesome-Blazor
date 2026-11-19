using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the new WaNumberTicker component (Web Awesome 3.15.0): RestartAsync throws before the first
/// render and afterwards invokes the element's restart method through the interop module (recorded, see
/// RecordingJSRuntime). The ValueFormatterFunction push is covered by ElementJsPropertyTests, attributes and
/// defaults by RenderedAttributeParityTests.
/// </summary>
public class WaNumberTickerIntegrationTests : IDisposable
{
    [Fact]
    public async Task RestartAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange & Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaNumberTicker>().RestartAsync());

        // Assert
        Assert.Contains("Cannot restart number ticker: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task RestartAsync_WithValidElement_InvokesRestart()
    {
        // Arrange
        var ticker = runtime.CreateRendered<WaNumberTicker>();

        // Act
        await ticker.RestartAsync();

        // Assert
        Assert.Empty(runtime.Module.AssertInvokedMethod("restart"));
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
