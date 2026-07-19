using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Layout;

/// <summary>
/// A layout utility component that creates a responsive container with consistent proportions to enclose content.
/// Corresponds to the wa-frame CSS utility class.
/// </summary>
public class WaFrame : ComponentBase
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
    /// Additional CSS class names appended to the wa-frame utility class on the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Frame properties
    /// <summary>
    /// Aspect ratio applied as a wa-frame:* modifier class; defaults to a square frame.
    /// </summary>
    [Parameter] public FrameAspectRatio AspectRatio { get; set; } = FrameAspectRatio.Square;

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The content to be displayed within the frame
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddElementReferenceCapture(4, __elementReference => Element = __elementReference);

        if (ChildContent is not null)
        {
            builder.AddContent(5, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Gets the CSS class string with frame utility and modifiers
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        // Base frame class with aspect ratio modifier
        var baseClass = AspectRatio == FrameAspectRatio.Square ? "wa-frame" : $"wa-frame:{AspectRatio.ToHtmlValue()}";
        classes.Add(baseClass);

        // Add user classes
        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    #endregion
}
