using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A dialog component that appears above the page and requires the user's immediate attention.
/// Corresponds to the wa-dialog Web Awesome component.
/// </summary>
public class WaDialog : ComponentBase
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

    // Dialog properties
    /// <summary>
    /// The dialog's label as displayed in the header. A relevant label is required for proper accessibility. If you
    /// need to display HTML, use <see cref="HeaderActionsContent"/> or a custom header instead.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Indicates whether or not the dialog is open. Toggle this to show and hide the dialog.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// Disables the header. This also removes the default close button.
    /// </summary>
    [Parameter] public bool WithoutHeader { get; set; }

    /// <summary>
    /// When enabled, the dialog is closed when the user clicks outside of it.
    /// </summary>
    [Parameter] public bool LightDismiss { get; set; }

    /// <summary>
    /// Only required for SSR. Set to true when slotting content into the footer (via <see cref="FooterContent"/>)
    /// so the server-rendered markup includes the footer before the component hydrates on the client.
    /// </summary>
    [Parameter] public bool WithFooter { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The main dialog content
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content for the dialog footer
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Content for additional header actions
    /// </summary>
    [Parameter] public RenderFragment? HeaderActionsContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the dialog opens.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

    /// <summary>
    /// Invoked when the dialog is requested to close. Avoid relying on this to prevent closing unless it would
    /// result in destructive behavior such as data loss.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }

    /// <summary>
    /// Invoked when the dialog sets initial focus after opening.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInitialFocus { get; set; }

    /// <summary>
    /// Invoked after the dialog opens and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }

    /// <summary>
    /// Invoked after the dialog closes and all animations are complete.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-dialog");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add dialog-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "label", Label);
        builder.AddAttribute(11, "open", Open);
        builder.AddAttribute(12, "without-header", WithoutHeader);
        builder.AddAttribute(13, "light-dismiss", LightDismiss);
        builder.AddAttribute(14, "with-footer", WithFooter);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "onwa-show", OnShow);

        builder.AddAttributeIfHasDelegate(21, "onwa-hide", OnHide);

        builder.AddAttributeIfHasDelegate(22, "onwa-initial-focus", OnInitialFocus);

        builder.AddAttributeIfHasDelegate(50, "onwa-after-show", OnAfterShow);

        builder.AddAttributeIfHasDelegate(51, "onwa-after-hide", OnAfterHide);

        // Add element reference capture
        builder.AddElementReferenceCapture(23, __dialogReference => Element = __dialogReference);

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
    /// Shows the dialog with focus management and modal behavior
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task ShowAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show dialog: component has not been rendered yet.");

        // wa-dialog exposes no hide() method in WA 3.0 - open/close is driven by the "open"
        // property; use it for both directions for symmetry
        await JSInterop.SetPropertyAsync(Element.Value, "open", true);
    }

    /// <summary>
    /// Hides the dialog with focus restoration
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task HideAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot hide dialog: component has not been rendered yet.");

        await JSInterop.SetPropertyAsync(Element.Value, "open", false);
    }

    /// <summary>
    /// Focuses the dialog or first focusable element within it
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus dialog: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    #endregion
}
