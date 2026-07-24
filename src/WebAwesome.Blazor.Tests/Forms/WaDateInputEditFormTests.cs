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
/// EditForm integration tests for WaDateInput (Pro; new in WA 3.8.0): two-way binding of its nullable
/// string Value through the standard CreateBinder(CurrentValueAsString) wiring, the DataAnnotations
/// validation lifecycle, and the setCustomValidity JS interop round-trip.
/// </summary>
public class WaDateInputEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new DateModel { Date = "2026-07-24" };
        var cut = RenderForm(model);

        var element = cut.Find("wa-date-input");
        Assert.Equal("2026-07-24", element.GetAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new DateModel { Date = "2026-07-24" };
        var cut = RenderForm(model);

        cut.Find("wa-date-input").Change("2026-08-01");

        Assert.Equal("2026-08-01", model.Date);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new DateModel { Date = "2026-07-24" };
        var cut = RenderForm(model);

        cut.Find("wa-date-input").Change("");

        var cssClass = cut.Find("wa-date-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new DateModel { Date = "2026-07-24" };
        var cut = RenderForm(model);

        cut.Find("wa-date-input").Change("");
        cut.Find("wa-date-input").Change("2026-08-01");

        var cssClass = cut.Find("wa-date-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new DateModel { Date = null };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        Assert.NotEmpty(capturedContext!.GetValidationMessages().ToList());
        Assert.Contains("invalid", cut.Find("wa-date-input").GetAttribute("class"));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new DateModel { Date = "2026-07-24" };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaDateInput>().Instance;

        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Date is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Date is not allowed", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class DateModel
    {
        [Required]
        public string? Date { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(DateModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaDateInput, string?>(
            model,
            model.Date,
            value => model.Date = value,
            () => model.Date,
            onEditContext: onEditContext);
    }

    #endregion
}
