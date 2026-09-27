using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using WebAwesome.Blazor.Tests.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaKnownDate (new in WA 3.8.0, typed DateOnly? since 3.12.0): the bound date renders
/// as the element's ISO value in any culture, the element's ISO value parses back, the empty value the element
/// reports for a partly filled date binds null, a value that is not an ISO date keeps the model and adds a
/// validation message, the bounds render as ISO dates, and the setCustomValidity round-trip works.
/// </summary>
public class WaKnownDateEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundDateAsIsoValue_InAnyCulture_WithValidClass()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var model = new BirthdayModel { Birthday = new DateOnly(1990, 5, 25) };
        var cut = RenderForm(model);

        var element = cut.Find("wa-known-date");
        Assert.Equal("1990-05-25", element.GetAttribute("value"));
        Assert.Contains("user-class", ClassesOf(element));
        Assert.Contains("valid", ClassesOf(element));
    }

    [Fact]
    public void UserChange_ParsesIsoValueIntoModel()
    {
        var model = new BirthdayModel { Birthday = new DateOnly(1990, 5, 25) };
        var cut = RenderForm(model);

        cut.Find("wa-known-date").Change("1985-11-02");

        Assert.Equal(new DateOnly(1985, 11, 2), model.Birthday);
    }

    [Fact]
    public void PartlyFilledDate_ReportedEmpty_BindsNull_AndFailsRequired()
    {
        var model = new BirthdayModel { Birthday = new DateOnly(1990, 5, 25) };
        var cut = RenderForm(model);

        cut.Find("wa-known-date").Change("");

        Assert.Null(model.Birthday);
        var classes = ClassesOf(cut.Find("wa-known-date"));
        Assert.Contains("modified", classes);
        Assert.Contains("invalid", classes);
    }

    [Theory]
    [InlineData("25.05.1990")]
    [InlineData("1990-5-25")]
    [InlineData("1990-02-29")]
    public void NonIsoUserInput_KeepsModel_AndAddsValidationMessage(string wireValue)
    {
        var model = new BirthdayModel { Birthday = new DateOnly(1990, 5, 25) };
        EditContext? editContext = null;
        var cut = RenderForm(model, context => editContext = context);

        cut.Find("wa-known-date").Change(wireValue);

        Assert.Equal(new DateOnly(1990, 5, 25), model.Birthday);
        Assert.Equal(["The Birthday field must be a date."], editContext!.GetValidationMessages(() => model.Birthday).ToArray());
    }

    [Fact]
    public void MinAndMax_RenderAsIsoDates()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var cut = RenderForm(new BirthdayModel(), configure: builder =>
        {
            builder.AddComponentParameter(10, nameof(WaKnownDate.Min), (DateOnly?)new DateOnly(1900, 1, 1));
            builder.AddComponentParameter(11, nameof(WaKnownDate.Max), (DateOnly?)new DateOnly(2026, 12, 31));
        });

        var element = cut.Find("wa-known-date");
        Assert.Equal("1900-01-01", element.GetAttribute("min"));
        Assert.Equal("2026-12-31", element.GetAttribute("max"));
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
        Assert.Contains("invalid", ClassesOf(cut.Find("wa-known-date")));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new BirthdayModel { Birthday = new DateOnly(1990, 5, 25) };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaKnownDate>().Instance;

        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Date is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Date is not allowed", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class BirthdayModel
    {
        [Required]
        public DateOnly? Birthday { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(BirthdayModel model, Action<EditContext>? onEditContext = null,
        Action<RenderTreeBuilder>? configure = null)
    {
        return RenderControlForm<WaKnownDate, DateOnly?>(
            model,
            model.Birthday,
            value => model.Birthday = value,
            () => model.Birthday,
            configureComponent: configure,
            onEditContext: onEditContext);
    }

    #endregion
}
