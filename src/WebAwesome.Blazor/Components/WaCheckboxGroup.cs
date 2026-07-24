using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// Gives a set of related checkboxes or switches a shared label, hint, and grouping semantics.
/// Corresponds to the wa-checkbox-group Web Awesome component.
/// <para>
/// The grouped checkboxes and switches remain independent form controls with their own name, value,
/// and validation; the group exists only to provide a shared label, hint, sizing, and accessible
/// grouping. It carries no value of its own, so it is not a form control and does not participate in
/// two-way binding.
/// </para>
/// </summary>
public class WaCheckboxGroup : ComponentBase
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

    /// <summary>
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Checkbox group properties
    /// <summary>
    /// Plain-text label rendered via the element's "label" attribute. Required for proper accessibility.
    /// For labels that contain HTML, use <see cref="MarkupLabel"/> instead.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Plain-text hint rendered via the element's "hint" attribute, describing how to use the group.
    /// For hints that contain HTML, use <see cref="MarkupHint"/> instead.
    /// </summary>
    [Parameter] public string? Hint { get; set; }

    /// <summary>
    /// The orientation in which to show grouped checkboxes. Defaults to vertical when not set.
    /// </summary>
    [Parameter] public WaOrientation? Orientation { get; set; }

    /// <summary>
    /// The group's size. When set, this size is applied to all grouped checkboxes and switches,
    /// overriding any size set on the individual items.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// Indicates that at least one option should be selected. This only adds a visual indicator to the
    /// label; because each checkbox is an independent control, the group does not enforce the requirement.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Reserves space for the hint even when it is not populated. Only required for server-side rendering
    /// when slotting in a hint element so the server-rendered markup includes it before hydration.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Reserves space for the label even when it is not populated. Only required for server-side rendering
    /// when slotting in a label element so the server-rendered markup includes it before hydration.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The grouped checkboxes or switches (WaCheckbox / WaSwitch components).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Rich markup label rendered into the "label" slot; takes precedence over <see cref="Label"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupLabel { get; set; }

    /// <summary>
    /// Rich markup hint rendered into the "hint" slot; takes precedence over <see cref="Hint"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupHint { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-checkbox-group");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", Class);
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add checkbox group specific attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "label", Label);
        builder.AddAttributeIfNotNullOrEmpty(11, "hint", Hint);
        builder.AddAttributeIfNotNull(12, "orientation", Orientation?.ToHtmlValue());
        builder.AddAttributeIfNotNull(13, "size", Size?.ToHtmlValue());
        builder.AddAttribute(14, "required", Required);
        builder.AddAttribute(15, "with-hint", WithHint);
        builder.AddAttribute(16, "with-label", WithLabel);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __checkboxGroupReference => Element = __checkboxGroupReference);

        // Add child content (checkboxes / switches)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        // Add label and hint markup slots
        if (MarkupLabel is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "label");
            builder.AddContent(42, MarkupLabel);
            builder.CloseElement();
        }

        if (MarkupHint is not null)
        {
            builder.OpenElement(45, "span");
            builder.AddAttribute(46, "slot", "hint");
            builder.AddContent(47, MarkupHint);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    #endregion
}
