using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the WaDateInput wrapper (Pro; new in WA 3.8.0): its content fragments render into their slots of
/// wa-date-input, and its imperative methods throw before the first render. Its attributes and defaults are
/// covered by RenderedAttributeParityTests and its events by EventCallbackBindingParityTests, both against the CEM.
/// </summary>
public class WaDateInputIntegrationTests : BunitContext
{
    public WaDateInputIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(nameof(WaDateInput.StartContent), "start")]
    [InlineData(nameof(WaDateInput.EndContent), "end")]
    [InlineData(nameof(WaDateInput.ClearIconContent), "clear-icon")]
    [InlineData(nameof(WaDateInput.ExpandIconContent), "expand-icon")]
    [InlineData(nameof(WaDateInput.FooterContent), "footer")]
    [InlineData(nameof(WaDateInput.PreviousIconContent), "previous-icon")]
    [InlineData(nameof(WaDateInput.NextIconContent), "next-icon")]
    [InlineData(nameof(WaDateInput.MarkupLabel), "label")]
    [InlineData(nameof(WaDateInput.MarkupHint), "hint")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        DateOnly? value = null;
        var cut = Render<WaDateInput>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-date-input")));
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
    }

    [Fact]
    public async Task ClearAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ClearAsync());
    }

    [Fact]
    public async Task ShowAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ShowAsync());
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.HideAsync());
    }
}
