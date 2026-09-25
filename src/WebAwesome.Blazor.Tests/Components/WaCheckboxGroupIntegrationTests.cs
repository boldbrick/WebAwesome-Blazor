using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Render tests for the content slots of the WaCheckboxGroup wrapper (new in WA 3.9.0), a grouping wrapper
/// without value binding or events: ChildContent renders into the default slot, MarkupLabel and MarkupHint into
/// the label and hint slots. Its attributes and defaults are covered by the CEM-driven
/// RenderedAttributeParityTests.
/// </summary>
public class WaCheckboxGroupIntegrationTests : BunitContext
{
    public WaCheckboxGroupIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(nameof(WaCheckboxGroup.ChildContent), "")]
    [InlineData(nameof(WaCheckboxGroup.MarkupLabel), "label")]
    [InlineData(nameof(WaCheckboxGroup.MarkupHint), "hint")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        var cut = Render<WaCheckboxGroup>(parameters => parameters.TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-checkbox-group")));
    }
}
