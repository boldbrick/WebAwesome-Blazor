namespace WebAwesome.Blazor.Components;

/// <summary>
/// A bubble chart that adds a third dimension to scatter plots by varying the size of each data point.
/// Corresponds to the wa-bubble-chart Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaBubbleChart : WaChartBase
{
    /// <summary>
    /// The Web Awesome default of <see cref="WaChartBase.Type"/> on this chart: what the element holds while the parameter
    /// is null, and what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaChartType DefaultType = WaChartType.Bubble;

    /// <inheritdoc />
    protected override WaChartType ElementDefaultType => DefaultType;

    /// <inheritdoc />
    protected override string TagName => "wa-bubble-chart";
}
