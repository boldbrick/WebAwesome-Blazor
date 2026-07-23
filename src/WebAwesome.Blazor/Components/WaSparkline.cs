using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A component that renders a small, simplified chart to visualize a trend in a data series.
/// Corresponds to the wa-sparkline Web Awesome component.
/// </summary>
public class WaSparkline : ComponentBase
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
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    // Common styling parameters
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Sparkline properties
    /// <summary>
    /// The sparkline's visual appearance.
    /// </summary>
    [Parameter] public WaSparklineAppearance? Appearance { get; set; }

    /// <summary>
    /// The type of curve used to connect the data points.
    /// </summary>
    [Parameter] public WaSparklineCurve? Curve { get; set; }

    /// <summary>
    /// The sparkline's data, expressed as a space-separated list of numbers (e.g. "10 20 40 25 35").
    /// </summary>
    [Parameter] public string? Data { get; set; }

    /// <summary>
    /// The label for assistive devices to announce.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// The trend the sparkline represents. Affects the color used to draw the sparkline.
    /// </summary>
    [Parameter] public WaSparklineTrend? Trend { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-sparkline");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add sparkline-specific attributes
        builder.AddAttributeIfNotNull(4, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttributeIfNotNull(5, "curve", Curve?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(6, "data", Data);
        builder.AddAttributeIfNotNullOrEmpty(7, "label", Label);
        builder.AddAttributeIfNotNull(8, "trend", Trend?.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __sparklineReference => Element = __sparklineReference);

        builder.CloseElement();
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Gets the CSS class string combining user classes
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    #endregion
}
