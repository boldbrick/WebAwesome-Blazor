using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the WaAnimation methods: before the first render they throw, afterwards each invokes its
/// element method or reads or writes its element property, with its argument, through the interop module
/// (recorded, see RecordingJSRuntime).
/// </summary>
public class WaAnimationIntegrationTests : IDisposable
{
    [Fact]
    public async Task CancelAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaAnimation>().CancelAsync());

        Assert.Contains("Cannot cancel animation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GetCurrentTimeAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaAnimation>().GetCurrentTimeAsync());

        Assert.Contains("Cannot get current time: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetKeyframesAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaAnimation>().SetKeyframesAsync(new object()));

        Assert.Contains("Cannot set keyframes: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task FinishAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaAnimation>().FinishAsync());

        Assert.Contains("Cannot finish animation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetCurrentTimeAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaAnimation>().SetCurrentTimeAsync(1000m));

        Assert.Contains("Cannot set current time: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task CancelAsync_WithValidElement_InvokesCancel()
    {
        await runtime.CreateRendered<WaAnimation>().CancelAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("cancel"));
    }

    [Fact]
    public async Task FinishAsync_WithValidElement_InvokesFinish()
    {
        await runtime.CreateRendered<WaAnimation>().FinishAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("finish"));
    }

    [Fact]
    public async Task GetCurrentTimeAsync_WithValidElement_ReturnsTheCurrentTimeProperty()
    {
        runtime.Module.NextResult = CurrentTime;

        var result = await runtime.CreateRendered<WaAnimation>().GetCurrentTimeAsync();

        runtime.Module.AssertGotProperty("currentTime");
        Assert.Equal(CurrentTime, result);
    }

    [Fact]
    public async Task SetCurrentTimeAsync_WithValidElement_SetsTheCurrentTimeProperty()
    {
        await runtime.CreateRendered<WaAnimation>().SetCurrentTimeAsync(CurrentTime);

        Assert.Equal(CurrentTime, runtime.Module.AssertSetProperty("currentTime"));
    }

    [Fact]
    public async Task SetKeyframesAsync_WithValidElement_SetsTheKeyframesProperty()
    {
        var keyframes = new[] { new { offset = 0, transform = "rotate(0deg)" } };

        await runtime.CreateRendered<WaAnimation>().SetKeyframesAsync(keyframes);

        Assert.Same(keyframes, runtime.Module.AssertSetProperty("keyframes"));
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const decimal CurrentTime = 1500m;

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
