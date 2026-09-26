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
/// <remarks>
/// <para>
/// The slides are <see cref="WaCarouselItem"/> components in <see cref="ChildContent"/>. To add or remove slides
/// dynamically, render them from a collection with <c>@foreach</c> and <c>@key</c> and change the collection; the
/// element picks the change up by itself (pagination, navigation and, with <see cref="Loop"/>, its clones of the
/// slides follow):
/// </para>
/// <code>
/// &lt;WaCarousel Pagination="true" Navigation="true"&gt;
///     @foreach (var photo in photos)
///     {
///         &lt;WaCarouselItem @key="photo.Id"&gt;&lt;img src="@photo.Url" alt="@photo.Title" /&gt;&lt;/WaCarouselItem&gt;
///     }
/// &lt;/WaCarousel&gt;
/// </code>
/// <para>
/// Removing the active slide from the collection works as well: the carousel keeps the active index, so the slide
/// after the removed one shows. When the removed slide was the last one, the new last slide shows, or the first one
/// when <see cref="Loop"/> is set; <see cref="OnSlideChange"/> reports that change. Web Awesome's imperative
/// <c>addSlide()</c> and <c>removeSlide()</c> are not wrapped: they move or remove elements Blazor renders.
/// </para>
/// </remarks>
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
    /// The Web Awesome default of <see cref="Orientation"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const WaOrientation DefaultOrientation = WaOrientation.Horizontal;

    /// <summary>
    /// The orientation in which the carousel lays out its slides.
    /// </summary>
    [Parameter] public WaOrientation Orientation { get; set; } = DefaultOrientation;

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
    /// The Web Awesome default of <see cref="AutoplayInterval"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const int DefaultAutoplayInterval = 3000;

    /// <summary>
    /// The amount of time, in milliseconds, between each automatic scroll.
    /// </summary>
    [Parameter] public int AutoplayInterval { get; set; } = DefaultAutoplayInterval;

    /// <summary>
    /// The Web Awesome default of <see cref="SlidesPerPage"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const int DefaultSlidesPerPage = 1;

    /// <summary>
    /// How many slides are shown at a given time.
    /// </summary>
    [Parameter] public int SlidesPerPage { get; set; } = DefaultSlidesPerPage;

    /// <summary>
    /// The Web Awesome default of <see cref="SlidesPerMove"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const int DefaultSlidesPerMove = 1;

    /// <summary>
    /// The number of slides the carousel advances when scrolling. Useful when <see cref="SlidesPerPage"/> is greater
    /// than one. It cannot be higher than <see cref="SlidesPerPage"/>.
    /// </summary>
    [Parameter] public int SlidesPerMove { get; set; } = DefaultSlidesPerMove;

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
    /// Optional next icon to use instead of the default, rendered into the element's "next-icon" slot. Works best with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? NextIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="NextIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? NextIconName { get; set; }

    /// <summary>
    /// Optional previous icon to use instead of the default, rendered into the element's "previous-icon" slot. Works best with a wa-icon.
    /// </summary>
    [Parameter] public RenderFragment? PreviousIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="PreviousIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? PreviousIconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-carousel");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddDefaultedAttribute(attributes, 4, "orientation", Orientation.ToHtmlValue(), DefaultOrientation.ToHtmlValue());
        builder.AddAttribute(5, "pagination", Pagination);
        builder.AddAttribute(6, "navigation", Navigation);
        builder.AddAttribute(7, "mouse-dragging", MouseDragging);
        builder.AddAttribute(8, "loop", Loop);
        builder.AddAttribute(9, "autoplay", Autoplay);
        builder.AddNumberAttribute(attributes, 10, "autoplay-interval", AutoplayInterval, DefaultAutoplayInterval);
        builder.AddNumberAttribute(attributes, 11, "slides-per-page", SlidesPerPage, DefaultSlidesPerPage);
        builder.AddNumberAttribute(attributes, 12, "slides-per-move", SlidesPerMove, DefaultSlidesPerMove);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(15, "onwa-slide-change", OnSlideChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __carouselReference => Element = __carouselReference);

        // Add child content (carousel items)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        // Add next icon slot content
        if (NextIconContent is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "next-icon");
            builder.AddContent(42, NextIconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(43, "next-icon", NextIconName);
        }

        // Add previous icon slot content
        if (PreviousIconContent is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "previous-icon");
            builder.AddContent(52, PreviousIconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(53, "previous-icon", PreviousIconName);
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

