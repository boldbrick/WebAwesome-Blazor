using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Layout;

/// <summary>
/// A layout utility component that distributes two or more items evenly across available space, either in a row or a column.
/// Corresponds to the wa-split CSS utility class.
/// </summary>
public class WaSplit : WaLayoutBase
{
    #region ------ Properties ------

    /// <summary>
    /// Direction to split items (row or column)
    /// </summary>
    [Parameter] public SplitDirection Direction { get; set; } = SplitDirection.Row;

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
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
    /// Gets the CSS class string with split utility and modifiers
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        // Base split class with direction modifier
        var baseClass = Direction == SplitDirection.Row ? "wa-split" : $"wa-split:{Direction.ToHtmlValue()}";
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

    #endregion
}
