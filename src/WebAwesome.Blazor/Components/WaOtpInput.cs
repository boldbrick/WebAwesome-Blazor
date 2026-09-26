using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An OTP input component that collects one-time passcodes, PINs, and other fixed-length codes, one character
/// per segment. Corresponds to the wa-otp-input Web Awesome component.
/// </summary>
public class WaOtpInput : WaInputBase<string?>
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

    /// <summary>
    /// Plain-text label rendered via the element's "label" attribute; <see cref="MarkupLabel"/> takes
    /// precedence when set.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Plain-text hint rendered via the element's "hint" attribute; <see cref="MarkupHint"/> takes
    /// precedence when set.
    /// </summary>
    [Parameter] public string? Hint { get; set; }

    /// <summary>
    /// Rich markup label rendered into the element's "label" slot; takes precedence over <see cref="Label"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupLabel { get; set; }

    /// <summary>
    /// Rich markup hint rendered into the element's "hint" slot; takes precedence over <see cref="Hint"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupHint { get; set; }

    #endregion

    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// Number of character segments to display. Overridden by <see cref="Format"/> when set.
    /// </summary>
    [Parameter] public int? Length { get; set; }

    /// <summary>
    /// Visual appearance of the segments.
    /// </summary>
    [Parameter] public WaOtpInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Allowed character class.
    /// </summary>
    [Parameter] public WaOtpInputType? Type { get; set; }

    /// <summary>
    /// Case transformation applied to entered characters.
    /// </summary>
    [Parameter] public WaOtpInputCase? Case { get; set; }

    /// <summary>
    /// Segment format string using <c>#</c> as a segment placeholder and any other character as a literal
    /// separator. Setting <see cref="Format"/> overrides <see cref="Length"/> (the segment count is derived from
    /// the number of <c>#</c> characters).
    /// </summary>
    /// <example>"### ###" produces two groups of three with a space between them.</example>
    /// <example>"####-####" produces two groups of four joined by a dash.</example>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// When true, the form is submitted automatically once all segments are filled.
    /// </summary>
    [Parameter] public bool AutoSubmit { get; set; }

    /// <summary>
    /// Automatically focuses the field when the page loads.
    /// </summary>
    [Parameter] public bool AutoFocus { get; set; }

    /// <summary>
    /// When true, entered characters are displayed as <c>--mask-char</c> instead of their real value.
    /// </summary>
    [Parameter] public bool Mask { get; set; }

    /// <summary>
    /// When true, empty segments show <c>--mask-char</c> as a hint instead of appearing blank, similar to how a
    /// password field communicates its expected length before anything is typed.
    /// </summary>
    [Parameter] public bool WithMask { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the control's value is cleared.
    /// </summary>
    [Parameter] public EventCallback OnClear { get; set; }

    /// <summary>
    /// Invoked once when all segments are filled.
    /// </summary>
    /// <remarks>
    /// The underlying Web Awesome event is cancelable via <c>event.preventDefault()</c> to stop
    /// <see cref="AutoSubmit"/> from submitting the form for this completion, but Blazor dispatches custom event
    /// callbacks asynchronously after the DOM event has already run its course, so a .NET handler cannot call back
    /// into the DOM synchronously to prevent the submission. This event is delivered as an informational
    /// notification only.
    /// </remarks>
    [Parameter] public EventCallback OnComplete { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-otp-input");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 11, "autocomplete", Autocomplete);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 12, "label", Label);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 13, "hint", Hint);

        // Add otp-input-specific attributes
        builder.AddAttributeIfNotNull(attributes, 20, "length", Length);
        builder.AddAttributeIfNotNull(attributes, 21, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 22, "type", Type?.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 23, "case", Case?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(attributes, 24, "format", Format);
        builder.AddAttribute(25, "autosubmit", AutoSubmit);
        builder.AddAttribute(26, "autofocus", AutoFocus);
        builder.AddAttribute(27, "mask", Mask);
        builder.AddAttribute(28, "with-mask", WithMask);

        // Add value binding
        builder.AddAttribute(31, "value", CurrentValueAsString);
        builder.AddAttribute(32, "onchange", EventCallback.Factory.CreateBinder<string?>(this, SetCurrentValueAsStringFromElement, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add otp-input-specific event handlers
        builder.AddAttributeIfHasDelegate(49, "onwa-invalid", OnInvalid);
        builder.AddAttributeIfHasDelegate(50, "onwa-clear", OnClear);
        builder.AddAttributeIfHasDelegate(51, "onwa-complete", OnComplete);

        // Add element reference capture
        builder.AddElementReferenceCapture(53, __otpInputReference => Element = __otpInputReference);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70, MarkupLabel, MarkupHint);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

    /// <inheritdoc />
    protected override string? LiveValuePropertyName => "value";

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Clears the current value and returns focus to the field.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ClearAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot clear the field before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "clear");
    }

    /// <summary>
    /// Focuses the field.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the field before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Removes focus from the field.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur the field before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Selects all entered characters in the hidden input.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task SelectAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot select text before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "select");
    }

    #endregion
}
