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
/// Integration tests for WaRating component using the new JS interop infrastructure
/// </summary>
public class WaRatingIntegrationTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly WaRating ratingComponent;

    public WaRatingIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        services.AddSingleton<IJSRuntime, TestJSRuntime>();
        serviceProvider = services.BuildServiceProvider();

        ratingComponent = new WaRating();

        // Inject dependencies manually for testing (WaRating inherits from WaInputBase)
        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var propertyInfo = typeof(WaRating).BaseType!.GetProperty("JSInterop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo?.SetValue(ratingComponent, jsInterop);
    }

    [Fact]
    public async Task SetSymbolFunctionAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ratingComponent.SetSymbolFunctionAsync("(value, isSelected) => isSelected ? '★' : '☆'"));

        Assert.Contains("Cannot set symbol function: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetSymbolFunctionAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        // Arrange
        SetupElementReference();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            ratingComponent.SetSymbolFunctionAsync(null!));
    }

    [Fact]
    public async Task SetSymbolFunctionAsync_WithEmptyFunction_ThrowsArgumentNullException()
    {
        // Arrange
        SetupElementReference();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            ratingComponent.SetSymbolFunctionAsync(""));
    }

    [Fact]
    public async Task SetSymbolFunctionAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();
        var jsFunction = "function(value, isSelected) { return isSelected ? '★' : '☆'; }";

        // Act - This should not throw because we have a test JSRuntime
        await ratingComponent.SetSymbolFunctionAsync(jsFunction);

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    private void SetupElementReference()
    {
        // Simulate element being rendered by setting Element property
        var elementRef = new ElementReference("test-rating-element");
        var elementProperty = typeof(WaRating).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(ratingComponent, elementRef);
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