using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using Xunit;

namespace WebAwesome.Blazor.Tests.Base;

/// <summary>
/// Validation tests for WebAwesome JavaScript interop service focusing on parameter validation
/// </summary>
public class WebAwesomeJSInteropValidationTests
{
    private readonly TestJSRuntime testJSRuntime;
    private readonly WebAwesomeJSInterop jsInterop;
    private readonly ElementReference validElement;
    private readonly ElementReference invalidElement;

    public WebAwesomeJSInteropValidationTests()
    {
        testJSRuntime = new TestJSRuntime();
        jsInterop = new WebAwesomeJSInterop(testJSRuntime);
        validElement = new ElementReference("test-element-id");
        invalidElement = new ElementReference(); // Invalid element with null ID
    }

    #region ------ Constructor Tests ------

    [Fact]
    public void Constructor_WithNullJSRuntime_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new WebAwesomeJSInterop(null!));
    }

    #endregion

    #region ------ SetCustomValidity Validation Tests ------

    [Fact]
    public async Task SetCustomValidityAsync_WithInvalidElement_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            jsInterop.SetCustomValidityAsync(invalidElement, "test"));

        Assert.Contains("Element reference is not valid", exception.Message);
    }

    #endregion

    #region ------ InvokeMethodAsync Validation Tests ------

    [Fact]
    public async Task InvokeMethodAsync_WithInvalidElement_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            jsInterop.InvokeMethodAsync<string>(invalidElement, "testMethod"));

        Assert.Contains("Element reference is not valid", exception.Message);
    }

    [Fact]
    public async Task InvokeMethodAsync_WithNullMethodName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.InvokeMethodAsync<string>(validElement, null!));
    }

    [Fact]
    public async Task InvokeMethodAsync_WithEmptyMethodName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.InvokeMethodAsync<string>(validElement, ""));
    }

    [Fact]
    public async Task InvokeMethodAsync_VoidOverload_WithInvalidElement_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            jsInterop.InvokeMethodAsync(invalidElement, "testMethod"));

        Assert.Contains("Element reference is not valid", exception.Message);
    }

    [Fact]
    public async Task InvokeMethodAsync_VoidOverload_WithNullMethodName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.InvokeMethodAsync(validElement, null!));
    }

    #endregion

    #region ------ SetPropertyAsync Validation Tests ------

    [Fact]
    public async Task SetPropertyAsync_WithInvalidElement_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            jsInterop.SetPropertyAsync(invalidElement, "testProperty", "value"));

        Assert.Contains("Element reference is not valid", exception.Message);
    }

    [Fact]
    public async Task SetPropertyAsync_WithNullPropertyName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.SetPropertyAsync(validElement, null!, "value"));
    }

    [Fact]
    public async Task SetPropertyAsync_WithEmptyPropertyName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.SetPropertyAsync(validElement, "", "value"));
    }

    #endregion

    #region ------ GetPropertyAsync Validation Tests ------

    [Fact]
    public async Task GetPropertyAsync_WithInvalidElement_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            jsInterop.GetPropertyAsync<string>(invalidElement, "testProperty"));

        Assert.Contains("Element reference is not valid", exception.Message);
    }

    [Fact]
    public async Task GetPropertyAsync_WithNullPropertyName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.GetPropertyAsync<string>(validElement, null!));
    }

    [Fact]
    public async Task GetPropertyAsync_WithEmptyPropertyName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.GetPropertyAsync<string>(validElement, ""));
    }

    #endregion

    #region ------ Test JSRuntime ------

    /// <summary>
    /// Simple test JS runtime that mimics module loading for testing
    /// </summary>
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

    /// <summary>
    /// Test JS object reference for testing module loading
    /// </summary>
    private class TestJSObjectReference : IJSObjectReference
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            // For testing, just return default values
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
}