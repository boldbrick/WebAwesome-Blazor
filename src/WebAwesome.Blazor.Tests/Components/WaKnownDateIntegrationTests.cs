using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Imperative method guard clauses of the WaKnownDate wrapper (new in WA 3.8.0): calling a method before the
/// first render must throw. Its attributes and defaults are covered by RenderedAttributeParityTests and its
/// events by EventCallbackBindingParityTests, both against the CEM.
/// </summary>
public class WaKnownDateIntegrationTests
{
    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaKnownDate();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaKnownDate();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
    }
}
