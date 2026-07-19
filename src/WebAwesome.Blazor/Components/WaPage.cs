using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A page component that provides the overall layout structure of a page, including header, navigation,
/// main content, and footer regions. Corresponds to the wa-page Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component.
/// </remarks>
public class WaPage : ComponentBase
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

    /// <summary>
    /// Additional CSS class names applied to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Page properties
    /// <summary>
    /// Determines whether or not to hide the default hamburger button. This will automatically flip to
    /// true if you add an element with <c>data-toggle-nav</c> anywhere in the element light DOM.
    /// Generally this will be set for you and you don't need to do anything, unless you're using SSR, in which case
    /// you should set this manually for initial page loads.
    /// </summary>
    [Parameter] public bool DisableNavigationToggle { get; set; }

    /// <summary>
    /// At what page width to hide the "navigation" slot and collapse into a hamburger button. Accepts both numbers
    /// (interpreted as px) and CSS lengths (e.g. <c>50em</c>), which are resolved based on the root element.
    /// </summary>
    [Parameter] public string? MobileBreakpoint { get; set; } = "768px";

    /// <summary>
    /// Where to place the navigation when in the mobile viewport.
    /// </summary>
    [Parameter] public WaPageNavigationPlacement? NavigationPlacement { get; set; }

    /// <summary>
    /// Whether or not the navigation drawer is open. Note, the navigation drawer is only "open" on mobile views.
    /// </summary>
    [Parameter] public bool NavOpen { get; set; }

    /// <summary>
    /// The view is a reflection of the "mobileBreakpoint": when the page is larger than the <see cref="MobileBreakpoint"/>
    /// (768px by default), it is considered to be a "desktop" view. The view is merely a way to distinguish when to
    /// show/hide the navigation. You can use additional media queries to make other adjustments to content as necessary.
    /// The default is "desktop" because the mobile navigation drawer isn't accessible via SSR due to the drawer requiring JS.
    /// </summary>
    [Parameter] public WaPageView? View { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The page's main content (default slot).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content to be shown on the right side of the page. Typically contains a table of contents, ads, etc. This
    /// section "sticks" to the top as the page scrolls.
    /// </summary>
    [Parameter] public RenderFragment? Aside { get; set; }

    /// <summary>
    /// The banner that gets displayed above the header. The banner will not be shown if no content is provided.
    /// </summary>
    [Parameter] public RenderFragment? Banner { get; set; }

    /// <summary>
    /// The content to display in the footer. This is always displayed underneath the viewport so will always make
    /// the page "scrollable".
    /// </summary>
    [Parameter] public RenderFragment? Footer { get; set; }

    /// <summary>
    /// The header to display at the top of the page. If a banner is present, the header will appear below the
    /// banner. The header will not be shown if there is no content.
    /// </summary>
    [Parameter] public RenderFragment? Header { get; set; }

    /// <summary>
    /// Footer to display inline below the main content.
    /// </summary>
    [Parameter] public RenderFragment? MainFooter { get; set; }

    /// <summary>
    /// Header to display inline above the main content.
    /// </summary>
    [Parameter] public RenderFragment? MainHeader { get; set; }

    /// <summary>
    /// The left side of the page. If you slot an element in here, you will override the default
    /// <see cref="Navigation"/> slot and will be handling navigation on your own. This also will not disable the
    /// fallback behavior of the navigation button. This section "sticks" to the top as the page scrolls.
    /// </summary>
    [Parameter] public RenderFragment? Menu { get; set; }

    /// <summary>
    /// The main content to display in the navigation area. This is displayed on the left side of the page, if
    /// <see cref="Menu"/> is not used. This section "sticks" to the top as the page scrolls.
    /// </summary>
    [Parameter] public RenderFragment? Navigation { get; set; }

    /// <summary>
    /// The footer for a navigation area. On mobile this will be the footer for <c>&lt;wa-drawer&gt;</c>.
    /// </summary>
    [Parameter] public RenderFragment? NavigationFooter { get; set; }

    /// <summary>
    /// The header for a navigation area. On mobile this will be the header for <c>&lt;wa-drawer&gt;</c>.
    /// </summary>
    [Parameter] public RenderFragment? NavigationHeader { get; set; }

    /// <summary>
    /// Use this slot to slot in your own button and icon for toggling the navigation drawer. By default it is a
    /// <c>&lt;wa-button&gt;</c> plus a 3 bars <c>&lt;wa-icon&gt;</c>.
    /// </summary>
    [Parameter] public RenderFragment? NavigationToggle { get; set; }

    /// <summary>
    /// Use this to slot in your own icon for toggling the navigation drawer. By default it is 3 bars
    /// <c>&lt;wa-icon&gt;</c>.
    /// </summary>
    [Parameter] public RenderFragment? NavigationToggleIcon { get; set; }

    /// <summary>
    /// The "skip to content" slot. You can override this if you would like to override the "Skip to content"
    /// button and add additional "Skip to X" links, they can be inserted here.
    /// </summary>
    [Parameter] public RenderFragment? SkipToContent { get; set; }

    /// <summary>
    /// A subheader to display below the <see cref="Header"/>. This is a good place to put things like breadcrumbs.
    /// </summary>
    [Parameter] public RenderFragment? Subheader { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-page");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add page-specific attributes
        builder.AddAttribute(10, "disable-navigation-toggle", DisableNavigationToggle);
        builder.AddAttributeIfNotNullOrEmpty(11, "mobile-breakpoint", MobileBreakpoint);
        builder.AddAttributeIfNotNull(12, "navigation-placement", NavigationPlacement?.ToHtmlValue());
        builder.AddAttribute(13, "nav-open", NavOpen);
        builder.AddAttributeIfNotNull(14, "view", View?.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __pageReference => Element = __pageReference);

        // Add named slot content
        AddSlot(builder, 40, "aside", Aside);
        AddSlot(builder, 43, "banner", Banner);
        AddSlot(builder, 46, "footer", Footer);
        AddSlot(builder, 49, "header", Header);
        AddSlot(builder, 52, "main-footer", MainFooter);
        AddSlot(builder, 55, "main-header", MainHeader);
        AddSlot(builder, 58, "menu", Menu);
        AddSlot(builder, 61, "navigation", Navigation);
        AddSlot(builder, 64, "navigation-footer", NavigationFooter);
        AddSlot(builder, 67, "navigation-header", NavigationHeader);
        AddSlot(builder, 70, "navigation-toggle", NavigationToggle);
        AddSlot(builder, 73, "navigation-toggle-icon", NavigationToggleIcon);
        AddSlot(builder, 76, "skip-to-content", SkipToContent);
        AddSlot(builder, 79, "subheader", Subheader);

        // Add main content
        if (ChildContent is not null)
        {
            builder.AddContent(90, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Hides the mobile navigation drawer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task HideNavigationAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide navigation: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hideNavigation");
    }

    /// <summary>
    /// Shows the mobile navigation drawer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ShowNavigationAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show navigation: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "showNavigation");
    }

    /// <summary>
    /// Toggles the mobile navigation drawer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ToggleNavigationAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot toggle navigation: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "toggleNavigation");
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

    /// <summary>
    /// Renders a named slot's content wrapped in a span with the appropriate slot attribute, when provided
    /// </summary>
    private static void AddSlot(RenderTreeBuilder builder, int sequence, string slotName, RenderFragment? content)
    {
        if (content is null)
            return;

        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "slot", slotName);
        builder.AddContent(sequence + 2, content);
        builder.CloseElement();
    }

    #endregion
}
