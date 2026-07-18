using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An avatar component used to represent a person or object.
/// Corresponds to the wa-avatar Web Awesome component.
/// </summary>
public class WaAvatar : ComponentBase
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

    /// <summary>
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Avatar properties
    /// <summary>
    /// The image source to use for the avatar.
    /// </summary>
    [Parameter] public string? Image { get; set; }

    /// <summary>
    /// Initials to use as a fallback when no image is available (1-2 characters max recommended).
    /// </summary>
    [Parameter] public string? Initials { get; set; }

    /// <summary>
    /// Indicates how the browser should load the image.
    /// </summary>
    [Parameter] public WaLoading Loading { get; set; } = WaLoading.Eager;

    /// <summary>
    /// A label to use to describe the avatar to assistive devices.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// The shape of the avatar.
    /// </summary>
    [Parameter] public WaAvatarShape Shape { get; set; } = WaAvatarShape.Circle;

    #endregion

    #region ------ Content ------

    /// <summary>
    /// Custom icon content to display when no image or initials are provided
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
        builder.OpenElement(0, "wa-avatar");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add avatar-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "image", Image);
        builder.AddAttributeIfNotNullOrEmpty(11, "initials", Initials);
        if (Loading != WaLoading.Eager)
            builder.AddAttribute(12, "loading", Loading.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(13, "label", Label);
        if (Shape != WaAvatarShape.Circle)
            builder.AddAttribute(14, "shape", Shape.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(15, __avatarReference => Element = __avatarReference);

        // Add icon slot content if provided
        if (IconContent is not null)
        {
            builder.OpenElement(20, "span");
            builder.AddAttribute(21, "slot", "icon");
            builder.AddContent(22, IconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(30, "icon", IconName);
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
