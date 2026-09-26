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
/// EditForm integration tests for WaTimeInput (new in WA 3.8.0, typed TimeOnly? since 3.12.0): the bound time renders
/// as the element emits it (24-hour HH:mm, or HH:mm:ss when the step shows seconds) in any culture, the element's
/// wire time parses back (with or without seconds and a fraction), an empty value binds null, a value that is not a
/// wire time keeps the model and adds a validation message, and the setCustomValidity round-trip works.
/// </summary>
public class WaTimeInputEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundTimeAsHoursAndMinutes_InAnyCulture()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var model = new TimeModel { Time = new TimeOnly(9, 30, 15) };
        var cut = RenderForm(model);

        var element = cut.Find("wa-time-input");
        Assert.Equal("09:30", element.GetAttribute("value"));
        Assert.Contains("user-class", ClassesOf(element));
        Assert.Contains("valid", ClassesOf(element));
    }

    [Theory]
    [InlineData("1", "09:30:15")]
    [InlineData("30", "09:30:15")]
    [InlineData("90", "09:30:15")]
    [InlineData("any", "09:30:15")]
    [InlineData("60", "09:30")]
    [InlineData("120", "09:30")]
    [InlineData("3600", "09:30")]
    [InlineData("0", "09:30")]
    [InlineData("-5", "09:30")]
    [InlineData("bogus", "09:30")]
    public void Step_DecidesWhetherSecondsAreRendered_AsTheElementDoes(string step, string expected)
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var model = new TimeModel { Time = new TimeOnly(9, 30, 15) };
        var cut = RenderForm(model, configure: builder => builder.AddComponentParameter(10, nameof(WaTimeInput.Step), step));

        Assert.Equal(expected, cut.Find("wa-time-input").GetAttribute("value"));
    }

    [Theory]
    [InlineData("14:05", 14, 5, 0, 0)]
    [InlineData("9:05", 9, 5, 0, 0)]
    [InlineData("14:05:09", 14, 5, 9, 0)]
    [InlineData("14:05:09.250", 14, 5, 9, 250)]
    [InlineData("00:00", 0, 0, 0, 0)]
    [InlineData("23:59:59", 23, 59, 59, 0)]
    public void UserChange_ParsesWireTimeIntoModel(string wireValue, int hour, int minute, int second, int millisecond)
    {
        var model = new TimeModel();
        var cut = RenderForm(model);

        cut.Find("wa-time-input").Change(wireValue);

        Assert.Equal(new TimeOnly(hour, minute, second, millisecond), model.Time);
    }

    [Fact]
    public void UserClear_BindsNull_AndFailsRequired()
    {
        var model = new TimeModel { Time = new TimeOnly(9, 30) };
        var cut = RenderForm(model);

        cut.Find("wa-time-input").Change("");

        Assert.Null(model.Time);
        var classes = ClassesOf(cut.Find("wa-time-input"));
        Assert.Contains("modified", classes);
        Assert.Contains("invalid", classes);
    }

    [Theory]
    [InlineData("2:30 PM")]
    [InlineData("14.30")]
    [InlineData("24:00")]
    [InlineData("14:60")]
    [InlineData("14:5")]
    [InlineData("14:05:60")]
    [InlineData("123:00")]
    [InlineData("14:05:09.")]
    public void NonWireUserInput_KeepsModel_AndAddsValidationMessage(string wireValue)
    {
        var model = new TimeModel { Time = new TimeOnly(9, 30) };
        EditContext? editContext = null;
        var cut = RenderForm(model, context => editContext = context);

        cut.Find("wa-time-input").Change(wireValue);

        Assert.Equal(new TimeOnly(9, 30), model.Time);
        Assert.Equal(["The Time field must be a time."], editContext!.GetValidationMessages(() => model.Time).ToArray());
        Assert.Contains("invalid", ClassesOf(cut.Find("wa-time-input")));
    }

    [Fact]
    public void MinAndMax_RenderAsWireTimes()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var cut = RenderForm(new TimeModel(), configure: builder =>
        {
            builder.AddComponentParameter(10, nameof(WaTimeInput.Min), (TimeOnly?)new TimeOnly(22, 0));
            builder.AddComponentParameter(11, nameof(WaTimeInput.Max), (TimeOnly?)new TimeOnly(6, 30, 45));
        });

        // an overnight range (min after max) is the element's own concept and passes through as given
        var element = cut.Find("wa-time-input");
        Assert.Equal("22:00", element.GetAttribute("min"));
        Assert.Equal("06:30:45", element.GetAttribute("max"));
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
        Assert.Contains("invalid", ClassesOf(cut.Find("wa-time-input")));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new TimeModel { Time = new TimeOnly(9, 30) };
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
        public TimeOnly? Time { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(TimeModel model, Action<EditContext>? onEditContext = null,
        Action<RenderTreeBuilder>? configure = null)
    {
        return RenderControlForm<WaTimeInput, TimeOnly?>(
            model,
            model.Time,
            value => model.Time = value,
            () => model.Time,
            configureComponent: configure,
            onEditContext: onEditContext);
    }

    #endregion
}
