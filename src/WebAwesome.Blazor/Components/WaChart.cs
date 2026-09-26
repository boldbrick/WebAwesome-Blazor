namespace WebAwesome.Blazor.Components;

/// <summary>
/// A flexible wrapper around Chart.js for building themed data visualizations, supporting
/// advanced configuration such as mixed chart types, custom plugins, and direct Chart.js access.
/// Corresponds to the wa-chart Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaChart : WaChartBase
{
    /// <summary>
    /// The Web Awesome default of <see cref="WaChartBase.Type"/> on this chart: what the element holds while the parameter
    /// is null, and what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaChartType DefaultType = WaChartType.Bar;

    /// <inheritdoc />
    protected override WaChartType ElementDefaultType => DefaultType;

    /// <inheritdoc />
    protected override string TagName => "wa-chart";
}
