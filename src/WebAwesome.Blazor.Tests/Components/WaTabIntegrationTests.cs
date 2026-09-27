using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the WaTab focus methods: before the first render they throw, afterwards FocusAsync and
/// BlurAsync invoke focus and blur on the element through the interop module (recorded, see RecordingJSRuntime).
/// </summary>
public class WaTabIntegrationTests : IDisposable
{
    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaTab>().FocusAsync());

        Assert.Contains("Cannot focus tab: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaTab>().BlurAsync());

        Assert.Contains("Cannot blur tab: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task FocusAsync_WithValidElement_InvokesFocus()
    {
        await runtime.CreateRendered<WaTab>().FocusAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("focus"));
    }

    [Fact]
    public async Task BlurAsync_WithValidElement_InvokesBlur()
    {
        await runtime.CreateRendered<WaTab>().BlurAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("blur"));
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
