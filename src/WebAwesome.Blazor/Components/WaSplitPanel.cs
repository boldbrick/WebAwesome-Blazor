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

    // Split panel properties
    [Parameter] public WaOrientation Orientation { get; set; } = WaOrientation.Horizontal;
    [Parameter] public decimal? Position { get; set; }
    [Parameter] public int? PositionInPixels { get; set; }
    [Parameter] public WaPrimary? Primary { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Snap { get; set; }
    [Parameter] public int SnapThreshold { get; set; } = 12;

    #endregion

    #region ------ Events ------

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

        // Add event handlers
        // TODO: This event needs to be mapped to the Web Awesome component events
        if (OnReposition.HasDelegate)
        {
            // Custom event handler will need JavaScript interop
        }

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
    /// <returns>The current position as a percentage</returns>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to get the underlying wa-split-panel's position property.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task<decimal> GetPositionAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should return Element.position property on the underlying wa-split-panel element
        throw new NotImplementedException("GetPositionAsync requires JavaScript interop implementation. " +
            "This should get the underlying wa-split-panel element's position property.");
    }

    /// <summary>
    /// Gets the current position of the divider in pixels.
    /// </summary>
    /// <returns>The current position in pixels</returns>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to get the underlying wa-split-panel's positionInPixels property.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task<int> GetPositionInPixelsAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should return Element.positionInPixels property on the underlying wa-split-panel element
        throw new NotImplementedException("GetPositionInPixelsAsync requires JavaScript interop implementation. " +
            "This should get the underlying wa-split-panel element's positionInPixels property.");
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
/// Event arguments for split panel reposition events
/// </summary>
public class WaSplitPanelRepositionEventArgs : EventArgs
{
    public decimal Position { get; set; }
    public int PositionInPixels { get; set; }
}
