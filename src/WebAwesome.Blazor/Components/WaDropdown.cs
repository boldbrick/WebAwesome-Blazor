using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A dropdown component that exposes additional content in a panel when activated.
/// Corresponds to the wa-dropdown Web Awesome component.
/// </summary>
public class WaDropdown : ComponentBase
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

    // Dropdown properties
    [Parameter] public bool Open { get; set; }
    [Parameter] public WaPlacement Placement { get; set; } = WaPlacement.BottomStart;
    [Parameter] public int Distance { get; set; } = 8;
    [Parameter] public int Skidding { get; set; } = 0;

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The main dropdown content (dropdown items)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The trigger element that opens the dropdown
    /// </summary>
    [Parameter] public RenderFragment? TriggerContent { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }
    [Parameter] public EventCallback<EventArgs> OnSelect { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-dropdown");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add dropdown-specific attributes
        builder.AddAttribute(10, "open", Open);
        if (Placement != WaPlacement.BottomStart)
            builder.AddAttribute(11, "placement", Placement.ToHtmlValue());
        if (Distance != 8)
            builder.AddAttribute(12, "distance", Distance);
        if (Skidding != 0)
            builder.AddAttribute(13, "skidding", Skidding);

        // Add event handlers
        if (OnShow.HasDelegate)
            builder.AddAttribute(20, "wa-show", OnShow);

        if (OnHide.HasDelegate)
            builder.AddAttribute(21, "wa-hide", OnHide);

        if (OnSelect.HasDelegate)
            builder.AddAttribute(22, "wa-select", OnSelect);

        // Add element reference capture
        builder.AddElementReferenceCapture(23, __dropdownReference => Element = __dropdownReference);

        // Add trigger slot content
        if (TriggerContent is not null)
        {
            builder.OpenElement(30, "div");
            builder.AddAttribute(31, "slot", "trigger");
            builder.AddContent(32, TriggerContent);
            builder.CloseElement();
        }

        // Add main content (dropdown items)
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
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

// TODO: Dropdown positioning and interaction management
// Dropdowns require JavaScript for:
// - Popup positioning using Floating UI or similar
// - Outside-click detection for dismissal
// - Keyboard navigation (arrow keys, enter, escape)
// - Focus management and restoration
// - Submenu opening/closing on hover/click
// - wa-select event with proper detail data
// - Integration with data-dropdown declarative API
// Currently provides basic binding support. Advanced dropdown behaviors require JS interop.
