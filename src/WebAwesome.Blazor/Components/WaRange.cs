using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A range/slider input component for selecting numeric values within a specified range.
/// Corresponds to the wa-slider Web Awesome component.
/// </summary>
public class WaRange : WaLabeledInputBase<decimal>
{
    #region ------ Form Control Properties ------

    /// <summary>
    /// Makes the input read-only, allowing its value to be seen but not edited.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    #endregion

    #region ------ Range Properties ------

    /// <summary>
    /// The minimum value allowed.
    /// </summary>
    [Parameter] public decimal Min { get; set; } = 0;

    /// <summary>
    /// The maximum value allowed.
    /// </summary>
    [Parameter] public decimal Max { get; set; } = 100;

    /// <summary>
    /// The granularity the value must adhere to when incrementing and decrementing.
    /// </summary>
    [Parameter] public decimal Step { get; set; } = 1;

    /// <summary>
    /// The orientation of the slider.
    /// </summary>
    [Parameter] public WaOrientation? Orientation { get; set; }

    /// <summary>
    /// Draws a tooltip above the thumb when the control has focus or is dragged.
    /// </summary>
    [Parameter] public bool WithTooltip { get; set; }

    /// <summary>
    /// Draws markers at each step along the slider.
    /// </summary>
    [Parameter] public bool WithMarkers { get; set; }

    /// <summary>
    /// The placement of the tooltip in reference to the slider's thumb.
    /// </summary>
    [Parameter] public WaTooltipSide? TooltipPlacement { get; set; }

    /// <summary>
    /// The distance in pixels from which to offset the tooltip from the slider's thumb.
    /// </summary>
    [Parameter] public int? TooltipDistance { get; set; }

    /// <summary>
    /// The starting value from which to draw the slider's fill, which is based on its current value.
    /// </summary>
    [Parameter] public decimal? IndicatorOffset { get; set; }

    /// <summary>
    /// Automatically focuses the slider when the page loads.
    /// </summary>
    [Parameter] public bool AutoFocus { get; set; }

    // Range selection (dual thumb)
    /// <summary>
    /// Converts the slider to a range slider with two thumbs.
    /// </summary>
    [Parameter] public bool Range { get; set; }

    /// <summary>
    /// The minimum value of a range selection. Used only when <see cref="Range"/> is set.
    /// </summary>
    [Parameter] public decimal? MinValue { get; set; }

    /// <summary>
    /// The maximum value of a range selection. Used only when <see cref="Range"/> is set.
    /// </summary>
    [Parameter] public decimal? MaxValue { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the user changes <see cref="MinValue"/> in range selection mode, after the change event, with
    /// the new minimum.
    /// </summary>
    [Parameter] public EventCallback<decimal> OnMinValueChange { get; set; }

    /// <summary>
    /// Invoked when the user changes <see cref="MaxValue"/> in range selection mode, after the change event, with
    /// the new maximum.
    /// </summary>
    [Parameter] public EventCallback<decimal> OnMaxValueChange { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints are not satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Content Slots ------

    /// <summary>
    /// One or more reference labels shown below the slider (e.g. one <c>&lt;span&gt;</c> per label), rendered into the
    /// element's "reference" slot; the labels are spread evenly along the track.
    /// </summary>
    [Parameter] public RenderFragment? ReferenceContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-slider");

        // Add common attributes from base
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        AddLabelAndHintAttributes(builder, 12);

        // Add slider-specific attributes
        builder.AddNumberAttribute(20, "min", Min);
        builder.AddNumberAttribute(21, "max", Max);
        builder.AddNumberAttribute(22, "step", Step);
        builder.AddAttributeIfNotNull(23, "orientation", Orientation?.ToHtmlValue());
        builder.AddAttribute(24, "with-tooltip", WithTooltip);
        builder.AddAttribute(25, "with-markers", WithMarkers);
        builder.AddAttributeIfNotNull(26, "tooltip-placement", TooltipPlacement?.ToHtmlValue());
        builder.AddAttributeIfNotNull(27, "indicator-offset", IndicatorOffset);
        builder.AddAttributeIfNotNull(28, "tooltip-distance", TooltipDistance);
        builder.AddAttribute(29, "autofocus", AutoFocus);

        // Range selection attributes
        builder.AddAttribute(30, "range", Range);
        if (Range)
        {
            builder.AddAttributeIfNotNull(31, "min-value", MinValue);
            builder.AddAttributeIfNotNull(32, "max-value", MaxValue);
        }
        else
        {
            builder.AddNumberAttribute(33, "value", CurrentValue);
        }

        // SSR hints for slotted label and hint content
        AddWithHintAndLabelAttributes(builder, 14);

        // Add value binding; the element's live value is a JS number, which Blazor's built-in change reader cannot
        // carry, so the handlers listen to the "numericchange" alias of the change event that delivers it as an
        // invariant-culture string ("min,max" in range mode)
        if (!Range)
        {
            builder.AddAttribute(40, Constants.NumericChangeEventAttribute, EventCallback.Factory.Create<ChangeEventArgs>(this, HandleValueChange));
            builder.SetUpdatesAttributeName("value");
        }
        else
        {
            builder.AddAttribute(41, Constants.NumericChangeEventAttribute, EventCallback.Factory.Create<ChangeEventArgs>(this, HandleRangeValueChangeAsync));
        }

        // Add common event handlers; the input event carries the same JS number, so OnInput is bound to its alias
        AddCommonEventHandlers(builder, 50, includeInputHandler: false);
        AddNumericInputHandler(builder, 56);

        // Add slider-specific event handlers
        builder.AddAttributeIfHasDelegate(57, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(60, __sliderReference => Element = __sliderReference);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70);

        // Add reference labels; the wrapper takes no box of its own, so each label is an item of the slot's flex row
        if (ReferenceContent is not null)
        {
            builder.OpenElement(80, "span");
            builder.AddAttribute(81, "slot", "reference");
            builder.AddAttribute(82, "style", Constants.TransparentSlotWrapperStyle);
            builder.AddContent(83, ReferenceContent);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Formats with the invariant culture, the form <see cref="TryParseValueFromString"/> reads back (the base class would
    /// use the current culture, e.g. "2,5").
    /// </remarks>
    protected override string? FormatValueAsString(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out decimal result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (ChangeEventArgsExtensions.TryParseJsNumber(value, out result))
        {
            validationErrorMessage = null;
            return true;
        }

        result = default;
        validationErrorMessage = $"The {DisplayName ?? FieldIdentifier.FieldName} field must be a number.";
        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Range mode needs no sync: min-value and max-value map to the live minValue/maxValue properties.
    /// </remarks>
    protected override string? LiveValuePropertyName => Range ? null : "value";

    /// <inheritdoc />
    protected override object? GetLiveValue() => (double)CurrentValue;

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets a custom value formatter function for tooltips and screen readers.
    /// </summary>
    /// <param name="jsFunction">JavaScript function string that formats values for display</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered or the operation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when jsFunction is null or empty</exception>
    public async Task SetValueFormatterAsync(string jsFunction)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set value formatter: component has not been rendered yet.");

        if (string.IsNullOrEmpty(jsFunction))
            throw new ArgumentNullException(nameof(jsFunction));

        await JSInterop.SetPropertyAsync(Element.Value, "valueFormatter", jsFunction);
    }

    #endregion

    #region ------ Internals ------

    // handles the change event, whose value the numericchange alias delivers as a JS-formatted number; records the
    // element's live value (a JS number) before assigning the model, and leaves the model unchanged when the value
    // cannot be parsed
    private void HandleValueChange(ChangeEventArgs args)
    {
        if (!ChangeEventArgsExtensions.TryParseJsNumber(args.GetStringValue(), out var value)) return;

        MarkLiveValueSynced((double)value);
        CurrentValue = value;
    }

    // handles the range-mode change event, whose value the numericchange alias delivers as "<minValue>,<maxValue>"
    // (JS-formatted numbers); updates MinValue/MaxValue and reports each bound that changed
    private async Task HandleRangeValueChangeAsync(ChangeEventArgs args)
    {
        var parts = args.GetStringValue()?.Split(RangeValueSeparator);
        if (parts is not { Length: 2 }) return;

        if (ChangeEventArgsExtensions.TryParseJsNumber(parts[0], out var minValue) && minValue != MinValue)
        {
            MinValue = minValue;
            await OnMinValueChange.InvokeAsync(minValue);
        }

        if (ChangeEventArgsExtensions.TryParseJsNumber(parts[1], out var maxValue) && maxValue != MaxValue)
        {
            MaxValue = maxValue;
            await OnMaxValueChange.InvokeAsync(maxValue);
        }
    }

    // separates the min and max value in the range-mode change payload built by the JS initializer
    private const char RangeValueSeparator = ',';

    #endregion
}
