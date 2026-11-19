using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaRichTextEditor (Web Awesome 3.15.0), which derives from WaLabeledInputBase of
/// string like WaTextArea: the rendered value and validation classes, the commit binding through the "onchange"
/// binder, OnInput leaving the model alone (the editor has no immediate binding), the validation lifecycle with
/// its messages, and the setCustomValidity/resetValidity round trips. The live value sync is a case of
/// LiveValueSyncTests (LiveValuePropertyName "value"); the editor's text length counts without markup, which is the
/// element's own validation and not reproduced here.
/// </summary>
public class WaRichTextEditorEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        // Arrange & Act
        var cut = RenderForm(new NoteModel { Body = "<p>Ada</p>" });

        // Assert
        var element = cut.Find("wa-rich-text-editor");
        Assert.Equal("<p>Ada</p>", element.GetAttribute("value"));
        var classes = ClassesOf(element);
        Assert.Contains("user-class", classes);
        Assert.Contains("valid", classes);
        Assert.DoesNotContain("invalid", classes);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        // Arrange
        var model = new NoteModel { Body = "<p>Ada</p>" };
        var cut = RenderForm(model);

        // Act
        cut.Find("wa-rich-text-editor").Change("<p>Grace Hopper</p>");

        // Assert
        Assert.Equal("<p>Grace Hopper</p>", model.Body);
    }

    [Fact]
    public void UserChange_ToEmpty_BindsTheEmptyString()
    {
        // Arrange
        var model = new NoteModel { Body = "<p>Ada</p>" };
        var cut = RenderForm(model);

        // Act
        cut.Find("wa-rich-text-editor").Change(string.Empty);

        // Assert
        Assert.Equal(string.Empty, model.Body);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        // Arrange
        var cut = RenderForm(new NoteModel { Body = "<p>Ada</p>" });

        // Act - MinLength(10) violated
        cut.Find("wa-rich-text-editor").Change("<p>ab</p>");

        // Assert
        var classes = ClassesOf(cut.Find("wa-rich-text-editor"));
        Assert.Contains("modified", classes);
        Assert.Contains("invalid", classes);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        // Arrange
        var cut = RenderForm(new NoteModel { Body = "<p>Ada</p>" });

        // Act
        cut.Find("wa-rich-text-editor").Change("<p>ab</p>");
        cut.Find("wa-rich-text-editor").Change("<p>Grace Hopper</p>");

        // Assert
        var classes = ClassesOf(cut.Find("wa-rich-text-editor"));
        Assert.Contains("modified", classes);
        Assert.Contains("valid", classes);
        Assert.DoesNotContain("invalid", classes);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        // Arrange - Required violated
        EditContext? capturedContext = null;
        var cut = RenderForm(new NoteModel { Body = string.Empty }, editContext => capturedContext = editContext);

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.NotNull(capturedContext);
        Assert.NotEmpty(capturedContext!.GetValidationMessages());
        Assert.Contains("invalid", ClassesOf(cut.Find("wa-rich-text-editor")));
    }

    [Fact]
    public void ValidSubmit_ProducesNoValidationMessages()
    {
        // Arrange
        EditContext? capturedContext = null;
        var cut = RenderForm(new NoteModel { Body = "<p>Grace Hopper</p>" }, editContext => capturedContext = editContext);

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.Empty(capturedContext!.GetValidationMessages());
    }

    [Fact]
    public void OnInput_DoesNotChangeTheModel()
    {
        // Arrange
        var model = new NoteModel { Body = "<p>Ada</p>" };
        var inputs = 0;
        var cut = RenderForm(model, configureComponent: builder => builder.AddComponentParameter(10, nameof(WaRichTextEditor.OnInput),
            EventCallback.Factory.Create<ChangeEventArgs>(this, _ => inputs++)));

        // Act
        cut.Find("wa-rich-text-editor").Input("<p>Grace</p>");

        // Assert - the model follows the commit only
        Assert.Equal(1, inputs);
        Assert.Equal("<p>Ada</p>", model.Body);
        Assert.DoesNotContain("modified", ClassesOf(cut.Find("wa-rich-text-editor")));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();
        var cut = RenderForm(new NoteModel { Body = "<p>Ada</p>" });
        var component = cut.FindComponent<WaRichTextEditor>().Instance;

        // Act
        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Body is not allowed"));

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Body is not allowed", invocation.Arguments[1]);
    }

    [Fact]
    public async Task ResetValidityAsync_InvokesElementMethod()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();
        var cut = RenderForm(new NoteModel { Body = "<p>Ada</p>" });
        var component = cut.FindComponent<WaRichTextEditor>().Instance;

        // Act
        await cut.InvokeAsync(() => component.ResetValidityAsync());

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("resetValidity", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class NoteModel
    {
        [Required]
        [MinLength(10)]
        public string? Body { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(NoteModel model, Action<EditContext>? onEditContext = null,
        Action<RenderTreeBuilder>? configureComponent = null)
    {
        return RenderControlForm<WaRichTextEditor, string?>(
            model,
            model.Body,
            value => model.Body = value,
            () => model.Body,
            configureComponent,
            onEditContext);
    }

    #endregion
}
