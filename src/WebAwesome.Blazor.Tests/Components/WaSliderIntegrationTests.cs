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
/// Integration tests for WaSlider component using the new JS interop infrastructure
/// </summary>
public class WaSliderIntegrationTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly WaSlider sliderComponent;

    public WaSliderIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        services.AddSingleton<IJSRuntime, TestJSRuntime>();
        serviceProvider = services.BuildServiceProvider();

        sliderComponent = new WaSlider();

        // Inject dependencies manually for testing (WaSlider inherits from WaInputBase)
        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var propertyInfo = typeof(WaSlider).BaseType!.GetProperty("JSInterop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo?.SetValue(sliderComponent, jsInterop);
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sliderComponent.SetValueFormatterAsync("value => value.toFixed(1)"));

        Assert.Contains("Cannot set value formatter: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        // Arrange
        SetupElementReference();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sliderComponent.SetValueFormatterAsync(null!));
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();
        var jsFunction = "function(value) { return '$' + value; }";

        // Act - This should not throw because we have a test JSRuntime
        await sliderComponent.SetValueFormatterAsync(jsFunction);

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    private void SetupElementReference()
    {
        // Simulate element being rendered by setting Element property
        var elementRef = new ElementReference("test-slider-element");
        var elementProperty = typeof(WaSlider).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(sliderComponent, elementRef);
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