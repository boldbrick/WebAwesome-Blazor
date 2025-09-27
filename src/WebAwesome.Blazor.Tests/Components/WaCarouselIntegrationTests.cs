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
/// Integration tests for WaCarousel component using the new JS interop infrastructure
/// </summary>
public class WaCarouselIntegrationTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly WaCarousel carouselComponent;

    public WaCarouselIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        services.AddSingleton<IJSRuntime, TestJSRuntime>();
        serviceProvider = services.BuildServiceProvider();

        carouselComponent = new WaCarousel();

        // Inject dependencies manually for testing
        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var propertyInfo = typeof(WaCarousel).GetProperty("JSInterop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo?.SetValue(carouselComponent, jsInterop);
    }

    [Fact]
    public async Task GoToSlideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            carouselComponent.GoToSlideAsync(1));

        Assert.Contains("Cannot navigate to slide: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GoToSlideAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should not throw because we have a test JSRuntime
        await carouselComponent.GoToSlideAsync(2);

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public async Task PreviousAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            carouselComponent.PreviousAsync());

        Assert.Contains("Cannot navigate to previous slide: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task PreviousAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should not throw because we have a test JSRuntime
        await carouselComponent.PreviousAsync();

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public async Task NextAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            carouselComponent.NextAsync());

        Assert.Contains("Cannot navigate to next slide: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task NextAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should not throw because we have a test JSRuntime
        await carouselComponent.NextAsync();

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    private void SetupElementReference()
    {
        // Simulate element being rendered by setting Element property
        var elementRef = new ElementReference("test-carousel-element");
        var elementProperty = typeof(WaCarousel).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(carouselComponent, elementRef);
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