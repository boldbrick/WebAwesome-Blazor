using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Layout;

/// <summary>
/// A layout component that arranges elements into rows and columns that automatically adapt to the available space.
/// Corresponds to the wa-grid utility class.
/// </summary>
public class WaGrid : WaLayoutBase
{
    #region ------ Properties ------

    /// <summary>
    /// The minimum column size. Can be specified in any CSS unit (e.g., "20ch", "200px", "6rem").
    /// Default is "20ch" as per Web Awesome documentation.
    /// </summary>
    [Parameter] public string? MinColumnSize { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass("wa-grid"));
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
    /// Gets the combined style string including user styles and minimum column size
    /// </summary>
    private string GetCombinedStyle()
    {
        var styles = new List<string>();

        if (!string.IsNullOrEmpty(MinColumnSize))
            styles.Add($"--min-column-size: {MinColumnSize}");

        if (!string.IsNullOrEmpty(Style))
            styles.Add(Style);

        return string.Join("; ", styles);
    }

    #endregion
}
