using System;
using System.Threading;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Base;

/// <summary>
/// Integration tests for WebAwesomeJSInterop icon library methods
/// </summary>
public class WebAwesomeJSInteropIconTests
{
    [Fact]
    public async Task RegisterIconLibraryAsync_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());
        var options = new IconLibraryOptions();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.RegisterIconLibraryAsync(null!, options));
    }

    [Fact]
    public async Task RegisterIconLibraryAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.RegisterIconLibraryAsync("test", null!));
    }

    [Fact]
    public async Task UnregisterIconLibraryAsync_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.UnregisterIconLibraryAsync(null!));
    }

    [Fact]
    public async Task SetDefaultIconFamilyAsync_WithNullFamily_ThrowsArgumentNullException()
    {
        // Arrange
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.SetDefaultIconFamilyAsync(null!));
    }

    private class MockJSRuntime : Microsoft.JSInterop.IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            throw new NotImplementedException();
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            throw new NotImplementedException();
        }
    }
}