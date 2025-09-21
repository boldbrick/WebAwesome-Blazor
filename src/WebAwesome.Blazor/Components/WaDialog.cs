using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A dialog component that appears above the page and requires the user's immediate attention.
/// Corresponds to the wa-dialog Web Awesome component.
/// </summary>
public class WaDialog : ComponentBase
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

    // Dialog properties
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Open { get; set; }
    [Parameter] public bool WithoutHeader { get; set; }
    [Parameter] public bool LightDismiss { get; set; }

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

    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }
    [Parameter] public EventCallback<EventArgs> OnInitialFocus { get; set; }

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

        // Add event handlers
        if (OnShow.HasDelegate)
            builder.AddAttribute(20, "wa-show", OnShow);

        if (OnHide.HasDelegate)
            builder.AddAttribute(21, "wa-hide", OnHide);

        if (OnInitialFocus.HasDelegate)
            builder.AddAttribute(22, "wa-initial-focus", OnInitialFocus);

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
}

// TODO: Dialog modal behavior and focus management
// Dialogs require JavaScript for:
// - Modal behavior (backdrop, disable background interaction)
// - Focus trapping and management
// - ESC key handling
// - Initial focus setting
// - Preventing closure (wa-hide event cancellation)
// - Body scroll locking
// Currently provides basic binding support. Advanced modal behaviors require JS interop.
