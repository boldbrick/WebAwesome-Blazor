namespace WebAwesome.Blazor.Components;

/// <summary>
/// A pie chart that displays the proportional composition of a whole as slices of a circle.
/// Corresponds to the wa-pie-chart Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaPieChart : WaChartBase
{
    /// <summary>
    /// The Web Awesome default of <see cref="WaChartBase.Type"/> on this chart: what the element holds while the parameter
    /// is null, and what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaChartType DefaultType = WaChartType.Pie;

    /// <inheritdoc />
    protected override WaChartType ElementDefaultType => DefaultType;

    /// <inheritdoc />
    protected override string TagName => "wa-pie-chart";
}
