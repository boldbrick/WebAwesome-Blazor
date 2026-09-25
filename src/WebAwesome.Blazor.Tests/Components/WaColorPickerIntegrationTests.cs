using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the WaColorPicker swatch and formatting methods: before the first render they throw,
/// afterwards SetSwatchesAsync sets the swatches property and GetFormattedValueAsync invokes getFormattedValue
/// with the format's JS name and returns its result, through the interop module (recorded, see RecordingJSRuntime).
/// </summary>
public class WaColorPickerIntegrationTests : IDisposable
{
    [Fact]
    public async Task SetSwatchesAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaColorPicker>().SetSwatchesAsync(new[] { "#ff0000", "#00ff00" }));

        Assert.Contains("Cannot set swatches: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetSwatchesAsync_WithNullColors_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaColorPicker>().SetSwatchesAsync(null!));
    }

    [Fact]
    public async Task GetFormattedValueAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaColorPicker>().GetFormattedValueAsync(WaColorFormat.Rgb));

        Assert.Contains("Cannot get formatted value: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetSwatchesAsync_WithValidElement_SetsTheSwatchesProperty()
    {
        var colors = new[] { "#ff0000", "#00ff00", "#0000ff" };

        await runtime.CreateRendered<WaColorPicker>().SetSwatchesAsync(colors);

        Assert.Same(colors, runtime.Module.AssertSetProperty("swatches"));
    }

    [Theory]
    [InlineData(WaColorFormat.Hex, "hex")]
    [InlineData(WaColorFormat.Rgb, "rgb")]
    [InlineData(WaColorFormat.Hsl, "hsl")]
    [InlineData(WaColorFormat.Hsv, "hsv")]
    public async Task GetFormattedValueAsync_WithValidElement_InvokesGetFormattedValueWithTheFormat(WaColorFormat format, string expected)
    {
        runtime.Module.NextResult = FormattedValue;

        var result = await runtime.CreateRendered<WaColorPicker>().GetFormattedValueAsync(format);

        Assert.Equal(new object[] { expected }, runtime.Module.AssertInvokedMethod("getFormattedValue"));
        Assert.Equal(FormattedValue, result);
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const string FormattedValue = "rgb(255, 0, 0)";

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
