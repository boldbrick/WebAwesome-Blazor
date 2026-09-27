using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// Verifies the opt-in Immediate binding of WaInput, WaTextArea and WaNumberInput (Web Awesome 3.12.0
/// bindings, GitHub issue #1): with Immediate=true a single "oninput" handler updates the bound value (and
/// the EditContext) on every keystroke and then invokes the user's OnInput, which therefore sees the current
/// value; with Immediate=false an OnInput delegate is the only "oninput" handler and the model changes only
/// on "onchange"; without either there is no "oninput" handler at all. Rendered inside an EditForm with
/// bUnit; the handler count is read from bUnit's markup, which renders each event handler attribute as
/// "blazor:on&lt;event&gt;".
/// </summary>
public class ImmediateBindingTests : FormControlTestBase
{
    #region ------ Immediate=true ------

    [Fact]
    public void WaInput_Immediate_SingleInputHandler_UpdatesValueThenInvokesOnInput()
    {
        // Arrange
        var model = new TextModel { Text = "Ada" };
        var observed = new List<(string? EventValue, string? ModelValue)>();
        EditContext? editContext = null;
        var cut = RenderText<WaInput>(model, immediate: true, onInput: args => observed.Add((args.Value as string, model.Text)),
            onEditContext: context => editContext = context);

        // Act
        cut.Find("wa-input").Input("Grace");

        // Assert - the value was already updated when OnInput ran
        Assert.Equal("Grace", model.Text);
        Assert.Equal(("Grace", "Grace"), Assert.Single(observed));
        Assert.True(editContext!.IsModified(() => model.Text));
        Assert.Equal(1, CountHandlers(cut.Markup, InputHandlerAttribute));
    }

    [Fact]
    public void WaTextArea_Immediate_SingleInputHandler_UpdatesValueThenInvokesOnInput()
    {
        // Arrange
        var model = new TextModel { Text = "Ada" };
        var observed = new List<(string? EventValue, string? ModelValue)>();
        var cut = RenderText<WaTextArea>(model, immediate: true, onInput: args => observed.Add((args.Value as string, model.Text)));

        // Act
        cut.Find("wa-textarea").Input("Grace Hopper");

        // Assert
        Assert.Equal("Grace Hopper", model.Text);
        Assert.Equal(("Grace Hopper", "Grace Hopper"), Assert.Single(observed));
        Assert.Equal(1, CountHandlers(cut.Markup, InputHandlerAttribute));
    }

    [Fact]
    public void WaNumberInput_Immediate_SingleInputHandler_UpdatesValueThenInvokesOnInput()
    {
        // Arrange
        var model = new NumberModel { Amount = 1m };
        var observed = new List<(string? EventValue, decimal? ModelValue)>();
        var cut = RenderControlForm<WaNumberInput, decimal?>(model, model.Amount, value => model.Amount = value, () => model.Amount,
            builder => ConfigureImmediate(builder, true, EventCallback.Factory.Create<ChangeEventArgs>(this,
                args => observed.Add((args.Value as string, model.Amount)))));

        // Act
        cut.Find("wa-number-input").Input("42");

        // Assert
        Assert.Equal(42m, model.Amount);
        Assert.Equal(("42", (decimal?)42m), Assert.Single(observed));
        Assert.Equal(1, CountHandlers(cut.Markup, InputHandlerAttribute));
    }

    [Fact]
    public void WaInput_Immediate_WithoutOnInput_StillUpdatesValueOnInput()
    {
        // Arrange
        var model = new TextModel { Text = "Ada" };
        var cut = RenderText<WaInput>(model, immediate: true, onInput: null);

        // Act
        cut.Find("wa-input").Input("Grace");

        // Assert
        Assert.Equal("Grace", model.Text);
    }

    [Fact]
    public void WaInput_Immediate_ChangeStillCommitsValue()
    {
        // Arrange - the change binder keeps running, so the committed value is exact
        var model = new TextModel { Text = "Ada" };
        var cut = RenderText<WaInput>(model, immediate: true, onInput: null);

        // Act
        cut.Find("wa-input").Input("Grac");
        cut.Find("wa-input").Change("Grace");

        // Assert
        Assert.Equal("Grace", model.Text);
    }

    #endregion

    #region ------ Immediate=false ------

    [Fact]
    public void WaInput_NotImmediate_OnInputOnly_ModelChangesOnCommit()
    {
        // Arrange
        var model = new TextModel { Text = "Ada" };
        var observed = new List<(string? EventValue, string? ModelValue)>();
        var cut = RenderText<WaInput>(model, immediate: false, onInput: args => observed.Add((args.Value as string, model.Text)));

        // Act
        cut.Find("wa-input").Input("Grace");
        var modelAfterInput = model.Text;
        cut.Find("wa-input").Change("Grace");

        // Assert
        Assert.Equal(("Grace", "Ada"), Assert.Single(observed));
        Assert.Equal("Ada", modelAfterInput);
        Assert.Equal("Grace", model.Text);
        Assert.Equal(1, CountHandlers(cut.Markup, InputHandlerAttribute));
    }

    [Fact]
    public void WaTextArea_NotImmediate_OnInputOnly_ModelChangesOnCommit()
    {
        // Arrange
        var model = new TextModel { Text = "Ada" };
        var inputCount = 0;
        var cut = RenderText<WaTextArea>(model, immediate: false, onInput: _ => inputCount++);

        // Act
        cut.Find("wa-textarea").Input("Grace");
        var modelAfterInput = model.Text;
        cut.Find("wa-textarea").Change("Grace");

        // Assert
        Assert.Equal(1, inputCount);
        Assert.Equal("Ada", modelAfterInput);
        Assert.Equal("Grace", model.Text);
    }

    [Fact]
    public void WaNumberInput_NotImmediate_OnInputOnly_ModelChangesOnCommit()
    {
        // Arrange
        var model = new NumberModel { Amount = 1m };
        var inputCount = 0;
        var cut = RenderControlForm<WaNumberInput, decimal?>(model, model.Amount, value => model.Amount = value, () => model.Amount,
            builder => ConfigureImmediate(builder, false, EventCallback.Factory.Create<ChangeEventArgs>(this, _ => inputCount++)));

        // Act
        cut.Find("wa-number-input").Input("42");
        var modelAfterInput = model.Amount;
        cut.Find("wa-number-input").Change("42");

        // Assert
        Assert.Equal(1, inputCount);
        Assert.Equal(1m, modelAfterInput);
        Assert.Equal(42m, model.Amount);
    }

    [Theory]
    [InlineData("wa-input")]
    [InlineData("wa-textarea")]
    [InlineData("wa-number-input")]
    public void NeitherImmediateNorOnInput_RendersNoInputHandler(string tag)
    {
        // Arrange & Act
        var cut = tag switch
        {
            "wa-input" => RenderText<WaInput>(new TextModel(), immediate: false, onInput: null),
            "wa-textarea" => RenderText<WaTextArea>(new TextModel(), immediate: false, onInput: null),
            _ => RenderNumber(new NumberModel())
        };

        // Assert
        Assert.Equal(0, CountHandlers(cut.Markup, InputHandlerAttribute));
        Assert.Throws<MissingEventHandlerException>(() => cut.Find(tag).Input("x"));
    }

    #endregion

    #region ------ Internals ------

    private const string InputHandlerAttribute = "blazor:oninput";

    private class TextModel
    {
        public string? Text { get; set; }
    }

    private class NumberModel
    {
        public decimal? Amount { get; set; }
    }

    private IRenderedComponent<EditForm> RenderText<TComponent>(TextModel model, bool immediate, Action<ChangeEventArgs>? onInput,
        Action<EditContext>? onEditContext = null)
        where TComponent : IComponent
    {
        EventCallback<ChangeEventArgs>? callback = onInput is null ? null : EventCallback.Factory.Create(this, onInput);
        return RenderControlForm<TComponent, string?>(model, model.Text, value => model.Text = value, () => model.Text,
            builder => ConfigureImmediate(builder, immediate, callback), onEditContext);
    }

    private IRenderedComponent<EditForm> RenderNumber(NumberModel model)
    {
        return RenderControlForm<WaNumberInput, decimal?>(model, model.Amount, value => model.Amount = value, () => model.Amount,
            builder => ConfigureImmediate(builder, false, null));
    }

    private static void ConfigureImmediate(RenderTreeBuilder builder, bool immediate, EventCallback<ChangeEventArgs>? onInput)
    {
        builder.AddComponentParameter(10, nameof(WaInput.Immediate), immediate);
        if (onInput.HasValue)
            builder.AddComponentParameter(11, nameof(WaInput.OnInput), onInput.Value);
    }

    private static int CountHandlers(string markup, string handlerAttribute)
        => Regex.Matches(markup, Regex.Escape(handlerAttribute) + "=").Count;

    #endregion
}
