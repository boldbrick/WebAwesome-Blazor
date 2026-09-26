using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A component that renders iframe content with zoom and interaction controls.
/// Corresponds to the wa-zoomable-frame Web Awesome component.
/// </summary>
/// <remarks>
/// Provides zoom controls and pan functionality for iframe content.
/// Default aspect ratio is 16:9, customizable via CSS aspect-ratio property.
/// </remarks>
public class WaZoomableFrame : ComponentBase
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used to call methods on the underlying Web Awesome element.
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

    // Common styling parameters
    /// <summary>
    /// Additional CSS class names applied to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Frame content properties
    /// <summary>
    /// The URL of the content to display. Ignored when <see cref="SrcDoc"/> is set.
    /// </summary>
    [Parameter] public string? Src { get; set; }

    /// <summary>
    /// Inline HTML to display. Takes precedence over <see cref="Src"/> when set.
    /// </summary>
    [Parameter] public string? SrcDoc { get; set; }

    // Zoom properties
    /// <summary>
    /// The Web Awesome default of <see cref="Zoom"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const double DefaultZoom = 1.0;

    /// <summary>
    /// The current zoom of the frame, e.g. 0 = 0% and 1 = 100%.
    /// </summary>
    [Parameter] public double Zoom { get; set; } = DefaultZoom;

    /// <summary>
    /// The Web Awesome default of <see cref="ZoomLevels"/>, 25% to 200% in steps of 25% (the element's "25% 50% ... 200%"):
    /// what the element holds while the parameter is null or empty, and what is rendered in its place once the attribute has
    /// been rendered.
    /// </summary>
    public static readonly IReadOnlyList<double> DefaultZoomLevels = [0.25, 0.5, 0.75, 1, 1.25, 1.5, 1.75, 2];

    /// <summary>
    /// The zoom levels to step through when using the zoom controls, as factors (<c>1</c> is 100%), in order; null or
    /// empty leaves the element's default, 25% to 200% in steps of 25%. Does not restrict programmatic changes to the zoom.
    /// </summary>
    [Parameter] public IReadOnlyList<double>? ZoomLevels { get; set; }

    // Control properties
    /// <summary>
    /// Removes the zoom controls.
    /// </summary>
    [Parameter] public bool WithoutControls { get; set; }

    /// <summary>
    /// Disables interaction with the frame content.
    /// </summary>
    [Parameter] public bool WithoutInteraction { get; set; }

    /// <summary>
    /// Enables automatic theme syncing (light/dark mode and theme selector classes) from the host document to the
    /// iframe.
    /// </summary>
    [Parameter] public bool WithThemeSync { get; set; }

    /// <summary>
    /// Whether the iframe is allowed to be displayed in fullscreen mode.
    /// </summary>
    [Parameter] public bool AllowFullScreen { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Loading"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaLoading DefaultLoading = WaLoading.Eager;

    /// <summary>
    /// Indicates when the browser should load the iframe.
    /// </summary>
    [Parameter] public WaLoading? Loading { get; set; }

    /// <summary>
    /// Indicates which referrer to send when fetching the frame's content.
    /// </summary>
    [Parameter] public WaReferrerPolicy? ReferrerPolicy { get; set; }

    /// <summary>
    /// Sandboxes the frame's content, lifting the given restrictions (e.g. <c>WaIframeSandbox.AllowScripts | WaIframeSandbox.AllowSameOrigin</c>); <see cref="WaIframeSandbox.None"/> applies all of them, null sets no sandbox.
    /// </summary>
    [Parameter] public WaIframeSandbox? Sandbox { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The zoom-in control icon to use instead of the default, rendered into the element's "zoom-in-icon" slot.
    /// </summary>
    [Parameter] public RenderFragment? ZoomInIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="ZoomInIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? ZoomInIconName { get; set; }

    /// <summary>
    /// The zoom-out control icon to use instead of the default, rendered into the element's "zoom-out-icon" slot.
    /// </summary>
    [Parameter] public RenderFragment? ZoomOutIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="ZoomOutIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? ZoomOutIconName { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the internal iframe finishes loading (native <c>load</c> event re-dispatched on the host
    /// element).
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnLoad { get; set; }

    /// <summary>
    /// Invoked when the internal iframe fails to load (native <c>error</c> event re-dispatched on the host
    /// element).
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnError { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-zoomable-frame");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add frame content attributes (srcdoc takes precedence over src)
        if (!string.IsNullOrEmpty(SrcDoc))
        {
            builder.AddAttribute(10, "srcdoc", SrcDoc);
        }
        else
        {
            builder.AddAttributeIfNotNullOrEmpty(10, "src", Src);
        }

        // Add zoom attributes
        builder.AddNumberAttribute(attributes, 20, "zoom", Zoom, DefaultZoom);
        builder.AddNumberListAttribute(attributes, 21, "zoom-levels", ZoomLevels, DefaultZoomLevels);

        // Add control attributes
        builder.AddAttribute(30, "without-controls", WithoutControls);
        builder.AddAttribute(31, "without-interaction", WithoutInteraction);
        builder.AddAttribute(36, "with-theme-sync", WithThemeSync);

        // Add remaining iframe passthrough attributes
        builder.AddAttribute(32, "allowfullscreen", AllowFullScreen);
        builder.AddAttributeIfNotNull(attributes, 33, "loading", Loading?.ToHtmlValue(), DefaultLoading.ToHtmlValue());
        builder.AddAttributeIfNotNull(34, "referrerpolicy", ReferrerPolicy?.ToHtmlValue());
        builder.AddAttributeIfNotNull(35, "sandbox", Sandbox?.ToHtmlValue());

        // native load/error events re-dispatched by wa-zoomable-frame on the host element (non-bubbling,
        // composed); delivered through Blazor's built-in non-bubbling event registration (no
        // registerCustomEventType needed)
        builder.AddAttributeIfHasDelegate(41, "onload", OnLoad);
        builder.AddAttributeIfHasDelegate(42, "onerror", OnError);

        // Add element reference capture
        builder.AddElementReferenceCapture(50, __frameReference => Element = __frameReference);

        // Add zoom-in icon slot content
        if (ZoomInIconContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "zoom-in-icon");
            builder.AddContent(62, ZoomInIconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(63, "zoom-in-icon", ZoomInIconName);
        }

        // Add zoom-out icon slot content
        if (ZoomOutIconContent is not null)
        {
            builder.OpenElement(70, "span");
            builder.AddAttribute(71, "slot", "zoom-out-icon");
            builder.AddContent(72, ZoomOutIconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(73, "zoom-out-icon", ZoomOutIconName);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets the zoom level programmatically
    /// </summary>
    /// <param name="zoomLevel">The zoom level (1.0 = 100%)</param>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task SetZoomAsync(double zoomLevel)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set zoom: component has not been rendered yet.");

        // wa-zoomable-frame exposes no setZoom() method in WA 3.0 - zoom is a reactive
        // property; only zoomIn()/zoomOut() exist as methods
        await JSInterop.SetPropertyAsync(Element.Value, "zoom", zoomLevel);
        Zoom = zoomLevel;
    }

    /// <summary>
    /// Resets zoom to 100% (wa-zoomable-frame exposes no reset() method in WA 3.0; this sets
    /// the zoom property back to 1.0).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ResetAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reset zoom: component has not been rendered yet.");

        await JSInterop.SetPropertyAsync(Element.Value, "zoom", 1.0);
        Zoom = 1.0;
    }

    /// <summary>
    /// Zooms in by one level
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ZoomInAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot zoom in: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "zoomIn");
    }

    /// <summary>
    /// Zooms out by one level
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ZoomOutAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot zoom out: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "zoomOut");
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
