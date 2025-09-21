using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A tooltip component that displays additional information based on a specific action.
/// Corresponds to the wa-tooltip Web Awesome component.
/// </summary>
public class WaTooltip : ComponentBase
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

    // Tooltip properties
    [Parameter] public string? For { get; set; }
    [Parameter] public WaPlacement Placement { get; set; } = WaPlacement.Top;
    [Parameter] public WaTrigger Trigger { get; set; } = WaTrigger.Hover;
    [Parameter] public bool Open { get; set; }
    [Parameter] public bool WithoutArrow { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The tooltip content
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-tooltip");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add tooltip-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "for", For);
        if (Placement != WaPlacement.Top)
            builder.AddAttribute(11, "placement", Placement.ToHtmlValue());
        if (Trigger != WaTrigger.Hover)
            builder.AddAttribute(12, "trigger", Trigger.ToHtmlValue());
        builder.AddAttribute(13, "open", Open);
        builder.AddAttribute(14, "without-arrow", WithoutArrow);

        // Add event handlers
        if (OnShow.HasDelegate)
            builder.AddAttribute(20, "wa-show", OnShow);

        if (OnHide.HasDelegate)
            builder.AddAttribute(21, "wa-hide", OnHide);

        // Add element reference capture
        builder.AddElementReferenceCapture(22, __tooltipReference => Element = __tooltipReference);

        // Add content
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

// TODO: Tooltip positioning and interaction
// Tooltips require JavaScript for proper positioning using libraries like Floating UI.
// Currently provides basic binding support. Advanced features like collision detection,
// dynamic positioning, and proper focus management require JS interop implementation.
