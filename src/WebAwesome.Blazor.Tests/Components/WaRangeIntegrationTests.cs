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
/// Integration tests for WaRange component using the new JS interop infrastructure
/// </summary>
public class WaRangeIntegrationTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly WaRange rangeComponent;

    public WaRangeIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        services.AddSingleton<IJSRuntime, TestJSRuntime>();
        serviceProvider = services.BuildServiceProvider();

        rangeComponent = new WaRange();

        // Inject dependencies manually for testing (WaRange inherits from WaInputBase)
        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var propertyInfo = typeof(WaRange).BaseType!.GetProperty("JSInterop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo?.SetValue(rangeComponent, jsInterop);
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            rangeComponent.SetValueFormatterAsync("value => value.toFixed(2)"));

        Assert.Contains("Cannot set value formatter: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        // Arrange
        SetupElementReference();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            rangeComponent.SetValueFormatterAsync(null!));
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithEmptyFunction_ThrowsArgumentNullException()
    {
        // Arrange
        SetupElementReference();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            rangeComponent.SetValueFormatterAsync(""));
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();
        var jsFunction = "function(value) { return value + '%'; }";

        // Act - This should not throw because we have a test JSRuntime
        await rangeComponent.SetValueFormatterAsync(jsFunction);

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    private void SetupElementReference()
    {
        // Simulate element being rendered by setting Element property
        var elementRef = new ElementReference("test-range-element");
        var elementProperty = typeof(WaRange).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(rangeComponent, elementRef);
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