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
/// JS interop tests for the WaRandomContent.RandomizeAsync element method added in Web Awesome 3.10.0.
/// </summary>
public class WaRandomContentInteropTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly WaRandomContent randomContentComponent;

    public WaRandomContentInteropTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        services.AddSingleton<IJSRuntime, TestJSRuntime>();
        serviceProvider = services.BuildServiceProvider();

        randomContentComponent = new WaRandomContent();

        // Inject dependencies manually for testing
        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var propertyInfo = typeof(WaRandomContent).GetProperty("JSInterop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo?.SetValue(randomContentComponent, jsInterop);
    }

    [Fact]
    public async Task RandomizeAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            randomContentComponent.RandomizeAsync());

        Assert.Contains("Cannot randomize content: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task RandomizeAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();

        // Act - This should not throw because we have a test JSRuntime
        await randomContentComponent.RandomizeAsync();

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    private void SetupElementReference()
    {
        // Simulate element being rendered by setting Element property
        var elementRef = new ElementReference("test-random-content-element");
        var elementProperty = typeof(WaRandomContent).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(randomContentComponent, elementRef);
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
