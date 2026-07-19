using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A badge component used to draw attention and display statuses or counts.
/// Corresponds to the wa-badge Web Awesome component.
/// </summary>
public class WaBadge : ComponentBase
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

    // Visual properties
    /// <summary>
    /// The badge's theme variant. Defaults to <c>brand</c> if not within another element with a variant.
    /// </summary>
    [Parameter] public WaVariant? Variant { get; set; }

    /// <summary>
    /// The badge's visual appearance.
    /// </summary>
    [Parameter] public WaAppearance? Appearance { get; set; }

    /// <summary>
    /// Draws a pill-style badge with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Adds an animation to draw attention to the badge.
    /// </summary>
    [Parameter] public WaAttention? Attention { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The badge's content (text/count)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-badge");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNull(4, "variant", Variant?.ToHtmlValue());
        builder.AddAttributeIfNotNull(5, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttribute(6, "pill", Pill);
        builder.AddAttributeIfNotNull(7, "attention", Attention?.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(10, __badgeReference => Element = __badgeReference);

        // Add child content
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
