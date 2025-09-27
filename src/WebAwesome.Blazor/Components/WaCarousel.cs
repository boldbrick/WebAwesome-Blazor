using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A carousel component that displays content slides along a horizontal or vertical axis.
/// Corresponds to the wa-carousel Web Awesome component.
/// </summary>
public class WaCarousel : ComponentBase
{
    #region ------ Injected Services ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

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

    // Carousel properties
    [Parameter] public WaOrientation Orientation { get; set; } = WaOrientation.Horizontal;
    [Parameter] public bool Pagination { get; set; }
    [Parameter] public bool Navigation { get; set; }
    [Parameter] public bool MouseDragging { get; set; }
    [Parameter] public bool Loop { get; set; }
    [Parameter] public bool Autoplay { get; set; }
    [Parameter] public int AutoplayInterval { get; set; } = 3000;
    [Parameter] public int SlidesPerPage { get; set; } = 1;
    [Parameter] public int SlidesPerMove { get; set; } = 1;

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<WaSlideChangeEventArgs> OnSlideChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The carousel's content (WaCarouselItem components)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-carousel");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttribute(4, "orientation", Orientation.ToHtmlValue());
        builder.AddAttribute(5, "pagination", Pagination);
        builder.AddAttribute(6, "navigation", Navigation);
        builder.AddAttribute(7, "mouse-dragging", MouseDragging);
        builder.AddAttribute(8, "loop", Loop);
        builder.AddAttribute(9, "autoplay", Autoplay);
        builder.AddAttribute(10, "autoplay-interval", AutoplayInterval);
        builder.AddAttribute(11, "slides-per-page", SlidesPerPage);
        builder.AddAttribute(12, "slides-per-move", SlidesPerMove);

        // Add event handlers
        if (OnSlideChange.HasDelegate)
            builder.AddAttribute(15, "wa-slide-change", OnSlideChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __carouselReference => Element = __carouselReference);

        // Add child content (carousel items)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Programmatically navigates to the specified slide.
    /// </summary>
    /// <param name="index">The zero-based index of the slide to show</param>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task GoToSlideAsync(int index)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot navigate to slide: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "goToSlide", index);
    }

    /// <summary>
    /// Programmatically navigates to the previous slide.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task PreviousAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot navigate to previous slide: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "previous");
    }

    /// <summary>
    /// Programmatically navigates to the next slide.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task NextAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot navigate to next slide: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "next");
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

