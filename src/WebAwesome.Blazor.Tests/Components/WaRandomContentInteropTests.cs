using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the WaRandomContent.RandomizeAsync element method added in Web Awesome 3.10.0: before the
/// first render it throws, afterwards it invokes randomize through the interop module (recorded, see
/// RecordingJSRuntime).
/// </summary>
public class WaRandomContentInteropTests : IDisposable
{
    [Fact]
    public async Task RandomizeAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaRandomContent>().RandomizeAsync());

        Assert.Contains("Cannot randomize content: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task RandomizeAsync_WithValidElement_InvokesRandomize()
    {
        await runtime.CreateRendered<WaRandomContent>().RandomizeAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("randomize"));
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
