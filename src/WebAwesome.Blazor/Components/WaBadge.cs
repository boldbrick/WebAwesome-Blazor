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
    /// May be <see langword="null"/> if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    // Common styling parameters
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    // Visual properties
    [Parameter] public WaVariant? Variant { get; set; }
    [Parameter] public WaAppearance? Appearance { get; set; }
    [Parameter] public bool Pill { get; set; }
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
