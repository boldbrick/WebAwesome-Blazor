using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A comparison component used to compare visual differences between similar content with a sliding panel.
/// Corresponds to the wa-comparison Web Awesome component.
/// </summary>
public class WaComparison : ComponentBase
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
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
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

    // Comparison properties
    /// <summary>
    /// The position of the divider as a percentage.
    /// </summary>
    [Parameter] public decimal? Position { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the position changes.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The main comparison content (default slot).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The before content, often an <c>&lt;img&gt;</c> or <c>&lt;svg&gt;</c> element.
    /// </summary>
    [Parameter] public RenderFragment? BeforeContent { get; set; }

    /// <summary>
    /// The after content, often an <c>&lt;img&gt;</c> or <c>&lt;svg&gt;</c> element.
    /// </summary>
    [Parameter] public RenderFragment? AfterContent { get; set; }

    /// <summary>
    /// The icon used inside the handle.
    /// </summary>
    [Parameter] public RenderFragment? HandleContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-comparison");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add comparison-specific attributes
        builder.AddAttributeIfNotNull(10, "position", Position);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "change", OnChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __comparisonReference => Element = __comparisonReference);

        // Add before slot content
        if (BeforeContent is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "before");
            builder.AddContent(42, BeforeContent);
            builder.CloseElement();
        }

        // Add after slot content
        if (AfterContent is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "after");
            builder.AddContent(52, AfterContent);
            builder.CloseElement();
        }

        // Add handle slot content
        if (HandleContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "handle");
            builder.AddContent(62, HandleContent);
            builder.CloseElement();
        }

        // Add main content
        if (ChildContent is not null)
        {
            builder.AddContent(70, ChildContent);
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
