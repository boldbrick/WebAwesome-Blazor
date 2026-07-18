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
    /// May be <see langword="null"/> if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
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

    // ResizeObserver options
    /// <summary>
    /// Disables the observer.
    /// </summary>
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

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (Element != null)
        {
            await JSInterop.SetPropertyAsync(Element.Value, "disabled", Disabled);
        }

        await base.OnParametersSetAsync();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Disconnects the resize observer
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task DisconnectAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot disconnect observer: component has not been rendered yet.");

        // the underlying wa-resize-observer element exposes stopObserver()/startObserver(), not disconnect()/reconnect()
        await JSInterop.InvokeMethodAsync(Element.Value, "stopObserver");
    }

    /// <summary>
    /// Reconnects the resize observer
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ReconnectAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reconnect observer: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "startObserver");
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
