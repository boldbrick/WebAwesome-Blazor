using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Imperative method guard clauses of the WaAccordion wrapper (new in WA 3.8.0): calling a method before the
/// first render must throw. Its attributes and defaults are covered by RenderedAttributeParityTests and its
/// expand/collapse events by EventCallbackBindingParityTests, both against the CEM.
/// </summary>
public class WaAccordionIntegrationTests
{
    [Fact]
    public async Task ExpandAllAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordion();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ExpandAllAsync());
    }

    [Fact]
    public async Task CollapseAllAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordion();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.CollapseAllAsync());
    }
}
