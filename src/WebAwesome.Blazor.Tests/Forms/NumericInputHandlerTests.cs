using System.Collections.Generic;
using Bunit;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// The C# half of OnInput on the number-valued wrappers (review X8): wa-slider's input event carries a JS number,
/// which Blazor's built-in input reader cannot carry, so WaSlider and WaRange bind OnInput to the JS initializer's
/// "numericinput" alias and hand the callback the value as a string, like the built-in input event would; the
/// handler is bound only when OnInput has a delegate. wa-rating dispatches no input event, so WaRating binds none.
/// The browser half (the alias delivering a real drag or key press) is event-dispatch.spec.js.
/// </summary>
public class NumericInputHandlerTests : FormControlTestBase
{
    [Fact]
    public void WaSlider_NumericInput_InvokesOnInputWithTheStringValue()
    {
        // Arrange
        var received = new List<object?>();
        var cut = Render<WaSlider>(p => p
            .Add(c => c.Value, 50m)
            .Add(c => c.ValueExpression, () => BoundSlider)
            .Add(c => c.OnInput, (ChangeEventArgs e) => received.Add(e.Value)));

        // Act
        cut.Find("wa-slider").NumericInput("51");

        // Assert
        Assert.Equal(new object?[] { "51" }, received);
    }

    [Fact]
    public void WaRange_RangeMode_NumericInput_InvokesOnInputWithBothValues()
    {
        // Arrange
        var received = new List<object?>();
        var cut = Render<WaRange>(p => p
            .Add(c => c.Range, true)
            .Add(c => c.MinValue, 20m)
            .Add(c => c.MaxValue, 80m)
            .Add(c => c.ValueExpression, () => BoundRange)
            .Add(c => c.OnInput, (ChangeEventArgs e) => received.Add(e.Value)));

        // Act
        cut.Find("wa-slider").NumericInput("21,80");

        // Assert
        Assert.Equal(new object?[] { "21,80" }, received);
    }

    [Fact]
    public void WaSlider_WithoutOnInput_BindsNoInputHandler()
    {
        // Act
        var cut = Render<WaSlider>(p => p
            .Add(c => c.Value, 50m)
            .Add(c => c.ValueExpression, () => BoundSlider));

        // Assert - neither the alias nor the built-in input event, which would reject the JS number
        var slider = cut.Find("wa-slider");
        Assert.False(slider.HasAttribute(NumericInputHandler));
        Assert.False(slider.HasAttribute(InputHandler));
    }

    [Fact]
    public void WaSlider_WithOnInput_BindsTheAliasNotTheBuiltInInputEvent()
    {
        // Act
        var cut = Render<WaSlider>(p => p
            .Add(c => c.Value, 50m)
            .Add(c => c.ValueExpression, () => BoundSlider)
            .Add(c => c.OnInput, (ChangeEventArgs _) => { }));

        // Assert
        var slider = cut.Find("wa-slider");
        Assert.True(slider.HasAttribute(NumericInputHandler));
        Assert.False(slider.HasAttribute(InputHandler));
    }

    [Fact]
    public void WaRating_WithOnInput_BindsNoInputHandler()
    {
        // Act
        var cut = Render<WaRating>(p => p
            .Add(c => c.Value, 2m)
            .Add(c => c.ValueExpression, () => BoundRating)
            .Add(c => c.OnInput, (ChangeEventArgs _) => { }));

        // Assert - wa-rating dispatches no input event
        var rating = cut.Find("wa-rating");
        Assert.False(rating.HasAttribute(NumericInputHandler));
        Assert.False(rating.HasAttribute(InputHandler));
    }

    #region ------ Internals ------

    // bUnit exposes each bound event handler as a "blazor:on<event>" markup attribute
    private const string NumericInputHandler = "blazor:onnumericinput";
    private const string InputHandler = "blazor:oninput";

    private decimal? BoundSlider { get; set; }

    private decimal BoundRange { get; set; }

    private decimal BoundRating { get; set; }

    #endregion
}
