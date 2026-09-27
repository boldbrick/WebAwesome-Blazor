using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A bar chart that compares quantities across categories using rectangular bars.
/// Corresponds to the wa-bar-chart Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaBarChart : WaChartBase
{
    /// <summary>
    /// The Web Awesome default of <see cref="Orientation"/>: what the element holds while the parameter is null, and
    /// what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaOrientation DefaultOrientation = WaOrientation.Vertical;

    /// <summary>
    /// The orientation of the bars.
    /// </summary>
    [Parameter] public WaOrientation? Orientation { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="WaChartBase.Type"/> on this chart: what the element holds while the parameter
    /// is null, and what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaChartType DefaultType = WaChartType.Bar;

    /// <inheritdoc />
    protected override WaChartType ElementDefaultType => DefaultType;

    /// <inheritdoc />
    protected override string TagName => "wa-bar-chart";

    /// <inheritdoc />
    protected override void AddExtraAttributes(RenderTreeBuilder builder, int sequence)
    {
        builder.AddAttributeIfNotNull(WaAttributeMemory.Of(this), sequence, "orientation", Orientation?.ToHtmlValue(), DefaultOrientation.ToHtmlValue());
    }
}
