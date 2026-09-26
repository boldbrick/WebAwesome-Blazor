using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A multiline input component for editing <see cref="string"/> values.
/// Corresponds to the wa-textarea Web Awesome component.
/// </summary>
public class WaTextArea : WaLabeledInputBase<string?>
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
    /// Minimum number of characters required for a valid value.
    /// </summary>
    [Parameter] public int? MinLength { get; set; }

    /// <summary>
    /// Maximum number of characters allowed for the value.
    /// </summary>
    [Parameter] public int? MaxLength { get; set; }

    /// <summary>
    /// Value of the browser's "autocomplete" attribute controlling autofill behavior.
    /// </summary>
    [Parameter] public string? Autocomplete { get; set; }

    #endregion

    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// Placeholder text to show as a hint when the input is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The number of rows to display by default. When not set, the Web Awesome default applies.
    /// </summary>
    [Parameter] public int? Rows { get; set; }

    /// <summary>
    /// The textarea's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Controls how the textarea can be resized.
    /// </summary>
    [Parameter] public WaResize? Resize { get; set; }

    /// <summary>
    /// Enables spell checking on the textarea.
    /// </summary>
    [Parameter] public bool? Spellcheck { get; set; }

    /// <summary>
    /// Controls whether and how text input is automatically capitalized as it is entered by the user.
    /// </summary>
    [Parameter] public WaAutoCapitalize? AutoCapitalize { get; set; }

    /// <summary>
    /// Turns the browser's autocorrect feature on (true, rendered "on") or off (false, rendered "off"); null leaves
    /// the element's default, which is off.
    /// </summary>
    [Parameter] public bool? AutoCorrect { get; set; }

    /// <summary>
    /// Indicates that the textarea should receive focus on page load.
    /// </summary>
    [Parameter] public bool AutoFocus { get; set; }

    /// <summary>
    /// Used to customize the label or icon of the Enter key on virtual keyboards.
    /// </summary>
    [Parameter] public WaEnterKeyHint? EnterKeyHint { get; set; }

    /// <summary>
    /// Tells the browser what type of data will be entered by the user, allowing it to display the appropriate
    /// virtual keyboard on supportive devices.
    /// </summary>
    [Parameter] public WaInputMode? InputMode { get; set; }

    /// <summary>
    /// Shows a character count below the textarea. When <see cref="MaxLength"/> is set, shows
    /// the remaining characters instead.
    /// </summary>
    [Parameter] public bool WithCount { get; set; }

    /// <summary>
    /// Binds the value on every keystroke (the "input" event) instead of only when the change is committed (the
    /// "change" event, e.g. on blur), so the model is current while the user is typing - for example when a key
    /// handler such as Ctrl+Enter reads it. Named after the equivalent MudBlazor and Fluent UI Blazor parameter.
    /// <see cref="WaInputBase{TValue}.OnInput"/> is still invoked, after the value has been updated.
    /// </summary>
    [Parameter] public bool Immediate { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-textarea");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNull(9, "minlength", MinLength);
        builder.AddAttributeIfNotNull(10, "maxlength", MaxLength);
        builder.AddAttributeIfNotNullOrEmpty(11, "autocomplete", Autocomplete);
        AddLabelAndHintAttributes(builder, 12);

        // Add textarea-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(20, "placeholder", Placeholder);
        builder.AddAttributeIfNotNull(21, "rows", Rows);
        builder.AddAttributeIfNotNull(22, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttributeIfNotNull(23, "resize", Resize?.ToHtmlValue());
        builder.AddTrueFalseAttribute(24, "spellcheck", Spellcheck);
        builder.AddAttributeIfNotNull(30, "autocapitalize", AutoCapitalize?.ToHtmlValue());
        builder.AddOnOffAttribute(31, "autocorrect", AutoCorrect);
        builder.AddAttribute(32, "autofocus", AutoFocus);
        builder.AddAttributeIfNotNull(33, "enterkeyhint", EnterKeyHint?.ToHtmlValue());
        builder.AddAttributeIfNotNull(34, "inputmode", InputMode?.ToHtmlValue());
        AddWithHintAndLabelAttributes(builder, 14);
        builder.AddAttribute(37, "with-count", WithCount);

        // Add value binding
        builder.AddAttribute(25, "value", CurrentValueAsString);
        builder.AddAttribute(26, "onchange", EventCallback.Factory.CreateBinder<string?>(this, SetCurrentValueAsStringFromElement, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers; with immediate binding, the value binder and OnInput share one oninput handler
        AddCommonEventHandlers(builder, 40, includeInputHandler: !Immediate);
        if (Immediate)
            builder.AddAttribute(46, "oninput", CreateImmediateInputHandler());

        // Add textarea-specific event handlers
        builder.AddAttributeIfHasDelegate(47, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(50, __textAreaReference => Element = __textAreaReference);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 60);

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
    /// Removes focus from the textarea.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task BlurAsync()
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot blur: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Sets focus on the textarea.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task FocusAsync()
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot focus: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Selects all the text in the textarea.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task SelectAsync()
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot select: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "select");
    }

    /// <summary>
    /// The textarea's scroll position. Pass null for <paramref name="top"/> and
    /// <paramref name="left"/> to get the current scroll position without changing it.
    /// </summary>
    /// <param name="top">The vertical scroll position to set, in pixels</param>
    /// <param name="left">The horizontal scroll position to set, in pixels</param>
    /// <returns>The resulting scroll position</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task<WaTextAreaScrollPosition?> ScrollPositionAsync(double? top = null, double? left = null)
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot get or set scroll position: component has not been rendered yet.");

        if (top.HasValue || left.HasValue)
        {
            return await JSInterop.InvokeMethodAsync<WaTextAreaScrollPosition?>(Element.Value, "scrollPosition", new { top, left });
        }

        return await JSInterop.InvokeMethodAsync<WaTextAreaScrollPosition?>(Element.Value, "scrollPosition");
    }

    /// <summary>
    /// Replaces a range of text with a new string.
    /// </summary>
    /// <param name="replacement">The replacement text</param>
    /// <param name="start">The zero-based index of the start of the range to replace</param>
    /// <param name="end">The zero-based index of the end of the range to replace</param>
    /// <param name="selectMode">How the selection should be set after the replacement</param>
    /// <remarks>
    /// The element dispatches no input or change event for this change, so the resulting value is read back and
    /// assigned to the bound value (which also notifies the edit context).
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task SetRangeTextAsync(string replacement, int? start = null, int? end = null, WaTextAreaSelectMode selectMode = WaTextAreaSelectMode.Preserve)
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot set range text: component has not been rendered yet.");

        // the DOM setRangeText overloads accept either one argument or all four; null start/end would coerce to 0 in JS
        if (start.HasValue && end.HasValue)
            await JSInterop.InvokeMethodAsync(Element.Value, "setRangeText", replacement, start.Value, end.Value, selectMode.ToHtmlValue());
        else
            await JSInterop.InvokeMethodAsync(Element.Value, "setRangeText", replacement);

        SetCurrentValueAsStringFromElement(await JSInterop.GetPropertyAsync<string?>(Element.Value, "value"));
    }

    /// <summary>
    /// Sets the start and end positions of the text selection (0-based).
    /// </summary>
    /// <param name="selectionStart">The zero-based index of the start of the selection</param>
    /// <param name="selectionEnd">The zero-based index of the end of the selection</param>
    /// <param name="selectionDirection">The direction in which the selection is considered to have been performed</param>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task SetSelectionRangeAsync(int selectionStart, int selectionEnd, WaTextAreaSelectionDirection selectionDirection = WaTextAreaSelectionDirection.None)
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot set selection range: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "setSelectionRange", selectionStart, selectionEnd, selectionDirection.ToHtmlValue());
    }

    #endregion
}

/// <summary>
/// Represents the scroll position of a <see cref="WaTextArea"/>, as returned by <see cref="WaTextArea.ScrollPositionAsync"/>.
/// </summary>
/// <param name="Top">The vertical scroll position, in pixels</param>
/// <param name="Left">The horizontal scroll position, in pixels</param>
public record WaTextAreaScrollPosition(double Top, double Left);

/// <summary>
/// The selection behavior to apply after replacing a range of text via <see cref="WaTextArea.SetRangeTextAsync"/>.
/// </summary>
public enum WaTextAreaSelectMode
{
    /// <summary>Selects the newly inserted text.</summary>
    Select,
    /// <summary>Collapses the selection to the start of the newly inserted text.</summary>
    Start,
    /// <summary>Collapses the selection to the end of the newly inserted text.</summary>
    End,
    /// <summary>Attempts to preserve the selection.</summary>
    Preserve
}

/// <summary>
/// The direction of a text selection set via <see cref="WaTextArea.SetSelectionRangeAsync"/>.
/// </summary>
public enum WaTextAreaSelectionDirection
{
    /// <summary>The selection direction is unknown or irrelevant.</summary>
    None,
    /// <summary>The selection was performed in the start-to-end direction.</summary>
    Forward,
    /// <summary>The selection was performed in the end-to-start direction.</summary>
    Backward
}

/// <summary>
/// Extension methods for <see cref="WaTextAreaSelectMode"/> and <see cref="WaTextAreaSelectionDirection"/>.
/// </summary>
public static class WaTextAreaEnumExtensions
{
    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="selectMode">The select mode value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "preserve"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="selectMode"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaTextAreaSelectMode selectMode)
    {
        return selectMode switch
        {
            WaTextAreaSelectMode.Select => "select",
            WaTextAreaSelectMode.Start => "start",
            WaTextAreaSelectMode.End => "end",
            WaTextAreaSelectMode.Preserve => "preserve",
            _ => throw new ArgumentOutOfRangeException(nameof(selectMode), selectMode, null)
        };
    }

    /// <summary>
    /// Converts the value to its Web Awesome attribute string.
    /// </summary>
    /// <param name="selectionDirection">The selection direction value to convert</param>
    /// <returns>The lowercase attribute string, e.g. "none"</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="selectionDirection"/> is not a defined enum value</exception>
    public static string ToHtmlValue(this WaTextAreaSelectionDirection selectionDirection)
    {
        return selectionDirection switch
        {
            WaTextAreaSelectionDirection.None => "none",
            WaTextAreaSelectionDirection.Forward => "forward",
            WaTextAreaSelectionDirection.Backward => "backward",
            _ => throw new ArgumentOutOfRangeException(nameof(selectionDirection), selectionDirection, null)
        };
    }
}
