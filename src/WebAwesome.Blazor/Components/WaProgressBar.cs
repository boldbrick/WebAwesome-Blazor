using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A progress bar component used to show the status of an ongoing operation.
/// Corresponds to the wa-progress-bar Web Awesome component.
/// </summary>
public class WaProgressBar : ComponentBase
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
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    // Common styling parameters
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Progress bar properties
    /// <summary>
    /// The current progress as a percentage, 0 to 100.
    /// </summary>
    [Parameter] public int Value { get; set; } = 0;

    /// <summary>
    /// A custom label for assistive devices.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// When true, <see cref="Value"/> is ignored, the label is hidden, and the progress bar is drawn in an
    /// indeterminate state.
    /// </summary>
    [Parameter] public bool Indeterminate { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// Content to display inside the progress bar (typically percentage text)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-progress-bar");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add progress bar-specific attributes
        if (!Indeterminate)
            builder.AddAttribute(10, "value", Value);
        builder.AddAttributeIfNotNullOrEmpty(11, "label", Label);
        builder.AddAttribute(12, "indeterminate", Indeterminate);

        // Add element reference capture
        builder.AddElementReferenceCapture(13, __progressBarReference => Element = __progressBarReference);

        // Add content (progress text)
        if (ChildContent is not null)
        {
            builder.AddContent(20, ChildContent);
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
