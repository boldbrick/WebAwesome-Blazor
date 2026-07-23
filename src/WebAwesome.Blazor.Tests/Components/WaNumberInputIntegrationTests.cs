using Bunit;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.Forms;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaNumberInput component introduced in Web Awesome 3.2.0.
/// Covers rendering, attribute emission, slots, and JS interop method wiring. WaNumberInput derives
/// from WaInputBase&lt;decimal?&gt;, so like the other WaInputBase-derived controls it requires a
/// cascading EditContext to render; tests use <see cref="FormControlTestBase.RenderControlForm{TComponent, TValue}"/>
/// for that purpose. EditForm binding and validation lifecycle are covered separately in
/// Forms/WaNumberInputEditFormTests.cs.
/// </summary>
public class WaNumberInputIntegrationTests : FormControlTestBase
{
    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        // Arrange & Act
        var model = new NumberModel();
        var cut = RenderControlForm<WaNumberInput, decimal?>(model, model.Amount, v => model.Amount = v, () => model.Amount);

        // Assert
        var element = cut.Find("wa-number-input");
        Assert.False(element.HasAttribute("appearance"));
        Assert.False(element.HasAttribute("autofocus"));
        Assert.False(element.HasAttribute("enterkeyhint"));
        Assert.False(element.HasAttribute("inputmode"));
        Assert.False(element.HasAttribute("max"));
        Assert.False(element.HasAttribute("min"));
        Assert.False(element.HasAttribute("pill"));
        Assert.False(element.HasAttribute("placeholder"));
        Assert.False(element.HasAttribute("step"));
        Assert.False(element.HasAttribute("without-steppers"));
    }

    [Fact]
    public void Parameters_WhenSet_RenderExpectedAttributes()
    {
        // Arrange & Act
        var model = new NumberModel { Amount = 5m };
        var cut = RenderControlForm<WaNumberInput, decimal?>(model, model.Amount, v => model.Amount = v, () => model.Amount,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaNumberInput.Appearance), WaInputAppearance.Filled);
                builder.AddComponentParameter(11, nameof(WaNumberInput.AutoFocus), true);
                builder.AddComponentParameter(12, nameof(WaNumberInput.EnterKeyHint), "go");
                builder.AddComponentParameter(13, nameof(WaNumberInput.InputMode), "decimal");
                builder.AddComponentParameter(14, nameof(WaNumberInput.Max), 100m);
                builder.AddComponentParameter(15, nameof(WaNumberInput.Min), 0m);
                builder.AddComponentParameter(16, nameof(WaNumberInput.Pill), true);
                builder.AddComponentParameter(17, nameof(WaNumberInput.Placeholder), "Enter a number");
                builder.AddComponentParameter(18, nameof(WaNumberInput.Step), "0.5");
                builder.AddComponentParameter(19, nameof(WaNumberInput.WithoutSteppers), true);
            });

        // Assert
        var element = cut.Find("wa-number-input");
        Assert.Equal("5", element.GetAttribute("value"));
        Assert.Equal("filled", element.GetAttribute("appearance"));
        Assert.True(element.HasAttribute("autofocus"));
        Assert.Equal("go", element.GetAttribute("enterkeyhint"));
        Assert.Equal("decimal", element.GetAttribute("inputmode"));
        Assert.Equal("100", element.GetAttribute("max"));
        Assert.Equal("0", element.GetAttribute("min"));
        Assert.True(element.HasAttribute("pill"));
        Assert.Equal("Enter a number", element.GetAttribute("placeholder"));
        Assert.Equal("0.5", element.GetAttribute("step"));
        Assert.True(element.HasAttribute("without-steppers"));
    }

    [Fact]
    public void Slots_WhenProvided_RenderIntoNamedSlots()
    {
        // Arrange & Act
        var model = new NumberModel();
        var cut = RenderControlForm<WaNumberInput, decimal?>(model, model.Amount, v => model.Amount = v, () => model.Amount,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaNumberInput.StartContent),
                    (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "start")));
                builder.AddComponentParameter(11, nameof(WaNumberInput.EndContent),
                    (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "end")));
                builder.AddComponentParameter(12, nameof(WaNumberInput.IncrementIconContent),
                    (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "up")));
                builder.AddComponentParameter(13, nameof(WaNumberInput.DecrementIconContent),
                    (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "down")));
            });

        // Assert
        Assert.Equal("start", cut.Find("span[slot='start']").TextContent);
        Assert.Equal("end", cut.Find("span[slot='end']").TextContent);
        Assert.Equal("up", cut.Find("span[slot='increment-icon']").TextContent);
        Assert.Equal("down", cut.Find("span[slot='decrement-icon']").TextContent);
    }

    [Fact]
    public void IconNames_WhenProvidedWithoutContent_RenderWaIconIntoSlots()
    {
        // Arrange & Act
        var model = new NumberModel();
        var cut = RenderControlForm<WaNumberInput, decimal?>(model, model.Amount, v => model.Amount = v, () => model.Amount,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaNumberInput.StartIconName), "dollar-sign");
                builder.AddComponentParameter(11, nameof(WaNumberInput.EndIconName), "percent");
            });

        // Assert
        Assert.Equal("dollar-sign", cut.Find("wa-icon[slot='start']").GetAttribute("name"));
        Assert.Equal("percent", cut.Find("wa-icon[slot='end']").GetAttribute("name"));
    }

    [Fact]
    public void OnInvalid_WhenWired_ReceivesDomEvent()
    {
        // Arrange
        var invalidCount = 0;
        var model = new NumberModel();
        var cut = RenderControlForm<WaNumberInput, decimal?>(model, model.Amount, v => model.Amount = v, () => model.Amount,
            builder => builder.AddComponentParameter(10, nameof(WaNumberInput.OnInvalid),
                Microsoft.AspNetCore.Components.EventCallback.Factory.Create<EventArgs>(this, () => invalidCount++)));

        // Act
        cut.Find("wa-number-input").TriggerEvent("onwa-invalid", new EventArgs());

        // Assert
        Assert.Equal(1, invalidCount);
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaNumberInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
        Assert.Contains("Cannot focus the input before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaNumberInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
        Assert.Contains("Cannot blur the input before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task SelectAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaNumberInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.SelectAsync());
        Assert.Contains("Cannot select text before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task StepUpAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaNumberInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.StepUpAsync());
        Assert.Contains("Cannot step up before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task StepDownAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaNumberInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.StepDownAsync());
        Assert.Contains("Cannot step down before the component is rendered", exception.Message);
    }

    #region ------ Internals ------

    private class NumberModel
    {
        public decimal? Amount { get; set; }
    }

    #endregion
}
