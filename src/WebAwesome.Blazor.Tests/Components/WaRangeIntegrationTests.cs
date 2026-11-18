using System;
using System.Linq;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the focus/blur/stepUp/stepDown methods WaRange shares with WaSlider (the interop module is
/// recorded, see RecordingJSRuntime).
/// </summary>
public class WaRangeIntegrationTests : IDisposable
{
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

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
