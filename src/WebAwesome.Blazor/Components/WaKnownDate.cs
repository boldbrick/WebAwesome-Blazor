using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental form control for entering a known calendar date as separate day, month, and year fields
/// (e.g. a birthday). Corresponds to the wa-known-date Web Awesome component.
/// </summary>
public class WaKnownDate : WaInputBase<string?>
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

    #region ------ Visual &amp; Behavior Properties ------

    /// <summary>
    /// The known date's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// BCP-47 locale override. When empty, the inherited <c>lang</c> attribute is used.
    /// </summary>
    [Parameter] public string? Locale { get; set; }

    /// <summary>
    /// Earliest selectable date as <c>YYYY-MM-DD</c>.
    /// </summary>
    [Parameter] public string? Min { get; set; }

    /// <summary>
    /// Latest selectable date as <c>YYYY-MM-DD</c>.
    /// </summary>
    [Parameter] public string? Max { get; set; }

    /// <summary>
    /// Draws pill-style fields with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Only required for SSR. Set to true if you're slotting in a hint element.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Only required for SSR. Set to true if you're slotting in a label element.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

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
        builder.OpenElement(0, "wa-known-date");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNullOrEmpty(11, "autocomplete", Autocomplete);
        builder.AddAttributeIfNotNullOrEmpty(12, "label", Label);
        builder.AddAttributeIfNotNullOrEmpty(13, "hint", Hint);

        // Add known-date-specific attributes
        builder.AddAttributeIfNotNull(20, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(21, "locale", Locale);
        builder.AddAttributeIfNotNullOrEmpty(22, "min", Min);
        builder.AddAttributeIfNotNullOrEmpty(23, "max", Max);
        builder.AddAttribute(24, "pill", Pill);
        builder.AddAttribute(25, "with-hint", WithHint);
        builder.AddAttribute(26, "with-label", WithLabel);

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
        AddLabelAndHintSlots(builder, 60, MarkupLabel, MarkupHint);

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
