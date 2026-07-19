using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new JS-interop methods added in Web Awesome 3.0.0: <see cref="WaInput.SetRangeTextAsync"/>,
/// <see cref="WaTextArea.SetRangeTextAsync"/>, <see cref="WaTextArea.ScrollPositionAsync"/>,
/// and the newly-added <see cref="WaRating.BlurAsync"/>/<see cref="WaRating.FocusAsync"/> methods
/// </summary>
public class WaNewJSInteropMethodsTests : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly RecordingJSObjectReference recordingModule;

    public WaNewJSInteropMethodsTests()
    {
        var services = new ServiceCollection();
        services.AddWebAwesome();
        recordingModule = new RecordingJSObjectReference();
        services.AddSingleton<IJSRuntime>(new RecordingJSRuntime(recordingModule));
        serviceProvider = services.BuildServiceProvider();
    }

    #region ------ WaInput.SetRangeTextAsync Argument Shape ------

    [Fact]
    public async Task WaInput_SetRangeTextAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.SetRangeTextAsync("replacement"));

        Assert.Contains("component is rendered", exception.Message);
    }

    [Fact]
    public async Task WaInput_SetRangeTextAsync_WithStartAndEnd_Passes4Args()
    {
        // Arrange
        var component = CreateRenderedComponent<WaInput>();

        // Act
        await component.SetRangeTextAsync("hi", 1, 3, "select");

        // Assert
        Assert.Equal("setRangeText", recordingModule.LastMethodName);
        Assert.Equal(4, recordingModule.LastArgs.Length);
        Assert.Equal("hi", recordingModule.LastArgs[0]);
        Assert.Equal(1, recordingModule.LastArgs[1]);
        Assert.Equal(3, recordingModule.LastArgs[2]);
        Assert.Equal("select", recordingModule.LastArgs[3]);
    }

    [Fact]
    public async Task WaInput_SetRangeTextAsync_WithoutStartOrEnd_PassesOnly1Arg()
    {
        // Arrange
        var component = CreateRenderedComponent<WaInput>();

        // Act
        await component.SetRangeTextAsync("hi");

        // Assert
        Assert.Equal("setRangeText", recordingModule.LastMethodName);
        Assert.Single(recordingModule.LastArgs);
        Assert.Equal("hi", recordingModule.LastArgs[0]);
    }

    #endregion

    #region ------ WaTextArea.SetRangeTextAsync Argument Shape ------

    [Fact]
    public async Task WaTextArea_SetRangeTextAsync_WithStartAndEnd_Passes4Args()
    {
        // Arrange
        var component = CreateRenderedComponent<WaTextArea>();

        // Act
        await component.SetRangeTextAsync("hi", 0, 2, WaTextAreaSelectMode.End);

        // Assert
        Assert.Equal("setRangeText", recordingModule.LastMethodName);
        Assert.Equal(4, recordingModule.LastArgs.Length);
        Assert.Equal("hi", recordingModule.LastArgs[0]);
        Assert.Equal(0, recordingModule.LastArgs[1]);
        Assert.Equal(2, recordingModule.LastArgs[2]);
        Assert.Equal("end", recordingModule.LastArgs[3]);
    }

    [Fact]
    public async Task WaTextArea_SetRangeTextAsync_WithoutStartOrEnd_PassesOnly1Arg()
    {
        // Arrange
        var component = CreateRenderedComponent<WaTextArea>();

        // Act
        await component.SetRangeTextAsync("hi");

        // Assert
        Assert.Equal("setRangeText", recordingModule.LastMethodName);
        Assert.Single(recordingModule.LastArgs);
        Assert.Equal("hi", recordingModule.LastArgs[0]);
    }

    [Fact]
    public async Task WaTextArea_SetRangeTextAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaTextArea();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.SetRangeTextAsync("hi"));

        Assert.Contains("Cannot set range text: component has not been rendered yet", exception.Message);
    }

    #endregion

    #region ------ WaTextArea.ScrollPositionAsync ------

    [Fact]
    public async Task WaTextArea_ScrollPositionAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaTextArea();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ScrollPositionAsync());

        Assert.Contains("Cannot get or set scroll position: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task WaTextArea_ScrollPositionAsync_WithoutArguments_QueriesCurrentPosition()
    {
        // Arrange
        var component = CreateRenderedComponent<WaTextArea>();
        recordingModule.NextResult = new WaTextAreaScrollPosition(10, 20);

        // Act
        var result = await component.ScrollPositionAsync();

        // Assert
        Assert.Equal("scrollPosition", recordingModule.LastMethodName);
        Assert.Empty(recordingModule.LastArgs);
        Assert.NotNull(result);
        Assert.Equal(10, result!.Top);
        Assert.Equal(20, result.Left);
    }

    [Fact]
    public async Task WaTextArea_ScrollPositionAsync_WithArguments_PassesTopAndLeft()
    {
        // Arrange
        var component = CreateRenderedComponent<WaTextArea>();

        // Act
        await component.ScrollPositionAsync(top: 5, left: 15);

        // Assert
        Assert.Equal("scrollPosition", recordingModule.LastMethodName);
        Assert.Single(recordingModule.LastArgs);
    }

    #endregion

    #region ------ WaTextArea Selection Enums ------

    [Fact]
    public void WaTextAreaSelectMode_ToHtmlValue_ReturnsCorrectStrings()
    {
        // Assert
        Assert.Equal("select", WaTextAreaSelectMode.Select.ToHtmlValue());
        Assert.Equal("start", WaTextAreaSelectMode.Start.ToHtmlValue());
        Assert.Equal("end", WaTextAreaSelectMode.End.ToHtmlValue());
        Assert.Equal("preserve", WaTextAreaSelectMode.Preserve.ToHtmlValue());
    }

    [Fact]
    public void WaTextAreaSelectionDirection_ToHtmlValue_ReturnsCorrectStrings()
    {
        // Assert
        Assert.Equal("none", WaTextAreaSelectionDirection.None.ToHtmlValue());
        Assert.Equal("forward", WaTextAreaSelectionDirection.Forward.ToHtmlValue());
        Assert.Equal("backward", WaTextAreaSelectionDirection.Backward.ToHtmlValue());
    }

    #endregion

    #region ------ WaRating.BlurAsync / FocusAsync ------

    [Fact]
    public async Task WaRating_BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaRating();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.BlurAsync());

        Assert.Contains("Cannot blur: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task WaRating_FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaRating();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.FocusAsync());

        Assert.Contains("Cannot focus: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task WaRating_BlurAsync_WithValidElement_InvokesBlur()
    {
        // Arrange
        var component = CreateRenderedComponent<WaRating>();

        // Act
        await component.BlurAsync();

        // Assert
        Assert.Equal("blur", recordingModule.LastMethodName);
    }

    [Fact]
    public async Task WaRating_FocusAsync_WithValidElement_InvokesFocus()
    {
        // Arrange
        var component = CreateRenderedComponent<WaRating>();

        // Act
        await component.FocusAsync();

        // Assert
        Assert.Equal("focus", recordingModule.LastMethodName);
    }

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// Creates a component instance, injects the test JS interop service, and simulates the element having been
    /// rendered by directly setting the "Element" property via reflection
    /// </summary>
    private T CreateRenderedComponent<T>() where T : new()
    {
        var component = new T();

        var jsInterop = serviceProvider.GetRequiredService<WebAwesomeJSInterop>();
        var jsInteropProperty = typeof(T).GetProperty("JSInterop", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? typeof(T).BaseType?.GetProperty("JSInterop", BindingFlags.NonPublic | BindingFlags.Instance);
        jsInteropProperty?.SetValue(component, jsInterop);

        var elementProperty = typeof(T).GetProperty("Element", BindingFlags.Public | BindingFlags.Instance);
        elementProperty?.SetValue(component, new ElementReference("test-element"));

        return component;
    }

    #endregion

    #region ------ Test JSRuntime / JSObjectReference ------

    private class RecordingJSRuntime : IJSRuntime
    {
        private readonly RecordingJSObjectReference module;

        public RecordingJSRuntime(RecordingJSObjectReference module)
        {
            this.module = module;
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (identifier == "import")
            {
                return ValueTask.FromResult((TValue)(object)module);
            }

            throw new NotImplementedException($"Test runtime does not implement {identifier}");
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            return InvokeAsync<TValue>(identifier, args);
        }
    }

    /// <summary>
    /// Records the last "invokeMethod" call, capturing the method name and the trailing args array so that tests
    /// can assert on the exact argument shape passed by the wrapper
    /// </summary>
    private class RecordingJSObjectReference : IJSObjectReference
    {
        public string? LastMethodName { get; private set; }
        public object[] LastArgs { get; private set; } = Array.Empty<object>();
        public object? NextResult { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (identifier == "invokeMethod" && args is { Length: 3 })
            {
                LastMethodName = args[1] as string;
                LastArgs = args[2] as object[] ?? Array.Empty<object>();
            }

            if (NextResult is not null)
            {
                return ValueTask.FromResult((TValue)NextResult);
            }

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
