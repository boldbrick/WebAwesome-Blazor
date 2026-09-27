namespace WebAwesome.Blazor.Components;

/// <summary>
/// A radar chart that compares multiple variables at once by plotting data on a radial grid.
/// Corresponds to the wa-radar-chart Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaRadarChart : WaChartBase
{
    /// <summary>
    /// The Web Awesome default of <see cref="WaChartBase.Type"/> on this chart: what the element holds while the parameter
    /// is null, and what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaChartType DefaultType = WaChartType.Radar;

    /// <inheritdoc />
    protected override WaChartType ElementDefaultType => DefaultType;

    /// <inheritdoc />
    protected override string TagName => "wa-radar-chart";
}
