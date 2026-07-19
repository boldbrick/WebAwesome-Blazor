using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A dropdown item component that represents an individual menu item within a dropdown.
/// Corresponds to the wa-dropdown-item Web Awesome component.
/// </summary>
public class WaDropdownItem : ComponentBase
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used to invoke methods on the underlying element.
    /// </summary>
    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

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
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    // Common styling parameters
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Dropdown item properties
    /// <summary>
    /// An optional value for the menu item. This is useful for determining which item was selected when
    /// listening to the dropdown's select event.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// Set to <see cref="WaDropdownItemType.Checkbox"/> to make the item a checkbox.
    /// </summary>
    [Parameter] public WaDropdownItemType Type { get; set; } = WaDropdownItemType.Normal;

    /// <summary>
    /// Checks the dropdown item. Only valid when <see cref="Type"/> is <see cref="WaDropdownItemType.Checkbox"/>.
    /// </summary>
    [Parameter] public bool Checked { get; set; }

    /// <summary>
    /// Disables the dropdown item.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The type of menu item to render.
    /// </summary>
    [Parameter] public WaVariant? Variant { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the dropdown item loses focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Emitted when the dropdown item gains focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

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
    /// Convenience alternative to <see cref="IconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? IconName { get; set; }

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

        // Add event handlers
        builder.AddAttributeIfHasDelegate(16, "blur", OnBlur);
        builder.AddAttributeIfHasDelegate(17, "focus", OnFocus);

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
        else
        {
            builder.AddIconSlot(25, "icon", IconName);
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

    #region ------ Public Methods ------

    /// <summary>
    /// Opens the submenu, if this item has one.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task OpenSubmenuAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot open submenu: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "openSubmenu");
    }

    /// <summary>
    /// Closes the submenu, if this item has one.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task CloseSubmenuAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot close submenu: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "closeSubmenu");
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
