using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A pagination component that renders page controls for navigating multi-page content.
/// Corresponds to the wa-pagination Web Awesome component.
/// </summary>
public class WaPagination : ComponentBase
{
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

    // Visual & behavior properties
    /// <summary>
    /// The pagination's visual appearance.
    /// </summary>
    [Parameter] public WaPaginationAppearance? Appearance { get; set; }

    /// <summary>
    /// The pagination's layout. The default <see cref="WaPaginationFormat.Standard"/> format shows the full page
    /// list with ellipses; <see cref="WaPaginationFormat.Compact"/> collapses it into a short "1 of 5" label
    /// flanked by the previous and next buttons, useful in tight spaces like toolbars and cards.
    /// </summary>
    [Parameter] public WaPaginationFormat? Format { get; set; }

    /// <summary>
    /// Disables the pagination.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The number of pages to always show at the start and end.
    /// </summary>
    [Parameter] public int? BoundaryCount { get; set; }

    /// <summary>
    /// The number of pages to show on each side of the current page.
    /// </summary>
    [Parameter] public int? SiblingCount { get; set; }

    /// <summary>
    /// Renders nothing when there's only one page.
    /// </summary>
    [Parameter] public bool HideSinglePage { get; set; }

    /// <summary>
    /// A URL template used to render page items as links instead of buttons. When set, items render as
    /// <c>&lt;a&gt;</c> elements for SSR, SEO, and no-JS support. Provide a string with <c>{page}</c> as a
    /// placeholder for the page number, e.g. <c>/products?page={page}</c>.
    /// </summary>
    /// <remarks>
    /// The upstream attribute also accepts a JavaScript function <c>(page: number) =&gt; string</c>. Functions
    /// cannot cross the Blazor interop boundary, so only the string template form is exposed here.
    /// </remarks>
    [Parameter] public string? HrefTemplate { get; set; }

    /// <summary>
    /// A label that describes the pagination to assistive devices. This won't be shown on the screen, but it will
    /// be announced by screen readers. Especially useful when more than one pagination control exists on the same
    /// page.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// The current page, starting at 1.
    /// </summary>
    [Parameter] public int? Page { get; set; }

    /// <summary>
    /// The number of items shown per page.
    /// </summary>
    [Parameter] public int? PageSize { get; set; }

    /// <summary>
    /// The total number of items to paginate.
    /// </summary>
    [Parameter] public int? Total { get; set; }

    /// <summary>
    /// Shows buttons that jump to the first and last pages.
    /// </summary>
    [Parameter] public bool WithEdges { get; set; }

    /// <summary>
    /// Shows a summary of the items on the current page, e.g. "1-10 of 237".
    /// </summary>
    [Parameter] public bool WithSummary { get; set; }

    /// <summary>
    /// Hides the previous and next buttons.
    /// </summary>
    [Parameter] public bool WithoutNav { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the page is about to change but before it does.
    /// </summary>
    /// <remarks>
    /// The underlying Web Awesome event is cancelable via <c>event.preventDefault()</c>, but Blazor dispatches
    /// custom event callbacks asynchronously after the DOM event has already run its course, so a .NET handler
    /// cannot call back into the DOM synchronously to prevent the page change. This event is delivered as an
    /// informational notification only.
    /// </remarks>
    [Parameter] public EventCallback<WaPaginationPageChangeEventArgs> OnBeforePageChange { get; set; }

    /// <summary>
    /// Invoked after the page changes.
    /// </summary>
    [Parameter] public EventCallback<WaPaginationPageChangeEventArgs> OnPageChange { get; set; }

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// An icon to use in lieu of the default first icon.
    /// </summary>
    [Parameter] public RenderFragment? FirstIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="FirstIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? FirstIconName { get; set; }

    /// <summary>
    /// An icon to use in lieu of the default last icon.
    /// </summary>
    [Parameter] public RenderFragment? LastIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="LastIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? LastIconName { get; set; }

    /// <summary>
    /// An icon to use in lieu of the default next icon.
    /// </summary>
    [Parameter] public RenderFragment? NextIconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="NextIconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? NextIconName { get; set; }

    /// <summary>
    /// An icon to use in lieu of the default previous icon.
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
        builder.OpenElement(0, "wa-pagination");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNull(4, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttributeIfNotNull(5, "format", Format?.ToHtmlValue());
        builder.AddAttribute(6, "disabled", Disabled);
        builder.AddAttributeIfNotNull(7, "boundary-count", BoundaryCount);
        builder.AddAttributeIfNotNull(8, "sibling-count", SiblingCount);
        builder.AddAttribute(9, "hide-single-page", HideSinglePage);
        builder.AddAttributeIfNotNullOrEmpty(10, "href-template", HrefTemplate);
        builder.AddAttributeIfNotNullOrEmpty(11, "label", Label);
        builder.AddAttributeIfNotNull(12, "page", Page);
        builder.AddAttributeIfNotNull(13, "page-size", PageSize);
        builder.AddAttributeIfNotNull(14, "total", Total);
        builder.AddAttribute(15, "with-edges", WithEdges);
        builder.AddAttribute(16, "with-summary", WithSummary);
        builder.AddAttribute(17, "without-nav", WithoutNav);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(30, "onwa-before-page-change", OnBeforePageChange);
        builder.AddAttributeIfHasDelegate(31, "onwa-page-change", OnPageChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(35, __paginationReference => Element = __paginationReference);

        // Add first icon slot content
        if (FirstIconContent is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "first-icon");
            builder.AddContent(42, FirstIconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(45, "first-icon", FirstIconName);
        }

        // Add last icon slot content
        if (LastIconContent is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "last-icon");
            builder.AddContent(52, LastIconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(55, "last-icon", LastIconName);
        }

        // Add next icon slot content
        if (NextIconContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "next-icon");
            builder.AddContent(62, NextIconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(65, "next-icon", NextIconName);
        }

        // Add previous icon slot content
        if (PreviousIconContent is not null)
        {
            builder.OpenElement(70, "span");
            builder.AddAttribute(71, "slot", "previous-icon");
            builder.AddContent(72, PreviousIconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(75, "previous-icon", PreviousIconName);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Internals ------

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
