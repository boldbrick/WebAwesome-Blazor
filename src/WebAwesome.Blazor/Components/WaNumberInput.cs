using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A numeric input component for editing <see cref="decimal"/> values, with optional increment/decrement steppers.
/// Corresponds to the wa-number-input Web Awesome component.
/// </summary>
public class WaNumberInput : WaInputBase<decimal?>
{
    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// The input's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Indicates that the input should receive focus on page load.
    /// </summary>
    [Parameter] public bool AutoFocus { get; set; }

    /// <summary>
    /// Used to customize the label or icon of the Enter key on virtual keyboards.
    /// </summary>
    [Parameter] public string? EnterKeyHint { get; set; }

    /// <summary>
    /// Tells the browser what type of data will be entered by the user, allowing it to display the appropriate
    /// virtual keyboard on supportive devices.
    /// </summary>
    [Parameter] public string? InputMode { get; set; }

    /// <summary>
    /// The input's maximum value.
    /// </summary>
    [Parameter] public decimal? Max { get; set; }

    /// <summary>
    /// The input's minimum value.
    /// </summary>
    [Parameter] public decimal? Min { get; set; }

    /// <summary>
    /// Draws a pill-style input with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Placeholder text to show as a hint when the input is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Specifies the granularity that the value must adhere to. Set to <c>any</c> to disable stepping constraints.
    /// </summary>
    [Parameter] public string? Step { get; set; }

    /// <summary>
    /// Hides the increment and decrement stepper buttons.
    /// </summary>
    [Parameter] public bool WithoutSteppers { get; set; }

    /// <summary>
    /// Used for SSR. Determines whether the SSRed component has the hint slot rendered on initial paint.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Used for SSR. Determines whether the SSRed component has the label slot rendered on initial paint.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// Content to display at the start of the input.
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the input.
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="StartContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? StartIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="EndContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? EndIconName { get; set; }

    /// <summary>
    /// Content to display in place of the default increment icon.
    /// </summary>
    [Parameter] public RenderFragment? IncrementIconContent { get; set; }

    /// <summary>
    /// Content to display in place of the default decrement icon.
    /// </summary>
    [Parameter] public RenderFragment? DecrementIconContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-number-input");

        // Add common attributes
        var sequence = AddCommonAttributes(builder, 1);

        // Add number-input-specific attributes
        builder.AddAttributeIfNotNull(20, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttribute(21, "autofocus", AutoFocus);
        builder.AddAttributeIfNotNullOrEmpty(22, "enterkeyhint", EnterKeyHint);
        builder.AddAttributeIfNotNullOrEmpty(23, "inputmode", InputMode);
        builder.AddAttributeIfNotNull(24, "max", Max);
        builder.AddAttributeIfNotNull(25, "min", Min);
        builder.AddAttribute(26, "pill", Pill);
        builder.AddAttributeIfNotNullOrEmpty(27, "placeholder", Placeholder);
        builder.AddAttributeIfNotNullOrEmpty(28, "step", Step);
        builder.AddAttribute(29, "without-steppers", WithoutSteppers);
        builder.AddAttribute(39, "with-hint", WithHint);
        builder.AddAttribute(46, "with-label", WithLabel);

        // Add value binding
        builder.AddAttribute(31, "value", CurrentValueAsString);
        builder.AddAttribute(32, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add number-input-specific event handlers
        builder.AddAttributeIfHasDelegate(49, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(53, __numberInputReference => Element = __numberInputReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "start");
            builder.AddContent(62, StartContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(100, "start", StartIconName);
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(65, "span");
            builder.AddAttribute(66, "slot", "end");
            builder.AddContent(67, EndContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(105, "end", EndIconName);
        }

        // Add increment-icon slot content
        if (IncrementIconContent is not null)
        {
            builder.OpenElement(80, "span");
            builder.AddAttribute(81, "slot", "increment-icon");
            builder.AddContent(82, IncrementIconContent);
            builder.CloseElement();
        }

        // Add decrement-icon slot content
        if (DecrementIconContent is not null)
        {
            builder.OpenElement(85, "span");
            builder.AddAttribute(86, "slot", "decrement-icon");
            builder.AddContent(87, DecrementIconContent);
            builder.CloseElement();
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out decimal? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = null;
            validationErrorMessage = null;
            return true;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue))
        {
            result = parsedValue;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = $"The {DisplayName} field must be a number.";
        return false;
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets focus on the input.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the input before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Removes focus from the input.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur the input before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Selects all the text in the input.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task SelectAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot select text before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "select");
    }

    /// <summary>
    /// Increments the value of the input by the value of the <see cref="Step"/> attribute.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task StepUpAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot step up before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "stepUp");
    }

    /// <summary>
    /// Decrements the value of the input by the value of the <see cref="Step"/> attribute.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task StepDownAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot step down before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "stepDown");
    }

    #endregion
}
