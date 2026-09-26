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
/// <remarks>
/// This is a Pro component.
/// </remarks>
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
    /// The Web Awesome default of <see cref="Appearance"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaSparklineAppearance DefaultAppearance = WaSparklineAppearance.Solid;

    /// <summary>
    /// The sparkline's visual appearance.
    /// </summary>
    [Parameter] public WaSparklineAppearance? Appearance { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Curve"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaSparklineCurve DefaultCurve = WaSparklineCurve.Linear;

    /// <summary>
    /// The type of curve used to connect the data points.
    /// </summary>
    [Parameter] public WaSparklineCurve? Curve { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Data"/>: no data, rendered as the empty attribute in place of null or an empty
    /// list once the attribute has been rendered.
    /// </summary>
    public static readonly IReadOnlyList<double> DefaultData = [];

    /// <summary>
    /// The sparkline's data points, in order (e.g. <c>new[] { 10.0, 20, 40, 25, 35 }</c>), rendered in the invariant`n    /// culture and separated by a space; null or empty draws nothing.
    /// </summary>
    [Parameter] public IReadOnlyList<double>? Data { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Label"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLabel = "";

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
        var attributes = builder.OpenWaElement(this, 0, "wa-sparkline");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add sparkline-specific attributes
        builder.AddAttributeIfNotNull(attributes, 4, "appearance", Appearance?.ToHtmlValue(), DefaultAppearance.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 5, "curve", Curve?.ToHtmlValue(), DefaultCurve.ToHtmlValue());
        builder.AddNumberListAttribute(attributes, 6, "data", Data, DefaultData);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 7, "label", Label, DefaultLabel);
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
