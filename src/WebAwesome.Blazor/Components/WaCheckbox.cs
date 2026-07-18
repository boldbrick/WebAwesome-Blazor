using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A checkbox input component for editing <see cref="bool"/> values.
/// Corresponds to the wa-checkbox Web Awesome component.
/// </summary>
public class WaCheckbox : WaInputBase<bool>
{
    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// Draws the checkbox in an indeterminate state. This is usually applied to checkboxes that represent a
    /// "select all/none" behavior when associated checkboxes have a mix of checked and unchecked states.
    /// </summary>
    [Parameter] public bool Indeterminate { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the checked state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> OnCheckedChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The content to display next to the checkbox (typically the label text)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-checkbox");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add checkbox-specific attributes
        builder.AddAttribute(20, "checked", BindConverter.FormatValue(CurrentValue));
        builder.AddAttribute(21, "indeterminate", Indeterminate);

        // Include the "value" attribute so that when this is posted by a form, "true"
        // is included in the form fields. That's how <input type="checkbox"> works normally.
        builder.AddAttribute(22, "value", bool.TrueString);

        // Add value binding
        builder.AddAttribute(23, "onchange", EventCallback.Factory.CreateBinder<bool>(this, __value => CurrentValue = __value, CurrentValue));
        builder.SetUpdatesAttributeName("checked");

        // Add common event handlers
        AddCommonEventHandlers(builder, 30);

        // Add checkbox-specific event handlers
        if (OnCheckedChange.HasDelegate)
            builder.AddAttribute(40, "wa-change", OnCheckedChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(41, __checkboxReference => Element = __checkboxReference);

        // Add child content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(50, ChildContent);
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 60);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out bool result, [NotNullWhen(false)] out string? validationErrorMessage)
        => throw new NotSupportedException($"This component does not parse string inputs. Bind to the '{nameof(CurrentValue)}' property, not '{nameof(CurrentValueAsString)}'.");

    #endregion

}
