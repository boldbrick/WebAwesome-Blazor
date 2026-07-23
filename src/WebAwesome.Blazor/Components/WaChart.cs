namespace WebAwesome.Blazor.Components;

/// <summary>
/// A flexible wrapper around Chart.js for building themed data visualizations, supporting
/// advanced configuration such as mixed chart types, custom plugins, and direct Chart.js access.
/// Corresponds to the wa-chart Web Awesome component.
/// </summary>
public class WaChart : WaChartBase
{
    /// <inheritdoc />
    protected override string TagName => "wa-chart";
}
