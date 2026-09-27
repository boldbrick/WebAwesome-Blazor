using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaTextArea. Until 3.12.0 WaTextArea derived directly from
/// <see cref="Microsoft.AspNetCore.Components.Forms.InputBase{TValue}"/> with its own IFormValidation and
/// CSS class merging; since the 3.12.0 bindings it derives from WaInputBase&lt;string?&gt; like the other text
/// inputs. These tests guard that rebase end to end: binding, the validation lifecycle and CSS classes, the
/// setCustomValidity/resetValidity round-trips, Immediate binding validating while typing, and the key events
/// the textarea gained from the base class.
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

    [Fact]
    public async Task ResetValidityAsync_InvokesElementMethod()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();

        var model = new TextAreaModel { Bio = "Ada" };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaTextArea>().Instance;

        await cut.InvokeAsync(() => component.ResetValidityAsync());

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("resetValidity", invocation.Arguments[1]);
    }

    [Fact]
    public void ImmediateInput_ValidatesWhileTyping()
    {
        var model = new TextAreaModel { Bio = "Ada" };
        var cut = RenderForm(model, configureComponent: builder => builder.AddComponentParameter(10, nameof(WaTextArea.Immediate), true));

        // MinLength(3) violated by an in-progress value, before any commit
        cut.Find("wa-textarea").Input("ab");

        Assert.Equal("ab", model.Bio);
        var cssClass = cut.Find("wa-textarea").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void WithoutImmediate_InputDoesNotChangeModel()
    {
        var model = new TextAreaModel { Bio = "Ada" };
        var inputCount = 0;
        var cut = RenderForm(model, configureComponent: builder => builder.AddComponentParameter(10, nameof(WaTextArea.OnInput),
            EventCallback.Factory.Create<ChangeEventArgs>(this, _ => inputCount++)));

        cut.Find("wa-textarea").Input("Grace");

        Assert.Equal(1, inputCount);
        Assert.Equal("Ada", model.Bio);
        Assert.DoesNotContain("modified", cut.Find("wa-textarea").GetAttribute("class"));
    }

    [Fact]
    public void OnKeyDown_InheritedFromWaInputBase_IsWired()
    {
        var model = new TextAreaModel { Bio = "Ada" };
        KeyboardEventArgs? received = null;
        var cut = RenderForm(model, configureComponent: builder => builder.AddComponentParameter(10, nameof(WaTextArea.OnKeyDown),
            EventCallback.Factory.Create<KeyboardEventArgs>(this, args => received = args)));

        cut.Find("wa-textarea").KeyDown(new KeyboardEventArgs { Key = "Enter", CtrlKey = true });

        Assert.NotNull(received);
        Assert.Equal("Enter", received!.Key);
        Assert.True(received.CtrlKey);
    }

    [Fact]
    public void Rows_OmittedByDefault()
    {
        var model = new TextAreaModel { Bio = "Ada" };
        var cut = RenderForm(model);

        Assert.False(cut.Find("wa-textarea").HasAttribute("rows"));
    }

    #region ------ Internals ------

    private class TextAreaModel
    {
        [Required]
        [MinLength(3)]
        public string? Bio { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(TextAreaModel model, Action<EditContext>? onEditContext = null,
        Action<RenderTreeBuilder>? configureComponent = null)
    {
        return RenderControlForm<WaTextArea, string?>(
            model,
            model.Bio,
            value => model.Bio = value,
            () => model.Bio,
            configureComponent,
            onEditContext);
    }

    #endregion
}
