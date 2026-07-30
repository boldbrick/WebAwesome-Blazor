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
/// EditForm integration tests for WaOtpInput, new in Web Awesome 3.11.0. Covers two-way binding of
/// its nullable string Value, the DataAnnotations validation lifecycle, and the setCustomValidity /
/// resetValidity round-trip. WaOtpInput deliberately does not expose a CustomError parameter; unlike
/// WaTextArea (which implements IFormValidation independently), custom validity is managed
/// imperatively here through the inherited WaInputBase&lt;TValue&gt; IFormValidation implementation
/// (SetCustomValidityAsync/ResetValidityAsync), exactly like the other WaInputBase-derived controls.
/// </summary>
public class WaOtpInputEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new OtpModel { Code = "123456" };
        var cut = RenderForm(model);

        var element = cut.Find("wa-otp-input");
        Assert.Equal("123456", element.GetAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new OtpModel { Code = "123456" };
        var cut = RenderForm(model);

        cut.Find("wa-otp-input").Change("654321");

        Assert.Equal("654321", model.Code);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new OtpModel { Code = "123456" };
        var cut = RenderForm(model);

        // StringLength(6, MinimumLength = 6) violated
        cut.Find("wa-otp-input").Change("12");

        var cssClass = cut.Find("wa-otp-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new OtpModel { Code = "123456" };
        var cut = RenderForm(model);

        cut.Find("wa-otp-input").Change("12");
        cut.Find("wa-otp-input").Change("654321");

        var cssClass = cut.Find("wa-otp-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new OtpModel { Code = "" };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        var messages = capturedContext!.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-otp-input").GetAttribute("class"));
    }

    [Fact]
    public void WithoutValueExpression_ThrowsLikeBuiltInInputs()
    {
        // bare usage outside @bind-Value must fail exactly like Blazor's built-in inputs
        var exception = Assert.ThrowsAny<Exception>(() => Render<WaOtpInput>());
        Assert.Contains("ValueExpression", exception.Message);
    }

    [Fact]
    public void CustomError_ParameterDoesNotExist()
    {
        // WaOtpInput deliberately relies on the IFormValidation route (SetCustomValidityAsync /
        // ResetValidityAsync) instead of a declarative CustomError parameter, matching the other 17
        // form controls that don't expose one.
        Assert.Null(typeof(WaOtpInput).GetProperty("CustomError"));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new OtpModel { Code = "123456" };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaOtpInput>().Instance;

        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Code has already been used"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Code has already been used", invocation.Arguments[1]);
    }

    [Fact]
    public async Task ResetValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", _ => true).SetVoidResult();

        var model = new OtpModel { Code = "123456" };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaOtpInput>().Instance;

        await cut.InvokeAsync(() => component.ResetValidityAsync());

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("resetValidity", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class OtpModel
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string? Code { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(OtpModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaOtpInput, string?>(
            model,
            model.Code,
            value => model.Code = value,
            () => model.Code,
            onEditContext: onEditContext);
    }

    #endregion
}
