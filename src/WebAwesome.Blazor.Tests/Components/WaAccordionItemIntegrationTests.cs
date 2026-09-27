using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the WaAccordionItem wrapper (new in WA 3.8.0): its content fragments render into their slots, and
/// its imperative methods throw before the first render. Its attributes and defaults are covered by the
/// CEM-driven RenderedAttributeParityTests.
/// </summary>
public class WaAccordionItemIntegrationTests : BunitContext
{
    public WaAccordionItemIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(nameof(WaAccordionItem.ChildContent), "")]
    [InlineData(nameof(WaAccordionItem.LabelContent), "label")]
    [InlineData(nameof(WaAccordionItem.IconContent), "icon")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        var cut = Render<WaAccordionItem>(parameters => parameters.TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-accordion-item")));
    }

    [Fact]
    public async Task ExpandAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordionItem();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ExpandAsync());
    }

    [Fact]
    public async Task CollapseAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordionItem();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.CollapseAsync());
    }

    [Fact]
    public async Task ToggleAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordionItem();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ToggleAsync());
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordionItem();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }
}
