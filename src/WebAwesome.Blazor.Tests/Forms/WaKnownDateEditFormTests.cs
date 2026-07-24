using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaKnownDate (new in WA 3.8.0): two-way binding of its nullable string
/// Value, the DataAnnotations validation lifecycle, and the setCustomValidity JS interop round-trip.
/// </summary>
public class WaKnownDateEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new BirthdayModel { Birthday = "1990-05-25" };
        var cut = RenderForm(model);

        var element = cut.Find("wa-known-date");
        Assert.Equal("1990-05-25", element.GetAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new BirthdayModel { Birthday = "1990-05-25" };
        var cut = RenderForm(model);

        cut.Find("wa-known-date").Change("1985-11-02");

        Assert.Equal("1985-11-02", model.Birthday);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new BirthdayModel { Birthday = "1990-05-25" };
        var cut = RenderForm(model);

        cut.Find("wa-known-date").Change("");

        var cssClass = cut.Find("wa-known-date").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new BirthdayModel { Birthday = "1990-05-25" };
        var cut = RenderForm(model);

        cut.Find("wa-known-date").Change("");
        cut.Find("wa-known-date").Change("1985-11-02");

        var cssClass = cut.Find("wa-known-date").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new BirthdayModel { Birthday = null };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        Assert.NotEmpty(capturedContext!.GetValidationMessages().ToList());
        Assert.Contains("invalid", cut.Find("wa-known-date").GetAttribute("class"));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new BirthdayModel { Birthday = "1990-05-25" };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaKnownDate>().Instance;

        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Birthday is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Birthday is not allowed", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class BirthdayModel
    {
        [Required]
        public string? Birthday { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(BirthdayModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaKnownDate, string?>(
            model,
            model.Birthday,
            value => model.Birthday = value,
            () => model.Birthday,
            onEditContext: onEditContext);
    }

    #endregion
}
