using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// Shared base for the Web Awesome chart components, providing the common Chart.js-backed
/// configuration surface (label, axes, legend, grid, value bounds, and display toggles).
/// Concrete chart wrappers supply the specific custom-element tag.
/// </summary>
public abstract class WaChartBase : ComponentBase
{
    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// A label for the chart, used for accessibility.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// A description of the chart, used for accessibility.
    /// </summary>
    [Parameter] public string? Description { get; set; }

    /// <summary>
    /// The type of chart to render. When unset, the specific chart component's own default type applies.
    /// </summary>
    [Parameter] public WaChartType? Type { get; set; }

    /// <summary>
    /// Which axes to show grid lines on.
    /// </summary>
    [Parameter] public WaChartGrid? Grid { get; set; }

    /// <summary>
    /// The base axis of the dataset. <c>x</c> for vertical bars and <c>y</c> for horizontal bars.
    /// </summary>
    [Parameter] public WaChartAxis? IndexAxis { get; set; }

    /// <summary>
    /// The position of the legend relative to the chart.
    /// </summary>
    [Parameter] public WaChartLegendPosition? LegendPosition { get; set; }

    /// <summary>
    /// The minimum value for the value axis.
    /// </summary>
    [Parameter] public double? Min { get; set; }

    /// <summary>
    /// The maximum value for the value axis.
    /// </summary>
    [Parameter] public double? Max { get; set; }

    /// <summary>
    /// Stacks datasets on top of each other along the value axis.
    /// </summary>
    [Parameter] public bool Stacked { get; set; }

    /// <summary>
    /// Disables chart animations.
    /// </summary>
    [Parameter] public bool WithoutAnimation { get; set; }

    /// <summary>
    /// Hides the legend.
    /// </summary>
    [Parameter] public bool WithoutLegend { get; set; }

    /// <summary>
    /// Hides tooltips over data points.
    /// </summary>
    [Parameter] public bool WithoutTooltip { get; set; }

    /// <summary>
    /// A label for the x-axis.
    /// </summary>
    [Parameter] public string? XLabel { get; set; }

    /// <summary>
    /// A label for the y-axis.
    /// </summary>
    [Parameter] public string? YLabel { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The chart's content, typically an optional <c>&lt;script type="application/json"&gt;</c> element
    /// containing the Chart.js configuration object.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, TagName);

        // common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // chart configuration attributes
        builder.AddAttributeIfNotNullOrEmpty(4, "label", Label);
        builder.AddAttributeIfNotNullOrEmpty(5, "description", Description);
        builder.AddAttributeIfNotNull(6, "type", Type?.ToHtmlValue());
        builder.AddAttributeIfNotNull(7, "grid", Grid?.ToHtmlValue());
        builder.AddAttributeIfNotNull(8, "index-axis", IndexAxis?.ToHtmlValue());
        builder.AddAttributeIfNotNull(9, "legend-position", LegendPosition?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(10, "min", Min?.ToString(CultureInfo.InvariantCulture));
        builder.AddAttributeIfNotNullOrEmpty(11, "max", Max?.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(12, "stacked", Stacked);
        builder.AddAttribute(13, "without-animation", WithoutAnimation);
        builder.AddAttribute(14, "without-legend", WithoutLegend);
        builder.AddAttribute(15, "without-tooltip", WithoutTooltip);
        builder.AddAttributeIfNotNullOrEmpty(16, "xLabel", XLabel);
        builder.AddAttributeIfNotNullOrEmpty(17, "yLabel", YLabel);

        // chart-specific attributes contributed by derived components
        AddExtraAttributes(builder, 18);

        // element reference capture
        builder.AddElementReferenceCapture(30, __chartReference => Element = __chartReference);

        // config script / content
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Interface for descendants ------

    /// <summary>
    /// The custom-element tag rendered by the concrete chart component (e.g. <c>wa-bar-chart</c>).
    /// </summary>
    protected abstract string TagName { get; }

    /// <summary>
    /// Emits attributes specific to a derived chart component. The default implementation emits none.
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The base sequence number reserved for derived attributes</param>
    protected virtual void AddExtraAttributes(RenderTreeBuilder builder, int sequence)
    {
    }

    #endregion

    #region ------ Internals ------

    private string GetCombinedCssClass()
    {
        return string.IsNullOrEmpty(Class) ? string.Empty : Class;
    }

    #endregion
}
