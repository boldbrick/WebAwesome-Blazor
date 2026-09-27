using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Render tests for the WaStepperStep content slots: ChildContent (the label, default slot), DescriptionContent
/// ("description") and IconContent ("icon") each render into their named slot of wa-step, and the IconName
/// convenience renders a wa-icon into the icon slot but yields to IconContent when both are set (pattern of
/// WaIconSugarTests/WaCardEnhancementTests). Attributes and defaults are covered by the CEM-driven
/// RenderedAttributeParityTests.
/// </summary>
public class WaStepperStepIntegrationTests : BunitContext
{
    public WaStepperStepIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(nameof(WaStepperStep.ChildContent), "")]
    [InlineData(nameof(WaStepperStep.DescriptionContent), "description")]
    [InlineData(nameof(WaStepperStep.IconContent), "icon")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        var cut = Render<WaStepperStep>(parameters => parameters.TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-step")));
    }

    [Fact]
    public void IconName_RendersWaIconIntoIconSlot()
    {
        var cut = Render<WaStepperStep>(parameters => parameters.Add(p => p.IconName, "star"));

        Assert.Equal("star", cut.Find("wa-step > wa-icon[slot='icon']").GetAttribute("name"));
    }

    [Fact]
    public void WithoutIconNameOrIconContent_RendersNoIcon()
    {
        var cut = Render<WaStepperStep>();

        Assert.Empty(cut.FindAll("wa-icon"));
    }

    [Fact]
    public void IconContent_WinsOverIconName()
    {
        var cut = Render<WaStepperStep>(parameters => parameters
            .Add(p => p.IconContent, SlotProbe.Fragment)
            .Add(p => p.IconName, "star"));

        Assert.Equal("icon", SlotProbe.SlotOf(cut.Find("wa-step")));
        Assert.Empty(cut.FindAll("wa-icon[slot='icon']"));
    }
}
