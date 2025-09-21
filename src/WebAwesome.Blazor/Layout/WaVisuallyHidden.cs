using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Layout;

/// <summary>
/// A utility component that makes content accessible to assistive devices without displaying it on screen.
/// Corresponds to the wa-visually-hidden CSS utility class.
/// </summary>
public class WaVisuallyHidden : ComponentBase
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

    // Visually hidden properties
    [Parameter] public bool Force { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The content that should be hidden visually but available to screen readers
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
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
    /// Gets the CSS class string with visually hidden utility
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        // Add appropriate visually hidden class
        if (Force)
        {
            classes.Add("wa-visually-hidden-force");
        }
        else
        {
            classes.Add("wa-visually-hidden");
        }

        // Add user classes
        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    #endregion
}
