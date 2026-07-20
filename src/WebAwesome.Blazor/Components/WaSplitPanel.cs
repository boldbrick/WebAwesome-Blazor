using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A split panel component that displays two adjacent panels with a resizable divider.
/// Corresponds to the wa-split-panel Web Awesome component.
/// </summary>
public class WaSplitPanel : ComponentBase
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

    // Split panel properties
    /// <summary>
    /// Sets the split panel's orientation.
    /// </summary>
    [Parameter] public WaOrientation Orientation { get; set; } = WaOrientation.Horizontal;

    /// <summary>
    /// The current position of the divider from the primary panel's edge as a percentage between 0 and 100. Defaults to 50% of the container's initial size.
    /// </summary>
    [Parameter] public decimal? Position { get; set; }

    /// <summary>
    /// The current position of the divider from the primary panel's edge, in pixels.
    /// </summary>
    [Parameter] public int? PositionInPixels { get; set; }

    /// <summary>
    /// Designates which panel maintains its size while the other grows or shrinks when the host element is resized. When unset, both panels resize proportionally.
    /// </summary>
    [Parameter] public WaPrimary? Primary { get; set; }

    /// <summary>
    /// Disables resizing. The position may still change as a result of resizing the host element.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// One or more space-separated values at which the divider should snap. Values can be in pixels or percentages, e.g. "100px 50%".
    /// </summary>
    [Parameter] public string? Snap { get; set; }

    /// <summary>
    /// How close the divider must be to a snap point, in pixels, before snapping occurs.
    /// </summary>
    [Parameter] public int SnapThreshold { get; set; } = 12;

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the divider's position changes.
    /// </summary>
    [Parameter] public EventCallback<WaSplitPanelRepositionEventArgs> OnReposition { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// Content for the start panel
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content for the end panel
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// Custom divider content (typically an icon)
    /// </summary>
    [Parameter] public RenderFragment? DividerContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-split-panel");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttribute(4, "orientation", Orientation.ToHtmlValue());
        builder.AddAttributeIfNotNull(5, "position", Position);
        builder.AddAttributeIfNotNull(6, "position-in-pixels", PositionInPixels);
        builder.AddAttributeIfNotNull(7, "primary", Primary?.ToHtmlValue());
        builder.AddAttribute(8, "disabled", Disabled);
        builder.AddAttributeIfNotNullOrEmpty(9, "snap", Snap);
        builder.AddAttribute(10, "snap-threshold", SnapThreshold);

        // Add event handlers; the interop module's createEventArgs reads position and
        // position-in-pixels from the element, as the wa-reposition event carries no detail
        builder.AddAttributeIfHasDelegate(21, "onwa-reposition", OnReposition);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __splitPanelReference => Element = __splitPanelReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(30, "div");
            builder.AddAttribute(31, "slot", "start");
            builder.AddContent(32, StartContent);
            builder.CloseElement();
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(40, "div");
            builder.AddAttribute(41, "slot", "end");
            builder.AddContent(42, EndContent);
            builder.CloseElement();
        }

        // Add divider slot content
        if (DividerContent is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "divider");
            builder.AddContent(52, DividerContent);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Gets the current position of the divider as a percentage.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current position as a percentage</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task<decimal> GetPositionAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get position: component has not been rendered yet.");

        return await JSInterop.GetPropertyAsync<decimal>(Element.Value, "position");
    }

    /// <summary>
    /// Gets the current position of the divider in pixels.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current position in pixels</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task<int> GetPositionInPixelsAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get position in pixels: component has not been rendered yet.");

        return await JSInterop.GetPropertyAsync<int>(Element.Value, "positionInPixels");
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
    /// Handles reposition events from the Web Awesome component
    /// </summary>

    #endregion
}
