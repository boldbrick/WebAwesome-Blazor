using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaColorPicker: two-way binding of its non-nullable string Value
/// through the standard CreateBinder(CurrentValueAsString) wiring shared with WaInput/WaSelect, the
/// DataAnnotations validation lifecycle, and the setCustomValidity JS interop round-trip.
/// </summary>
public class WaColorPickerEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new ColorModel { Color = "#ff0000" };
        var cut = RenderForm(model);

        var element = cut.Find("wa-color-picker");
        Assert.Equal("#ff0000", element.GetAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new ColorModel { Color = "#ff0000" };
        var cut = RenderForm(model);

        cut.Find("wa-color-picker").Change("#00ff00");

        Assert.Equal("#00ff00", model.Color);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new ColorModel { Color = "#ff0000" };
        var cut = RenderForm(model);

        // Required is violated by an empty value
        cut.Find("wa-color-picker").Change("");

        var cssClass = cut.Find("wa-color-picker").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new ColorModel { Color = "#ff0000" };
        var cut = RenderForm(model);

        cut.Find("wa-color-picker").Change("");
        cut.Find("wa-color-picker").Change("#00ff00");

        var cssClass = cut.Find("wa-color-picker").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new ColorModel { Color = "" };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        var messages = capturedContext!.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-color-picker").GetAttribute("class"));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new ColorModel { Color = "#ff0000" };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaColorPicker>().Instance;

        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Color is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Color is not allowed", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class ColorModel
    {
        [Required]
        public string Color { get; set; } = string.Empty;
    }

    private IRenderedComponent<EditForm> RenderForm(ColorModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaColorPicker, string>(
            model,
            model.Color,
            value => model.Color = value,
            () => model.Color,
            onEditContext: onEditContext);
    }

    #endregion
}
