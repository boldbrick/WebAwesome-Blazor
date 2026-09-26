namespace WebAwesome.Blazor.Components;

/// <summary>
/// A doughnut chart that displays proportional data as slices arranged in a ring with a hollow center.
/// Corresponds to the wa-doughnut-chart Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaDoughnutChart : WaChartBase
{
    /// <summary>
    /// The Web Awesome default of <see cref="WaChartBase.Type"/> on this chart: what the element holds while the parameter
    /// is null, and what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaChartType DefaultType = WaChartType.Doughnut;

    /// <inheritdoc />
    protected override WaChartType ElementDefaultType => DefaultType;

    /// <inheritdoc />
    protected override string TagName => "wa-doughnut-chart";
}
