using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A single-line input component for editing <see cref="string"/> values.
/// Corresponds to the wa-input Web Awesome component.
/// </summary>
public class WaInput : WaInputBase<string?>
{
    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// Placeholder text to show as a hint when the input is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The type of input. Works the same as a native <c>&lt;input&gt;</c> element, but only a subset of types
    /// are supported.
    /// </summary>
    [Parameter] public WaInputType Type { get; set; } = WaInputType.Text;

    /// <summary>
    /// The input's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Draws a pill-style input with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Adds a clear button when the input is not empty.
    /// </summary>
    [Parameter] public bool WithClear { get; set; }

    /// <summary>
    /// Adds a button to toggle the password's visibility. Only applies to password types.
    /// </summary>
    [Parameter] public bool PasswordToggle { get; set; }

    /// <summary>
    /// Enables spell checking on the input.
    /// </summary>
    [Parameter] public bool? Spellcheck { get; set; }

    // Input-specific validation
    /// <summary>
    /// A regular expression pattern to validate input against.
    /// </summary>
    [Parameter] public string? Pattern { get; set; }

    /// <summary>
    /// The input's minimum value. Only applies to date and number input types.
    /// </summary>
    [Parameter] public decimal? Min { get; set; }

    /// <summary>
    /// The input's maximum value. Only applies to date and number input types.
    /// </summary>
    [Parameter] public decimal? Max { get; set; }

    /// <summary>
    /// Specifies the granularity that the value must adhere to. Only applies to date and number input types.
    /// </summary>
    [Parameter] public decimal? Step { get; set; }

    /// <summary>
    /// Controls whether and how text input is automatically capitalized as it is entered by the user.
    /// </summary>
    [Parameter] public string? AutoCapitalize { get; set; }

    /// <summary>
    /// Indicates whether the browser's autocorrect feature is on or off.
    /// </summary>
    [Parameter] public string? AutoCorrect { get; set; }

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
    /// Determines whether or not the password is currently visible. Only applies to password input types.
    /// </summary>
    [Parameter] public bool PasswordVisible { get; set; }

    /// <summary>
    /// Used for SSR. Determines whether the SSRed component has the hint slot rendered on initial paint.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Used for SSR. Determines whether the SSRed component has the label slot rendered on initial paint.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    /// <summary>
    /// Hides the browser's built-in increment/decrement spin buttons for number inputs.
    /// </summary>
    [Parameter] public bool WithoutSpinButtons { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the clear button is activated.
    /// </summary>
    [Parameter] public EventCallback OnClear { get; set; }

    /// <summary>
    /// Invoked when the password visibility toggle button is activated.
    /// </summary>
    [Parameter] public EventCallback OnPasswordToggle { get; set; }

    /// <summary>
    /// Invoked when the password's visibility changes.
    /// </summary>
    [Parameter] public EventCallback<bool> OnPasswordVisibilityChange { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// Content to display at the start of the input
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the input
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

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-input");

        // Add common attributes
        var sequence = AddCommonAttributes(builder, 1);

        // Add input-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(20, "placeholder", Placeholder);
        builder.AddAttribute(21, "type", Type.ToHtmlValue());
        builder.AddAttributeIfNotNull(22, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttribute(23, "pill", Pill);
        builder.AddAttribute(24, "with-clear", WithClear);
        builder.AddAttribute(25, "password-toggle", PasswordToggle);
        builder.AddAttributeIfNotNull(26, "spellcheck", Spellcheck);
        builder.AddAttributeIfNotNullOrEmpty(27, "pattern", Pattern);
        builder.AddAttributeIfNotNull(28, "min", Min);
        builder.AddAttributeIfNotNull(29, "max", Max);
        builder.AddAttributeIfNotNull(30, "step", Step);
        builder.AddAttributeIfNotNullOrEmpty(33, "autocapitalize", AutoCapitalize);
        builder.AddAttributeIfNotNullOrEmpty(34, "autocorrect", AutoCorrect);
        builder.AddAttribute(35, "autofocus", AutoFocus);
        builder.AddAttributeIfNotNullOrEmpty(36, "enterkeyhint", EnterKeyHint);
        builder.AddAttributeIfNotNullOrEmpty(37, "inputmode", InputMode);
        builder.AddAttribute(38, "password-visible", PasswordVisible);
        builder.AddAttribute(39, "with-hint", WithHint);
        builder.AddAttribute(46, "with-label", WithLabel);
        builder.AddAttribute(48, "without-spin-buttons", WithoutSpinButtons);

        // Add value binding
        builder.AddAttribute(31, "value", CurrentValueAsString);
        builder.AddAttribute(32, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add input-specific event handlers
        builder.AddAttributeIfHasDelegate(50, "wa-clear", OnClear);

        builder.AddAttributeIfHasDelegate(51, "wa-password-toggle", OnPasswordToggle);

        builder.AddAttributeIfHasDelegate(52, "wa-password-visibility-change", OnPasswordVisibilityChange);

        builder.AddAttributeIfHasDelegate(49, "wa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(53, __inputReference => Element = __inputReference);

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
    /// Replaces a range of text with a new string.
    /// </summary>
    /// <param name="replacement">The replacement text</param>
    /// <param name="start">The zero-based index of the first character to replace</param>
    /// <param name="end">The zero-based index of the character after the last character to replace</param>
    /// <param name="selectMode">How the selection should be set after the text is replaced. One of
    /// <c>select</c>, <c>start</c>, <c>end</c>, or <c>preserve</c> (default)</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task SetRangeTextAsync(string replacement, int? start = null, int? end = null, string selectMode = "preserve")
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set range text before the component is rendered. Element reference is null.");

        // the DOM setRangeText overloads accept either one argument or all four; null start/end would coerce to 0 in JS
        if (start.HasValue && end.HasValue)
            await JSInterop.InvokeMethodAsync(Element.Value, "setRangeText", replacement, start.Value, end.Value, selectMode);
        else
            await JSInterop.InvokeMethodAsync(Element.Value, "setRangeText", replacement);
    }

    /// <summary>
    /// Sets the start and end positions of the text selection (0-based).
    /// </summary>
    /// <param name="selectionStart">The zero-based index of the first selected character</param>
    /// <param name="selectionEnd">The zero-based index of the character after the last selected character</param>
    /// <param name="selectionDirection">The direction in which selection is considered to have occurred. One of
    /// <c>forward</c>, <c>backward</c>, or <c>none</c> (default)</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task SetSelectionRangeAsync(int selectionStart, int selectionEnd, string selectionDirection = "none")
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set the selection range before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "setSelectionRange", selectionStart, selectionEnd, selectionDirection);
    }

    /// <summary>
    /// Displays the browser picker for the input element (only works if the browser supports it for the input
    /// type).
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ShowPickerAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot show the picker before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "showPicker");
    }

    /// <summary>
    /// Decrements the value of a numeric input type by the value of the <see cref="Step"/> attribute.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task StepDownAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot step down before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "stepDown");
    }

    /// <summary>
    /// Increments the value of a numeric input type by the value of the <see cref="Step"/> attribute.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task StepUpAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot step up before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "stepUp");
    }

    #endregion
}
