using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for WaSlider.SetValueFormatterAsync: it rejects a missing function, throws before the first
/// render, and afterwards sets the element's valueFormatter property to the function through the interop module
/// (recorded, see RecordingJSRuntime).
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
    public async Task SetValueFormatterAsync_WithValidElement_SetsTheValueFormatterProperty()
    {
        await runtime.CreateRendered<WaSlider>().SetValueFormatterAsync(Formatter);

        Assert.Equal(Formatter, runtime.Module.AssertSetProperty("valueFormatter"));
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
