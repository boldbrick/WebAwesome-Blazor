using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A drawer component that slides in from a container to expose additional options and information.
/// Corresponds to the wa-drawer Web Awesome component.
/// </summary>
public class WaDrawer : ComponentBase
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
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Drawer properties
    /// <summary>
    /// The drawer's label as displayed in the header. A relevant label is required for proper accessibility. If you
    /// need to display HTML, use <see cref="HeaderActionsContent"/> or a custom header instead.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Indicates whether or not the drawer is open. Toggle this to show and hide the drawer.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// The direction from which the drawer opens.
    /// </summary>
    [Parameter] public WaDrawerPlacement Placement { get; set; } = WaDrawerPlacement.End;

    /// <summary>
    /// Disables the header. This also removes the default close button.
    /// </summary>
    [Parameter] public bool WithoutHeader { get; set; }

    /// <summary>
    /// When enabled, the drawer is closed when the user clicks outside of it.
    /// </summary>
    [Parameter] public bool LightDismiss { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The main drawer content
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content for the drawer footer
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Content for additional header actions
    /// </summary>
    [Parameter] public RenderFragment? HeaderActionsContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the drawer opens.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked when the drawer is requested to close. Avoid relying on this to prevent closing unless it would
    /// result in destructive behavior such as data loss.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <summary>
    /// Invoked when the drawer sets initial focus after opening.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInitialFocus { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-drawer");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add drawer-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "label", Label);
        builder.AddAttribute(11, "open", Open);
        if (Placement != WaDrawerPlacement.End)
            builder.AddAttribute(12, "placement", Placement.ToHtmlValue());
        builder.AddAttribute(13, "without-header", WithoutHeader);
        builder.AddAttribute(14, "light-dismiss", LightDismiss);

        // Add event handlers
        if (OnShow.HasDelegate)
            builder.AddAttribute(20, "wa-show", OnShow);

        if (OnHide.HasDelegate)
            builder.AddAttribute(21, "wa-hide", OnHide);

        if (OnInitialFocus.HasDelegate)
            builder.AddAttribute(22, "wa-initial-focus", OnInitialFocus);

        // Add element reference capture
        builder.AddElementReferenceCapture(23, __drawerReference => Element = __drawerReference);

        // Add header actions slot content
        if (HeaderActionsContent is not null)
        {
            builder.OpenElement(30, "div");
            builder.AddAttribute(31, "slot", "header-actions");
            builder.AddContent(32, HeaderActionsContent);
            builder.CloseElement();
        }

        // Add main content
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        // Add footer slot content
        if (FooterContent is not null)
        {
            builder.OpenElement(50, "div");
            builder.AddAttribute(51, "slot", "footer");
            builder.AddContent(52, FooterContent);
            builder.CloseElement();
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
    /// Shows the drawer with focus management and slide-in animation
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show drawer: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "show");
    }

    /// <summary>
    /// Hides the drawer with focus restoration and slide-out animation
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide drawer: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "hide");
    }

    /// <summary>
    /// Focuses the drawer or first focusable element within it
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus drawer: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
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
