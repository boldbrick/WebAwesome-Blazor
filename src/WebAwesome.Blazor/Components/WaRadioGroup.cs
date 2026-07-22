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

    /// <summary>
    /// Reserves space for the hint even when it is not populated.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Reserves space for the label even when it is not populated.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the radio group's selected value changes.
    /// </summary>
    [Parameter] public EventCallback<string?> OnValueChange { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints are not satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

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
        builder.AddAttribute(22, "with-hint", WithHint);
        builder.AddAttribute(23, "with-label", WithLabel);

        // Add value binding
        builder.AddAttribute(30, "value", CurrentValueAsString);
        builder.AddAttribute(31, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add radio group specific event handlers
        builder.AddAttributeIfHasDelegate(50, "onwa-change", OnValueChange);
        builder.AddAttributeIfHasDelegate(52, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(53, __radioGroupReference => Element = __radioGroupReference);

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

    #region ------ Public Methods ------

    /// <summary>
    /// Sets focus on the radio group.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    #endregion

}
