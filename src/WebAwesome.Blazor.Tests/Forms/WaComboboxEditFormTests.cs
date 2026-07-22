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
/// EditForm integration tests for WaCombobox in single-selection mode: two-way binding of its nullable
/// string Value through the standard CreateBinder(CurrentValueAsString) wiring, with nested WaOption
/// children, the DataAnnotations validation lifecycle, and the setCustomValidity JS interop round-trip.
/// Multiple-selection mode (SelectedValues/SelectedValuesChanged) is a separate, non-InputBase-mediated
/// binding path and is out of scope for these EditForm/@bind-Value scenarios.
/// </summary>
public class WaComboboxEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        var model = new ComboboxModel { Fruit = "apple" };
        var cut = RenderForm(model);

        var element = cut.Find("wa-combobox");
        Assert.Equal("apple", element.GetAttribute("value"));
        Assert.Equal(2, cut.FindAll("wa-option").Count);

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding()
    {
        var model = new ComboboxModel { Fruit = "apple" };
        var cut = RenderForm(model);

        cut.Find("wa-combobox").Change("banana");

        Assert.Equal("banana", model.Fruit);
    }

    [Fact]
    public void InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new ComboboxModel { Fruit = "apple" };
        var cut = RenderForm(model);

        // Required is violated by clearing the selection
        cut.Find("wa-combobox").Change("");

        var cssClass = cut.Find("wa-combobox").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new ComboboxModel { Fruit = "apple" };
        var cut = RenderForm(model);

        cut.Find("wa-combobox").Change("");
        cut.Find("wa-combobox").Change("banana");

        var cssClass = cut.Find("wa-combobox").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_ProducesValidationMessages()
    {
        var model = new ComboboxModel { Fruit = null };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        var messages = capturedContext!.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-combobox").GetAttribute("class"));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new ComboboxModel { Fruit = "apple" };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaCombobox>().Instance;

        await cut.InvokeAsync(() => component.SetCustomValidityAsync("Fruit is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Fruit is not allowed", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class ComboboxModel
    {
        [Required]
        public string? Fruit { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(ComboboxModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaCombobox, string?>(
            model,
            model.Fruit,
            value => model.Fruit = value,
            () => model.Fruit,
            configureComponent: builder =>
            {
                builder.AddComponentParameter(10, "ChildContent", (RenderFragment)(childBuilder =>
                {
                    childBuilder.OpenComponent<WaOption>(0);
                    childBuilder.AddComponentParameter(1, nameof(WaOption.Value), "apple");
                    childBuilder.CloseComponent();

                    childBuilder.OpenComponent<WaOption>(2);
                    childBuilder.AddComponentParameter(3, nameof(WaOption.Value), "banana");
                    childBuilder.CloseComponent();
                }));
            },
            onEditContext: onEditContext);
    }

    #endregion
}
