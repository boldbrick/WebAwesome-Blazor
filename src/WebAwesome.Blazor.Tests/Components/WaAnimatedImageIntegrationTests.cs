using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the WaAnimatedImage methods: before the first render they throw, afterwards they set the
/// element's play property to the right value through the interop module (recorded, see RecordingJSRuntime).
/// </summary>
public class WaAnimatedImageIntegrationTests : IDisposable
{
    [Fact]
    public async Task PlayAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaAnimatedImage>().PlayAsync());

        Assert.Contains("Cannot play animation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task PauseAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaAnimatedImage>().PauseAsync());

        Assert.Contains("Cannot pause animation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task PlayAsync_WithValidElement_SetsPlayTrue()
    {
        await runtime.CreateRendered<WaAnimatedImage>().PlayAsync();

        Assert.Equal(true, runtime.Module.AssertSetProperty("play"));
    }

    [Fact]
    public async Task PauseAsync_WithValidElement_SetsPlayFalse()
    {
        await runtime.CreateRendered<WaAnimatedImage>().PauseAsync();

        Assert.Equal(false, runtime.Module.AssertSetProperty("play"));
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
