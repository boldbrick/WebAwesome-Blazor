using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// WaPopup.RepositionAsync must call the element's reposition() through the interop module. It used to invoke
/// the global eval with "arguments[0].reposition()", which runs in global scope where arguments is undefined,
/// so every call threw a ReferenceError in the browser.
/// </summary>
public class WaPopupRepositionTests : BunitContext
{
    public WaPopupRepositionTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Strict;
    }

    [Fact]
    public async Task RepositionAsync_InvokesRepositionOnElement_ThroughInteropModule()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaPopup>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.RepositionAsync());

        // Assert - strict mode also fails the test on any other (global) JS invocation
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("reposition", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";

    #endregion
}
