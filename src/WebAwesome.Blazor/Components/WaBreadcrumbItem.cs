using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A breadcrumb item component that represents different links within a breadcrumb navigation.
/// Corresponds to the wa-breadcrumb-item Web Awesome component.
/// </summary>
public class WaBreadcrumbItem : ComponentBase
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

    /// <summary>
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Breadcrumb item properties
    /// <summary>
    /// Optional URL to navigate to when the breadcrumb item is activated. When set, a link is rendered internally;
    /// when unset, a button is rendered instead.
    /// </summary>
    [Parameter] public string? Href { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The main breadcrumb item content (label)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content to display at the start of the breadcrumb item
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the breadcrumb item
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="StartContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? StartIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="EndContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? EndIconName { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the breadcrumb item is clicked.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-breadcrumb-item");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add breadcrumb item-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "href", Href);

        // Add event handlers
        if (OnClick.HasDelegate)
            builder.AddAttribute(20, "onclick", OnClick);

        // Add element reference capture
        builder.AddElementReferenceCapture(21, __breadcrumbItemReference => Element = __breadcrumbItemReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(30, "span");
            builder.AddAttribute(31, "slot", "start");
            builder.AddContent(32, StartContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(60, "start", StartIconName);
        }

        // Add main content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "end");
            builder.AddContent(52, EndContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(70, "end", EndIconName);
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
