using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A utility component that provides a declarative interface to the ResizeObserver API.
/// Corresponds to the wa-resize-observer Web Awesome component.
/// </summary>
/// <remarks>
/// Reports changes to the dimensions of the elements it wraps through the wa-resize event.
/// Provides ResizeObserverEntry objects with target element and dimension information.
/// </remarks>
public class WaResizeObserver : ComponentBase
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

    // ResizeObserver options
    [Parameter] public bool Disabled { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The content to observe for resize changes
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the observed elements are resized
    /// </summary>
    [Parameter] public EventCallback<ResizeEventArgs> OnResize { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-resize-observer");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add resize observer attributes
        builder.AddAttribute(10, "disabled", Disabled);

        // Add event handlers
        if (OnResize.HasDelegate)
            builder.AddAttribute(20, "wa-resize", OnResize);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __observerReference => Element = __observerReference);

        // Add child content to observe
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // TODO: JS Interop needed
            // Initialize ResizeObserver to watch child elements
            // Call: await JSRuntime.InvokeVoidAsync("webAwesome.resizeObserver.initialize", Element);

            // The JavaScript should:
            // 1. Create new ResizeObserver instance
            // 2. Observe all child elements within the component
            // 3. When resize occurs, emit wa-resize event with ResizeObserverEntry array
            // 4. Handle disabled state to start/stop observation
            // 5. ResizeObserverEntry should include:
            //    - target: the observed element
            //    - contentRect: the new dimensions
            //    - borderBoxSize, contentBoxSize arrays
            // 6. Handle disconnection/reconnection on component updates
            // 7. Respect disabled attribute to pause/resume observation
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnParametersSetAsync()
    {
        // TODO: JS Interop needed
        // Update observer state when Disabled parameter changes
        // Call: await JSRuntime.InvokeVoidAsync("webAwesome.resizeObserver.setDisabled", Element, Disabled);

        await base.OnParametersSetAsync();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Disconnects the resize observer
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Stop observing resize changes
    /// </remarks>
    public async Task DisconnectAsync()
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.resizeObserver.disconnect", Element);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Reconnects the resize observer
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Resume observing resize changes
    /// </remarks>
    public async Task ReconnectAsync()
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.resizeObserver.reconnect", Element);
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
/// Event args for resize events
/// </summary>
public class ResizeEventArgs : EventArgs
{
    /// <summary>
    /// Array of ResizeObserverEntry objects describing the size changes
    /// </summary>
    public object[]? ResizeObserverEntries { get; set; }
}
