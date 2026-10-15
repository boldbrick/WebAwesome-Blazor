using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

// the obsolete string methods stay covered until they are removed in the 3.15.0 train
#pragma warning disable CS0618

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the obsolete WaSelect.SetGetTagFunctionAsync: it rejects a missing function, throws before the
/// first render, and afterwards throws NotSupportedException naming GetTagFunction without sending the JavaScript source
/// text, which wa-select cannot call (the interop module is recorded, see RecordingJSRuntime).
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
    public async Task SetGetTagFunctionAsync_WithValidElement_ThrowsNotSupportedAndSendsNothing()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
            runtime.CreateRendered<WaSelect>().SetGetTagFunctionAsync(TagFunction));

        Assert.Contains(nameof(WaSelect.GetTagFunction), exception.Message);
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
    private const string TagFunction = "function(option, index) { return `<wa-tag with-remove>${option.label}</wa-tag>`; }";

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
