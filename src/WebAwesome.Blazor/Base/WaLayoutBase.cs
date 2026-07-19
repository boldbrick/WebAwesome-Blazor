using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Layout;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Base class for Web Awesome layout utility components that provides common functionality
/// for gap, alignment, and styling
/// </summary>
public abstract class WaLayoutBase : ComponentBase
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

    // Common styling parameters
    /// <summary>
    /// Additional CSS class names appended to the layout utility class on the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Layout properties
    /// <summary>
    /// Spacing token applied as a wa-gap-* modifier class between child elements.
    /// </summary>
    [Parameter] public GapSize? Gap { get; set; }

    /// <summary>
    /// Cross-axis alignment token applied as a wa-align-items-* modifier class.
    /// </summary>
    [Parameter] public AlignItems? AlignItems { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The child content to be arranged by this layout component
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Protected Methods ------

    /// <summary>
    /// Gets the CSS class string combining layout utility class, user classes, and modifiers
    /// </summary>
    protected string GetCombinedCssClass(string baseLayoutClass)
    {
        var classes = new List<string> { baseLayoutClass };

        if (Gap.HasValue)
            classes.Add($"wa-gap-{Gap.Value.ToHtmlValue()}");

        if (AlignItems.HasValue)
            classes.Add($"wa-align-items-{AlignItems.Value.ToHtmlValue()}");

        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    /// <summary>
    /// Adds common attributes to the render tree builder
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="sequence">The starting sequence number</param>
    /// <param name="elementName">The HTML element name to use</param>
    /// <param name="baseLayoutClass">The base CSS class for this layout type</param>
    /// <returns>The next available sequence number</returns>
    protected int AddCommonLayoutAttributes(RenderTreeBuilder builder, int sequence, string elementName, string baseLayoutClass)
    {
        builder.OpenElement(sequence + 0, elementName);
        builder.AddMultipleAttributes(sequence + 1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(sequence + 2, "class", GetCombinedCssClass(baseLayoutClass));
        builder.AddAttributeIfNotNullOrEmpty(sequence + 3, "style", Style);
        builder.AddElementReferenceCapture(sequence + 4, __elementReference => Element = __elementReference);

        if (ChildContent is not null)
        {
            builder.AddContent(sequence + 5, ChildContent);
        }

        builder.CloseElement();

        return sequence + 6;
    }

    #endregion
}
