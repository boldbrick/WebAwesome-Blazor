using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A divider component used to visually separate or group elements.
/// Corresponds to the wa-divider Web Awesome component.
/// </summary>
public class WaDivider : ComponentBase
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
    /// Additional CSS classes applied to the created element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline styles applied to the created element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Orientation"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const WaOrientation DefaultOrientation = WaOrientation.Horizontal;

    /// <summary>
    /// The divider's orientation.
    /// </summary>
    [Parameter] public WaOrientation Orientation { get; set; } = DefaultOrientation;

    /// <summary>
    /// The Web Awesome default of <see cref="LabelPlacement"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaDividerLabelPlacement DefaultLabelPlacement = WaDividerLabelPlacement.Center;

    /// <summary>
    /// Where the label (<see cref="ChildContent"/>) sits along the divider.
    /// </summary>
    [Parameter] public WaDividerLabelPlacement? LabelPlacement { get; set; }

    /// <summary>
    /// Only required for SSR. Set to true when slotting a label in (via <see cref="ChildContent"/>) so the
    /// server-rendered markup includes the label's layout before the component hydrates on the client; the
    /// element recomputes this from the slot itself once hydrated.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// An optional label to show along the divider (default slot).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-divider");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add divider-specific attributes
        builder.AddDefaultedAttribute(attributes, 10, "orientation", Orientation.ToHtmlValue(), DefaultOrientation.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 12, "label-placement", LabelPlacement?.ToHtmlValue(), DefaultLabelPlacement.ToHtmlValue());
        builder.AddAttribute(13, "with-label", WithLabel);

        // Add element reference capture
        builder.AddElementReferenceCapture(11, __dividerReference => Element = __dividerReference);

        // Add the label (default slot)
        if (ChildContent is not null)
        {
            builder.AddContent(20, ChildContent);
        }

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
