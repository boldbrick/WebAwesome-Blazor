using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An option component that defines selectable items within form controls such as select.
/// Corresponds to the wa-option Web Awesome component.
/// </summary>
public class WaOption : ComponentBase
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

    // Option properties
    [Parameter] public string? Value { get; set; }
    [Parameter] public bool Selected { get; set; }
    [Parameter] public bool Disabled { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<bool> OnSelectedChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The option's content (label)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content to display at the start of the option (typically icons)
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the option (typically icons)
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-option");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "value", Value);
        builder.AddAttribute(5, "selected", Selected);
        builder.AddAttribute(6, "disabled", Disabled);

        // Add event handlers
        if (OnSelectedChange.HasDelegate)
            builder.AddAttribute(10, "wa-change", OnSelectedChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(11, __optionReference => Element = __optionReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(20, "span");
            builder.AddAttribute(21, "slot", "start");
            builder.AddContent(22, StartContent);
            builder.CloseElement();
        }

        // Add main content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "end");
            builder.AddContent(42, EndContent);
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
