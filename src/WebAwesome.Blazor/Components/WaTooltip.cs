using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A tooltip component that displays additional information based on a specific action.
/// Corresponds to the wa-tooltip Web Awesome component.
/// </summary>
public class WaTooltip : ComponentBase
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

    // Tooltip properties
    /// <summary>
    /// The id of the element the tooltip is anchored to.
    /// </summary>
    [Parameter] public string? For { get; set; }

    /// <summary>
    /// The preferred placement of the tooltip. The actual placement may vary as needed to keep the tooltip inside the viewport.
    /// </summary>
    [Parameter] public WaPlacement Placement { get; set; } = WaPlacement.Top;

    /// <summary>
    /// Controls how the tooltip is activated.
    /// </summary>
    [Parameter] public WaTrigger Trigger { get; set; } = WaTrigger.Hover;

    /// <summary>
    /// Indicates whether the tooltip is open. Can be used in lieu of <see cref="ShowAsync"/>/<see cref="HideAsync"/>.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// Removes the arrow from the tooltip.
    /// </summary>
    [Parameter] public bool WithoutArrow { get; set; }

    /// <summary>
    /// Disables the tooltip so that it won't show when triggered.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The distance in pixels from which to offset the tooltip away from its target.
    /// </summary>
    [Parameter] public int? Distance { get; set; }

    /// <summary>
    /// The amount of time to wait, in milliseconds, before hiding the tooltip after activation.
    /// </summary>
    [Parameter] public int? HideDelay { get; set; }

    /// <summary>
    /// The amount of time to wait, in milliseconds, before showing the tooltip after activation.
    /// </summary>
    [Parameter] public int? ShowDelay { get; set; }

    /// <summary>
    /// The distance in pixels from which to offset the tooltip along its target.
    /// </summary>
    [Parameter] public int? Skidding { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The tooltip content
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the tooltip begins to show.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked when the tooltip begins to hide.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <summary>
    /// Invoked after the tooltip shows and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }

    /// <summary>
    /// Invoked after the tooltip hides and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-tooltip");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add tooltip-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "for", For);
        if (Placement != WaPlacement.Top)
            builder.AddAttribute(11, "placement", Placement.ToHtmlValue());
        if (Trigger != WaTrigger.Hover)
            builder.AddAttribute(12, "trigger", Trigger.ToHtmlValue());
        builder.AddAttribute(13, "open", Open);
        builder.AddAttribute(14, "without-arrow", WithoutArrow);
        builder.AddAttribute(15, "disabled", Disabled);
        builder.AddAttributeIfNotNull(16, "distance", Distance);
        builder.AddAttributeIfNotNull(17, "hide-delay", HideDelay);
        builder.AddAttributeIfNotNull(18, "show-delay", ShowDelay);
        builder.AddAttributeIfNotNull(19, "skidding", Skidding);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "wa-show", OnShow);

        builder.AddAttributeIfHasDelegate(21, "wa-hide", OnHide);

        builder.AddAttributeIfHasDelegate(50, "wa-after-show", OnAfterShow);

        builder.AddAttributeIfHasDelegate(51, "wa-after-hide", OnAfterHide);

        // Add element reference capture
        builder.AddElementReferenceCapture(22, __tooltipReference => Element = __tooltipReference);

        // Add content
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
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

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Shows the tooltip with dynamic positioning
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show tooltip: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "show");
    }

    /// <summary>
    /// Hides the tooltip
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide tooltip: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
    }

    /// <summary>
    /// Recalculates and updates the tooltip position
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task RepositionAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reposition tooltip: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "reposition");
    }

    #endregion
}
