using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaRadioGroup: two-way binding of its nullable string Value through
/// the standard CreateBinder(CurrentValueAsString) wiring, with nested WaRadio children, and the
/// DataAnnotations validation lifecycle.
/// </summary>
public class WaRadioGroupEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new RadioModel { Choice = "a" };
        var cut = RenderForm(model);

        var element = cut.Find("wa-radio-group");
        Assert.Equal("a", element.GetAttribute("value"));
        Assert.Equal(2, cut.FindAll("wa-radio").Count);

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new RadioModel { Choice = "a" };
        var cut = RenderForm(model);

        cut.Find("wa-radio-group").Change("b");

        Assert.Equal("b", model.Choice);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new RadioModel { Choice = "a" };
        var cut = RenderForm(model);

        // Required is violated by an empty selection
        cut.Find("wa-radio-group").Change("");

        var cssClass = cut.Find("wa-radio-group").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new RadioModel { Choice = "a" };
        var cut = RenderForm(model);

        cut.Find("wa-radio-group").Change("");
        cut.Find("wa-radio-group").Change("b");

        var cssClass = cut.Find("wa-radio-group").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new RadioModel { Choice = null };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        var messages = capturedContext!.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-radio-group").GetAttribute("class"));
    }

    #region ------ Internals ------

    private class RadioModel
    {
        [Required]
        public string? Choice { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(RadioModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaRadioGroup, string?>(
            model,
            model.Choice,
            value => model.Choice = value,
            () => model.Choice,
            configureComponent: builder =>
            {
                builder.AddComponentParameter(10, "ChildContent", (RenderFragment)(childBuilder =>
                {
                    childBuilder.OpenComponent<WaRadio>(0);
                    childBuilder.AddComponentParameter(1, nameof(WaRadio.Value), "a");
                    childBuilder.CloseComponent();

                    childBuilder.OpenComponent<WaRadio>(2);
                    childBuilder.AddComponentParameter(3, nameof(WaRadio.Value), "b");
                    childBuilder.CloseComponent();
                }));
            },
            onEditContext: onEditContext);
    }

    #endregion
}
