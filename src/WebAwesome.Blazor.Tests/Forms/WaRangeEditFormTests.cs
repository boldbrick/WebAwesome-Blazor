using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaRange: two-way binding of its non-nullable decimal Value, and the
/// DataAnnotations validation lifecycle. Unlike the string-valued controls, WaRange's non-range-mode
/// onchange binds directly via <c>EventCallback.Factory.CreateBinder&lt;decimal&gt;</c> over
/// <c>CurrentValue</c> rather than <c>CurrentValueAsString</c>, so the numeric conversion happens
/// through Microsoft's BindConverter rather than through WaRange's own TryParseValueFromString
/// override (which is effectively unreachable from normal user input as a result).
/// </summary>
public class WaRangeEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new RangeModel { Volume = 50m };
        var cut = RenderForm(model);

        var element = cut.Find("wa-slider");
        Assert.Equal("50", element.GetAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new RangeModel { Volume = 50m };
        var cut = RenderForm(model);

        cut.Find("wa-slider").Change("75");

        Assert.Equal(75m, model.Volume);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new RangeModel { Volume = 50m };
        var cut = RenderForm(model);

        // Range(0, 100) violated by a value outside the range
        cut.Find("wa-slider").Change("150");

        var cssClass = cut.Find("wa-slider").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new RangeModel { Volume = 50m };
        var cut = RenderForm(model);

        cut.Find("wa-slider").Change("150");
        cut.Find("wa-slider").Change("60");

        var cssClass = cut.Find("wa-slider").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new RangeModel { Volume = 150m };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        var messages = capturedContext!.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-slider").GetAttribute("class"));
    }

    [Fact]
    public void WithoutValueExpression_ThrowsLikeBuiltInInputs()
    {
        // bare usage outside @bind-Value must fail exactly like Blazor's built-in inputs, for the
        // decimal-valued control family (WaRange/WaRating)
        var exception = Assert.ThrowsAny<Exception>(() => Render<WaRange>());
        Assert.Contains("ValueExpression", exception.Message);
    }

    #region ------ Internals ------

    private class RangeModel
    {
        [Range(typeof(decimal), "0", "100")]
        public decimal Volume { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(RangeModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaRange, decimal>(
            model,
            model.Volume,
            value => model.Volume = value,
            () => model.Volume,
            onEditContext: onEditContext);
    }

    #endregion
}
