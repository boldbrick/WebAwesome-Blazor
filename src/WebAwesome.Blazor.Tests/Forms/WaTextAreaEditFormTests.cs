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
/// EditForm integration tests for WaTextArea. Unlike the other form controls, WaTextArea derives
/// directly from <see cref="Microsoft.AspNetCore.Components.Forms.InputBase{TValue}"/> rather than
/// WaInputBase, and implements IFormValidation and the CSS class merging logic independently, so its
/// binding, validation lifecycle, and setCustomValidity round-trip are verified here rather than
/// assumed from the WaInputBase-derived controls.
/// </summary>
public class WaTextAreaEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new TextAreaModel { Bio = "Ada" };
        var cut = RenderForm(model);

        var element = cut.Find("wa-textarea");
        Assert.Equal("Ada", element.GetAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new TextAreaModel { Bio = "Ada" };
        var cut = RenderForm(model);

        cut.Find("wa-textarea").Change("Grace Hopper");

        Assert.Equal("Grace Hopper", model.Bio);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new TextAreaModel { Bio = "Ada" };
        var cut = RenderForm(model);

        // MinLength(3) violated
        cut.Find("wa-textarea").Change("ab");

        var cssClass = cut.Find("wa-textarea").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new TextAreaModel { Bio = "Ada" };
        var cut = RenderForm(model);

        cut.Find("wa-textarea").Change("ab");
        cut.Find("wa-textarea").Change("Grace Hopper");

        var cssClass = cut.Find("wa-textarea").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new TextAreaModel { Bio = "" };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        var messages = capturedContext!.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-textarea").GetAttribute("class"));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new TextAreaModel { Bio = "Ada" };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaTextArea>().Instance;

        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Bio is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Bio is not allowed", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class TextAreaModel
    {
        [Required]
        [MinLength(3)]
        public string? Bio { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(TextAreaModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaTextArea, string?>(
            model,
            model.Bio,
            value => model.Bio = value,
            () => model.Bio,
            onEditContext: onEditContext);
    }

    #endregion
}
