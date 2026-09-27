using System;
using System.Linq;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for WaRange.SetValueFormatterAsync: it rejects a missing function, throws before the first
/// render, and afterwards sets the element's valueFormatter property to the function through the interop module
/// (recorded, see RecordingJSRuntime); and for the focus/blur/stepUp/stepDown methods it shares with WaSlider.
/// </summary>
public class WaRangeIntegrationTests : IDisposable
{
    [Fact]
    public async Task SetValueFormatterAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaRange>().SetValueFormatterAsync(Formatter));

        Assert.Contains("Cannot set value formatter: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaRange>().SetValueFormatterAsync(null!));
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithEmptyFunction_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaRange>().SetValueFormatterAsync(string.Empty));
    }

    [Fact]
    public async Task SetValueFormatterAsync_WithValidElement_SetsTheValueFormatterProperty()
    {
        await runtime.CreateRendered<WaRange>().SetValueFormatterAsync(Formatter);

        Assert.Equal(Formatter, runtime.Module.AssertSetProperty("valueFormatter"));
    }

    [Theory]
    [MemberData(nameof(ElementMethods))]
    public async Task ElementMethods_WithValidElement_InvokeTheElementMethod(string methodName, Func<WaRange, Task> invoke)
    {
        // Act - WaRange gained these from WaSliderBase in 3.12.0 (WaSlider had them, WaRange did not)
        await invoke(runtime.CreateRendered<WaRange>());

        // Assert
        Assert.Empty(runtime.Module.AssertInvokedMethod(methodName));
    }

    [Theory]
    [MemberData(nameof(ElementMethods))]
    public async Task ElementMethods_WithNullElement_ThrowInvalidOperationException(string methodName, Func<WaRange, Task> invoke)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => invoke(runtime.CreateUnrendered<WaRange>()));

        // Assert - thrown before any interop call, so the element method was not invoked
        Assert.Contains("component has not been rendered yet", exception.Message);
        Assert.DoesNotContain(runtime.Module.Invocations, i => i.Args.Contains(methodName));
    }

    /// <summary>
    /// The element methods WaRange invokes, with the wrapper call invoking each.
    /// </summary>
    public static TheoryData<string, Func<WaRange, Task>> ElementMethods => new()
    {
        { "focus", range => range.FocusAsync() },
        { "blur", range => range.BlurAsync() },
        { "stepUp", range => range.StepUpAsync() },
        { "stepDown", range => range.StepDownAsync() },
    };

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const string Formatter = "function(value) { return value + '%'; }";

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
