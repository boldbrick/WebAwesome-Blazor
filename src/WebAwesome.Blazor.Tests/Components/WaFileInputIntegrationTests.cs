using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaFileInput component introduced in Web Awesome 3.2.0. WaFileInput is
/// a ComponentBase (not a WaInputBase-derived form control, since selected files are not exposed as a
/// bindable scalar value), so these tests cover rendering, slots, and event wiring rather than EditForm binding.
/// </summary>
public class WaFileInputIntegrationTests : BunitContext
{
    public WaFileInputIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DefaultRender_OmitsOptionalAttributesAndUsesFalseFlags()
    {
        // Arrange & Act
        var cut = Render<WaFileInput>();

        // Assert
        var element = cut.Find("wa-file-input");
        Assert.False(element.HasAttribute("accept"));
        Assert.False(element.HasAttribute("hint"));
        Assert.False(element.HasAttribute("label"));
        Assert.False(element.HasAttribute("multiple"));
        Assert.False(element.HasAttribute("required"));
        Assert.False(element.HasAttribute("size"));
        Assert.False(element.HasAttribute("with-hint"));
        Assert.False(element.HasAttribute("with-label"));
    }

    [Fact]
    public void Parameters_WhenSet_RenderExpectedAttributes()
    {
        // Arrange & Act
        var cut = Render<WaFileInput>(parameters => parameters
            .Add(p => p.Accept, "image/*")
            .Add(p => p.Hint, "Max 5 MB")
            .Add(p => p.Label, "Attachment")
            .Add(p => p.Multiple, true)
            .Add(p => p.Required, true)
            .Add(p => p.Size, WaSize.Large)
            .Add(p => p.WithHint, true)
            .Add(p => p.WithLabel, true));

        // Assert
        var element = cut.Find("wa-file-input");
        Assert.Equal("image/*", element.GetAttribute("accept"));
        Assert.Equal("Max 5 MB", element.GetAttribute("hint"));
        Assert.Equal("Attachment", element.GetAttribute("label"));
        Assert.True(element.HasAttribute("multiple"));
        Assert.True(element.HasAttribute("required"));
        Assert.Equal("large", element.GetAttribute("size"));
        Assert.True(element.HasAttribute("with-hint"));
        Assert.True(element.HasAttribute("with-label"));
    }

    [Fact]
    public void Slots_WhenProvided_RenderIntoNamedSlots()
    {
        // Arrange & Act
        var cut = Render<WaFileInput>(parameters => parameters
            .Add(p => p.DropzoneContent, builder => builder.AddContent(0, "Drop files here"))
            .Add(p => p.LabelContent, builder => builder.AddContent(0, "Rich label"))
            .Add(p => p.HintContent, builder => builder.AddContent(0, "Rich hint")));

        // Assert
        Assert.Equal("Drop files here", cut.Find("span[slot='dropzone']").TextContent);
        Assert.Equal("Rich label", cut.Find("span[slot='label']").TextContent);
        Assert.Equal("Rich hint", cut.Find("span[slot='hint']").TextContent);
    }

    [Fact]
    public void Events_WhenWired_ReceiveDomEvents()
    {
        // Arrange
        var changeCount = 0;
        var inputCount = 0;
        var focusCount = 0;
        var blurCount = 0;
        var invalidCount = 0;
        var cut = Render<WaFileInput>(parameters => parameters
            .Add(p => p.OnChange, () => changeCount++)
            .Add(p => p.OnInput, () => inputCount++)
            .Add(p => p.OnFocus, () => focusCount++)
            .Add(p => p.OnBlur, () => blurCount++)
            .Add(p => p.OnInvalid, () => invalidCount++));

        // Act
        var element = cut.Find("wa-file-input");
        element.TriggerEvent("onchange", new EventArgs());
        element.TriggerEvent("oninput", new EventArgs());
        element.TriggerEvent("onfocus", new Microsoft.AspNetCore.Components.Web.FocusEventArgs());
        element.TriggerEvent("onblur", new Microsoft.AspNetCore.Components.Web.FocusEventArgs());
        element.TriggerEvent("onwa-invalid", new EventArgs());

        // Assert
        Assert.Equal(1, changeCount);
        Assert.Equal(1, inputCount);
        Assert.Equal(1, focusCount);
        Assert.Equal(1, blurCount);
        Assert.Equal(1, invalidCount);
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaFileInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
        Assert.Contains("Cannot focus the file input before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaFileInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
        Assert.Contains("Cannot blur the file input before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task SetCustomValidityAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaFileInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.SetCustomValidityAsync("bad file"));
        Assert.Contains("Cannot set custom validity before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task SetCustomValidityAsync_WithRenderedElement_ReachesInteropModule()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();
        var cut = Render<WaFileInput>();
        var component = cut.Instance;

        // Act
        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Please choose a smaller file"));

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Please choose a smaller file", invocation.Arguments[1]);
    }

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
}
