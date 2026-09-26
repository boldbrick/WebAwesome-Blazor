using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental form control for entering a known calendar date as separate day, month, and year fields
/// (e.g. a birthday), bound to a date. Corresponds to the wa-known-date Web Awesome component.
/// </summary>
/// <remarks>
/// The value travels as ISO <c>yyyy-MM-dd</c>, culture-free. While a field is only partly filled the element reports
/// an empty value, which binds as null; a value that is not an ISO date adds the validation message
/// "The {field} field must be a date." to the edit context.
/// </remarks>
public class WaKnownDate : WaLabeledInputBase<DateOnly?>
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
    /// The known date's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Locale"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLocale = "";

    /// <summary>
    /// BCP-47 locale override. When empty, the inherited <c>lang</c> attribute is used.
    /// </summary>
    [Parameter] public string? Locale { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Min"/>: no bound, which the element holds as the empty attribute; it is rendered in
    /// place of null once the attribute has been rendered.
    /// </summary>
    public static readonly DateOnly? DefaultMin = null;

    /// <summary>
    /// The earliest valid date; null sets no bound.
    /// </summary>
    [Parameter] public DateOnly? Min { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Max"/>: no bound, which the element holds as the empty attribute; it is rendered in
    /// place of null once the attribute has been rendered.
    /// </summary>
    public static readonly DateOnly? DefaultMax = null;

    /// <summary>
    /// The latest valid date; null sets no bound.
    /// </summary>
    [Parameter] public DateOnly? Max { get; set; }

    /// <summary>
    /// Draws pill-style fields with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-known-date");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 11, "autocomplete", Autocomplete, DefaultAutocomplete);
        AddLabelAndHintAttributes(builder, 12);

        // Add known-date-specific attributes
        builder.AddAttributeIfNotNull(attributes, 20, "appearance", Appearance?.ToHtmlValue(), DefaultAppearance.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(attributes, 21, "locale", Locale, DefaultLocale);
        builder.AddDateAttribute(attributes, 22, "min", Min, DefaultMin);
        builder.AddDateAttribute(attributes, 23, "max", Max, DefaultMax);
        builder.AddAttribute(24, "pill", Pill);
        AddWithHintAndLabelAttributes(builder, 14);

        // Add value binding
        builder.AddAttribute(30, "value", CurrentValueAsString);
        builder.AddAttribute(31, "onchange", EventCallback.Factory.CreateBinder<string?>(this, SetCurrentValueAsStringFromElement, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add known-date-specific event handlers
        builder.AddAttributeIfHasDelegate(50, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(51, __knownDateReference => Element = __knownDateReference);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 60);

        builder.CloseElement();
    }

    /// <summary>
    /// Formats the value as ISO <c>yyyy-MM-dd</c>, culture-free, the form the element reads; null renders no value.
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The wire string</returns>
    protected override string? FormatValueAsString(DateOnly? value)
        => value.HasValue ? WaWireFormat.FormatDate(value.Value) : null;

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out DateOnly? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = null;
            validationErrorMessage = null;
            return true;
        }

        if (WaWireFormat.TryParseDate(value, out var date))
        {
            result = date;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = FormatValidationMessage(Constants.DateValidationMessageFormat);
        return false;
    }

    /// <inheritdoc />
    protected override string? LiveValuePropertyName => "value";

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Focuses the first empty field, or the first field when all are filled.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the known date before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Removes focus from the known date.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur the known date before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    #endregion
}
