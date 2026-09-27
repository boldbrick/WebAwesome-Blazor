using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using WebAwesome.Blazor.Tests.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaDateRangeInput (Pro; the range-mode wrapper of wa-date-input since 3.12.0): the
/// bound WaDateRange renders as the element's from/to value in any culture and always with mode="range", the
/// element's value parses back into the model (a single date as a half-filled range, a reversed range ordered), an
/// empty value binds null, and a value that is not the wire form keeps the model and adds a validation message.
/// </summary>
public class WaDateRangeInputEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundRangeAsWireValue_InAnyCulture_InRangeMode()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var model = new RangeModel { Stay = new WaDateRange(May1, May7) };
        var cut = RenderForm(model);

        var element = cut.Find("wa-date-input");
        Assert.Equal("2026-05-01/2026-05-07", element.GetAttribute("value"));
        Assert.Equal("range", element.GetAttribute("mode"));
        Assert.Contains("valid", ClassesOf(element));
    }

    [Fact]
    public void HalfFilledRange_RendersItsOneDate()
    {
        var model = new RangeModel { Stay = new WaDateRange(May1, null) };
        var cut = RenderForm(model);

        Assert.Equal("2026-05-01", cut.Find("wa-date-input").GetAttribute("value"));
    }

    [Fact]
    public void NullAndEmptyRange_RenderNoValue_ButStillRangeMode()
    {
        var cut = RenderForm(new RangeModel { Stay = null });
        var emptyCut = RenderForm(new RangeModel { Stay = default(WaDateRange) });

        Assert.False(cut.Find("wa-date-input").HasAttribute("value"));
        Assert.False(emptyCut.Find("wa-date-input").HasAttribute("value"));
        Assert.Equal("range", cut.Find("wa-date-input").GetAttribute("mode"));
    }

    [Fact]
    public void UserPicksFullRange_ParsesIntoModel()
    {
        var model = new RangeModel();
        var cut = RenderForm(model);

        cut.Find("wa-date-input").Change("2026-05-01/2026-05-07");

        Assert.Equal(new WaDateRange(May1, May7), model.Stay);
    }

    [Fact]
    public void UserPicksFirstDateOnly_BindsHalfFilledRange()
    {
        // Web Awesome reports a range with one end picked as that single date
        var model = new RangeModel();
        var cut = RenderForm(model);

        cut.Find("wa-date-input").Change("2026-05-01");

        Assert.Equal(new WaDateRange(May1, null), model.Stay);
        Assert.False(model.Stay!.Value.IsComplete);
    }

    [Fact]
    public void ReversedRange_BindsOrdered()
    {
        var model = new RangeModel();
        var cut = RenderForm(model);

        cut.Find("wa-date-input").Change("2026-05-07/2026-05-01");

        Assert.Equal(new WaDateRange(May1, May7), model.Stay);
    }

    [Fact]
    public void UserClear_BindsNull_AndFailsRequired()
    {
        var model = new RangeModel { Stay = new WaDateRange(May1, May7) };
        var cut = RenderForm(model);

        cut.Find("wa-date-input").Change("");

        Assert.Null(model.Stay);
        Assert.Contains("invalid", ClassesOf(cut.Find("wa-date-input")));
    }

    [Theory]
    [InlineData("01.05.2026/07.05.2026")]
    [InlineData("2026-05-01/")]
    [InlineData("2026-05-01/2026-05-07/2026-05-09")]
    [InlineData("2026-05-01 to 2026-05-07")]
    public void NonWireUserInput_KeepsModel_AndAddsValidationMessage(string wireValue)
    {
        var model = new RangeModel { Stay = new WaDateRange(May1, May7) };
        EditContext? editContext = null;
        var cut = RenderForm(model, context => editContext = context);

        cut.Find("wa-date-input").Change(wireValue);

        Assert.Equal(new WaDateRange(May1, May7), model.Stay);
        Assert.Equal(["The Stay field must be a date range."], editContext!.GetValidationMessages(() => model.Stay).ToArray());
        Assert.Contains("invalid", ClassesOf(cut.Find("wa-date-input")));
    }

    [Fact]
    public void MinRangeAndMaxRange_Render()
    {
        var cut = RenderForm(new RangeModel(), configure: builder =>
        {
            builder.AddComponentParameter(10, nameof(WaDateRangeInput.MinRange), (int?)2);
            builder.AddComponentParameter(11, nameof(WaDateRangeInput.MaxRange), (int?)14);
        });

        var element = cut.Find("wa-date-input");
        Assert.Equal("2", element.GetAttribute("min-range"));
        Assert.Equal("14", element.GetAttribute("max-range"));
    }

    #region ------ Internals ------

    private static readonly DateOnly May1 = new(2026, 5, 1);
    private static readonly DateOnly May7 = new(2026, 5, 7);

    private class RangeModel
    {
        [Required]
        public WaDateRange? Stay { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(RangeModel model, Action<EditContext>? onEditContext = null,
        Action<Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder>? configure = null)
    {
        return RenderControlForm<WaDateRangeInput, WaDateRange?>(
            model,
            model.Stay,
            value => model.Stay = value,
            () => model.Stay,
            configureComponent: configure,
            onEditContext: onEditContext);
    }

    #endregion
}
