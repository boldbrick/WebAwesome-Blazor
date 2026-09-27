using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A callout component used to display important messages inline.
/// Corresponds to the wa-callout Web Awesome component.
/// </summary>
public class WaCallout : ComponentBase
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

    // Callout properties
    /// <summary>
    /// The Web Awesome default of <see cref="Variant"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaVariant DefaultVariant = WaVariant.Brand;

    /// <summary>
    /// The callout's theme variant. Null (the default) leaves the element's default: brand, unless the callout is
    /// within another element with a variant.
    /// </summary>
    [Parameter] public WaVariant? Variant { get; set; }

    /// <summary>
    /// The callout's visual appearance. Null (the default) leaves the element's own styling.
    /// </summary>
    [Parameter] public WaAppearance? Appearance { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Size"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const WaSize DefaultSize = WaSize.Medium;

    /// <summary>
    /// The callout's size.
    /// </summary>
    [Parameter] public WaSize Size { get; set; } = DefaultSize;

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The main callout content (message)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Icon content to display at the start of the callout
    /// </summary>
    [Parameter] public RenderFragment? IconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="IconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? IconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-callout");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add callout-specific attributes
        builder.AddAttributeIfNotNull(attributes, 10, "variant", Variant?.ToHtmlValue(), DefaultVariant.ToHtmlValue());
        builder.AddAttributeIfNotNull(11, "appearance", Appearance?.ToHtmlValue());
        builder.AddDefaultedAttribute(attributes, 12, "size", Size.ToHtmlValue(), DefaultSize.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(13, __calloutReference => Element = __calloutReference);

        // Add icon slot content
        if (IconContent is not null)
        {
            builder.OpenElement(20, "span");
            builder.AddAttribute(21, "slot", "icon");
            builder.AddContent(22, IconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(25, "icon", IconName);
        }

        // Add main content (message)
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
