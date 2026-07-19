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

    // Carousel properties
    /// <summary>
    /// The orientation in which the carousel lays out its slides.
    /// </summary>
    [Parameter] public WaOrientation Orientation { get; set; } = WaOrientation.Horizontal;

    /// <summary>
    /// Shows the carousel's pagination indicators.
    /// </summary>
    [Parameter] public bool Pagination { get; set; }

    /// <summary>
    /// Shows the carousel's navigation.
    /// </summary>
    [Parameter] public bool Navigation { get; set; }

    /// <summary>
    /// Allows the slides to be scrolled through by dragging them with the mouse.
    /// </summary>
    [Parameter] public bool MouseDragging { get; set; }

    /// <summary>
    /// Allows the user to navigate the carousel in the same direction indefinitely.
    /// </summary>
    [Parameter] public bool Loop { get; set; }

    /// <summary>
    /// When set, the slides scroll automatically when the user is not interacting with them.
    /// </summary>
    [Parameter] public bool Autoplay { get; set; }

    /// <summary>
    /// The amount of time, in milliseconds, between each automatic scroll.
    /// </summary>
    [Parameter] public int AutoplayInterval { get; set; } = 3000;

    /// <summary>
    /// How many slides are shown at a given time.
    /// </summary>
    [Parameter] public int SlidesPerPage { get; set; } = 1;

    /// <summary>
    /// The number of slides the carousel advances when scrolling. Useful when <see cref="SlidesPerPage"/> is greater
    /// than one. It cannot be higher than <see cref="SlidesPerPage"/>.
    /// </summary>
    [Parameter] public int SlidesPerMove { get; set; } = 1;

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the active slide changes.
    /// </summary>
    [Parameter] public EventCallback<WaSlideChangeEventArgs> OnSlideChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The carousel's content (WaCarouselItem components)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Icon rendered into the next-icon slot, replacing the default next navigation icon.
    /// </summary>
    [Parameter] public string? NextIconName { get; set; }

    /// <summary>
    /// Icon rendered into the previous-icon slot, replacing the default previous navigation icon.
    /// </summary>
    [Parameter] public string? PreviousIconName { get; set; }

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
        builder.AddAttributeIfHasDelegate(15, "onwa-slide-change", OnSlideChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __carouselReference => Element = __carouselReference);

        // Add child content (carousel items)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        // Add navigation icon slots
        builder.AddIconSlot(40, "next-icon", NextIconName);
        builder.AddIconSlot(45, "previous-icon", PreviousIconName);

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

