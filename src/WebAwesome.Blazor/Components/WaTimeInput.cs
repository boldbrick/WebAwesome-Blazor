using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental time picker with segmented text entry and a column-based popup, bound to a time of day.
/// Corresponds to the wa-time-input Web Awesome component.
/// </summary>
/// <remarks>
/// The value travels as 24-hour <c>HH:mm</c>, or <c>HH:mm:ss</c> when the <see cref="Step"/> shows seconds (below a
/// minute, not a whole number of minutes, or "any"), exactly as the element emits it; formatting and parsing are
/// culture-free, and fractions of a second are not sent. An empty element value binds as null; a value that is not a
/// wire time adds the validation message "The {field} field must be a time." to the edit context.
/// </remarks>
public class WaTimeInput : WaPopupInputBase<TimeOnly?>, IWaClearableControl, IWaAffixedControl
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
    /// The Web Awesome default of <see cref="Autocomplete"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultAutocomplete = "";

    /// <summary>
    /// Value of the browser's "autocomplete" attribute controlling autofill behavior.
    /// </summary>
    [Parameter] public string? Autocomplete { get; set; }

    #endregion

    #region ------ Visual &amp; Behavior Properties ------

    /// <summary>
    /// The Web Awesome default of <see cref="Appearance"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaInputAppearance DefaultAppearance = WaInputAppearance.Outlined;

    /// <summary>
    /// The time picker's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="HourFormat"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaTimeHourFormat DefaultHourFormat = WaTimeHourFormat.Auto;

    /// <summary>
    /// Whether the UI uses a 12-hour or 24-hour clock. <see cref="WaTimeHourFormat.Auto"/> follows the resolved locale.
    /// </summary>
    [Parameter] public WaTimeHourFormat? HourFormat { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Min"/>: no bound, which the element holds as the empty attribute; it is rendered in
    /// place of null once the attribute has been rendered.
    /// </summary>
    public static readonly TimeOnly? DefaultMin = null;

    /// <summary>
    /// The earliest selectable time; null sets no bound. May be later than <see cref="Max"/> to represent an overnight
    /// range. Rendered as <c>HH:mm</c>, or <c>HH:mm:ss</c> when it has seconds.
    /// </summary>
    [Parameter] public TimeOnly? Min { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Max"/>: no bound, which the element holds as the empty attribute; it is rendered in
    /// place of null once the attribute has been rendered.
    /// </summary>
    public static readonly TimeOnly? DefaultMax = null;

    /// <summary>
    /// The latest selectable time; null sets no bound. Rendered as <c>HH:mm</c>, or <c>HH:mm:ss</c> when it has seconds.
    /// </summary>
    [Parameter] public TimeOnly? Max { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Step"/> (60 seconds, minute precision): what the element holds while the
    /// parameter is null, and what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public static readonly WaStep DefaultStep = 60;

    /// <summary>
    /// The granularity, in seconds, matching HTML <c>&lt;input type="time"&gt;</c>. The default <c>60</c> hides the
    /// seconds segment; values below 60 (or not a whole number of minutes) reveal it; <see cref="WaStep.Any"/> reveals it
    /// and disables step-mismatch enforcement. A number converts implicitly (<c>Step="1"</c>).
    /// </summary>
    [Parameter] public WaStep? Step { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Placement"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaPickerPlacement DefaultPlacement = WaPickerPlacement.BottomStart;

    /// <summary>
    /// The preferred placement of the time picker popup, above or below the field. When null, the attribute is omitted and
    /// Web Awesome's default (bottom-start) applies.
    /// </summary>
    [Parameter] public WaPickerPlacement? Placement { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Distance"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const int DefaultDistance = 0;

    /// <summary>
    /// Distance in pixels between the popup and the input.
    /// </summary>
    [Parameter] public int? Distance { get; set; }

    /// <summary>
    /// Draws a pill-style time picker with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Shows a clear button when the time picker has a value.
    /// </summary>
    [Parameter] public bool WithClear { get; set; }

    /// <summary>
    /// Renders a "Now" button in the popup footer.
    /// </summary>
    [Parameter] public bool WithNow { get; set; }

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
    /// Content placed at the start of the input.
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content placed at the end of the input.
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// An icon to use in lieu of the default clear icon.
    /// </summary>
    [Parameter] public RenderFragment? ClearIconContent { get; set; }

    /// <summary>
    /// The icon to show on the popup toggle button. Defaults to a clock icon.
    /// </summary>
    [Parameter] public RenderFragment? ExpandIconContent { get; set; }

    /// <summary>
    /// Content shown below the column picker in the popup. Replaces the default Now button when present.
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-time-input");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 11, "autocomplete", Autocomplete, DefaultAutocomplete);
        AddLabelAndHintAttributes(builder, 12);

        // Add time-input-specific attributes
        builder.AddAttributeIfNotNull(attributes, 20, "appearance", Appearance?.ToHtmlValue(), DefaultAppearance.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 21, "hour-format", HourFormat?.ToHtmlValue(), DefaultHourFormat.ToHtmlValue());
        builder.AddTimeAttribute(attributes, 22, "min", Min, DefaultMin);
        builder.AddTimeAttribute(attributes, 23, "max", Max, DefaultMax);
        builder.AddStepAttribute(attributes, 24, "step", Step, DefaultStep);
        builder.AddAttributeIfNotNull(attributes, 25, "placement", Placement?.ToHtmlValue(), DefaultPlacement.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 26, "distance", Distance, DefaultDistance);
        builder.AddAttribute(27, "open", Open);
        builder.AddAttribute(28, "pill", Pill);
        FormControlRendering.AddWithClearAttribute(builder, 29, this);
        builder.AddAttribute(30, "with-now", WithNow);
        AddWithHintAndLabelAttributes(builder, 14);

        // Add value binding
        builder.AddAttribute(35, "value", CurrentValueAsString);
        builder.AddAttribute(36, "onchange", EventCallback.Factory.CreateBinder<string?>(this, SetCurrentValueAsStringFromElement, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add time-input-specific event handlers
        FormControlRendering.AddClearEventHandler(builder, 50, this);
        AddPopupEventHandlers(builder, 51);
        builder.AddAttributeIfHasDelegate(55, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(56, __timeInputReference => Element = __timeInputReference);

        // Add start and end slot content
        FormControlRendering.AddAffixSlots(builder, 60, this);

        // Add clear-icon slot content
        FormControlRendering.AddClearIconSlot(builder, 70, this);

        // Add expand-icon slot content
        if (ExpandIconContent is not null)
        {
            builder.OpenElement(75, "span");
            builder.AddAttribute(76, "slot", "expand-icon");
            builder.AddContent(77, ExpandIconContent);
            builder.CloseElement();
        }

        // Add footer slot content
        if (FooterContent is not null)
        {
            builder.OpenElement(80, "span");
            builder.AddAttribute(81, "slot", "footer");
            builder.AddContent(82, FooterContent);
            builder.CloseElement();
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 90);

        builder.CloseElement();
    }

    /// <summary>
    /// Formats the value as the element emits it: 24-hour <c>HH:mm</c>, or <c>HH:mm:ss</c> when the <see cref="Step"/>
    /// shows seconds; culture-free. Null renders no value.
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The wire string</returns>
    protected override string? FormatValueAsString(TimeOnly? value)
        => value.HasValue ? WaWireFormat.FormatTime(value.Value, Step?.ShowsTimeSeconds == true) : null;

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out TimeOnly? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = null;
            validationErrorMessage = null;
            return true;
        }

        if (WaWireFormat.TryParseTime(value, out var time))
        {
            result = time;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = FormatValidationMessage(Constants.TimeValidationMessageFormat);
        return false;
    }

    /// <inheritdoc />
    protected override string? LiveValuePropertyName => "value";

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets focus on the first empty (else first) segment.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the time picker before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Removes focus from the time picker.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur the time picker before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    #endregion
}
