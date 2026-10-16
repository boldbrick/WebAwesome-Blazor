using System;
using System.Globalization;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions.DataGrid;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// The descriptors of the visual presets of phase 2 (design section 6.3): ProgressBar, ProgressRing, Rating, Avatar,
/// Image, ColorSwatch, Sparkline, Trend, DataBar and CopyButton.
/// </summary>
public class VisualPresetTests
{
    [Fact]
    public void ProgressBar_WritesTheScaleTheLabelAndTheThresholdsInAscendingOrder()
    {
        var bar = WaDataGridFormatters.ProgressBar(WaDataGridPercentScale.Percent, showLabel: true,
            [new WaDataGridThreshold(80, WaVariant.Success), new WaDataGridThreshold(0, WaVariant.Danger)], 1, "-", English);

        Assert.Equal(
            """{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.progressBar"},"args":{"culture":"en-US","nullText":"-","scale":"percent","showLabel":true,"thresholds":[{"from":0,"variant":"danger"},{"from":80,"variant":"success"}],"maxFractionDigits":1},"uses":["wa-progress-bar"]}""",
            bar.ToString());
    }

    [Fact]
    public void ProgressRing_WithoutOptions_WritesOnlyTheCulture()
    {
        var ring = WaDataGridFormatters.ProgressRing(culture: English);

        Assert.Equal("""{"culture":"en-US"}""", ArgsOf(ring));
        Assert.Equal(["wa-progress-ring"], ring.Tags);
    }

    [Fact]
    public void ProgressPresets_RejectANullThresholdOrDigitsOutOfRange()
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.ProgressBar(thresholds: [null!]));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.ProgressRing(maxFractionDigits: 101));
        Assert.Throws<ArgumentException>(() => new WaDataGridThreshold(double.NaN, WaVariant.Brand));
    }

    [Fact]
    public void Rating_WritesMaxPrecisionAndSize()
    {
        var rating = WaDataGridFormatters.Rating(10, 0.5, WaSize.Small, culture: English);

        Assert.Equal("""{"culture":"en-US","max":10,"precision":0.5,"size":"s"}""", ArgsOf(rating));
        Assert.Equal(["wa-rating"], rating.Tags);
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Rating(0));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Rating(precision: 0));
    }

    [Fact]
    public void Avatar_WritesTheFieldsTheShapeAndTheSize()
    {
        var avatar = WaDataGridFormatters.Avatar("photo.url", "initials", "name", WaAvatarShape.Rounded, showLabel: true, size: "2rem");

        Assert.Equal("""{"imageField":"photo.url","initialsField":"initials","labelField":"name","shape":"rounded","showLabel":true,"size":"2rem"}""", ArgsOf(avatar));
        Assert.Equal(["wa-avatar"], avatar.Tags);
    }

    [Theory]
    [InlineData("2rem; color: red")]
    [InlineData("{}")]
    [InlineData(" ")]
    public void Avatar_RejectsASizeThatIsNotALength(string size)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Avatar(size: size));
    }

    [Fact]
    public void Image_WritesTheTemplatesAndTheSize()
    {
        var image = WaDataGridFormatters.Image("/photos/{id}.jpg", "Photo of {name}", 32, 24, culture: English);

        Assert.Equal("""{"culture":"en-US","src":"/photos/{id}.jpg","alt":"Photo of {name}","width":32,"height":24}""", ArgsOf(image));
        Assert.Empty(image.Tags);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/png;base64,{data}")]
    [InlineData("mailto:{email}")]
    [InlineData("/photos/{id} .jpg")]
    public void Image_AllowsOnlyHttpHttpsAndRelativeUrls(string template)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Image(template));
    }

    [Fact]
    public void Image_RejectsANonPositiveSize()
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Image(width: 0));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Image(height: -1));
    }

    [Fact]
    public void ColorSwatchSparklineAndCopyButton_WriteTheirOptions()
    {
        Assert.Equal("""{"showText":true}""", ArgsOf(WaDataGridFormatters.ColorSwatch(showText: true)));

        var sparkline = WaDataGridFormatters.Sparkline(WaSparklineAppearance.Gradient, WaSparklineCurve.Natural, WaSparklineTrend.Positive,
            "trend", "Sales of {name}", culture: English);
        Assert.Equal("""{"culture":"en-US","appearance":"gradient","curve":"natural","trend":"positive","trendField":"trend","label":"Sales of {name}"}""", ArgsOf(sparkline));
        Assert.Equal(["wa-sparkline"], sparkline.Tags);

        var copy = WaDataGridFormatters.CopyButton(showValue: true, copyLabel: "Copy", successLabel: "Copied");
        Assert.Equal("""{"showValue":true,"copyLabel":"Copy","successLabel":"Copied"}""", ArgsOf(copy));
        Assert.Equal(["wa-copy-button"], copy.Tags);
    }

    [Fact]
    public void CopyButton_WithoutLabels_LeavesTheLocalizedDefaultsOfTheElement()
    {
        Assert.Equal("{}", ArgsOf(WaDataGridFormatters.CopyButton()));
    }

    [Fact]
    public void Trend_WritesTheDefaultsAndTheFormatOfATextPreset()
    {
        var trend = WaDataGridFormatters.Trend(culture: English);
        Assert.Equal(
            """{"culture":"en-US","positiveVariant":"success","negativeVariant":"danger","upIcon":{"name":"arrow-trend-up"},"downIcon":{"name":"arrow-trend-down"}}""",
            ArgsOf(trend));
        Assert.Equal(["wa-icon"], trend.Tags);

        var percent = WaDataGridFormatters.Trend(WaDataGridFormatters.Percent(maxFractionDigits: 1, culture: English), WaVariant.Brand, WaVariant.Warning, "up", "down", culture: English);
        Assert.Equal(
            """{"culture":"en-US","format":{"name":"wab.dataGrid.percent","args":{"culture":"en-US","maxFractionDigits":1}},"positiveVariant":"brand","negativeVariant":"warning","upIcon":{"name":"up"},"downIcon":{"name":"down"}}""",
            ArgsOf(percent));
    }

    [Fact]
    public void CompositePresets_RejectAFormatThatIsNotATextPreset()
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Trend(WaDataGridFormatters.Badge(new System.Collections.Generic.Dictionary<string, WaVariant>())));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.DataBar(format: WaJsFunction.Registered("myapp.format")));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.DataBar(format: WaDataGridFormatters.Template("{a}")));
        Assert.Throws<ArgumentException>(() => WaDataGridFooters.Sum(WaJsFunction.Module("./x.js")));
    }

    [Fact]
    public void DataBar_WritesTheLimitsTheVariantAndTheFormat()
    {
        var bar = WaDataGridFormatters.DataBar(0, 100, WaVariant.Success, showValue: true, WaDataGridFormatters.Number(culture: English), culture: English);

        Assert.Equal(
            """{"culture":"en-US","min":0,"max":100,"variant":"success","showValue":true,"format":{"name":"wab.dataGrid.number","args":{"culture":"en-US"}}}""",
            ArgsOf(bar));
        Assert.Empty(bar.Tags);
        Assert.Equal("""{"culture":"en-US"}""", ArgsOf(WaDataGridFormatters.DataBar(culture: English)));
    }

    [Fact]
    public void DataBar_RejectsLimitsThatAreNotFiniteOrInOrder()
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.DataBar(min: double.NaN));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.DataBar(max: double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.DataBar(10, 10));
    }

    #region ------ Internals ------

    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    private static string? ArgsOf(WaJsFunction function) => function.Args?.GetRawText();

    #endregion
}
