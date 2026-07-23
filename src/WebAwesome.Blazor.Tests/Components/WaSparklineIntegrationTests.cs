using Bunit;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaSparkline component introduced in Web Awesome 3.2.0.
/// </summary>
public class WaSparklineIntegrationTests : BunitContext
{
    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        // Arrange & Act
        var cut = Render<WaSparkline>();

        // Assert
        var element = cut.Find("wa-sparkline");
        Assert.False(element.HasAttribute("appearance"));
        Assert.False(element.HasAttribute("curve"));
        Assert.False(element.HasAttribute("data"));
        Assert.False(element.HasAttribute("label"));
        Assert.False(element.HasAttribute("trend"));
    }

    [Fact]
    public void Data_WhenSet_RendersDataAttribute()
    {
        // Arrange & Act
        var cut = Render<WaSparkline>(parameters => parameters
            .Add(p => p.Data, "10 20 40 25 35")
            .Add(p => p.Label, "Weekly sales"));

        // Assert
        var element = cut.Find("wa-sparkline");
        Assert.Equal("10 20 40 25 35", element.GetAttribute("data"));
        Assert.Equal("Weekly sales", element.GetAttribute("label"));
    }

    [Theory]
    [InlineData(WaSparklineAppearance.Gradient, "gradient")]
    [InlineData(WaSparklineAppearance.Line, "line")]
    [InlineData(WaSparklineAppearance.Solid, "solid")]
    public void Appearance_MapsToHtmlValue(WaSparklineAppearance appearance, string expected)
    {
        // Arrange & Act
        var cut = Render<WaSparkline>(parameters => parameters.Add(p => p.Appearance, appearance));

        // Assert
        Assert.Equal(expected, cut.Find("wa-sparkline").GetAttribute("appearance"));
    }

    [Theory]
    [InlineData(WaSparklineCurve.Linear, "linear")]
    [InlineData(WaSparklineCurve.Natural, "natural")]
    [InlineData(WaSparklineCurve.Step, "step")]
    public void Curve_MapsToHtmlValue(WaSparklineCurve curve, string expected)
    {
        // Arrange & Act
        var cut = Render<WaSparkline>(parameters => parameters.Add(p => p.Curve, curve));

        // Assert
        Assert.Equal(expected, cut.Find("wa-sparkline").GetAttribute("curve"));
    }

    [Theory]
    [InlineData(WaSparklineTrend.Positive, "positive")]
    [InlineData(WaSparklineTrend.Negative, "negative")]
    [InlineData(WaSparklineTrend.Neutral, "neutral")]
    public void Trend_MapsToHtmlValue(WaSparklineTrend trend, string expected)
    {
        // Arrange & Act
        var cut = Render<WaSparkline>(parameters => parameters.Add(p => p.Trend, trend));

        // Assert
        Assert.Equal(expected, cut.Find("wa-sparkline").GetAttribute("trend"));
    }

    [Fact]
    public void Class_And_Style_AreCombinedAndApplied()
    {
        // Arrange & Act
        var cut = Render<WaSparkline>(parameters => parameters
            .Add(p => p.Class, "custom-class")
            .Add(p => p.Style, "width: 200px;"));

        // Assert
        var element = cut.Find("wa-sparkline");
        Assert.Equal("custom-class", element.GetAttribute("class"));
        Assert.Equal("width: 200px;", element.GetAttribute("style"));
    }
}
