using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the ResetValidityAsync method added to the form controls in Web Awesome 3.3.0
/// (IFormValidation gained resetValidity alongside the existing setCustomValidity). Covers the
/// WaInputBase-derived path and the standalone controls (WaButton, WaFileInput, WaRadio, WaTextArea).
/// </summary>
public class WaResetValidityTests : BunitContext
{
    public WaResetValidityTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task ResetValidityAsync_WithNullElement_Throws_OnWaInputBaseDerived()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => new WaInput().ResetValidityAsync());
        Assert.Contains("Cannot reset validity before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task ResetValidityAsync_WithNullElement_Throws_OnStandaloneControls()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => new WaButton().ResetValidityAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => new WaFileInput().ResetValidityAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => new WaRadio().ResetValidityAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => new WaTextArea().ResetValidityAsync());
    }

    [Fact]
    public async Task ResetValidityAsync_WithRenderedElement_InvokesResetValidityOnElement()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaButton>();

        await cut.InvokeAsync(() => cut.Instance.ResetValidityAsync());

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("resetValidity", invocation.Arguments[1]);
    }

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
}
