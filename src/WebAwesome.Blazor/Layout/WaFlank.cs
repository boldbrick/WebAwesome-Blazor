using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Layout;

/// <summary>
/// A layout utility component that positions two items side-by-side, with one item flanking content that stretches to fill available space.
/// Corresponds to the wa-flank CSS utility class.
/// </summary>
public class WaFlank : WaLayoutBase
{
    #region ------ Properties ------

    /// <summary>
    /// Position of the flanking item relative to the main content
    /// </summary>
    [Parameter] public FlankPosition Position { get; set; } = FlankPosition.Start;

    /// <summary>
    /// Target size for the flanking item (uses --flank-size CSS custom property)
    /// </summary>
    [Parameter] public string? FlankSize { get; set; }

    /// <summary>
    /// Minimum percentage of container width that main content should occupy before wrapping (uses --content-percentage CSS custom property)
    /// </summary>
    [Parameter] public string? ContentPercentage { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", GetCombinedStyle());
        builder.AddElementReferenceCapture(4, __elementReference => Element = __elementReference);

        if (ChildContent is not null)
        {
            builder.AddContent(5, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Gets the CSS class string with flank utility and modifiers
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        // Base flank class with position modifier
        var baseClass = Position == FlankPosition.Start ? "wa-flank" : $"wa-flank:{Position.ToHtmlValue()}";
        classes.Add(baseClass);

        // Add gap class if specified
        if (Gap.HasValue)
            classes.Add($"wa-gap-{Gap.Value.ToHtmlValue()}");

        // Add align-items class if specified
        if (AlignItems.HasValue)
            classes.Add($"wa-align-items-{AlignItems.Value.ToHtmlValue()}");

        // Add user classes
        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    /// <summary>
    /// Gets the combined style string including user styles and flank-specific CSS custom properties
    /// </summary>
    private string GetCombinedStyle()
    {
        var styles = new List<string>();

        if (!string.IsNullOrEmpty(FlankSize))
            styles.Add($"--flank-size: {FlankSize}");

        if (!string.IsNullOrEmpty(ContentPercentage))
            styles.Add($"--content-percentage: {ContentPercentage}");

        if (!string.IsNullOrEmpty(Style))
            styles.Add(Style);

        return string.Join("; ", styles);
    }

    #endregion
}
