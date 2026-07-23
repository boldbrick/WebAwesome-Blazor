using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for the new WaNumberInput component introduced in Web Awesome 3.2.0:
/// two-way binding of its nullable decimal Value, the DataAnnotations validation lifecycle, the
/// invalid-parse path through TryParseValueFromString, and the setCustomValidity JS interop round-trip.
/// </summary>
public class WaNumberInputEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new NumberModel { Amount = 50m };
        var cut = RenderForm(model);

        var element = cut.Find("wa-number-input");
        Assert.Equal("50", element.GetAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new NumberModel { Amount = 50m };
        var cut = RenderForm(model);

        cut.Find("wa-number-input").Change("75");

        Assert.Equal(75m, model.Amount);
    }

    [Fact]
    public void EmptyUserInput_ClearsTheNullableValue()
    {
        var model = new NumberModel { Amount = 50m };
        var cut = RenderForm(model);

        cut.Find("wa-number-input").Change("");

        Assert.Null(model.Amount);
    }

    [Fact]
    public void UnparseableUserInput_ProducesValidationErrorAndInvalidCssClass()
    {
        var model = new NumberModel { Amount = 50m };
        var cut = RenderForm(model);

        // not a valid decimal -> TryParseValueFromString fails with a validation error message
        cut.Find("wa-number-input").Change("not-a-number");

        var cssClass = cut.Find("wa-number-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
        Assert.Equal(50m, model.Amount);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new NumberModel { Amount = 50m };
        var cut = RenderForm(model);

        // Range(0, 100) violated by a value outside the range
        cut.Find("wa-number-input").Change("150");

        var cssClass = cut.Find("wa-number-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new NumberModel { Amount = 50m };
        var cut = RenderForm(model);

        cut.Find("wa-number-input").Change("150");
        cut.Find("wa-number-input").Change("60");

        var cssClass = cut.Find("wa-number-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new NumberModel { Amount = 150m };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        var messages = capturedContext!.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-number-input").GetAttribute("class"));
    }

    [Fact]
    public void WithoutValueExpression_ThrowsLikeBuiltInInputs()
    {
        // bare usage outside @bind-Value must fail exactly like Blazor's built-in inputs
        var exception = Assert.ThrowsAny<Exception>(() => Render<WaNumberInput>());
        Assert.Contains("ValueExpression", exception.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new NumberModel { Amount = 50m };
        var cut = RenderForm(model);
        var input = cut.FindComponent<WaNumberInput>().Instance;

        await cut.InvokeAsync(() => input.SetCustomValidityAsync("Amount is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Amount is not allowed", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class NumberModel
    {
        [Range(typeof(decimal), "0", "100")]
        public decimal? Amount { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(NumberModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaNumberInput, decimal?>(
            model,
            model.Amount,
            value => model.Amount = value,
            () => model.Amount,
            onEditContext: onEditContext);
    }

    #endregion
}
