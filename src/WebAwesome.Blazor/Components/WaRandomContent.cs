using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// Selects one or more child elements at random and displays them, hiding the rest. Useful for
/// rotating testimonials, a tip of the day, or featured content.
/// Corresponds to the wa-random-content Web Awesome component.
/// </summary>
public class WaRandomContent : ComponentBase
{
    #region ------ Injected Services ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

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
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Random content properties
    /// <summary>
    /// Number of children to show simultaneously. Clamped to the range [1, child count].
    /// </summary>
    [Parameter] public int Items { get; set; } = 1;

    /// <summary>
    /// Selection strategy used when choosing which children to show.
    /// </summary>
    [Parameter] public WaRandomContentMode Mode { get; set; } = WaRandomContentMode.Unique;

    /// <summary>
    /// Rotate the content automatically. Set the cadence with <see cref="AutoplayInterval"/>.
    /// </summary>
    [Parameter] public bool Autoplay { get; set; }

    /// <summary>
    /// Autoplay cadence in milliseconds.
    /// </summary>
    [Parameter] public int AutoplayInterval { get; set; } = 3000;

    /// <summary>
    /// Entrance animation for newly shown children.
    /// </summary>
    [Parameter] public WaRandomContentAnimation Animation { get; set; } = WaRandomContentAnimation.None;

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked whenever the displayed selection changes, including on first render, on
    /// <see cref="RandomizeAsync"/>, and on each autoplay tick.
    /// </summary>
    [Parameter] public EventCallback<WaContentChangeEventArgs> OnContentChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The pool of children to choose from. Only direct element children are eligible; unselected
    /// children are hidden with the <c>hidden</c> attribute.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-random-content");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttribute(4, "items", Items);
        builder.AddAttribute(5, "mode", Mode.ToHtmlValue());
        builder.AddAttribute(6, "autoplay", Autoplay);
        builder.AddAttribute(7, "autoplay-interval", AutoplayInterval);
        builder.AddAttribute(8, "animation", Animation.ToHtmlValue());

        // Add event handlers
        builder.AddAttributeIfHasDelegate(15, "onwa-content-change", OnContentChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __randomContentReference => Element = __randomContentReference);

        // Add child content (the pool of children to choose from)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Selects a new set of children using the current mode.
    /// </summary>
    /// <remarks>
    /// The underlying element method returns the elements now shown; those are live DOM nodes that
    /// cannot be marshaled across Blazor JS interop, so the return value is not surfaced. Observe the
    /// resulting selection through <see cref="OnContentChange"/> instead.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task RandomizeAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot randomize content: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "randomize");
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
