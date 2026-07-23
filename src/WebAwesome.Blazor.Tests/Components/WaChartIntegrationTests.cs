using Bunit;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the chart component family introduced in Web Awesome 3.3.0
/// (WaChart and its presets). The shared surface lives on WaChartBase; these tests exercise
/// it through the concrete wrappers and verify each preset renders its own tag.
/// </summary>
public class WaChartIntegrationTests : BunitContext
{
    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        var cut = Render<WaChart>();

        var element = cut.Find("wa-chart");
        Assert.False(element.HasAttribute("label"));
        Assert.False(element.HasAttribute("description"));
        Assert.False(element.HasAttribute("type"));
        Assert.False(element.HasAttribute("grid"));
        Assert.False(element.HasAttribute("index-axis"));
        Assert.False(element.HasAttribute("legend-position"));
        Assert.False(element.HasAttribute("min"));
        Assert.False(element.HasAttribute("max"));
        Assert.False(element.HasAttribute("xLabel"));
        Assert.False(element.HasAttribute("yLabel"));
        // boolean attributes are omitted when false
        Assert.False(element.HasAttribute("stacked"));
        Assert.False(element.HasAttribute("without-animation"));
        Assert.False(element.HasAttribute("without-legend"));
        Assert.False(element.HasAttribute("without-tooltip"));
    }

    [Fact]
    public void EachPreset_RendersItsOwnTag()
    {
        Assert.NotNull(Render<WaChart>().Find("wa-chart"));
        Assert.NotNull(Render<WaBarChart>().Find("wa-bar-chart"));
        Assert.NotNull(Render<WaBubbleChart>().Find("wa-bubble-chart"));
        Assert.NotNull(Render<WaDoughnutChart>().Find("wa-doughnut-chart"));
        Assert.NotNull(Render<WaLineChart>().Find("wa-line-chart"));
        Assert.NotNull(Render<WaPieChart>().Find("wa-pie-chart"));
        Assert.NotNull(Render<WaPolarAreaChart>().Find("wa-polar-area-chart"));
        Assert.NotNull(Render<WaRadarChart>().Find("wa-radar-chart"));
        Assert.NotNull(Render<WaScatterChart>().Find("wa-scatter-chart"));
    }

    [Fact]
    public void Attributes_WhenSet_RenderOnBarChart()
    {
        var cut = Render<WaBarChart>(parameters => parameters
            .Add(p => p.Label, "Quarterly Revenue")
            .Add(p => p.Description, "Revenue by quarter")
            .Add(p => p.Type, WaChartType.Bar)
            .Add(p => p.Grid, WaChartGrid.Both)
            .Add(p => p.IndexAxis, WaChartAxis.Y)
            .Add(p => p.LegendPosition, WaChartLegendPosition.Bottom)
            .Add(p => p.Min, 0)
            .Add(p => p.Max, 100)
            .Add(p => p.Stacked, true)
            .Add(p => p.WithoutLegend, true)
            .Add(p => p.XLabel, "Quarter")
            .Add(p => p.YLabel, "USD")
            .Add(p => p.Orientation, WaOrientation.Horizontal));

        var element = cut.Find("wa-bar-chart");
        Assert.Equal("Quarterly Revenue", element.GetAttribute("label"));
        Assert.Equal("Revenue by quarter", element.GetAttribute("description"));
        Assert.Equal("bar", element.GetAttribute("type"));
        Assert.Equal("both", element.GetAttribute("grid"));
        Assert.Equal("y", element.GetAttribute("index-axis"));
        Assert.Equal("bottom", element.GetAttribute("legend-position"));
        Assert.Equal("0", element.GetAttribute("min"));
        Assert.Equal("100", element.GetAttribute("max"));
        Assert.True(element.HasAttribute("stacked"));
        Assert.True(element.HasAttribute("without-legend"));
        Assert.Equal("Quarter", element.GetAttribute("xLabel"));
        Assert.Equal("USD", element.GetAttribute("yLabel"));
        Assert.Equal("horizontal", element.GetAttribute("orientation"));
    }

    [Fact]
    public void Orientation_IsUniqueToBarChart()
    {
        // the base chart surface does not expose orientation; only WaBarChart does
        Assert.Null(typeof(WaChart).GetProperty("Orientation"));
        Assert.NotNull(typeof(WaBarChart).GetProperty("Orientation"));
    }

    [Fact]
    public void Min_And_Max_UseInvariantCulture()
    {
        var cut = Render<WaLineChart>(parameters => parameters
            .Add(p => p.Min, 1.5)
            .Add(p => p.Max, 99.75));

        var element = cut.Find("wa-line-chart");
        Assert.Equal("1.5", element.GetAttribute("min"));
        Assert.Equal("99.75", element.GetAttribute("max"));
    }

    [Fact]
    public void ChildContent_RendersConfigScript()
    {
        var cut = Render<WaChart>(parameters => parameters
            .AddChildContent("<script type=\"application/json\">{\"type\":\"bar\"}</script>"));

        Assert.Contains("application/json", cut.Markup);
    }

    [Fact]
    public void Class_And_Style_AreApplied()
    {
        var cut = Render<WaPieChart>(parameters => parameters
            .Add(p => p.Class, "custom-class")
            .Add(p => p.Style, "height: 20rem;"));

        var element = cut.Find("wa-pie-chart");
        Assert.Equal("custom-class", element.GetAttribute("class"));
        Assert.Equal("height: 20rem;", element.GetAttribute("style"));
    }
}
