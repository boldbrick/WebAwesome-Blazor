using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
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
        // TODO: This event needs to be mapped to the Web Awesome component events
        if (OnSlideChange.HasDelegate)
        {
            // Custom event handler will need JavaScript interop
        }

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
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-carousel's goToSlide method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task GoToSlideAsync(int index)
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.goToSlide(index) method on the underlying wa-carousel element
        throw new NotImplementedException("GoToSlideAsync requires JavaScript interop implementation. " +
            "This should call the underlying wa-carousel element's goToSlide method.");
    }

    /// <summary>
    /// Programmatically navigates to the previous slide.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-carousel's previous method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task PreviousAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.previous() method on the underlying wa-carousel element
        throw new NotImplementedException("PreviousAsync requires JavaScript interop implementation. " +
            "This should call the underlying wa-carousel element's previous method.");
    }

    /// <summary>
    /// Programmatically navigates to the next slide.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-carousel's next method.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public Task NextAsync()
    {
        // TODO: Implement JavaScript interop call
        // Should call Element.next() method on the underlying wa-carousel element
        throw new NotImplementedException("NextAsync requires JavaScript interop implementation. " +
            "This should call the underlying wa-carousel element's next method.");
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
/// Event arguments for carousel slide change events
/// </summary>
public class WaSlideChangeEventArgs : EventArgs
{
    public int Index { get; set; }
}
