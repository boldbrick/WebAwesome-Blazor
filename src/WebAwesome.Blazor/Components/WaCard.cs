using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A card component that can be used to group related subjects in a container.
/// Corresponds to the wa-card Web Awesome component.
/// </summary>
public class WaCard : ComponentBase
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

    // Card properties
    [Parameter] public WaAppearance Appearance { get; set; } = WaAppearance.Outlined;
    [Parameter] public bool WithHeader { get; set; }
    [Parameter] public bool WithFooter { get; set; }
    [Parameter] public bool WithMedia { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The main card content
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content for the card header
    /// </summary>
    [Parameter] public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Content for the card footer
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Media content for the card (images, videos, etc.)
    /// </summary>
    [Parameter] public RenderFragment? MediaContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-card");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add card-specific attributes
        if (Appearance != WaAppearance.Outlined)
            builder.AddAttribute(10, "appearance", Appearance.ToHtmlValue());
        builder.AddAttribute(11, "with-header", WithHeader || HeaderContent is not null);
        builder.AddAttribute(12, "with-footer", WithFooter || FooterContent is not null);
        builder.AddAttribute(13, "with-media", WithMedia || MediaContent is not null);

        // Add element reference capture
        builder.AddElementReferenceCapture(14, __cardReference => Element = __cardReference);

        // Add media slot content
        if (MediaContent is not null)
        {
            builder.OpenElement(20, "div");
            builder.AddAttribute(21, "slot", "media");
            builder.AddContent(22, MediaContent);
            builder.CloseElement();
        }

        // Add header slot content
        if (HeaderContent is not null)
        {
            builder.OpenElement(30, "div");
            builder.AddAttribute(31, "slot", "header");
            builder.AddContent(32, HeaderContent);
            builder.CloseElement();
        }

        // Add main content
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        // Add footer slot content
        if (FooterContent is not null)
        {
            builder.OpenElement(50, "div");
            builder.AddAttribute(51, "slot", "footer");
            builder.AddContent(52, FooterContent);
            builder.CloseElement();
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
