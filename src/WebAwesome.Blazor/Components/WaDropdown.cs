using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A dropdown component that exposes additional content in a panel when activated.
/// Corresponds to the wa-dropdown Web Awesome component.
/// </summary>
public class WaDropdown : ComponentBase
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

    // Dropdown properties
    /// <summary>
    /// Opens or closes the dropdown.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// The placement of the dropdown menu in reference to the trigger. The menu will shift to a more optimal
    /// location if the preferred placement doesn't have enough room.
    /// </summary>
    [Parameter] public WaPlacement Placement { get; set; } = WaPlacement.BottomStart;

    /// <summary>
    /// The distance of the dropdown menu from its trigger.
    /// </summary>
    [Parameter] public int Distance { get; set; } = 8;

    /// <summary>
    /// The offset of the dropdown menu along its trigger.
    /// </summary>
    [Parameter] public int Skidding { get; set; } = 0;

    /// <summary>
    /// The size of dropdown items slotted into the default slot (i.e. <c>wa-dropdown-item</c>).
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

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

    /// <summary>
    /// Invoked when the dropdown is about to show.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked when the dropdown is about to hide.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <summary>
    /// Invoked when an item in the dropdown is selected.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnSelect { get; set; }

    /// <summary>
    /// Invoked after the dropdown opens and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }

    /// <summary>
    /// Invoked after the dropdown closes and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

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
        builder.AddAttributeIfNotNull(14, "size", Size?.ToHtmlValue());

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "wa-show", OnShow);

        builder.AddAttributeIfHasDelegate(21, "wa-hide", OnHide);

        builder.AddAttributeIfHasDelegate(22, "wa-select", OnSelect);

        builder.AddAttributeIfHasDelegate(50, "wa-after-show", OnAfterShow);

        builder.AddAttributeIfHasDelegate(51, "wa-after-hide", OnAfterHide);

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

    #region ------ Public Methods ------

    /// <summary>
    /// Shows the dropdown with dynamic positioning
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show dropdown: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "show");
    }

    /// <summary>
    /// Hides the dropdown with focus restoration
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide dropdown: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
    }

    /// <summary>
    /// Recalculates and updates the dropdown position
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task RepositionAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reposition dropdown: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "reposition");
    }

    #endregion
}
