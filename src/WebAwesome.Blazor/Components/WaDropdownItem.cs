using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A dropdown item component that represents an individual menu item within a dropdown.
/// Corresponds to the wa-dropdown-item Web Awesome component.
/// </summary>
public class WaDropdownItem : ComponentBase
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

    // Dropdown item properties
    [Parameter] public string? Value { get; set; }
    [Parameter] public WaDropdownItemType Type { get; set; } = WaDropdownItemType.Normal;
    [Parameter] public bool Checked { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public WaVariant? Variant { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The main dropdown item content (label)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Icon content to display at the start of the item
    /// </summary>
    [Parameter] public RenderFragment? IconContent { get; set; }

    /// <summary>
    /// Details content to display at the end of the item (e.g., keyboard shortcuts)
    /// </summary>
    [Parameter] public RenderFragment? DetailsContent { get; set; }

    /// <summary>
    /// Submenu content for nested dropdown items
    /// </summary>
    [Parameter] public RenderFragment? SubmenuContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-dropdown-item");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add dropdown item-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "value", Value);
        if (Type != WaDropdownItemType.Normal)
            builder.AddAttribute(11, "type", Type.ToHtmlValue());
        builder.AddAttribute(12, "checked", Checked);
        builder.AddAttribute(13, "disabled", Disabled);
        builder.AddAttributeIfNotNull(14, "variant", Variant?.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(15, __dropdownItemReference => Element = __dropdownItemReference);

        // Add icon slot content
        if (IconContent is not null)
        {
            builder.OpenElement(20, "span");
            builder.AddAttribute(21, "slot", "icon");
            builder.AddContent(22, IconContent);
            builder.CloseElement();
        }

        // Add main content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        // Add details slot content
        if (DetailsContent is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "details");
            builder.AddContent(42, DetailsContent);
            builder.CloseElement();
        }

        // Add submenu slot content
        if (SubmenuContent is not null)
        {
            builder.OpenElement(50, "div");
            builder.AddAttribute(51, "slot", "submenu");
            builder.AddContent(52, SubmenuContent);
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
