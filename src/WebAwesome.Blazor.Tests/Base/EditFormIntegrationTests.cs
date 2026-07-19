using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Base;

/// <summary>
/// End-to-end EditForm scenarios for the WaInputBase-derived form controls: two-way
/// binding through the Blazor input model, DataAnnotations validation lifecycle,
/// validation CSS class merging, and the setCustomValidity JS interop round-trip.
/// Rendered with bUnit against the real EditForm/EditContext plumbing.
/// </summary>
public class EditFormIntegrationTests : TestContext
{
    [Fact]
    public void WaInput_InEditForm_RendersBoundValueAndValidClass()
    {
        var model = new TestModel { Name = "Ada" };
        var cut = RenderForm(model);

        var input = cut.Find("wa-input");
        Assert.Equal("Ada", input.GetAttribute("value"));

        // untouched field reports the standard InputBase "valid" state, merged with the user class
        var cssClass = input.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void WaInput_UserInput_UpdatesModelThroughBinding()
    {
        var model = new TestModel { Name = "Ada" };
        var cut = RenderForm(model);

        cut.Find("wa-input").Change("Grace");

        Assert.Equal("Grace", model.Name);
    }

    [Fact]
    public void WaInput_InvalidUserInput_GetsModifiedInvalidCssClasses()
    {
        var model = new TestModel { Name = "Ada" };
        var cut = RenderForm(model);

        // MinLength(3) violated -> DataAnnotationsValidator flags the field on change
        cut.Find("wa-input").Change("ab");

        var cssClass = cut.Find("wa-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void WaInput_CorrectedUserInput_ReturnsToValidCssClass()
    {
        var model = new TestModel { Name = "Ada" };
        var cut = RenderForm(model);

        cut.Find("wa-input").Change("ab");
        cut.Find("wa-input").Change("Grace");

        var cssClass = cut.Find("wa-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void WaInput_FailedSubmit_ProducesValidationMessages()
    {
        var model = new TestModel { Name = "" };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        cut.Find("form").Submit();

        Assert.NotNull(capturedContext);
        var messages = capturedContext.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-input").GetAttribute("class"));
    }

    [Fact]
    public void WaInput_WithoutValueExpression_ThrowsLikeBuiltInInputs()
    {
        // bare usage outside @bind-Value must fail exactly like Blazor's InputText
        var exception = Assert.ThrowsAny<Exception>(() => RenderComponent<WaInput>());
        Assert.Contains("ValueExpression", exception.Message);
    }

    [Fact]
    public async Task WaInput_SetCustomValidityAsync_ReachesInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new TestModel { Name = "Ada" };
        var cut = RenderForm(model);
        var input = cut.FindComponent<WaInput>().Instance;

        await cut.InvokeAsync(() => input.SetCustomValidityAsync("Name is not allowed"));

        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("Name is not allowed", invocation.Arguments[1]);
    }

    [Fact]
    public void WaCheckbox_UserChange_ReadsRealCheckedStateViaInterop()
    {
        // the wrapper reads the custom element's .checked property back through JS interop
        // (Blazor's built-in binder cannot see it on non-INPUT tags) - simulate the browser
        // reporting a checked state of true
        var module = JSInterop.SetupModule(InteropModulePath);
        module.Setup<bool>("getProperty", invocation => Equals(invocation.Arguments[1], "checked")).SetResult(true);

        var model = new TestModel { Name = "Ada", Accepted = false };
        var cut = RenderCheckboxForm(model);

        cut.Find("wa-checkbox").Change(bool.TrueString);

        Assert.True(model.Accepted);
    }

    [Fact]
    public void WaCheckbox_InEditForm_RendersCheckedState()
    {
        var model = new TestModel { Name = "Ada", Accepted = true };
        var cut = RenderCheckboxForm(model);

        Assert.True(cut.Find("wa-checkbox").HasAttribute("checked"));
    }

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";

    private class TestModel
    {
        [Required]
        [MinLength(3)]
        public string? Name { get; set; }

        public bool Accepted { get; set; }
    }

    public EditFormIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<EditForm> RenderForm(TestModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderComponent<EditForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.ChildContent, (EditContext editContext) => builder =>
            {
                onEditContext?.Invoke(editContext);

                builder.OpenComponent<DataAnnotationsValidator>(0);
                builder.CloseComponent();

                builder.OpenComponent<WaInput>(1);
                builder.AddComponentParameter(2, nameof(WaInput.Value), model.Name);
                builder.AddComponentParameter(3, nameof(WaInput.ValueChanged),
                    EventCallback.Factory.Create<string?>(this, value => model.Name = value));
                builder.AddComponentParameter(4, nameof(WaInput.ValueExpression),
                    (Expression<Func<string?>>)(() => model.Name));
                builder.AddComponentParameter(5, nameof(WaInput.Class), "user-class");
                builder.CloseComponent();
            }));
    }

    private IRenderedComponent<EditForm> RenderCheckboxForm(TestModel model)
    {
        return RenderComponent<EditForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.ChildContent, (EditContext editContext) => builder =>
            {
                builder.OpenComponent<WaCheckbox>(0);
                builder.AddComponentParameter(1, nameof(WaCheckbox.Value), model.Accepted);
                builder.AddComponentParameter(2, nameof(WaCheckbox.ValueChanged),
                    EventCallback.Factory.Create<bool>(this, value => model.Accepted = value));
                builder.AddComponentParameter(3, nameof(WaCheckbox.ValueExpression),
                    (Expression<Func<bool>>)(() => model.Accepted));
                builder.CloseComponent();
            }));
    }

    #endregion
}
