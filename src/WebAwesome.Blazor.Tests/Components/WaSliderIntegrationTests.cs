using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

// the obsolete string methods stay covered until they are removed in the 3.15.0 train
#pragma warning disable CS0618

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the obsolete WaSlider.SetValueFormatterAsync: it rejects a missing function, throws before the first
/// render, and afterwards sends nothing, because wa-slider ignores JavaScript source text and a string would replace a
/// ValueFormatterFunction (the interop module is recorded, see RecordingJSRuntime).
/// </summary>
public class WaSliderIntegrationTests : IDisposable
{
    [Fact]
    public async Task SetValueFormatterAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaSlider>().SetValueFormatterAsync(Formatter));

        Assert.Contains("Cannot set value formatter: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaSlider>().SetValueFormatterAsync(null!));
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithValidElement_SendsNothing()
    {
        await runtime.CreateRendered<WaSlider>().SetValueFormatterAsync(Formatter);

        Assert.Empty(runtime.Module.Invocations);
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const string Formatter = "function(value) { return '$' + value; }";

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
