using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Infrastructure;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A full-screen viewer for a gallery of images and videos, with navigation, zoom, a counter and an optional thumbnail
/// strip. Corresponds to the wa-lightbox Web Awesome component (experimental).
/// </summary>
/// <remarks>
/// <para>
/// The gallery items are <see cref="WaLightboxItem"/> components in <see cref="ChildContent"/>. Clicking an item's
/// thumbnail opens the lightbox at that item; <see cref="Open"/> and <see cref="Index"/> open it from code.
/// </para>
/// <para>
/// The element changes <see cref="Open"/> and <see cref="Index"/> itself (the user closes the lightbox or moves to
/// another item), and the parameters are not written back. Follow the element with <see cref="OnHide"/> or
/// <see cref="OnAfterHide"/> and <see cref="OnItemChange"/> when the page keeps its own state.
/// </para>
/// <para>
/// Custom lightbox content that Web Awesome takes from a <c>&lt;template&gt;</c> inside an item is not supported:
/// Blazor renders the children of a template element as ordinary child nodes, not into the template's content.
/// </para>
/// </remarks>
public class WaLightbox : ComponentBase
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
    /// A collection of additional attributes that will be applied to the created element.
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

    /// <summary>
    /// The Web Awesome default of <see cref="Index"/>: what the element holds while the parameter is unchanged, and
    /// what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const int DefaultIndex = 0;

    /// <summary>
    /// The zero-based index of the current item. Set this before opening to choose which item the lightbox starts
    /// on. The element changes it itself while the user navigates; <see cref="OnItemChange"/> reports the change.
    /// </summary>
    [Parameter] public int Index { get; set; } = DefaultIndex;

    /// <summary>
    /// The Web Awesome default of <see cref="Label"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLabel = "";

    /// <summary>
    /// A label that describes the lightbox to assistive devices, for example "Vacation photos".
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Wraps around when navigating past the first or last item with the buttons or the keyboard.
    /// </summary>
    [Parameter] public bool Loop { get; set; }

    /// <summary>
    /// Indicates whether or not the lightbox is open. Toggle this to show and hide the lightbox. The element also
    /// closes itself (Escape, backdrop, close button, dragging the media away) without updating the parameter.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// Shows a strip of thumbnails for jumping between items.
    /// </summary>
    [Parameter] public bool WithThumbnails { get; set; }

    /// <summary>
    /// Hides the counter that shows the current item's position.
    /// </summary>
    [Parameter] public bool WithoutCounter { get; set; }

    /// <summary>
    /// Hides the previous and next buttons. Swiping and the arrow keys still work. The buttons are always hidden on
    /// touch screens, where swiping takes their place.
    /// </summary>
    [Parameter] public bool WithoutNavigation { get; set; }

    /// <summary>
    /// Disables zooming and removes the zoom button.
    /// </summary>
    [Parameter] public bool WithoutZoom { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the lightbox opens. The element's event is cancelable, but a .NET handler cannot cancel it.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked after the lightbox opens and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }

    /// <summary>
    /// Invoked when the lightbox is requested to close (Escape, the backdrop, the close button, dragging the media up
    /// or down, or closing from code). The element's event is cancelable, but a .NET handler cannot cancel it.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <summary>
    /// Invoked after the lightbox closes and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

    /// <summary>
    /// Invoked when the current item changes.
    /// </summary>
    [Parameter] public EventCallback<WaLightboxItemChangeEventArgs> OnItemChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The gallery: <see cref="WaLightboxItem"/> components, along with any markup needed to lay them out.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra buttons for the lightbox's toolbar, such as download or share, rendered into the element's "actions"
    /// slot. Works best with wa-button.
    /// </summary>
    [Parameter] public RenderFragment? ActionsContent { get; set; }

    /// <summary>
    /// Optional close icon to use instead of the default, rendered into the element's "close-icon" slot. Works best
    /// with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? CloseIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="CloseIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? CloseIconName { get; set; }

    /// <summary>
    /// Optional next icon to use instead of the default, rendered into the element's "next-icon" slot. Works best
    /// with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? NextIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="NextIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? NextIconName { get; set; }

    /// <summary>
    /// Optional previous icon to use instead of the default, rendered into the element's "previous-icon" slot. Works
    /// best with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? PreviousIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="PreviousIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? PreviousIconName { get; set; }

    /// <summary>
    /// Optional icon for the zoom button while the image can be zoomed in, rendered into the element's
    /// "zoom-in-icon" slot. Works best with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? ZoomInIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="ZoomInIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? ZoomInIconName { get; set; }

    /// <summary>
    /// Optional icon for the zoom button while the image is zoomed in, rendered into the element's "zoom-out-icon"
    /// slot. Works best with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? ZoomOutIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="ZoomOutIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? ZoomOutIconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-lightbox");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add lightbox-specific attributes
        builder.AddNumberAttribute(attributes, 10, "index", Index, DefaultIndex);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 11, "label", Label, DefaultLabel);
        builder.AddAttribute(12, "loop", Loop);
        builder.AddAttribute(13, "open", Open);
        builder.AddAttribute(14, "with-thumbnails", WithThumbnails);
        builder.AddAttribute(15, "without-counter", WithoutCounter);
        builder.AddAttribute(16, "without-navigation", WithoutNavigation);
        builder.AddAttribute(17, "without-zoom", WithoutZoom);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "onwa-show", OnShow);
        builder.AddAttributeIfHasDelegate(21, "onwa-after-show", OnAfterShow);
        builder.AddAttributeIfHasDelegate(22, "onwa-hide", OnHide);
        builder.AddAttributeIfHasDelegate(23, "onwa-after-hide", OnAfterHide);
        builder.AddAttributeIfHasDelegate(24, "onwa-item-change", OnItemChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __lightboxReference => Element = __lightboxReference);

        // Add main content
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        // Add slot content
        builder.AddSlotContent(50, "actions", ActionsContent);
        AddIconSlotContent(builder, 60, "close-icon", CloseIconContent, CloseIconName);
        AddIconSlotContent(builder, 70, "next-icon", NextIconContent, NextIconName);
        AddIconSlotContent(builder, 80, "previous-icon", PreviousIconContent, PreviousIconName);
        AddIconSlotContent(builder, 90, "zoom-in-icon", ZoomInIconContent, ZoomInIconName);
        AddIconSlotContent(builder, 100, "zoom-out-icon", ZoomOutIconContent, ZoomOutIconName);

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Shows the next item.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task NextAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show the next item: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "next");
    }

    /// <summary>
    /// Shows the previous item.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task PreviousAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show the previous item: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "previous");
    }

    /// <summary>
    /// Zooms in on the current image.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ZoomInAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot zoom in: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "zoomIn");
    }

    /// <summary>
    /// Zooms out of the current image.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ZoomOutAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot zoom out: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "zoomOut");
    }

    #endregion

    #region ------ Internals ------

    // renders an icon slot: the fragment wins over the icon name shortcut
    private static void AddIconSlotContent(RenderTreeBuilder builder, int sequence, string slotName, RenderFragment? content, string? iconName)
    {
        if (content is not null) builder.AddSlotContent(sequence, slotName, content);
        else builder.AddIconSlot(sequence + 5, slotName, iconName);
    }

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
