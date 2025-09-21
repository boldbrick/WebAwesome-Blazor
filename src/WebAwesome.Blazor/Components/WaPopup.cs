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

    // Popup positioning properties
    [Parameter] public WaPlacement Placement { get; set; } = WaPlacement.Top;
    [Parameter] public bool Active { get; set; }
    [Parameter] public string? Anchor { get; set; }
    [Parameter] public double Distance { get; set; } = 0;
    [Parameter] public double Skidding { get; set; } = 0;

    // Arrow properties
    [Parameter] public bool Arrow { get; set; }
    [Parameter] public WaArrowPlacement ArrowPlacement { get; set; } = WaArrowPlacement.Anchor;
    [Parameter] public double ArrowPadding { get; set; } = 10;

    // Flip behavior
    [Parameter] public bool Flip { get; set; }
    [Parameter] public string? FlipFallbackPlacements { get; set; }
    [Parameter] public string? FlipFallbackStrategy { get; set; } = "initial";
    [Parameter] public string? FlipBoundary { get; set; }
    [Parameter] public double FlipPadding { get; set; } = 0;

    // Shift behavior
    [Parameter] public bool Shift { get; set; }
    [Parameter] public string? ShiftBoundary { get; set; }
    [Parameter] public double ShiftPadding { get; set; } = 0;

    // Auto-size behavior
    [Parameter] public WaAutoSize AutoSize { get; set; } = WaAutoSize.None;
    [Parameter] public string? AutoSizeBoundary { get; set; }
    [Parameter] public double AutoSizePadding { get; set; } = 0;

    // Sync behavior
    [Parameter] public WaSync Sync { get; set; } = WaSync.None;

    // Hover bridge
    [Parameter] public bool HoverBridge { get; set; }

    // Boundary
    [Parameter] public string? Boundary { get; set; }

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
