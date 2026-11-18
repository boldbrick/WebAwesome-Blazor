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
/// One image or video of a <see cref="WaLightbox"/> gallery. Corresponds to the wa-lightbox-item Web Awesome
/// component (experimental).
/// </summary>
/// <remarks>
/// The default content is the item's thumbnail, usually an img; clicking it opens the lightbox at this item. Leave it
/// empty for an item that only appears in the lightbox (then set <see cref="Thumbnail"/> for the thumbnail strip).
/// </remarks>
public class WaLightboxItem : ComponentBase
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
    /// The Web Awesome default of <see cref="Alt"/>: what the element holds while the parameter is null, and what is
    /// rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultAlt = "";

    /// <summary>
    /// Alternative text for the full-size media. The element falls back to the slotted thumbnail's alt.
    /// </summary>
    [Parameter] public string? Alt { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Caption"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultCaption = "";

    /// <summary>
    /// A plain text caption to show with the item in the lightbox.
    /// </summary>
    [Parameter] public string? Caption { get; set; }

    /// <summary>
    /// The intrinsic height of the full-size media in pixels. Set it with <see cref="Width"/> so the lightbox can
    /// size the slide before the media loads.
    /// </summary>
    [Parameter] public int? Height { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Sizes"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultSizes = "";

    /// <summary>
    /// The sizes to use with <see cref="SrcSet"/>. When omitted, the lightbox uses the viewport width, and the zoomed
    /// width once the image is zoomed in.
    /// </summary>
    [Parameter] public string? Sizes { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Src"/>: what the element holds while the parameter is null, and what is
    /// rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultSrc = "";

    /// <summary>
    /// The URL of the full-size image or video. The thumbnail links to it, so the item still works without
    /// JavaScript and when opened in a new tab.
    /// </summary>
    [Parameter] public string? Src { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="SrcSet"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultSrcSet = "";

    /// <summary>
    /// A set of image sources for the full-size image, in the same format as the srcset attribute of an img.
    /// </summary>
    [Parameter] public string? SrcSet { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Thumbnail"/>: what the element holds while the parameter is null, and
    /// what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultThumbnail = "";

    /// <summary>
    /// The URL of a thumbnail to use in the lightbox's thumbnail strip and as a placeholder while the full-size media
    /// loads. Only needed when no img is slotted in.
    /// </summary>
    [Parameter] public string? Thumbnail { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Type"/>, which renders no attribute until the parameter first differs
    /// from it.
    /// </summary>
    public const WaLightboxItemType DefaultType = WaLightboxItemType.Image;

    /// <summary>
    /// The kind of media the item shows in the lightbox. Videos show a play button over their thumbnail.
    /// </summary>
    [Parameter] public WaLightboxItemType Type { get; set; } = DefaultType;

    /// <summary>
    /// The intrinsic width of the full-size media in pixels. Set it with <see cref="Height"/> so the lightbox can
    /// size the slide before the media loads.
    /// </summary>
    [Parameter] public int? Width { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The item's thumbnail, usually an img. Clicking it opens the lightbox at this item.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Optional play icon to use instead of the default, rendered into the element's "play-icon" slot. Works best
    /// with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? PlayIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="PlayIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? PlayIconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-lightbox-item");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add item-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(attributes, 10, "alt", Alt, DefaultAlt);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 11, "caption", Caption, DefaultCaption);
        builder.AddAttributeIfNotNull(12, "height", Height);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 13, "sizes", Sizes, DefaultSizes);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 14, "src", Src, DefaultSrc);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 15, "srcset", SrcSet, DefaultSrcSet);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 16, "thumbnail", Thumbnail, DefaultThumbnail);
        builder.AddDefaultedAttribute(attributes, 17, "type", Type.ToHtmlValue(), DefaultType.ToHtmlValue());
        builder.AddAttributeIfNotNull(18, "width", Width);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __itemReference => Element = __itemReference);

        // Add thumbnail content
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        // Add play icon slot content
        if (PlayIconContent is not null)
        {
            builder.AddSlotContent(50, "play-icon", PlayIconContent);
        }
        else
        {
            builder.AddIconSlot(55, "play-icon", PlayIconName);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets focus on the item's trigger.
    /// </summary>
    /// <param name="preventScroll">When true, the browser does not scroll the focused trigger into view.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task FocusAsync(bool preventScroll = false)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the lightbox item: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus", new { preventScroll });
    }

    #endregion

    #region ------ Internals ------

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
