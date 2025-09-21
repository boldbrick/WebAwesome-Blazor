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

        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // TODO: JS Interop needed
            // Initialize zoom and pan controls for the iframe
            // Call: await JSRuntime.InvokeVoidAsync("webAwesome.zoomableFrame.initialize", Element);

            // The JavaScript should:
            // 1. Create iframe element with proper src/srcdoc
            // 2. Implement zoom controls (zoom in, zoom out, reset, specific levels)
            // 3. Handle pan/drag functionality within the zoomed frame
            // 4. Parse zoom-levels attribute for custom zoom increments
            // 5. Apply CSS transforms for zoom and pan effects
            // 6. Handle mouse/touch events for user interaction
            // 7. Emit zoom-change events with current zoom level
            // 8. Respect without-controls and without-interaction flags
            // 9. Handle iframe load/error events
            // 10. Maintain accessibility for keyboard navigation
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets the zoom level programmatically
    /// </summary>
    /// <param name="zoomLevel">The zoom level (1.0 = 100%)</param>
    /// <remarks>
    /// TODO: JS Interop needed - Apply zoom level and update controls
    /// </remarks>
    public async Task SetZoomAsync(double zoomLevel)
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.zoomableFrame.setZoom", Element, zoomLevel);
        Zoom = zoomLevel;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Resets zoom to 100% and centers the content
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Reset zoom and pan to defaults
    /// </remarks>
    public async Task ResetAsync()
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.zoomableFrame.reset", Element);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Zooms in by one level
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Zoom in based on zoom-levels or default increment
    /// </remarks>
    public async Task ZoomInAsync()
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.zoomableFrame.zoomIn", Element);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Zooms out by one level
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Zoom out based on zoom-levels or default increment
    /// </remarks>
    public async Task ZoomOutAsync()
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.zoomableFrame.zoomOut", Element);
        await Task.CompletedTask;
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

/// <summary>
/// Event args for zoom change events
/// </summary>
public class ZoomChangeEventArgs : EventArgs
{
    /// <summary>
    /// The new zoom level (1.0 = 100%)
    /// </summary>
    public double ZoomLevel { get; set; }

    /// <summary>
    /// The previous zoom level
    /// </summary>
    public double PreviousZoomLevel { get; set; }
}
