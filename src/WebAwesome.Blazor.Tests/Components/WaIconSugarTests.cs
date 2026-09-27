using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Render tests for the icon convenience ("icon sugar") parameters of components with icon-shaped slots: an
/// icon name renders a wa-icon into its slot, nothing renders without one, and a fragment for the same slot
/// wins over the name (the wrapper's own C#-side choice, which no CEM check sees).
/// </summary>
public class WaIconSugarTests : BunitContext
{
    public WaIconSugarTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void WaButton_IconNames_RenderWaIconsIntoStartAndEndSlots()
    {
        var cut = Render<WaButton>(parameters => parameters
            .Add(p => p.StartIconName, "star")
            .Add(p => p.EndIconName, "arrow-right"));

        Assert.Equal("star", cut.Find("wa-button > wa-icon[slot='start']").GetAttribute("name"));
        Assert.Equal("arrow-right", cut.Find("wa-button > wa-icon[slot='end']").GetAttribute("name"));
    }

    [Fact]
    public void WaButton_WithoutIconNames_RendersNoIcons()
    {
        var cut = Render<WaButton>();

        Assert.Empty(cut.FindAll("wa-icon"));
    }

    [Fact]
    public void WaButton_StartContent_WinsOverStartIconName()
    {
        var cut = Render<WaButton>(parameters => parameters
            .Add(p => p.StartContent, SlotProbe.Fragment)
            .Add(p => p.StartIconName, "star"));

        Assert.Equal("start", SlotProbe.SlotOf(cut.Find("wa-button")));
        Assert.Empty(cut.FindAll("wa-icon[slot='start']"));
    }

    [Fact]
    public void WaCallout_IconName_RendersWaIconIntoIconSlot()
    {
        var cut = Render<WaCallout>(parameters => parameters.Add(p => p.IconName, "info-circle"));

        Assert.Equal("info-circle", cut.Find("wa-callout > wa-icon[slot='icon']").GetAttribute("name"));
    }

    [Fact]
    public void WaCallout_WithoutIconName_RendersNoIcon()
    {
        var cut = Render<WaCallout>();

        Assert.Empty(cut.FindAll("wa-icon"));
    }

    [Fact]
    public void WaDetails_IconNames_RenderWaIconsIntoExpandAndCollapseSlots()
    {
        var cut = Render<WaDetails>(parameters => parameters
            .Add(p => p.ExpandIconName, "chevron-down")
            .Add(p => p.CollapseIconName, "chevron-up"));

        Assert.Equal("chevron-down", cut.Find("wa-details > wa-icon[slot='expand-icon']").GetAttribute("name"));
        Assert.Equal("chevron-up", cut.Find("wa-details > wa-icon[slot='collapse-icon']").GetAttribute("name"));
    }

    [Fact]
    public void WaDetails_CollapseIcon_WinsOverCollapseIconName()
    {
        var cut = Render<WaDetails>(parameters => parameters
            .Add(p => p.CollapseIcon, SlotProbe.Fragment)
            .Add(p => p.CollapseIconName, "chevron-up"));

        Assert.Equal("collapse-icon", SlotProbe.SlotOf(cut.Find("wa-details")));
        Assert.Empty(cut.FindAll("wa-icon[slot='collapse-icon']"));
    }

    [Fact]
    public void WaCombobox_IconNames_RenderWaIconsIntoStartAndEndSlots()
    {
        string? value = null;
        var cut = Render<WaCombobox>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .Add(p => p.StartIconName, "search")
            .Add(p => p.EndIconName, "chevron-down"));

        Assert.Equal("search", cut.Find("wa-combobox > wa-icon[slot='start']").GetAttribute("name"));
        Assert.Equal("chevron-down", cut.Find("wa-combobox > wa-icon[slot='end']").GetAttribute("name"));
    }
}
