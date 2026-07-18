using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A popover component that displays interactive content when their anchor element is clicked.
/// Corresponds to the wa-popover Web Awesome component.
/// </summary>
public class WaPopover : ComponentBase
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
    /// May be <see langword="null"/> if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
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

    // Popover properties
    /// <summary>
    /// The ID of the popover's anchor element. This must be an interactive/focusable element such as a button.
    /// </summary>
    [Parameter] public string? For { get; set; }

    /// <summary>
    /// The preferred placement of the popover. Note that the actual placement may vary as needed to keep the
    /// popover inside of the viewport.
    /// </summary>
    [Parameter] public WaPlacement Placement { get; set; } = WaPlacement.Top;

    /// <summary>
    /// Shows or hides the popover.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// The distance in pixels from which to offset the popover away from its target.
    /// </summary>
    [Parameter] public int Distance { get; set; } = 8;

    /// <summary>
    /// Removes the arrow from the popover.
    /// </summary>
    [Parameter] public bool WithoutArrow { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The popover content
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the popover begins to show.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked when the popover begins to hide.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-popover");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add popover-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "for", For);
        if (Placement != WaPlacement.Top)
            builder.AddAttribute(11, "placement", Placement.ToHtmlValue());
        builder.AddAttribute(12, "open", Open);
        if (Distance != 8)
            builder.AddAttribute(13, "distance", Distance);
        builder.AddAttribute(14, "without-arrow", WithoutArrow);

        // Add event handlers
        if (OnShow.HasDelegate)
            builder.AddAttribute(20, "wa-show", OnShow);

        if (OnHide.HasDelegate)
            builder.AddAttribute(21, "wa-hide", OnHide);

        // Add element reference capture
        builder.AddElementReferenceCapture(22, __popoverReference => Element = __popoverReference);

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
    /// Shows the popover with dynamic positioning
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show popover: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "show");
    }

    /// <summary>
    /// Hides the popover
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide popover: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
    }

    /// <summary>
    /// Recalculates and updates the popover position
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task RepositionAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reposition popover: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "reposition");
    }

    #endregion

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSInterop.InvokeMethodAsync(Element.Value, "initialize");
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
