using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for WaRating.SetSymbolFunctionAsync: it rejects a missing function, throws before the first
/// render, and afterwards sets the element's getSymbol property to the function through the interop module
/// (recorded, see RecordingJSRuntime).
/// </summary>
public class WaRatingIntegrationTests : IDisposable
{
    [Fact]
    public async Task SetSymbolFunctionAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaRating>().SetSymbolFunctionAsync(SymbolFunction));

        Assert.Contains("Cannot set symbol function: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetSymbolFunctionAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaRating>().SetSymbolFunctionAsync(null!));
    }

    [Fact]
    public async Task SetSymbolFunctionAsync_WithEmptyFunction_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaRating>().SetSymbolFunctionAsync(string.Empty));
    }

    [Fact]
    public async Task SetSymbolFunctionAsync_WithValidElement_SetsTheGetSymbolProperty()
    {
        await runtime.CreateRendered<WaRating>().SetSymbolFunctionAsync(SymbolFunction);

        Assert.Equal(SymbolFunction, runtime.Module.AssertSetProperty("getSymbol"));
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const string SymbolFunction = "function(value, isSelected) { return isSelected ? '★' : '☆'; }";

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
