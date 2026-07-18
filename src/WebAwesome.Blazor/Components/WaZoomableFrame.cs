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

    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

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

    // Frame content properties
    [Parameter] public string? Src { get; set; }
    [Parameter] public string? SrcDoc { get; set; }

    // Zoom properties
    [Parameter] public double Zoom { get; set; } = 1.0;
    [Parameter] public string? ZoomLevels { get; set; }

    // Control properties
    [Parameter] public bool WithoutControls { get; set; }
    [Parameter] public bool WithoutInteraction { get; set; }

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

        // Add event handlers
        if (OnZoomChange.HasDelegate)
            builder.AddAttribute(40, "wa-zoom-change", OnZoomChange);
        if (OnLoad.HasDelegate)
            builder.AddAttribute(41, "wa-load", OnLoad);
        if (OnError.HasDelegate)
            builder.AddAttribute(42, "wa-error", OnError);

        // Add element reference capture
        builder.AddElementReferenceCapture(50, __frameReference => Element = __frameReference);

        // Add zoom control icon slots
        builder.AddIconSlot(60, "zoom-in-icon", ZoomInIconName);
        builder.AddIconSlot(65, "zoom-out-icon", ZoomOutIconName);

        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSInterop.InvokeMethodAsync(Element.Value, "initialize");
        }

        await base.OnAfterRenderAsync(firstRender);
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

        await JSInterop.InvokeMethodAsync(Element.Value, "setZoom", zoomLevel);
        Zoom = zoomLevel;
    }

    /// <summary>
    /// Resets zoom to 100% and centers the content
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ResetAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reset zoom: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "reset");
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
