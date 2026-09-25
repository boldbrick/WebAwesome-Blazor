using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Render tests for the content slots of the WaComparison component (wa-comparison, new in Web Awesome 3.0.0):
/// each fragment parameter must render into its named slot, ChildContent into the default slot. The position
/// attribute is covered by RenderedAttributeParityTests and the change event by EventCallbackBindingParityTests,
/// both against the CEM; Class, Style and unmatched attributes by CommonParameterRenderingTests.
/// </summary>
public class WaComparisonIntegrationTests : BunitContext
{
    public WaComparisonIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(nameof(WaComparison.ChildContent), "")]
    [InlineData(nameof(WaComparison.BeforeContent), "before")]
    [InlineData(nameof(WaComparison.AfterContent), "after")]
    [InlineData(nameof(WaComparison.HandleContent), "handle")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        var cut = Render<WaComparison>(parameters => parameters.TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-comparison")));
    }
}
