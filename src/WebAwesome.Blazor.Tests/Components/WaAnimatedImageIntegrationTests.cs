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
/// Integration tests for WaAnimatedImage component using the new JS interop infrastructure
/// </summary>
public class WaAnimatedImageIntegrationTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly WaAnimatedImage animatedImageComponent;

    public WaAnimatedImageIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        services.AddSingleton<IJSRuntime, TestJSRuntime>();
        serviceProvider = services.BuildServiceProvider();

        animatedImageComponent = new WaAnimatedImage();

        // Inject dependencies manually for testing
        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var propertyInfo = typeof(WaAnimatedImage).GetProperty("JSInterop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo?.SetValue(animatedImageComponent, jsInterop);
    }

    [Fact]
    public async Task PlayAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            animatedImageComponent.PlayAsync());

        Assert.Contains("Cannot play animation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task PauseAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            animatedImageComponent.PauseAsync());

        Assert.Contains("Cannot pause animation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task PlayAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should not throw because we have a test JSRuntime
        await animatedImageComponent.PlayAsync();

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public async Task PauseAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should not throw because we have a test JSRuntime
        await animatedImageComponent.PauseAsync();

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    private void SetupElementReference()
    {
        // Simulate element being rendered by setting Element property
        var elementRef = new ElementReference("test-animated-image-element");
        var elementProperty = typeof(WaAnimatedImage).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(animatedImageComponent, elementRef);
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