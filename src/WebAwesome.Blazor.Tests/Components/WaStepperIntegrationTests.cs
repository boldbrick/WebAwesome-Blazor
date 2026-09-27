using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Render tests for WaStepper: its ChildContent renders WaStep children inside wa-stepper. Its attributes
/// and defaults are covered by RenderedAttributeParityTests and its events by EventCallbackBindingParityTests,
/// both against the CEM; there is no C#-side mapping of OnBeforeStepChange/OnStepChange beyond direct binding, so
/// no TriggerEvent test is added for them here.
/// </summary>
public class WaStepperIntegrationTests : BunitContext
{
    public WaStepperIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ChildContent_RendersStepperStepChildrenInsideTheStepper()
    {
        var cut = Render<WaStepper>(parameters => parameters
            .Add(p => p.ChildContent, StepsFragment));

        var steps = cut.FindAll("wa-stepper > wa-step");
        Assert.Equal(2, steps.Count);
        Assert.Equal(FirstStepName, steps[0].GetAttribute("name"));
        Assert.Equal(SecondStepName, steps[1].GetAttribute("name"));
    }

    #region ------ Internals ------

    private const string FirstStepName = "first";
    private const string SecondStepName = "second";

    private static readonly RenderFragment StepsFragment = builder =>
    {
        builder.OpenComponent<WaStep>(0);
        builder.AddComponentParameter(1, nameof(WaStep.Name), FirstStepName);
        builder.CloseComponent();

        builder.OpenComponent<WaStep>(2);
        builder.AddComponentParameter(3, nameof(WaStep.Name), SecondStepName);
        builder.CloseComponent();
    };

    #endregion
}

/// <summary>
/// Interop tests for the WaStepper navigation methods: before the first render they throw, GoToAsync also rejects
/// a null or empty name, and afterwards each invokes its element method with its argument through the interop
/// module (recorded, see RecordingJSRuntime).
/// </summary>
public class WaStepperMethodBindingTests : IDisposable
{
    [Fact]
    public async Task GoToAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaStepper>().GoToAsync(StepName));

        Assert.Contains("Cannot go to step: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task NextAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaStepper>().NextAsync());

        Assert.Contains("Cannot go to next step: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task PreviousAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaStepper>().PreviousAsync());

        Assert.Contains("Cannot go to previous step: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GoToAsync_WithNullName_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaStepper>().GoToAsync(null!));
    }

    [Fact]
    public async Task GoToAsync_WithEmptyName_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaStepper>().GoToAsync(string.Empty));
    }

    [Fact]
    public async Task GoToAsync_WithValidElement_InvokesGoToWithTheName()
    {
        await runtime.CreateRendered<WaStepper>().GoToAsync(StepName);

        Assert.Equal(new object[] { StepName }, runtime.Module.AssertInvokedMethod("goTo"));
    }

    [Fact]
    public async Task NextAsync_WithValidElement_InvokesNext()
    {
        await runtime.CreateRendered<WaStepper>().NextAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("next"));
    }

    [Fact]
    public async Task PreviousAsync_WithValidElement_InvokesPrevious()
    {
        await runtime.CreateRendered<WaStepper>().PreviousAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("previous"));
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const string StepName = "step-two";

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
