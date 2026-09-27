using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using WebAwesome.Blazor.Tests.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaDateInput (Pro; new in WA 3.8.0, typed DateOnly? since 3.12.0): the bound date
/// renders as the element's ISO value in any culture, the element's ISO value parses back into the model, an empty
/// value binds null, a value that is not an ISO date keeps the model and adds a validation message, and the
/// DataAnnotations and setCustomValidity round-trips work.
/// </summary>
public class WaDateInputEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundDateAsIsoValue_InAnyCulture_WithValidClass()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var model = new DateModel { Date = SampleDate };
        var cut = RenderForm(model);

        var element = cut.Find("wa-date-input");
        Assert.Equal(SampleIso, element.GetAttribute("value"));
        Assert.Contains("user-class", ClassesOf(element));
        Assert.Contains("valid", ClassesOf(element));
    }

    [Fact]
    public void UserChange_ParsesIsoValueIntoModel()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var model = new DateModel { Date = SampleDate };
        var cut = RenderForm(model);

        cut.Find("wa-date-input").Change("2026-08-01");

        Assert.Equal(new DateOnly(2026, 8, 1), model.Date);
        Assert.Equal("2026-08-01", cut.Find("wa-date-input").GetAttribute("value"));
    }

    [Fact]
    public void UserClear_BindsNull_AndFailsRequired()
    {
        var model = new DateModel { Date = SampleDate };
        var cut = RenderForm(model);

        cut.Find("wa-date-input").Change("");

        Assert.Null(model.Date);
        var classes = ClassesOf(cut.Find("wa-date-input"));
        Assert.Contains("modified", classes);
        Assert.Contains("invalid", classes);
    }

    [Theory]
    [InlineData("24.07.2026")]
    [InlineData("07/24/2026")]
    [InlineData("2026-7-24")]
    [InlineData("2026-02-30")]
    [InlineData("2026-07-24T00:00:00")]
    public void NonIsoUserInput_KeepsModel_AndAddsValidationMessage(string wireValue)
    {
        var model = new DateModel { Date = SampleDate };
        EditContext? editContext = null;
        var cut = RenderForm(model, context => editContext = context);

        cut.Find("wa-date-input").Change(wireValue);

        Assert.Equal(SampleDate, model.Date);
        Assert.Equal(["The Date field must be a date."], editContext!.GetValidationMessages(() => model.Date).ToArray());
        Assert.Contains("invalid", ClassesOf(cut.Find("wa-date-input")));
    }

    [Fact]
    public void ValidationMessage_UsesTheDisplayName()
    {
        var model = new DateModel { Date = SampleDate };
        EditContext? editContext = null;
        var cut = RenderForm(model, context => editContext = context, builder => builder.AddComponentParameter(10, "DisplayName", "Arrival"));

        cut.Find("wa-date-input").Change("not a date");

        Assert.Equal(["The Arrival field must be a date."], editContext!.GetValidationMessages(() => model.Date).ToArray());
    }

    [Fact]
    public void CorrectedUserInput_ClearsTheParseMessage_AndReturnsToValid()
    {
        var model = new DateModel { Date = SampleDate };
        EditContext? editContext = null;
        var cut = RenderForm(model, context => editContext = context);

        cut.Find("wa-date-input").Change("bogus");
        cut.Find("wa-date-input").Change("2026-08-01");

        Assert.Equal(new DateOnly(2026, 8, 1), model.Date);
        Assert.Empty(editContext!.GetValidationMessages(() => model.Date));
        var classes = ClassesOf(cut.Find("wa-date-input"));
        Assert.Contains("modified", classes);
        Assert.Contains("valid", classes);
        Assert.DoesNotContain("invalid", classes);
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
        Assert.Contains("invalid", ClassesOf(cut.Find("wa-date-input")));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new DateModel { Date = SampleDate };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaDateInput>().Instance;

        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Date is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Date is not allowed", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private const string SampleIso = "2026-07-24";

    private static readonly DateOnly SampleDate = new(2026, 7, 24);

    private class DateModel
    {
        [Required]
        public DateOnly? Date { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(DateModel model, Action<EditContext>? onEditContext = null,
        Action<Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder>? configure = null)
    {
        return RenderControlForm<WaDateInput, DateOnly?>(
            model,
            model.Date,
            value => model.Date = value,
            () => model.Date,
            configureComponent: configure,
            onEditContext: onEditContext);
    }

    #endregion
}
