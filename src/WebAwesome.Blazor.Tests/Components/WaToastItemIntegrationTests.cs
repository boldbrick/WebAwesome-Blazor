using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaToastItem notification introduced in Web Awesome 3.3.0.
/// </summary>
public class WaToastItemIntegrationTests : BunitContext
{
    public WaToastItemIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        var cut = Render<WaToastItem>();

        var element = cut.Find("wa-toast-item");
        Assert.False(element.HasAttribute("duration"));
        Assert.False(element.HasAttribute("size"));
        Assert.False(element.HasAttribute("variant"));
    }

    /// <summary>
    /// The element carries the Blazor-owned marker, so the JS initializer hides it in place once it has hidden
    /// instead of letting Web Awesome remove a node that Blazor still tracks; the browser half is toast-items.spec.js.
    /// </summary>
    [Fact]
    public void Render_MarksElementAsBlazorOwned()
    {
        var cut = Render<WaToastItem>();

        Assert.True(cut.Find("wa-toast-item").HasAttribute(BlazorOwnedAttribute));
    }

    /// <summary>
    /// The marker the wrapper renders is the one the JS initializer looks for.
    /// </summary>
    [Fact]
    public void BlazorOwnedAttribute_MatchesLibraryConstantAndJsInitializer()
    {
        var constant = typeof(Constants).GetField(BlazorOwnedConstantName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(constant);
        Assert.Equal(BlazorOwnedAttribute, (string?)constant.GetRawConstantValue());

        var source = File.ReadAllText(Path.Combine(ApiParityData.WrapperProjectDirectory(), "wwwroot", JsInitializerEventRegistrations.FileName));
        Assert.Contains($"const blazorOwnedAttribute = '{BlazorOwnedAttribute}';", source);
    }

    [Fact]
    public void Parameters_WhenSet_RenderExpectedAttributes()
    {
        var cut = Render<WaToastItem>(parameters => parameters
            .Add(p => p.Duration, 0)
            .Add(p => p.Size, WaSize.Large)
            .Add(p => p.Variant, WaVariant.Success));

        var element = cut.Find("wa-toast-item");
        Assert.Equal("0", element.GetAttribute("duration"));
        Assert.Equal("l", element.GetAttribute("size"));
        Assert.Equal("success", element.GetAttribute("variant"));
    }

    [Fact]
    public void IconContent_RendersIntoIconSlot()
    {
        var cut = Render<WaToastItem>(parameters => parameters
            .Add(p => p.IconContent, builder => builder.AddContent(0, "bell")));

        Assert.Equal("bell", cut.Find("span[slot='icon']").TextContent);
    }

    [Fact]
    public void IconName_RendersWaIconIntoIconSlot_WhenNoFragment()
    {
        var cut = Render<WaToastItem>(parameters => parameters
            .Add(p => p.IconName, "circle-check"));

        var icon = cut.Find("wa-icon[slot='icon']");
        Assert.Equal("circle-check", icon.GetAttribute("name"));
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaToastItem();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.HideAsync());
        Assert.Contains("Cannot hide the toast item before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task HideAsync_WithRenderedElement_InvokesHideOnElement()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = Render<WaToastItem>();

        await cut.InvokeAsync(() => cut.Instance.HideAsync());

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("hide", invocation.Arguments[1]);
    }

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";

    // mirrors Constants.BlazorOwnedAttribute in the library (internal there, and InternalsVisibleTo applies to Debug
    // builds only)
    private const string BlazorOwnedAttribute = "data-wablazor-owned";
    private const string BlazorOwnedConstantName = "BlazorOwnedAttribute";
}
