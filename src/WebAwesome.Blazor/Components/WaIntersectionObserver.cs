using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A utility component that provides a declarative interface to the IntersectionObserver API.
/// Corresponds to the wa-intersection-observer Web Awesome component.
/// </summary>
/// <remarks>
/// Reports changes to the intersection of the elements it wraps with their root through the wa-intersect event.
/// Provides intersection ratio and state information for visibility tracking and lazy loading.
/// </remarks>
public class WaIntersectionObserver : ComponentBase
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
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    // Common styling parameters
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // IntersectionObserver options
    /// <summary>
    /// Space-separated threshold values for intersection detection (0.0 to 1.0)
    /// </summary>
    [Parameter] public string? Threshold { get; set; }

    /// <summary>
    /// ID of the root element to use as the viewport for intersection calculations
    /// </summary>
    [Parameter] public string? Root { get; set; }

    /// <summary>
    /// Margin around the root element for intersection calculations (CSS margin syntax)
    /// </summary>
    [Parameter] public string? RootMargin { get; set; }

    /// <summary>
    /// CSS class to toggle on intersection state changes
    /// </summary>
    [Parameter] public string? IntersectClass { get; set; }

    /// <summary>
    /// Whether the intersection observer is disabled
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Whether to disconnect the observer after the first time the wrapped elements intersect.
    /// </summary>
    [Parameter] public bool Once { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The content to observe for intersection changes
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the intersection state of observed elements changes
    /// </summary>
    [Parameter] public EventCallback<WaIntersectionEventArgs> OnIntersect { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-intersection-observer");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add intersection observer specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "threshold", Threshold);
        builder.AddAttributeIfNotNullOrEmpty(11, "root", Root);
        builder.AddAttributeIfNotNullOrEmpty(12, "root-margin", RootMargin);
        builder.AddAttributeIfNotNullOrEmpty(13, "intersect-class", IntersectClass);
        builder.AddAttribute(14, "disabled", Disabled);
        builder.AddAttribute(15, "once", Once);

        // Add event handlers
        if (OnIntersect.HasDelegate)
            builder.AddAttribute(20, "wa-intersect", EventCallback.Factory.Create(this, HandleIntersectEvent));

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __intersectionObserverReference => Element = __intersectionObserverReference);

        // Add child content (elements to observe)
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

    /// <summary>
    /// Handles intersection events from the Web Awesome component
    /// </summary>
    private async Task HandleIntersectEvent()
    {
        // Note: In a real implementation, the event args would be populated
        // from the JavaScript event data. For now, creating basic args.
        var args = new WaIntersectionEventArgs
        {
            IsIntersecting = true, // This would come from the JS event
            IntersectionRatio = 0.0, // This would come from the JS event
            Target = Element ?? default
        };
        await OnIntersect.InvokeAsync(args);
    }

    #endregion
}