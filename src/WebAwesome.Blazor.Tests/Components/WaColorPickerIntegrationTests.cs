using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaColorPicker component using the new JS interop infrastructure
/// </summary>
public class WaColorPickerIntegrationTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly WaColorPicker colorPickerComponent;

    public WaColorPickerIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        services.AddSingleton<IJSRuntime, TestJSRuntime>();
        serviceProvider = services.BuildServiceProvider();

        colorPickerComponent = new WaColorPicker();

        // Inject dependencies manually for testing (WaColorPicker inherits from WaInputBase)
        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var propertyInfo = typeof(WaColorPicker).BaseType!.GetProperty("JSInterop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo?.SetValue(colorPickerComponent, jsInterop);
    }

    [Fact]
    public async Task SetSwatchesAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            colorPickerComponent.SetSwatchesAsync(new[] { "#ff0000", "#00ff00" }));

        Assert.Contains("Cannot set swatches: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetSwatchesAsync_WithNullColors_ThrowsArgumentNullException()
    {
        // Arrange
        SetupElementReference();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            colorPickerComponent.SetSwatchesAsync(null!));
    }

    [Fact]
    public async Task SetSwatchesAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();
        var colors = new[] { "#ff0000", "#00ff00", "#0000ff" };

        // Act - This should not throw because we have a test JSRuntime
        await colorPickerComponent.SetSwatchesAsync(colors);

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public async Task GetFormattedValueAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            colorPickerComponent.GetFormattedValueAsync(WaColorFormat.Rgb));

        Assert.Contains("Cannot get formatted value: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GetFormattedValueAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should return default string (null) from test JSRuntime
        var result = await colorPickerComponent.GetFormattedValueAsync(WaColorFormat.Hex);

        // Assert - Test passed if no exception was thrown (result may be null from test runtime)
        Assert.True(true);
    }

    [Fact]
    public async Task GetFormattedValueAsync_WithDifferentFormats_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act & Assert - Test all format types
        await colorPickerComponent.GetFormattedValueAsync(WaColorFormat.Hex);
        await colorPickerComponent.GetFormattedValueAsync(WaColorFormat.Rgb);
        await colorPickerComponent.GetFormattedValueAsync(WaColorFormat.Hsl);
        await colorPickerComponent.GetFormattedValueAsync(WaColorFormat.Hsv);

        // Test passed if no exceptions were thrown
        Assert.True(true);
    }

    private void SetupElementReference()
    {
        // Simulate element being rendered by setting Element property
        var elementRef = new ElementReference("test-color-picker-element");
        var elementProperty = typeof(WaColorPicker).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(colorPickerComponent, elementRef);
    }

    #region ------ Test JSRuntime ------

    private class TestJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            // Simulate module import returning a mock module
            if (identifier == "import")
            {
                return ValueTask.FromResult((TValue)(object)new TestJSObjectReference());
            }

            throw new NotImplementedException($"Test runtime does not implement {identifier}");
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            return InvokeAsync<TValue>(identifier, args);
        }
    }

    private class TestJSObjectReference : IJSObjectReference
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            // Return default values for testing
            return ValueTask.FromResult(default(TValue)!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            return InvokeAsync<TValue>(identifier, args);
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    #endregion

    public void Dispose()
    {
        serviceProvider?.Dispose();
    }
}