using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A button group component that visually groups related buttons together.
/// Corresponds to the wa-button-group Web Awesome component.
/// </summary>
/// <remarks>
/// To set button sizes, apply the size property to individual WaButton components
/// within the group rather than using a group-level size property.
/// </remarks>
public class WaButtonGroup : ComponentBase
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
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
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

    // Button group properties
    /// <summary>
    /// A label for the button group. This is not displayed on the screen, but it is announced by assistive devices
    /// when interacting with the control and is strongly recommended.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// The button group's orientation.
    /// </summary>
    [Parameter] public WaOrientation? Orientation { get; set; }

    /// <summary>
    /// The button group's theme variant. Defaults to <c>neutral</c> if not within another element with a variant.
    /// </summary>
    [Parameter] public WaVariant? Variant { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The button group's content (typically WaButton components)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-button-group");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "label", Label);
        builder.AddAttributeIfNotNull(5, "orientation", Orientation?.ToHtmlValue());
        builder.AddAttributeIfNotNull(6, "variant", Variant?.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(10, __buttonGroupReference => Element = __buttonGroupReference);

        // Add child content (buttons)
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
