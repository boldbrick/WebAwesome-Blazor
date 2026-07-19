using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaSlider in single-value mode: two-way binding of its nullable
/// decimal Value (bound directly over CurrentValue via CreateBinder&lt;decimal?&gt;, same pattern as
/// WaRange/WaRating), and the DataAnnotations validation lifecycle. Range mode (MinValue/MaxValue,
/// dual-thumb) uses a separate, custom "onchange" handler (see WaSlider.HandleRangeValueChange) that
/// is not mediated through InputBase and is out of scope for these @bind-Value scenarios.
/// </summary>
public class WaSliderEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new SliderModel { Level = 50m };
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
        var model = new SliderModel { Level = 50m };
        var cut = RenderForm(model);

        cut.Find("wa-slider").Change("75");

        Assert.Equal(75m, model.Level);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new SliderModel { Level = 50m };
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
        var model = new SliderModel { Level = 50m };
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
        var model = new SliderModel { Level = 150m };
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
        // nullable decimal-valued control family (WaSlider)
        var exception = Assert.ThrowsAny<Exception>(() => RenderComponent<WaSlider>());
        Assert.Contains("ValueExpression", exception.Message);
    }

    #region ------ Internals ------

    private class SliderModel
    {
        [Range(typeof(decimal), "0", "100")]
        public decimal? Level { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(SliderModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaSlider, decimal?>(
            model,
            model.Level,
            value => model.Level = value,
            () => model.Level,
            onEditContext: onEditContext);
    }

    #endregion
}
