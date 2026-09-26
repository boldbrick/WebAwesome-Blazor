using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A single-line input component for editing <see cref="string"/> values.
/// Corresponds to the wa-input Web Awesome component.
/// </summary>
public class WaInput : WaLabeledInputBase<string?>, IWaClearableControl, IWaAffixedControl
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
    /// The Web Awesome default of <see cref="Placeholder"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultPlaceholder = "";

    /// <summary>
    /// Placeholder text to show as a hint when the input is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Type"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const WaInputType DefaultType = WaInputType.Text;

    /// <summary>
    /// The type of input. Works the same as a native <c>&lt;input&gt;</c> element, but only a subset of types
    /// are supported.
    /// </summary>
    [Parameter] public WaInputType Type { get; set; } = DefaultType;

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
    /// The Web Awesome default of <see cref="Spellcheck"/> (on): what the element holds while the parameter is null, and
    /// what is rendered in its place once the attribute has been rendered (a removed attribute would read as off).
    /// </summary>
    public const bool DefaultSpellcheck = true;

    /// <summary>
    /// Enables spell checking on the input.
    /// </summary>
    [Parameter] public bool? Spellcheck { get; set; }

    // Input-specific validation
    /// <summary>
    /// A regular expression pattern to validate input against.
    /// </summary>
    [Parameter] public string? Pattern { get; set; }

#if NET11_0_OR_GREATER
#error Use a union type here
#else
    // The min and max of wa-input are typed number | string (a number for a number input, an ISO date, time or local
    // date-time for the date and time types), which C# before 15 cannot express as one parameter type. Until then each
    // bound is a raw string plus one strongly typed accessor per value type; at most one of them may be set per bound.
    // On net11.0 this becomes a C# 15 union type, a deliberate breaking change (docs\technical.md, Multi-targeting).

    /// <summary>
    /// The input's minimum value as the raw attribute text, for a format none of the typed accessors covers. Only
    /// applies to date, time and number input types. Set at most one of <see cref="Min"/>, <see cref="MinDecimal"/>,
    /// <see cref="MinLong"/>, <see cref="MinULong"/>, <see cref="MinDate"/>, <see cref="MinTime"/> and
    /// <see cref="MinDateTime"/>; two throw <see cref="InvalidOperationException"/>.
    /// </summary>
    [Parameter] public string? Min { get; set; }

    /// <summary>
    /// The minimum of a number input, rendered in the invariant culture (<c>2.5</c>). See <see cref="Min"/>.
    /// </summary>
    [Parameter] public decimal? MinDecimal { get; set; }

    /// <summary>
    /// The minimum of a number input as an integer. See <see cref="Min"/>.
    /// </summary>
    [Parameter] public long? MinLong { get; set; }

    /// <summary>
    /// The minimum of a number input as an unsigned integer. See <see cref="Min"/>.
    /// </summary>
    [Parameter] public ulong? MinULong { get; set; }

    /// <summary>
    /// The earliest date of a <see cref="WaInputType.Date"/> input, rendered ISO <c>yyyy-MM-dd</c>. See <see cref="Min"/>.
    /// </summary>
    [Parameter] public DateOnly? MinDate { get; set; }

    /// <summary>
    /// The earliest time of a <see cref="WaInputType.Time"/> input, rendered 24-hour <c>HH:mm</c>, or <c>HH:mm:ss</c> when
    /// it has seconds. See <see cref="Min"/>.
    /// </summary>
    [Parameter] public TimeOnly? MinTime { get; set; }

    /// <summary>
    /// The earliest date and time of a <see cref="WaInputType.DateTimeLocal"/> input, rendered as the local date and time
    /// the native input reads (<c>yyyy-MM-ddTHH:mm</c>, with seconds and milliseconds when not zero; the kind is ignored).
    /// See <see cref="Min"/>.
    /// </summary>
    [Parameter] public DateTime? MinDateTime { get; set; }

    /// <summary>
    /// The input's maximum value as the raw attribute text, for a format none of the typed accessors covers. Only
    /// applies to date, time and number input types. Set at most one of <see cref="Max"/>, <see cref="MaxDecimal"/>,
    /// <see cref="MaxLong"/>, <see cref="MaxULong"/>, <see cref="MaxDate"/>, <see cref="MaxTime"/> and
    /// <see cref="MaxDateTime"/>; two throw <see cref="InvalidOperationException"/>.
    /// </summary>
    [Parameter] public string? Max { get; set; }

    /// <summary>
    /// The maximum of a number input, rendered in the invariant culture. See <see cref="Max"/>.
    /// </summary>
    [Parameter] public decimal? MaxDecimal { get; set; }

    /// <summary>
    /// The maximum of a number input as an integer. See <see cref="Max"/>.
    /// </summary>
    [Parameter] public long? MaxLong { get; set; }

    /// <summary>
    /// The maximum of a number input as an unsigned integer. See <see cref="Max"/>.
    /// </summary>
    [Parameter] public ulong? MaxULong { get; set; }

    /// <summary>
    /// The latest date of a <see cref="WaInputType.Date"/> input, rendered ISO <c>yyyy-MM-dd</c>. See <see cref="Max"/>.
    /// </summary>
    [Parameter] public DateOnly? MaxDate { get; set; }

    /// <summary>
    /// The latest time of a <see cref="WaInputType.Time"/> input, rendered 24-hour <c>HH:mm</c>, or <c>HH:mm:ss</c> when
    /// it has seconds. See <see cref="Max"/>.
    /// </summary>
    [Parameter] public TimeOnly? MaxTime { get; set; }

    /// <summary>
    /// The latest date and time of a <see cref="WaInputType.DateTimeLocal"/> input, rendered as the local date and time
    /// the native input reads. See <see cref="Max"/>.
    /// </summary>
    [Parameter] public DateTime? MaxDateTime { get; set; }
#endif

    /// <summary>
    /// Specifies the granularity that the value must adhere to, or <see cref="WaStep.Any"/>. Only applies to date and
    /// number input types. A number converts implicitly (<c>Step="0.5"</c>).
    /// </summary>
    [Parameter] public WaStep? Step { get; set; }

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
    /// Indicates that the input should receive focus on page load.
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
    /// Determines whether or not the password is currently visible. Only applies to password input types.
    /// </summary>
    /// <remarks>
    /// One-way: when the user activates the <see cref="PasswordToggle"/> button, the element flips its own
    /// visibility without dispatching an event or reflecting the attribute, so this parameter is not updated.
    /// </remarks>
    [Parameter] public bool PasswordVisible { get; set; }

    /// <summary>
    /// Hides the browser's built-in increment/decrement spin buttons for number inputs.
    /// </summary>
    [Parameter] public bool WithoutSpinButtons { get; set; }

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
    /// Invoked when the clear button is activated.
    /// </summary>
    [Parameter] public EventCallback OnClear { get; set; }

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

    /// <summary>
    /// An icon to use in lieu of the default clear icon (see <see cref="WithClear"/>), rendered into the element's "clear-icon" slot.
    /// </summary>
    [Parameter] public RenderFragment? ClearIconContent { get; set; }

    /// <summary>
    /// An icon to use in lieu of the default show password icon (see <see cref="PasswordToggle"/>), rendered into the element's "show-password-icon" slot.
    /// </summary>
    [Parameter] public RenderFragment? ShowPasswordIconContent { get; set; }

    /// <summary>
    /// An icon to use in lieu of the default hide password icon (see <see cref="PasswordToggle"/>), rendered into the element's "hide-password-icon" slot.
    /// </summary>
    [Parameter] public RenderFragment? HidePasswordIconContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-input");

        // Add common attributes
        var sequence = AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNull(9, "minlength", MinLength);
        builder.AddAttributeIfNotNull(10, "maxlength", MaxLength);
        builder.AddAttributeIfNotNullOrEmpty(11, "autocomplete", Autocomplete);
        AddLabelAndHintAttributes(builder, 12);

        // Add input-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(attributes, 20, "placeholder", Placeholder, DefaultPlaceholder);
        builder.AddDefaultedAttribute(attributes, 21, "type", Type.ToHtmlValue(), DefaultType.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 22, "appearance", Appearance?.ToHtmlValue(), DefaultAppearance.ToHtmlValue());
        builder.AddAttribute(23, "pill", Pill);
        FormControlRendering.AddWithClearAttribute(builder, 24, this);
        builder.AddAttribute(25, "password-toggle", PasswordToggle);
        builder.AddTrueFalseAttribute(attributes, 26, "spellcheck", Spellcheck, DefaultSpellcheck);
        builder.AddAttributeIfNotNullOrEmpty(27, "pattern", Pattern);
        builder.AddAttributeIfNotNullOrEmpty(28, "min", MinAttributeValue());
        builder.AddAttributeIfNotNullOrEmpty(29, "max", MaxAttributeValue());
        builder.AddStepAttribute(30, "step", Step);
        builder.AddAttributeIfNotNull(33, "autocapitalize", AutoCapitalize?.ToHtmlValue());
        builder.AddOnOffAttribute(34, "autocorrect", AutoCorrect);
        builder.AddAttribute(35, "autofocus", AutoFocus);
        builder.AddAttributeIfNotNull(36, "enterkeyhint", EnterKeyHint?.ToHtmlValue());
        builder.AddAttributeIfNotNull(37, "inputmode", InputMode?.ToHtmlValue());
        builder.AddAttribute(38, "password-visible", PasswordVisible);
        AddWithHintAndLabelAttributes(builder, 14);
        builder.AddAttribute(48, "without-spin-buttons", WithoutSpinButtons);

        // Add value binding
        builder.AddAttribute(31, "value", CurrentValueAsString);
        builder.AddAttribute(32, "onchange", EventCallback.Factory.CreateBinder<string?>(this, SetCurrentValueAsStringFromElement, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers; with immediate binding, the value binder and OnInput share one oninput handler
        AddCommonEventHandlers(builder, 40, includeInputHandler: !Immediate);
        if (Immediate)
            builder.AddAttribute(47, "oninput", CreateImmediateInputHandler());

        // Add input-specific event handlers
        FormControlRendering.AddClearEventHandler(builder, 50, this);
        builder.AddAttributeIfHasDelegate(49, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(53, __inputReference => Element = __inputReference);

        // Add start and end slot content (the fragment wins over the icon-name shortcut)
        FormControlRendering.AddAffixSlots(builder, 60, this, StartIconName, EndIconName);

        // Add clear-icon slot content
        FormControlRendering.AddClearIconSlot(builder, 110, this);

        // Add show-password-icon slot content
        if (ShowPasswordIconContent is not null)
        {
            builder.OpenElement(115, "span");
            builder.AddAttribute(116, "slot", "show-password-icon");
            builder.AddContent(117, ShowPasswordIconContent);
            builder.CloseElement();
        }

        // Add hide-password-icon slot content
        if (HidePasswordIconContent is not null)
        {
            builder.OpenElement(120, "span");
            builder.AddAttribute(121, "slot", "hide-password-icon");
            builder.AddContent(122, HidePasswordIconContent);
            builder.CloseElement();
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

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when more than one accessor of the same bound is set</exception>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        EnsureOneBound((nameof(Min), Min != null), (nameof(MinDecimal), MinDecimal.HasValue), (nameof(MinLong), MinLong.HasValue),
            (nameof(MinULong), MinULong.HasValue), (nameof(MinDate), MinDate.HasValue), (nameof(MinTime), MinTime.HasValue), (nameof(MinDateTime), MinDateTime.HasValue));
        EnsureOneBound((nameof(Max), Max != null), (nameof(MaxDecimal), MaxDecimal.HasValue), (nameof(MaxLong), MaxLong.HasValue),
            (nameof(MaxULong), MaxULong.HasValue), (nameof(MaxDate), MaxDate.HasValue), (nameof(MaxTime), MaxTime.HasValue), (nameof(MaxDateTime), MaxDateTime.HasValue));
    }
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
    /// Replaces a range of text with a new string.
    /// </summary>
    /// <param name="replacement">The replacement text</param>
    /// <param name="start">The zero-based index of the first character to replace</param>
    /// <param name="end">The zero-based index of the character after the last character to replace</param>
    /// <param name="selectMode">How the selection should be set after the text is replaced. One of
    /// <c>select</c>, <c>start</c>, <c>end</c>, or <c>preserve</c> (default)</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <remarks>
    /// The element dispatches no input or change event for this change, so the resulting value is read back and
    /// assigned to the bound value (which also notifies the edit context).
    /// </remarks>
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

        SetCurrentValueAsStringFromElement(await JSInterop.GetPropertyAsync<string?>(Element.Value, "value"));
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

    #region ------ Internals ------

    // the min attribute text of whichever accessor is set, in the invariant or ISO form the native input reads
    private string? MinAttributeValue() => BoundValue(Min, MinDecimal, MinLong, MinULong, MinDate, MinTime, MinDateTime);

    // the max attribute text of whichever accessor is set
    private string? MaxAttributeValue() => BoundValue(Max, MaxDecimal, MaxLong, MaxULong, MaxDate, MaxTime, MaxDateTime);

    private static string? BoundValue(string? raw, decimal? number, long? integer, ulong? unsigned, DateOnly? date, TimeOnly? time, DateTime? dateTime)
    {
        if (raw != null) return raw;
        if (number.HasValue) return number.Value.ToString(CultureInfo.InvariantCulture);
        if (integer.HasValue) return integer.Value.ToString(CultureInfo.InvariantCulture);
        if (unsigned.HasValue) return unsigned.Value.ToString(CultureInfo.InvariantCulture);
        if (date.HasValue) return WaWireFormat.FormatDate(date.Value);
        if (time.HasValue) return WaWireFormat.FormatTimeBound(time.Value);
        return dateTime.HasValue ? WaWireFormat.FormatLocalDateTime(dateTime.Value) : null;
    }

    // throws when more than one accessor of a bound is set
    private static void EnsureOneBound(params (string Name, bool IsSet)[] accessors)
    {
        var set = accessors.Where(a => a.IsSet).Select(a => a.Name).ToList();
        if (set.Count > 1)
            throw new InvalidOperationException($"WaInput: set at most one accessor of a bound, but {string.Join(" and ", set)} are all set.");
    }

    #endregion
}
