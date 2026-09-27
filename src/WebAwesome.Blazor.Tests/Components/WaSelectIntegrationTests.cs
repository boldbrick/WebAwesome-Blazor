using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for WaSelect.SetGetTagFunctionAsync: it rejects a missing function, throws before the first
/// render, and afterwards sets the element's getTag property to the function through the interop module
/// (recorded, see RecordingJSRuntime).
/// </summary>
public class WaSelectIntegrationTests : IDisposable
{
    [Fact]
    public async Task SetGetTagFunctionAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaSelect>().SetGetTagFunctionAsync(TagFunction));

        Assert.Contains("Cannot set get tag function: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetGetTagFunctionAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaSelect>().SetGetTagFunctionAsync(null!));
    }

    [Fact]
    public async Task SetGetTagFunctionAsync_WithEmptyFunction_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaSelect>().SetGetTagFunctionAsync(string.Empty));
    }

    [Fact]
    public async Task SetGetTagFunctionAsync_WithValidElement_SetsTheGetTagProperty()
    {
        await runtime.CreateRendered<WaSelect>().SetGetTagFunctionAsync(TagFunction);

        Assert.Equal(TagFunction, runtime.Module.AssertSetProperty("getTag"));
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const string TagFunction = "function(option, index) { return `<wa-tag with-remove>${option.label}</wa-tag>`; }";

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
