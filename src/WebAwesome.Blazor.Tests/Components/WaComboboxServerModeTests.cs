using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the WaCombobox server (event) mode added in Web Awesome 3.14.0: the four status slots
/// (EmptyContent/ErrorContent/LoadingContent/NoResultsContent), the ReloadAsync method, and the C#-side logic of
/// HandleOptionsRequestAsync that clears the element's own "loading" property once the OnOptionsRequest handler
/// for the latest request has completed and the resulting render has reached the DOM. The four slots and
/// ReloadAsync's guard follow the patterns of WaComboboxIntegrationTests; the loading-clear logic is exercised
/// through "onwa-options-request", the event the wrapper actually binds, under a mocked interop module (pattern of
/// Forms/LiveValueSyncTests.cs).
/// </summary>
public class WaComboboxServerModeTests : BunitContext
{
    public WaComboboxServerModeTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(nameof(WaCombobox.EmptyContent), "empty")]
    [InlineData(nameof(WaCombobox.ErrorContent), "error")]
    [InlineData(nameof(WaCombobox.LoadingContent), "loading")]
    [InlineData(nameof(WaCombobox.NoResultsContent), "no-results")]
    public void StatusSlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        string? value = null;
        var cut = Render<WaCombobox>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-combobox")));
    }

    [Fact]
    public async Task ReloadAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaCombobox();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.ReloadAsync());

        Assert.Contains("Cannot reload: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task OnOptionsRequest_ReceivesTheTypedQuery()
    {
        // Arrange
        string? receivedQuery = null;
        string? value = null;
        var cut = Render<WaCombobox>(p => p
            .Add(c => c.ValueExpression, () => value)
            .Add(c => c.Server, true)
            .Add(c => c.OnOptionsRequest, (WaOptionsRequestEventArgs args) => receivedQuery = args.Query));

        // Act
        await cut.Find("wa-combobox").TriggerEventAsync(OptionsRequestEventAttribute, new WaOptionsRequestEventArgs { Query = "abc" });

        // Assert
        Assert.Equal("abc", receivedQuery);
    }

    [Fact]
    public async Task OnOptionsRequest_HandlerCompletes_ClearsLoadingExactlyOnce()
    {
        // Arrange - Strict mode, so any call other than the expected setProperty("loading", false) fails the test
        JSInterop.Mode = JSRuntimeMode.Strict;
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid(SetPropertyIdentifier, i => Equals(i.Arguments[1], LoadingProperty)).SetVoidResult();
        string? value = null;
        var cut = Render<WaCombobox>(p => p
            .Add(c => c.ValueExpression, () => value)
            .Add(c => c.Server, true)
            .Add(c => c.OnOptionsRequest, (WaOptionsRequestEventArgs _) => Task.CompletedTask));

        // Act
        await cut.Find("wa-combobox").TriggerEventAsync(OptionsRequestEventAttribute, new WaOptionsRequestEventArgs { Query = "abc" });

        // Assert
        var call = Assert.Single(LoadingClearCalls(module));
        Assert.Equal(false, call.Arguments[2]);
    }

    [Fact]
    public async Task OnOptionsRequest_WithLoadingTrue_NeverClearsLoading()
    {
        // Arrange
        JSInterop.Mode = JSRuntimeMode.Strict;
        var module = JSInterop.SetupModule(InteropModulePath);
        string? value = null;
        var cut = Render<WaCombobox>(p => p
            .Add(c => c.ValueExpression, () => value)
            .Add(c => c.Server, true)
            .Add(c => c.Loading, true)
            .Add(c => c.OnOptionsRequest, (WaOptionsRequestEventArgs _) => Task.CompletedTask));

        // Act
        await cut.Find("wa-combobox").TriggerEventAsync(OptionsRequestEventAttribute, new WaOptionsRequestEventArgs { Query = "abc" });

        // Assert - no interop call at all, since Loading stays true and the wrapper never arranges a clear
        Assert.Empty(module.Invocations);
    }

    [Fact]
    public async Task OnOptionsRequest_SupersededRequest_OnlyTheLatestClearsLoading()
    {
        // Arrange - the first handler is still awaiting when the second request arrives
        JSInterop.Mode = JSRuntimeMode.Strict;
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid(SetPropertyIdentifier, i => Equals(i.Arguments[1], LoadingProperty)).SetVoidResult();
        var firstGate = new TaskCompletionSource();
        var secondGate = new TaskCompletionSource();
        string? value = null;
        var cut = Render<WaCombobox>(p => p
            .Add(c => c.ValueExpression, () => value)
            .Add(c => c.Server, true)
            .Add(c => c.OnOptionsRequest, (WaOptionsRequestEventArgs args) => args.Query == "first" ? firstGate.Task : secondGate.Task));
        var element = cut.Find("wa-combobox");

        // Act - fire both requests before either handler completes
        var firstRequest = element.TriggerEventAsync(OptionsRequestEventAttribute, new WaOptionsRequestEventArgs { Query = "first" });
        var secondRequest = element.TriggerEventAsync(OptionsRequestEventAttribute, new WaOptionsRequestEventArgs { Query = "second" });

        // let the second (latest) request's handler complete first
        secondGate.SetResult();
        await secondRequest;

        // Assert - the latest request's completion cleared loading exactly once
        Assert.Single(LoadingClearCalls(module));

        // Act - the stale first request's handler now completes too
        firstGate.SetResult();
        await firstRequest;

        // Assert - the superseded request's completion did not clear loading again
        Assert.Single(LoadingClearCalls(module));
    }

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
    private const string OptionsRequestEventAttribute = "onwa-options-request";
    private const string SetPropertyIdentifier = "setProperty";
    private const string LoadingProperty = "loading";

    private static Bunit.JSRuntimeInvocation[] LoadingClearCalls(Bunit.BunitJSModuleInterop module)
        => module.Invocations.Where(i => i.Identifier == SetPropertyIdentifier && Equals(i.Arguments[1], LoadingProperty)).ToArray();

    #endregion
}

/// <summary>
/// Interop test for WaCombobox.ReloadAsync: it invokes the element's "reload" method through the interop module
/// (recorded, see RecordingJSRuntime). The null-element guard is exercised through an unrendered instance above.
/// </summary>
public class WaComboboxReloadInteropTests : IDisposable
{
    [Fact]
    public async Task ReloadAsync_WithValidElement_InvokesReload()
    {
        await runtime.CreateRendered<WaCombobox>().ReloadAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("reload"));
    }

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
