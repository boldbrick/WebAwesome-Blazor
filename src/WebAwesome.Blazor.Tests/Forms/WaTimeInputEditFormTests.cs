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
/// EditForm integration tests for WaTimeInput (new in WA 3.8.0): two-way binding of its nullable string
/// Value, the DataAnnotations validation lifecycle, and the setCustomValidity JS interop round-trip.
/// </summary>
public class WaTimeInputEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new TimeModel { Time = "09:30:00" };
        var cut = RenderForm(model);

        var element = cut.Find("wa-time-input");
        Assert.Equal("09:30:00", element.GetAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new TimeModel { Time = "09:30:00" };
        var cut = RenderForm(model);

        cut.Find("wa-time-input").Change("14:15:00");

        Assert.Equal("14:15:00", model.Time);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new TimeModel { Time = "09:30:00" };
        var cut = RenderForm(model);

        cut.Find("wa-time-input").Change("");

        var cssClass = cut.Find("wa-time-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new TimeModel { Time = "09:30:00" };
        var cut = RenderForm(model);

        cut.Find("wa-time-input").Change("");
        cut.Find("wa-time-input").Change("14:15:00");

        var cssClass = cut.Find("wa-time-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new TimeModel { Time = null };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        Assert.NotEmpty(capturedContext!.GetValidationMessages().ToList());
        Assert.Contains("invalid", cut.Find("wa-time-input").GetAttribute("class"));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new TimeModel { Time = "09:30:00" };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaTimeInput>().Instance;

        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Time is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Time is not allowed", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class TimeModel
    {
        [Required]
        public string? Time { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(TimeModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaTimeInput, string?>(
            model,
            model.Time,
            value => model.Time = value,
            () => model.Time,
            onEditContext: onEditContext);
    }

    #endregion
}
