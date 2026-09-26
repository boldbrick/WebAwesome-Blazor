using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A range/slider input component for selecting numeric values within a specified range.
/// Corresponds to the wa-slider Web Awesome component.
/// </summary>
public class WaRange : WaSliderBase<decimal>
{
    #region ------ Events ------

    /// <summary>
    /// Invoked when the user changes <see cref="WaSliderBase{TValue}.MinValue"/> in range selection mode, after the
    /// change event, with the new minimum.
    /// </summary>
    [Parameter] public EventCallback<decimal> OnMinValueChange { get; set; }

    /// <summary>
    /// Invoked when the user changes <see cref="WaSliderBase{TValue}.MaxValue"/> in range selection mode, after the
    /// change event, with the new maximum.
    /// </summary>
    [Parameter] public EventCallback<decimal> OnMaxValueChange { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-slider");

        // Add common attributes from base
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        AddLabelAndHintAttributes(builder, 12);

        // Add slider-specific attributes
        builder.AddNumberAttribute(attributes, 20, "min", Min);
        builder.AddNumberAttribute(attributes, 21, "max", Max);
        builder.AddNumberAttribute(attributes, 22, "step", Step);
        builder.AddAttributeIfNotNull(attributes, 23, "orientation", Orientation?.ToHtmlValue());
        builder.AddAttribute(24, "with-tooltip", WithTooltip);
        builder.AddAttribute(25, "with-markers", WithMarkers);
        builder.AddAttributeIfNotNull(attributes, 26, "tooltip-placement", TooltipPlacement?.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 27, "indicator-offset", IndicatorOffset);
        builder.AddAttributeIfNotNull(attributes, 28, "tooltip-distance", TooltipDistance);
        builder.AddAttribute(29, "autofocus", AutoFocus);

        // Range selection attributes
        builder.AddAttribute(30, "range", Range);
        if (Range)
        {
            builder.AddAttributeIfNotNull(attributes, 31, "min-value", MinValue);
            builder.AddAttributeIfNotNull(attributes, 32, "max-value", MaxValue);
        }
        else
        {
            builder.AddNumberAttribute(attributes, 33, "value", CurrentValue);
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

    // handles the range-mode change event; updates MinValue/MaxValue and reports each bound that changed
    private async Task HandleRangeValueChangeAsync(ChangeEventArgs args)
    {
        var values = ParseRangeValues(args);
        if (values is null) return;

        var (minValue, maxValue) = values.Value;

        if (minValue.HasValue && minValue != MinValue)
        {
            MinValue = minValue;
            await OnMinValueChange.InvokeAsync(minValue.Value);
        }

        if (maxValue.HasValue && maxValue != MaxValue)
        {
            MaxValue = maxValue;
            await OnMaxValueChange.InvokeAsync(maxValue.Value);
        }
    }

    #endregion
}
