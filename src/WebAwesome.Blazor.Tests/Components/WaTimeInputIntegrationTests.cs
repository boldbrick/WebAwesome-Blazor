using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the WaTimeInput wrapper (new in WA 3.8.0): its hour-format member mapping as rendered, its content
/// fragments in their slots of wa-time-input, and its imperative methods throwing before the first render. Its
/// other attributes and defaults are covered by RenderedAttributeParityTests and its events by
/// EventCallbackBindingParityTests, both against the CEM.
/// </summary>
public class WaTimeInputIntegrationTests : BunitContext
{
    public WaTimeInputIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(WaTimeHourFormat.Auto, "auto")]
    [InlineData(WaTimeHourFormat.Twelve, "12")]
    [InlineData(WaTimeHourFormat.TwentyFour, "24")]
    public void HourFormat_RendersItsHourCycle(WaTimeHourFormat hourFormat, string expected)
    {
        // an intentional C# naming choice: the CEM checks see that "12"/"24" are in the union, not that Twelve
        // renders "12" rather than "24"
        string? value = null;
        var cut = Render<WaTimeInput>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .Add(p => p.HourFormat, hourFormat));

        Assert.Equal(expected, cut.Find("wa-time-input").GetAttribute("hour-format"));
    }

    [Theory]
    [InlineData(nameof(WaTimeInput.StartContent), "start")]
    [InlineData(nameof(WaTimeInput.EndContent), "end")]
    [InlineData(nameof(WaTimeInput.ClearIconContent), "clear-icon")]
    [InlineData(nameof(WaTimeInput.ExpandIconContent), "expand-icon")]
    [InlineData(nameof(WaTimeInput.FooterContent), "footer")]
    [InlineData(nameof(WaTimeInput.MarkupLabel), "label")]
    [InlineData(nameof(WaTimeInput.MarkupHint), "hint")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        string? value = null;
        var cut = Render<WaTimeInput>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-time-input")));
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaTimeInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaTimeInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
    }

    [Fact]
    public async Task ShowAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaTimeInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ShowAsync());
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaTimeInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.HideAsync());
    }
}
