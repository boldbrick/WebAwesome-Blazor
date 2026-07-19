using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A container that adds scrolling to elements that overflow their parent, with visual shadows
/// indicating there's more content to scroll and, optionally, scroll buttons.
/// Corresponds to the wa-scroller Web Awesome component.
/// </summary>
public class WaScroller : ComponentBase
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

    /// <summary>
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Scroller properties
    /// <summary>
    /// The scroller's orientation.
    /// </summary>
    [Parameter] public WaOrientation Orientation { get; set; } = WaOrientation.Horizontal;

    /// <summary>
    /// Removes the visible scrollbar.
    /// </summary>
    [Parameter] public bool WithoutScrollbar { get; set; }

    /// <summary>
    /// Removes the shadows that indicate more content is available to scroll.
    /// </summary>
    [Parameter] public bool WithoutShadow { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The scroller's content
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-scroller");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add scroller-specific attributes
        builder.AddAttribute(4, "orientation", Orientation.ToHtmlValue());
        builder.AddAttribute(5, "without-scrollbar", WithoutScrollbar);
        builder.AddAttribute(6, "without-shadow", WithoutShadow);

        // Add element reference capture
        builder.AddElementReferenceCapture(10, __scrollerReference => Element = __scrollerReference);

        // Add child content
        if (ChildContent is not null)
        {
            builder.AddContent(20, ChildContent);
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
}
