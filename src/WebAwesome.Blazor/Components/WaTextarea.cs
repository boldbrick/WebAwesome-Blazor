using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A multiline input component for editing <see cref="string"/> values.
/// </summary>
public class WaTextArea : InputBase<string?>, IFormValidation
{
    #region ------ Dependency Injection ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    // ----- Common parameters -----
    /// <summary>
    /// Additional CSS class names applied to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // --- Visual & behavior props ---
    /// <summary>
    /// Placeholder text to show as a hint when the input is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The number of rows to display by default.
    /// </summary>
    [Parameter] public int Rows { get; set; }

    /// <summary>
    /// The textarea's size.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// The textarea's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// Controls how the textarea can be resized.
    /// </summary>
    [Parameter] public WaResize? Resize { get; set; }

    /// <summary>
    /// Disables the textarea.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Makes the textarea readonly.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// Makes the textarea a required field.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    // Validation
    /// <summary>
    /// The minimum length of input that will be considered valid.
    /// </summary>
    [Parameter] public int? MinLength { get; set; }

    /// <summary>
    /// The maximum length of input that will be considered valid.
    /// </summary>
    [Parameter] public int? MaxLength { get; set; }

    // Browser behavior
    /// <summary>
    /// Value of the browser's "autocomplete" attribute controlling autofill behavior.
    /// </summary>
    [Parameter] public string? Autocomplete { get; set; }

    /// <summary>
    /// Enables spell checking on the textarea.
    /// </summary>
    [Parameter] public bool? Spellcheck { get; set; }

    /// <summary>
    /// Controls whether and how text input is automatically capitalized as it is entered by the user.
    /// </summary>
    [Parameter] public string? AutoCapitalize { get; set; }

    /// <summary>
    /// Indicates whether the browser's autocorrect feature is on or off.
    /// </summary>
    [Parameter] public string? AutoCorrect { get; set; }

    /// <summary>
    /// Indicates that the textarea should receive focus on page load.
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
    /// Used for SSR. If you're slotting in a hint element via <see cref="MarkupHint"/>, make sure to set this
    /// to true.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Used for SSR. If you're slotting in a label element via <see cref="MarkupLabel"/>, make sure to set this
    /// to true.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    /// <summary>
    /// Shows a character count below the textarea. When <see cref="MaxLength"/> is set, shows the remaining
    /// characters instead.
    /// </summary>
    [Parameter] public bool WithCount { get; set; }

    // Labels & hint (string or RenderFragment via MarkupX)
    /// <summary>
    /// Plain-text label rendered via the element's "label" attribute.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Rich markup label rendered into the "label" slot; takes precedence over <see cref="Label"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupLabel { get; set; }

    /// <summary>
    /// Plain-text hint rendered via the element's "hint" attribute.
    /// </summary>
    [Parameter] public string? Hint { get; set; }

    /// <summary>
    /// Rich markup hint rendered into the "hint" slot; takes precedence over <see cref="Hint"/> when set.
    /// </summary>
    [Parameter] public RenderFragment? MarkupHint { get; set; }

    #region ------ Events ------

    /// <summary>
    /// Emitted when the control loses focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Emitted when the control gains focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Emitted when the control receives input.
    /// </summary>
    [Parameter] public EventCallback<ChangeEventArgs> OnInput { get; set; }

    /// <summary>
    /// Emitted when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-textarea");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "name", NameAttributeValue);
        builder.AddAttributeIfNotNullOrEmpty(3, "class", String.Join(' ', Class, CssClass));
        builder.AddAttributeIfNotNullOrEmpty(4, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(5, "placeholder", Placeholder);
        builder.AddAttributeIfNotNull(6, "rows", Rows);
        builder.AddAttributeIfNotNull(7, "size", Size?.ToString().ToLowerInvariant());
        builder.AddAttributeIfNotNull(8, "appearance", Appearance?.ToString().ToLowerInvariant());
        builder.AddAttributeIfNotNull(9, "resize", Resize?.ToString().ToLowerInvariant());
        builder.AddAttribute(10, "disabled", Disabled);
        builder.AddAttribute(11, "readonly", Readonly);
        builder.AddAttribute(12, "required", Required);
        builder.AddAttributeIfNotNull(13, "minlength", MinLength);
        builder.AddAttributeIfNotNull(14, "maxlength", MaxLength);
        builder.AddAttributeIfNotNullOrEmpty(15, "autocomplete", Autocomplete);
        builder.AddAttributeIfNotNull(16, "spellcheck", Spellcheck);
        builder.AddAttributeIfNotNullOrEmpty(17, "label", Label);
        builder.AddAttributeIfNotNullOrEmpty(18, "hint", Hint);
        builder.AddAttribute(19, "value", CurrentValueAsString);
        builder.AddAttribute(20, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Additional attributes
        builder.AddAttributeIfNotNullOrEmpty(30, "autocapitalize", AutoCapitalize);
        builder.AddAttributeIfNotNullOrEmpty(31, "autocorrect", AutoCorrect);
        builder.AddAttribute(32, "autofocus", AutoFocus);
        builder.AddAttributeIfNotNullOrEmpty(33, "enterkeyhint", EnterKeyHint);
        builder.AddAttributeIfNotNullOrEmpty(34, "inputmode", InputMode);
        builder.AddAttribute(35, "with-hint", WithHint);
        builder.AddAttribute(36, "with-label", WithLabel);
        builder.AddAttribute(37, "with-count", WithCount);

        // Event handlers
        builder.AddAttributeIfHasDelegate(40, "onblur", OnBlur);
        builder.AddAttributeIfHasDelegate(41, "onfocus", OnFocus);
        builder.AddAttributeIfHasDelegate(42, "oninput", OnInput);
        builder.AddAttributeIfHasDelegate(43, "onwa-invalid", OnInvalid);

        builder.AddElementReferenceCapture(50, __inputReference => Element = __inputReference);
        if (MarkupLabel is not null)
        {
            builder.OpenElement(51, "span");
            builder.AddAttribute(52, "slot", "label");
            builder.AddContent(53, MarkupLabel);
            builder.CloseElement();
        }
        if (MarkupHint is not null)
        {
            builder.OpenElement(54, "span");
            builder.AddAttribute(55, "slot", "hint");
            builder.AddContent(56, MarkupHint);
            builder.CloseElement();
        }
        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

    #region ------ Implementation of IFormValidation ------

    /// <inheritdoc />
    public async Task SetCustomValidityAsync(string message)
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot set custom validity before the component is rendered. Element reference is null.");

        await JSInterop.SetCustomValidityAsync(Element.Value, message);
    }

    /// <inheritdoc />
    public async Task ResetValidityAsync()
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot reset validity before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "resetValidity");
    }

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
