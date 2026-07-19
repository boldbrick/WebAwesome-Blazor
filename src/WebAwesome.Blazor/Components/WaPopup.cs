using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A utility component that provides declarative positioning for popup containers using Floating UI.
/// Corresponds to the wa-popup Web Awesome component.
/// </summary>
/// <remarks>
/// This is a low-level positioning utility. Do not use directly for accessible experiences.
/// Build higher-level components like tooltips or dropdowns instead.
/// </remarks>
public class WaPopup : ComponentBase
{
    #region ------ Dependency Injection ------

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

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

    // Popup positioning properties
    /// <summary>
    /// The preferred placement of the popup. Note that the actual placement will vary as configured to keep
    /// the panel inside of the viewport.
    /// </summary>
    [Parameter] public WaPlacement Placement { get; set; } = WaPlacement.Top;

    /// <summary>
    /// Activates the positioning logic and shows the popup. When deactivated, the positioning logic is torn
    /// down and the popup is hidden.
    /// </summary>
    [Parameter] public bool Active { get; set; }

    /// <summary>
    /// The ID of the element the popup will be anchored to. The anchor must live outside of the popup; to
    /// anchor to an element inside the popup, use the anchor slot instead.
    /// </summary>
    [Parameter] public string? Anchor { get; set; }

    /// <summary>
    /// The distance in pixels from which to offset the panel away from its anchor.
    /// </summary>
    [Parameter] public double Distance { get; set; } = 0;

    /// <summary>
    /// The distance in pixels from which to offset the panel along its anchor.
    /// </summary>
    [Parameter] public double Skidding { get; set; } = 0;

    // Arrow properties
    /// <summary>
    /// Attaches an arrow to the popup. The arrow's size and color can be customized using the
    /// <c>--arrow-size</c> and <c>--arrow-color</c> custom properties.
    /// </summary>
    [Parameter] public bool Arrow { get; set; }

    /// <summary>
    /// The placement of the arrow. The default, <see cref="WaArrowPlacement.Anchor"/>, aligns the arrow as
    /// close to the center of the anchor as possible, considering available space and <see cref="ArrowPadding"/>.
    /// </summary>
    [Parameter] public WaArrowPlacement ArrowPlacement { get; set; } = WaArrowPlacement.Anchor;

    /// <summary>
    /// The amount of padding, in pixels, between the arrow and the edges of the popup. If the popup has a
    /// border-radius, this prevents the arrow from overflowing the corners.
    /// </summary>
    [Parameter] public double ArrowPadding { get; set; } = 10;

    // Flip behavior
    /// <summary>
    /// When set, the popup's placement flips to the opposite side to keep it in view. Use
    /// <see cref="FlipFallbackPlacements"/> to further configure how the fallback placement is determined.
    /// </summary>
    [Parameter] public bool Flip { get; set; }

    /// <summary>
    /// If the preferred placement doesn't fit, the popup is tested in these fallback placements until one
    /// fits. Must be a string of any number of placements separated by a space, e.g. <c>top bottom left</c>.
    /// </summary>
    [Parameter] public string? FlipFallbackPlacements { get; set; }

    /// <summary>
    /// When neither the preferred placement nor the fallback placements fit, this value determines whether
    /// the popup is positioned using the best available fit based on available space or as it was initially
    /// preferred.
    /// </summary>
    [Parameter] public string? FlipFallbackStrategy { get; set; } = "initial";

    /// <summary>
    /// The clipping element(s) that overflow is checked relative to when flipping. By default, the boundary
    /// includes overflow ancestors that will cause the element to be clipped.
    /// </summary>
    [Parameter] public string? FlipBoundary { get; set; }

    /// <summary>
    /// The amount of padding, in pixels, to exceed before the flip behavior occurs.
    /// </summary>
    [Parameter] public double FlipPadding { get; set; } = 0;

    // Shift behavior
    /// <summary>
    /// Moves the popup along the axis to keep it in view when clipped.
    /// </summary>
    [Parameter] public bool Shift { get; set; }

    /// <summary>
    /// The clipping element(s) that overflow is checked relative to when shifting. By default, the boundary
    /// includes overflow ancestors that will cause the element to be clipped.
    /// </summary>
    [Parameter] public string? ShiftBoundary { get; set; }

    /// <summary>
    /// The amount of padding, in pixels, to exceed before the shift behavior occurs.
    /// </summary>
    [Parameter] public double ShiftPadding { get; set; } = 0;

    // Auto-size behavior
    /// <summary>
    /// When set, causes the popup to automatically resize itself to prevent it from overflowing.
    /// </summary>
    [Parameter] public WaAutoSize AutoSize { get; set; } = WaAutoSize.None;

    /// <summary>
    /// The clipping element(s) that overflow is checked relative to when auto-sizing. By default, the
    /// boundary includes overflow ancestors that will cause the element to be clipped.
    /// </summary>
    [Parameter] public string? AutoSizeBoundary { get; set; }

    /// <summary>
    /// The amount of padding, in pixels, to exceed before the auto-size behavior occurs.
    /// </summary>
    [Parameter] public double AutoSizePadding { get; set; } = 0;

    // Sync behavior
    /// <summary>
    /// Syncs the popup's width or height to that of the anchor element.
    /// </summary>
    [Parameter] public WaSync Sync { get; set; } = WaSync.None;

    // Hover bridge
    /// <summary>
    /// When a gap exists between the anchor and the popup element, adds a "hover bridge" that fills the gap
    /// using an invisible element, so hover-based events such as <c>mouseenter</c>/<c>mouseleave</c> don't
    /// fire prematurely. The hover bridge is only drawn while the popup is active.
    /// </summary>
    [Parameter] public bool HoverBridge { get; set; }

    // Boundary
    /// <summary>
    /// The bounding box to use for flipping, shifting, and auto-sizing.
    /// </summary>
    [Parameter] public string? Boundary { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the popup is repositioned. This can happen when the anchor or popup's dimension change,
    /// when scrolling, or programmatically after calling <see cref="RepositionAsync"/>.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnReposition { get; set; }

    #endregion

    #region ------ Content Slots ------

    /// <summary>
    /// The anchor element slot
    /// </summary>
    [Parameter] public RenderFragment? AnchorContent { get; set; }

    /// <summary>
    /// The popup content
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-popup");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add positioning attributes
        if (Placement != WaPlacement.Top)
            builder.AddAttribute(10, "placement", Placement.ToHtmlValue());
        builder.AddAttribute(11, "active", Active);
        builder.AddAttributeIfNotNullOrEmpty(12, "anchor", Anchor);
        if (Distance != 0)
            builder.AddAttribute(13, "distance", Distance);
        if (Skidding != 0)
            builder.AddAttribute(14, "skidding", Skidding);

        // Add arrow attributes
        if (Arrow)
        {
            builder.AddAttribute(20, "arrow", true);
            if (ArrowPlacement != WaArrowPlacement.Anchor)
                builder.AddAttribute(21, "arrow-placement", ArrowPlacement.ToHtmlValue());
            if (ArrowPadding != 10)
                builder.AddAttribute(22, "arrow-padding", ArrowPadding);
        }

        // Add flip attributes
        if (Flip)
        {
            builder.AddAttribute(30, "flip", true);
            builder.AddAttributeIfNotNullOrEmpty(31, "flip-fallback-placements", FlipFallbackPlacements);
            builder.AddAttributeIfNotNullOrEmpty(32, "flip-fallback-strategy", FlipFallbackStrategy);
            builder.AddAttributeIfNotNullOrEmpty(33, "flip-boundary", FlipBoundary);
            if (FlipPadding != 0)
                builder.AddAttribute(34, "flip-padding", FlipPadding);
        }

        // Add shift attributes
        if (Shift)
        {
            builder.AddAttribute(40, "shift", true);
            builder.AddAttributeIfNotNullOrEmpty(41, "shift-boundary", ShiftBoundary);
            if (ShiftPadding != 0)
                builder.AddAttribute(42, "shift-padding", ShiftPadding);
        }

        // Add auto-size attributes
        if (AutoSize != WaAutoSize.None)
        {
            builder.AddAttribute(50, "auto-size", AutoSize.ToHtmlValue());
            builder.AddAttributeIfNotNullOrEmpty(51, "auto-size-boundary", AutoSizeBoundary);
            if (AutoSizePadding != 0)
                builder.AddAttribute(52, "auto-size-padding", AutoSizePadding);
        }

        // Add sync attributes
        if (Sync != WaSync.None)
            builder.AddAttribute(60, "sync", Sync.ToHtmlValue());

        // Add hover bridge
        if (HoverBridge)
            builder.AddAttribute(70, "hover-bridge", true);

        // Add boundary
        builder.AddAttributeIfNotNullOrEmpty(80, "boundary", Boundary);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(85, "wa-reposition", OnReposition);

        // Add element reference capture
        builder.AddElementReferenceCapture(90, __popupReference => Element = __popupReference);

        // Add anchor slot content
        if (AnchorContent is not null)
        {
            builder.OpenElement(100, "span");
            builder.AddAttribute(101, "slot", "anchor");
            builder.AddContent(102, AnchorContent);
            builder.CloseElement();
        }

        // Add main popup content
        if (ChildContent is not null)
        {
            builder.AddContent(110, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Repositions the popup programmatically
    /// </summary>
    /// <remarks>
    /// Only needed for virtual elements or when forcing manual repositioning.
    /// Most popup use cases with regular DOM anchors don't require this method.
    /// </remarks>
    public async Task RepositionAsync()
    {
        if (Element.HasValue)
        {
            await JSRuntime.InvokeVoidAsync("eval", $"arguments[0].reposition()", Element.Value);
        }
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
