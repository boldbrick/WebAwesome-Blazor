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

    // Card properties
    /// <summary>
    /// The card's visual appearance.
    /// </summary>
    [Parameter] public WaAppearance Appearance { get; set; } = WaAppearance.Outlined;

    /// <summary>
    /// The card's orientation.
    /// </summary>
    [Parameter] public WaOrientation? Orientation { get; set; }

    /// <summary>
    /// Renders the card with a header. This is set automatically when <see cref="HeaderContent"/> or
    /// <see cref="HeaderActionsContent"/> is provided.
    /// </summary>
    [Parameter] public bool WithHeader { get; set; }

    /// <summary>
    /// Renders the card with a footer. This is set automatically when <see cref="FooterContent"/> or
    /// <see cref="FooterActionsContent"/> is provided.
    /// </summary>
    [Parameter] public bool WithFooter { get; set; }

    /// <summary>
    /// Renders the card with a media section. This is set automatically when <see cref="MediaContent"/> is
    /// provided.
    /// </summary>
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
    /// Content for the card header actions
    /// </summary>
    [Parameter] public RenderFragment? HeaderActionsContent { get; set; }

    /// <summary>
    /// Content for the card footer
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Content for the card footer actions
    /// </summary>
    [Parameter] public RenderFragment? FooterActionsContent { get; set; }

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
        builder.AddAttributeIfNotNull(11, "orientation", Orientation?.ToHtmlValue());
        builder.AddAttribute(12, "with-header", WithHeader || HeaderContent is not null || HeaderActionsContent is not null);
        builder.AddAttribute(13, "with-footer", WithFooter || FooterContent is not null || FooterActionsContent is not null);
        builder.AddAttribute(14, "with-media", WithMedia || MediaContent is not null);

        // Add element reference capture
        builder.AddElementReferenceCapture(15, __cardReference => Element = __cardReference);

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

        // Add header actions slot content
        if (HeaderActionsContent is not null)
        {
            builder.OpenElement(35, "div");
            builder.AddAttribute(36, "slot", "header-actions");
            builder.AddContent(37, HeaderActionsContent);
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

        // Add footer actions slot content
        if (FooterActionsContent is not null)
        {
            builder.OpenElement(55, "div");
            builder.AddAttribute(56, "slot", "footer-actions");
            builder.AddContent(57, FooterActionsContent);
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
