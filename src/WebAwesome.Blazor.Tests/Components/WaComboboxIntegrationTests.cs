using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the WaCombobox component: its content fragments render into their slots of wa-combobox, and its
/// imperative methods throw before the first render. Its attributes and defaults are covered by
/// RenderedAttributeParityTests and its events by EventCallbackBindingParityTests, both against the CEM; the icon
/// names by WaIconSugarTests; multiple selection by MultipleSelectionBindingTests.
/// </summary>
public class WaComboboxIntegrationTests : BunitContext
{
    public WaComboboxIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(nameof(WaCombobox.ChildContent), "")]
    [InlineData(nameof(WaCombobox.StartContent), "start")]
    [InlineData(nameof(WaCombobox.EndContent), "end")]
    [InlineData(nameof(WaCombobox.ClearIconContent), "clear-icon")]
    [InlineData(nameof(WaCombobox.ExpandIconContent), "expand-icon")]
    [InlineData(nameof(WaCombobox.MarkupLabel), "label")]
    [InlineData(nameof(WaCombobox.MarkupHint), "hint")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        string? value = null;
        var cut = Render<WaCombobox>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-combobox")));
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaCombobox();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());

        Assert.Contains("Cannot blur: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaCombobox();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());

        Assert.Contains("Cannot focus: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaCombobox();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.HideAsync());

        Assert.Contains("Cannot hide: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ShowAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaCombobox();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.ShowAsync());

        Assert.Contains("Cannot show: component has not been rendered yet", exception.Message);
    }
}
