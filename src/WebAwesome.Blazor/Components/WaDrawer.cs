using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A drawer component that slides in from a container to expose additional options and information.
/// Corresponds to the wa-drawer Web Awesome component.
/// </summary>
public class WaDrawer : ComponentBase
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

    // Drawer properties
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Open { get; set; }
    [Parameter] public WaDrawerPlacement Placement { get; set; } = WaDrawerPlacement.End;
    [Parameter] public bool WithoutHeader { get; set; }
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

    [Parameter] public EventCallback<EventArgs> OnShow { get; set; }
    [Parameter] public EventCallback<EventArgs> OnHide { get; set; }
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
}

// TODO: Drawer slide-in behavior and focus management
// Drawers require JavaScript for:
// - Slide-in/out animations from different sides
// - Modal overlay behavior
// - Focus trapping and management
// - ESC key handling
// - Initial focus setting
// - Preventing closure (wa-hide event cancellation)
// - Responsive size handling
// Currently provides basic binding support. Advanced slide behaviors require JS interop.
