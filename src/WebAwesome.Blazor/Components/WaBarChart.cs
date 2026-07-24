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
    /// The orientation of the bars.
    /// </summary>
    [Parameter] public WaOrientation? Orientation { get; set; }

    /// <inheritdoc />
    protected override string TagName => "wa-bar-chart";

    /// <inheritdoc />
    protected override void AddExtraAttributes(RenderTreeBuilder builder, int sequence)
    {
        builder.AddAttributeIfNotNull(sequence, "orientation", Orientation?.ToHtmlValue());
    }
}
