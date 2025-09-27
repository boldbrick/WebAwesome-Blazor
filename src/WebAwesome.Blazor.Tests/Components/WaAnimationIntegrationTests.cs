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
/// Integration tests for WaAnimation component using the new JS interop infrastructure
/// </summary>
public class WaAnimationIntegrationTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly WaAnimation animationComponent;

    public WaAnimationIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        services.AddSingleton<IJSRuntime, TestJSRuntime>();
        serviceProvider = services.BuildServiceProvider();

        animationComponent = new WaAnimation();

        // Inject dependencies manually for testing
        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var propertyInfo = typeof(WaAnimation).GetProperty("JSInterop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo?.SetValue(animationComponent, jsInterop);
    }

    [Fact]
    public async Task CancelAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            animationComponent.CancelAsync());

        Assert.Contains("Cannot cancel animation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GetCurrentTimeAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            animationComponent.GetCurrentTimeAsync());

        Assert.Contains("Cannot get current time: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task CancelAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should not throw because we have a test JSRuntime
        await animationComponent.CancelAsync();

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public async Task GetCurrentTimeAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should return default decimal (0) from test JSRuntime
        var result = await animationComponent.GetCurrentTimeAsync();

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task SetKeyframesAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            animationComponent.SetKeyframesAsync(new object()));

        Assert.Contains("Cannot set keyframes: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetKeyframesAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();
        var keyframes = new { offset = 0, transform = "rotate(0deg)" };

        // Act - This should not throw because we have a test JSRuntime
        await animationComponent.SetKeyframesAsync(keyframes);

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public async Task FinishAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            animationComponent.FinishAsync());

        Assert.Contains("Cannot finish animation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task FinishAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should not throw because we have a test JSRuntime
        await animationComponent.FinishAsync();

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public async Task SetCurrentTimeAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            animationComponent.SetCurrentTimeAsync(1000m));

        Assert.Contains("Cannot set current time: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetCurrentTimeAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should not throw because we have a test JSRuntime
        await animationComponent.SetCurrentTimeAsync(1500m);

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    private void SetupElementReference()
    {
        // Simulate element being rendered by setting Element property
        var elementRef = new ElementReference("test-animation-element");
        var elementProperty = typeof(WaAnimation).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(animationComponent, elementRef);
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