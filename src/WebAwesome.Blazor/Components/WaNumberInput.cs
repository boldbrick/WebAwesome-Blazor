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
public class WaNumberInput : WaLabeledInputBase<decimal?>, IWaAffixedControl
{
    #region ------ Form Control Properties ------

    /// <summary>
    /// Makes the input read-only, allowing its value to be seen but not edited.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// Marks the input as required for form validation.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Value of the browser's "autocomplete" attribute controlling autofill behavior.
    /// </summary>
    [Parameter] public string? Autocomplete { get; set; }

    #endregion

    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// The Web Awesome default of <see cref="Appearance"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaInputAppearance DefaultAppearance = WaInputAppearance.Outlined;

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
    [Parameter] public WaEnterKeyHint? EnterKeyHint { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="InputMode"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaNumberInputMode DefaultInputMode = WaNumberInputMode.Numeric;

    /// <summary>
    /// Tells the browser what type of data will be entered by the user, allowing it to display the appropriate
    /// virtual keyboard on supportive devices.
    /// </summary>
    [Parameter] public WaNumberInputMode? InputMode { get; set; }

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
    /// The Web Awesome default of <see cref="Placeholder"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultPlaceholder = "";

    /// <summary>
    /// Placeholder text to show as a hint when the input is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Step"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public static readonly WaValueStep DefaultStep = 1;

    /// <summary>
    /// Specifies the granularity that the value must adhere to, or <see cref="WaValueStep.Any"/> to disable stepping
    /// constraints. A number converts implicitly (<c>Step="0.5"</c>).
    /// </summary>
    [Parameter] public WaValueStep? Step { get; set; }

    /// <summary>
    /// Hides the increment and decrement stepper buttons.
    /// </summary>
    [Parameter] public bool WithoutSteppers { get; set; }

    /// <summary>
    /// Binds the value on every keystroke (the "input" event) instead of only when the change is committed (the
    /// "change" event, e.g. on blur), so the model is current while the user is typing - for example when a key
    /// handler reads it. Named after the equivalent MudBlazor and Fluent UI Blazor parameter. <see cref="WaInputBase{TValue}.OnInput"/>
    /// is still invoked, after the value has been updated.
    /// </summary>
    [Parameter] public bool Immediate { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    /// <summary>
    /// Invoked before the value changes. The change can be prevented by cancelling the event.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnBeforeInput { get; set; }

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
        var attributes = builder.OpenWaElement(this, 0, "wa-number-input");

        // Add common attributes
        var sequence = AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNullOrEmpty(11, "autocomplete", Autocomplete);
        AddLabelAndHintAttributes(builder, 12);

        // Add number-input-specific attributes
        builder.AddAttributeIfNotNull(attributes, 20, "appearance", Appearance?.ToHtmlValue(), DefaultAppearance.ToHtmlValue());
        builder.AddAttribute(21, "autofocus", AutoFocus);
        builder.AddAttributeIfNotNull(22, "enterkeyhint", EnterKeyHint?.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 23, "inputmode", InputMode?.ToHtmlValue(), DefaultInputMode.ToHtmlValue());
        builder.AddAttributeIfNotNull(24, "max", Max);
        builder.AddAttributeIfNotNull(25, "min", Min);
        builder.AddAttribute(26, "pill", Pill);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 27, "placeholder", Placeholder, DefaultPlaceholder);
        builder.AddStepAttribute(attributes, 28, "step", Step, DefaultStep);
        builder.AddAttribute(29, "without-steppers", WithoutSteppers);
        AddWithHintAndLabelAttributes(builder, 14);

        // Add value binding
        builder.AddAttribute(31, "value", CurrentValueAsString);
        builder.AddAttribute(32, "onchange", EventCallback.Factory.CreateBinder<string?>(this, SetCurrentValueAsStringFromElement, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers; with immediate binding, the value binder and OnInput share one oninput handler
        AddCommonEventHandlers(builder, 40, includeInputHandler: !Immediate);
        if (Immediate)
            builder.AddAttribute(47, "oninput", CreateImmediateInputHandler());

        // Add number-input-specific event handlers
        builder.AddAttributeIfHasDelegate(49, "onwa-invalid", OnInvalid);
        builder.AddAttributeIfHasDelegate(50, "onbeforeinput", OnBeforeInput);

        // Add element reference capture
        builder.AddElementReferenceCapture(53, __numberInputReference => Element = __numberInputReference);

        // Add start and end slot content (the fragment wins over the icon-name shortcut)
        FormControlRendering.AddAffixSlots(builder, 60, this, StartIconName, EndIconName);

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

        if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue))
        {
            result = parsedValue;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = $"The {DisplayName} field must be a number.";
        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Formats with the invariant culture, the form the element's value attribute and live value property parse and
    /// <see cref="TryParseValueFromString"/> reads back (the base class would use the current culture, e.g. "2,5").
    /// </remarks>
    protected override string? FormatValueAsString(decimal? value) => value?.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    protected override string? LiveValuePropertyName => "value";

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
