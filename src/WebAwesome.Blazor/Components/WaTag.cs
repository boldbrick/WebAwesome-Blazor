using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A tag component used as labels to organize things or to indicate a selection.
/// Corresponds to the wa-tag Web Awesome component.
/// </summary>
public class WaTag : ComponentBase
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

    // Common styling parameters
    /// <summary>
    /// Additional CSS class names applied to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Tag properties
    /// <summary>
    /// The tag's theme variant.
    /// </summary>
    [Parameter] public WaVariant Variant { get; set; } = WaVariant.Neutral;

    /// <summary>
    /// The tag's visual appearance.
    /// </summary>
    [Parameter] public WaAppearance Appearance { get; set; } = WaAppearance.OutlinedFilled;

    /// <summary>
    /// The tag's size.
    /// </summary>
    [Parameter] public WaSize Size { get; set; } = WaSize.Medium;

    /// <summary>
    /// Draws a pill-style tag with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Makes the tag removable and shows a remove button.
    /// </summary>
    [Parameter] public bool WithRemove { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The main tag content (label)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the remove button is activated.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnRemove { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-tag");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add tag-specific attributes
        if (Variant != WaVariant.Neutral)
            builder.AddAttribute(10, "variant", Variant.ToHtmlValue());
        if (Appearance != WaAppearance.OutlinedFilled)
            builder.AddAttribute(11, "appearance", Appearance.ToHtmlValue());
        if (Size != WaSize.Medium)
            builder.AddAttribute(12, "size", Size.ToHtmlValue());
        builder.AddAttribute(13, "pill", Pill);
        builder.AddAttribute(14, "with-remove", WithRemove);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "wa-remove", OnRemove);

        // Add element reference capture
        builder.AddElementReferenceCapture(21, __tagReference => Element = __tagReference);

        // Add main content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
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
