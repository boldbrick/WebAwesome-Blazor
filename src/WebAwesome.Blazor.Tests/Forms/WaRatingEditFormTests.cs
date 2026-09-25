using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaRating: two-way binding of its non-nullable decimal Value, and the
/// DataAnnotations validation lifecycle. The value arrives through the "numericchange" alias of the change event
/// (HandleValueChange parses it with the invariant culture); these tests raise the alias with NumericChange, so
/// they cover the C# half only (see NumericChangeExtensions for where the JS alias is checked).
/// </summary>
public class WaRatingEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new RatingModel { Stars = 3m };
        var cut = RenderForm(model);

        var element = cut.Find("wa-rating");
        Assert.Equal("3", element.GetAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new RatingModel { Stars = 3m };
        var cut = RenderForm(model);

        cut.Find("wa-rating").NumericChange("4");

        Assert.Equal(4m, model.Stars);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new RatingModel { Stars = 3m };
        var cut = RenderForm(model);

        // Range(1, 5) violated by zero
        cut.Find("wa-rating").NumericChange("0");

        var cssClass = cut.Find("wa-rating").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new RatingModel { Stars = 3m };
        var cut = RenderForm(model);

        cut.Find("wa-rating").NumericChange("0");
        cut.Find("wa-rating").NumericChange("4");

        var cssClass = cut.Find("wa-rating").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new RatingModel { Stars = 0m };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        var messages = capturedContext!.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-rating").GetAttribute("class"));
    }

    #region ------ Internals ------

    private class RatingModel
    {
        [Range(typeof(decimal), "1", "5")]
        public decimal Stars { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(RatingModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaRating, decimal>(
            model,
            model.Stars,
            value => model.Stars = value,
            () => model.Stars,
            onEditContext: onEditContext);
    }

    #endregion
}
