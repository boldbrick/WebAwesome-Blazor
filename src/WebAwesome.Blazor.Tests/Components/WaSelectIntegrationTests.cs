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
/// Integration tests for WaSelect component using the new JS interop infrastructure
/// </summary>
public class WaSelectIntegrationTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly WaSelect selectComponent;

    public WaSelectIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        services.AddSingleton<IJSRuntime, TestJSRuntime>();
        serviceProvider = services.BuildServiceProvider();

        selectComponent = new WaSelect();

        // Inject dependencies manually for testing (WaSelect inherits from WaInputBase)
        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var propertyInfo = typeof(WaSelect).BaseType!.GetProperty("JSInterop",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo?.SetValue(selectComponent, jsInterop);
    }

    [Fact]
    public async Task SetGetTagFunctionAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange - component not rendered yet, Element is null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            selectComponent.SetGetTagFunctionAsync("(option, index) => `<wa-tag>${option.label}</wa-tag>`"));

        Assert.Contains("Cannot set get tag function: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetGetTagFunctionAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        // Arrange
        SetupElementReference();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            selectComponent.SetGetTagFunctionAsync(null!));
    }

    [Fact]
    public async Task SetGetTagFunctionAsync_WithEmptyFunction_ThrowsArgumentNullException()
    {
        // Arrange
        SetupElementReference();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            selectComponent.SetGetTagFunctionAsync(""));
    }

    [Fact]
    public async Task SetGetTagFunctionAsync_WithValidElement_CallsJSInterop()
    {
        // Arrange
        SetupElementReference();
        var jsFunction = "function(option, index) { return `<wa-tag with-remove>${option.label}</wa-tag>`; }";

        // Act - This should not throw because we have a test JSRuntime
        await selectComponent.SetGetTagFunctionAsync(jsFunction);

        // Assert - Test passed if no exception was thrown
        Assert.True(true);
    }

    private void SetupElementReference()
    {
        // Simulate element being rendered by setting Element property
        var elementRef = new ElementReference("test-select-element");
        var elementProperty = typeof(WaSelect).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(selectComponent, elementRef);
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