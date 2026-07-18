using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A radio group component that contains multiple radio buttons and functions as a single form control.
/// Corresponds to the wa-radio-group Web Awesome component.
/// </summary>
public class WaRadioGroup : WaInputBase<string?>
{
    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// The name of the radio group, submitted as a name/value pair with form data.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// The orientation in which to show radio items.
    /// </summary>
    [Parameter] public WaOrientation? Orientation { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the radio group's selected value changes.
    /// </summary>
    [Parameter] public EventCallback<string?> OnValueChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The radio buttons to display in this group
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-radio-group");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add radio group specific attributes
        builder.AddAttributeIfNotNullOrEmpty(20, "name", Name);
        builder.AddAttributeIfNotNull(21, "orientation", Orientation?.ToHtmlValue());

        // Add value binding
        builder.AddAttribute(30, "value", CurrentValueAsString);
        builder.AddAttribute(31, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add radio group specific event handlers
        if (OnValueChange.HasDelegate)
            builder.AddAttribute(50, "wa-change", OnValueChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(51, __radioGroupReference => Element = __radioGroupReference);

        // Add child content (radio buttons)
        if (ChildContent is not null)
        {
            builder.AddContent(60, ChildContent);
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

    #endregion

}
