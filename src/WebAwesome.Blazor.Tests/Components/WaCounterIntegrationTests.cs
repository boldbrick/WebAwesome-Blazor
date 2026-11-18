using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaCounter component (Web Awesome 3.15.0): each unit and the finished fragment
/// parameter renders into the slot of its own name (the slot parity check sees only that a slot exists, not which
/// parameter feeds it). The ValueFormatterFunction push is covered by ElementJsPropertyTests, attributes and defaults
/// by RenderedAttributeParityTests.
/// </summary>
public class WaCounterIntegrationTests : BunitContext
{
    public WaCounterIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(nameof(WaCounter.YearsContent), "years")]
    [InlineData(nameof(WaCounter.MonthsContent), "months")]
    [InlineData(nameof(WaCounter.DaysContent), "days")]
    [InlineData(nameof(WaCounter.HoursContent), "hours")]
    [InlineData(nameof(WaCounter.MinutesContent), "minutes")]
    [InlineData(nameof(WaCounter.SecondsContent), "seconds")]
    [InlineData(nameof(WaCounter.FinishedContent), "finished")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        // Arrange & Act
        var cut = Render<WaCounter>(parameters => parameters.TryAdd(parameterName, SlotProbe.Fragment));

        // Assert
        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-counter")));
    }

    [Fact]
    public void DefaultRender_RendersNoSlotContent()
    {
        // Arrange & Act
        var cut = Render<WaCounter>();

        // Assert
        Assert.Empty(cut.FindAll("[slot]"));
    }
}
