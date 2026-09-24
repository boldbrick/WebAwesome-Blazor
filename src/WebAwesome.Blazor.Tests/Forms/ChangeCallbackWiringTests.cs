using System;
using System.Collections.Generic;
using Bunit;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// Verifies the change callbacks the Web Awesome 3.12.0 bindings rewired to real events: WaCheckbox and
/// WaSwitch OnCheckedChange, WaRadioGroup.OnValueChange and WaSlider.OnValueChange used to listen for a
/// "wa-change" event Web Awesome never dispatches, and now fire from the element's native change handling
/// ("onchange", or the "onnumericchange" alias for wa-slider's number-valued change) after the bound value
/// has been updated. Also covers range-mode WaSlider, which renders and binds MinValue/MaxValue without
/// @bind-Value. Checked state is read back from the element through the mocked interop module's
/// "getProperty", as the wrappers do in the browser.
/// </summary>
public class ChangeCallbackWiringTests : FormControlTestBase
{
    #region ------ WaCheckbox / WaSwitch OnCheckedChange ------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WaCheckbox_OnCheckedChange_FiresFromChange_WithReadBackState(bool elementChecked)
    {
        // Arrange
        SetupCheckedReadBack(elementChecked);
        var bound = !elementChecked;
        var observed = new List<(bool Reported, bool Model)>();
        var cut = Render<WaCheckbox>(p => p
            .Add(c => c.Value, bound)
            .Add(c => c.ValueChanged, value => bound = value)
            .Add(c => c.ValueExpression, () => bound)
            .Add(c => c.OnCheckedChange, value => observed.Add((value, bound))));

        // Act - the change event's own value is a static placeholder; the real state is read back
        cut.Find("wa-checkbox").Change(bool.TrueString);

        // Assert
        Assert.Equal((elementChecked, elementChecked), Assert.Single(observed));
        Assert.Equal(elementChecked, bound);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WaSwitch_OnCheckedChange_FiresFromChange_WithReadBackState(bool elementChecked)
    {
        // Arrange
        SetupCheckedReadBack(elementChecked);
        var bound = !elementChecked;
        var observed = new List<(bool Reported, bool Model)>();
        var cut = Render<WaSwitch>(p => p
            .Add(c => c.Value, bound)
            .Add(c => c.ValueChanged, value => bound = value)
            .Add(c => c.ValueExpression, () => bound)
            .Add(c => c.OnCheckedChange, value => observed.Add((value, bound))));

        // Act
        cut.Find("wa-switch").Change(bool.TrueString);

        // Assert
        Assert.Equal((elementChecked, elementChecked), Assert.Single(observed));
        Assert.Equal(elementChecked, bound);
    }

    [Fact]
    public void WaCheckbox_DoesNotBindWaChange()
    {
        // Arrange
        var bound = false;
        var cut = Render<WaCheckbox>(p => p
            .Add(c => c.Value, bound)
            .Add(c => c.ValueChanged, value => bound = value)
            .Add(c => c.ValueExpression, () => bound)
            .Add(c => c.OnCheckedChange, _ => { }));

        // Act & Assert - wa-checkbox never dispatches wa-change
        Assert.Throws<MissingEventHandlerException>(() => cut.Find("wa-checkbox").TriggerEvent("onwa-change", new EventArgs()));
    }

    #endregion

    #region ------ WaRadioGroup OnValueChange ------

    [Fact]
    public void WaRadioGroup_OnValueChange_FiresFromChange_AfterValueUpdate()
    {
        // Arrange
        string? bound = "a";
        var observed = new List<(string? Reported, string? Model)>();
        var cut = Render<WaRadioGroup>(p => p
            .Add(c => c.Value, bound)
            .Add(c => c.ValueChanged, value => bound = value)
            .Add(c => c.ValueExpression, () => bound)
            .Add(c => c.OnValueChange, value => observed.Add((value, bound))));

        // Act
        cut.Find("wa-radio-group").Change("b");

        // Assert
        Assert.Equal(("b", "b"), Assert.Single(observed));
        Assert.Equal("b", bound);
    }

    #endregion

    #region ------ WaSlider OnValueChange and range mode ------

    [Fact]
    public void WaSlider_OnValueChange_FiresFromNumericChange_AfterValueUpdate()
    {
        // Arrange
        decimal? bound = 50m;
        var observed = new List<(decimal? Reported, decimal? Model)>();
        var cut = Render<WaSlider>(p => p
            .Add(c => c.Value, bound)
            .Add(c => c.ValueChanged, value => bound = value)
            .Add(c => c.ValueExpression, () => bound)
            .Add(c => c.OnValueChange, value => observed.Add((value, bound))));

        // Act
        cut.Find("wa-slider").NumericChange("42.5");

        // Assert
        Assert.Equal(((decimal?)42.5m, (decimal?)42.5m), Assert.Single(observed));
        Assert.Equal(42.5m, bound);
    }

    [Fact]
    public void WaSlider_UnparsableNumericChange_LeavesValue_AndReportsNothing()
    {
        // Arrange
        decimal? bound = 50m;
        var reportCount = 0;
        var cut = Render<WaSlider>(p => p
            .Add(c => c.Value, bound)
            .Add(c => c.ValueChanged, value => bound = value)
            .Add(c => c.ValueExpression, () => bound)
            .Add(c => c.OnValueChange, _ => reportCount++));

        // Act
        cut.Find("wa-slider").NumericChange("not-a-number");

        // Assert
        Assert.Equal(0, reportCount);
        Assert.Equal(50m, bound);
    }

    [Fact]
    public void WaSlider_RangeMode_RendersWithoutValueExpression()
    {
        // Arrange & Act - no @bind-Value, so no ValueExpression is supplied
        var cut = Render<WaSlider>(p => p
            .Add(c => c.Range, true)
            .Add(c => c.MinValue, 10m)
            .Add(c => c.MaxValue, 90m));

        // Assert
        var element = cut.Find("wa-slider");
        Assert.True(element.HasAttribute("range"));
        Assert.Equal("10", element.GetAttribute("min-value"));
        Assert.Equal("90", element.GetAttribute("max-value"));
        Assert.False(element.HasAttribute("value"));
    }

    [Fact]
    public void WaSlider_RangeMode_NumericChange_UpdatesMinAndMaxValue()
    {
        // Arrange
        decimal? minValue = 10m;
        decimal? maxValue = 90m;
        var valueChangeCount = 0;
        var cut = Render<WaSlider>(p => p
            .Add(c => c.Range, true)
            .Add(c => c.MinValue, minValue)
            .Add(c => c.MaxValue, maxValue)
            .Add(c => c.MinValueChanged, value => minValue = value)
            .Add(c => c.MaxValueChanged, value => maxValue = value)
            .Add(c => c.OnValueChange, _ => valueChangeCount++));

        // Act - the numericchange alias delivers "<minValue>,<maxValue>" in range mode
        cut.Find("wa-slider").NumericChange("20,80");

        // Assert
        Assert.Equal(20m, minValue);
        Assert.Equal(80m, maxValue);
        Assert.Equal(20m, cut.Instance.MinValue);
        Assert.Equal(80m, cut.Instance.MaxValue);
        Assert.Equal(0, valueChangeCount);
    }

    #endregion

    #region ------ Internals ------

    private void SetupCheckedReadBack(bool elementChecked)
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.Setup<bool>("getProperty", i => Equals(i.Arguments[1], "checked")).SetResult(elementChecked);
    }

    #endregion
}
