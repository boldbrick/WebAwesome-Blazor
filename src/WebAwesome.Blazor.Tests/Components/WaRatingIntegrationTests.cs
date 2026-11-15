using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

// the obsolete string methods stay covered until they are removed in the 3.15.0 train
#pragma warning disable CS0618

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the obsolete WaRating.SetSymbolFunctionAsync: it rejects a missing function, throws before the
/// first render, and afterwards throws NotSupportedException naming GetSymbolFunction without sending the JavaScript
/// source text, which wa-rating cannot call (the interop module
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
    public async Task SetSymbolFunctionAsync_WithValidElement_ThrowsNotSupportedAndSendsNothing()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
            runtime.CreateRendered<WaRating>().SetSymbolFunctionAsync(SymbolFunction));

        Assert.Contains(nameof(WaRating.GetSymbolFunction), exception.Message);
        Assert.Contains(RemovalTrain, exception.Message);
        Assert.Empty(runtime.Module.Invocations);
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const string RemovalTrain = "3.15.0";
    private const string SymbolFunction = "function(value, isSelected) { return isSelected ? '★' : '☆'; }";

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
