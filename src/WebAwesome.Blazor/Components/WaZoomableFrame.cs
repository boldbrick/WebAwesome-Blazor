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
    /// The current zoom of the frame, e.g. 0 = 0% and 1 = 100%.
    /// </summary>
    [Parameter] public double Zoom { get; set; } = 1.0;

    /// <summary>
    /// The zoom levels to step through when using the zoom controls. Does not restrict programmatic changes to the zoom.
    /// </summary>
    [Parameter] public string? ZoomLevels { get; set; }

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
    /// Whether the iframe is allowed to be displayed in fullscreen mode.
    /// </summary>
    [Parameter] public bool AllowFullScreen { get; set; }

    /// <summary>
    /// Indicates when the browser should load the iframe.
    /// </summary>
    [Parameter] public WaLoading? Loading { get; set; }

    /// <summary>
    /// Indicates which referrer to send when fetching the frame's content.
    /// </summary>
    [Parameter] public string? ReferrerPolicy { get; set; }

    /// <summary>
    /// Applies extra restrictions to the content in the frame, e.g. <c>allow-scripts allow-same-origin</c>.
    /// </summary>
    [Parameter] public string? Sandbox { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// Icon rendered into the zoom-in-icon slot, replacing the default zoom-in control icon.
    /// </summary>
    [Parameter] public string? ZoomInIconName { get; set; }

    /// <summary>
    /// Icon rendered into the zoom-out-icon slot, replacing the default zoom-out control icon.
    /// </summary>
    [Parameter] public string? ZoomOutIconName { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the zoom level changes
    /// </summary>
    [Parameter] public EventCallback<ZoomChangeEventArgs> OnZoomChange { get; set; }

    /// <summary>
    /// Emitted when the frame content loads
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnLoad { get; set; }

    /// <summary>
    /// Emitted when the frame content fails to load
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnError { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-zoomable-frame");

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
        if (Zoom != 1.0)
            builder.AddAttribute(20, "zoom", Zoom);
        builder.AddAttributeIfNotNullOrEmpty(21, "zoom-levels", ZoomLevels);

        // Add control attributes
        builder.AddAttribute(30, "without-controls", WithoutControls);
        builder.AddAttribute(31, "without-interaction", WithoutInteraction);

        // Add remaining iframe passthrough attributes
        builder.AddAttribute(32, "allowfullscreen", AllowFullScreen);
        builder.AddAttributeIfNotNull(33, "loading", Loading?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(34, "referrerpolicy", ReferrerPolicy);
        builder.AddAttributeIfNotNullOrEmpty(35, "sandbox", Sandbox);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(40, "onwa-zoom-change", OnZoomChange);
        builder.AddAttributeIfHasDelegate(41, "onwa-load", OnLoad);
        builder.AddAttributeIfHasDelegate(42, "onwa-error", OnError);

        // Add element reference capture
        builder.AddElementReferenceCapture(50, __frameReference => Element = __frameReference);

        // Add zoom control icon slots
        builder.AddIconSlot(60, "zoom-in-icon", ZoomInIconName);
        builder.AddIconSlot(65, "zoom-out-icon", ZoomOutIconName);

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
