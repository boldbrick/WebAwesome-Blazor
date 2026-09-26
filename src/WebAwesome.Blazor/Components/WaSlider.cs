using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A slider component that allows the user to select a single value or range within a given range.
/// Corresponds to the wa-slider Web Awesome component.
/// </summary>
/// <remarks>
/// In range mode (<see cref="WaSliderBase{TValue}.Range"/>) the selection is bound through
/// <see cref="WaSliderBase{TValue}.MinValue"/> and <see cref="WaSliderBase{TValue}.MaxValue"/>
/// (<c>@bind-MinValue</c>/<c>@bind-MaxValue</c>); <c>@bind-Value</c> is neither needed nor used, and
/// <see cref="OnValueChange"/> is not invoked. When no <c>ValueExpression</c> is supplied (no <c>@bind-Value</c>),
/// the slider falls back to an internal placeholder field, so no field of an enclosing <c>EditForm</c> model is
/// associated with it: the range values are not validated or tracked by the edit context. The slider must be in
/// range mode on its first render to use the fallback; single-value mode keeps requiring <c>@bind-Value</c> like any
/// other input.
/// </remarks>
public class WaSlider : WaSliderBase<decimal?>
{
    #region ------ Range Selection Mode ------

    /// <summary>
    /// Invoked with the new minimum value when the user commits a change in range selection mode (the element's
    /// change event); the <c>@bind-MinValue</c> counterpart of <see cref="WaSliderBase{TValue}.MinValue"/>.
    /// </summary>
    [Parameter] public EventCallback<decimal?> MinValueChanged { get; set; }

    /// <summary>
    /// Invoked with the new maximum value when the user commits a change in range selection mode (the element's
    /// change event); the <c>@bind-MaxValue</c> counterpart of <see cref="WaSliderBase{TValue}.MaxValue"/>.
    /// </summary>
    [Parameter] public EventCallback<decimal?> MaxValueChanged { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked with the new value when the user commits a change in single-value mode (the element's change
    /// event), after the bound value has been updated. Not invoked in range mode, see
    /// <see cref="MinValueChanged"/> and <see cref="MaxValueChanged"/>.
    /// </summary>
    [Parameter] public EventCallback<decimal?> OnValueChange { get; set; }

    #endregion

    #region ------ JavaScript Interop Properties ------

    /// <summary>
    /// Custom value formatting function for tooltips and screen readers.
    /// Note: This requires JavaScript interop to implement.
    /// </summary>
    /// <remarks>
    /// In the actual implementation, this would be a JavaScript function that formats
    /// the slider value for display in tooltips and for screen reader announcements.
    /// </remarks>
    public Func<decimal, string>? ValueFormatter { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    /// <remarks>
    /// In range mode without a supplied <c>ValueExpression</c>, points it at an internal placeholder field before
    /// the base class validates it, so <c>@bind-Value</c> is not required there (see the class remarks).
    /// </remarks>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        // InputBase requires a ValueExpression on its first parameter set, but range mode binds MinValue/MaxValue
        // instead; the fallback only applies while none is set, and a supplied ValueExpression overwrites it
        if (ValueExpression is null && parameters.TryGetValue<bool>(nameof(Range), out var range) && range)
            ValueExpression = () => rangeModeValue;

        return base.SetParametersAsync(parameters);
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-slider");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        AddLabelAndHintAttributes(builder, 12);

        // Add slider-specific attributes
        builder.AddNumberAttribute(20, "min", Min);
        builder.AddNumberAttribute(21, "max", Max);
        builder.AddNumberAttribute(22, "step", Step);
        builder.AddAttributeIfNotNull(23, "indicator-offset", IndicatorOffset);
        builder.AddAttribute(24, "range", Range);
        builder.AddAttributeIfNotNull(25, "orientation", Orientation?.ToHtmlValue());
        builder.AddAttribute(26, "with-tooltip", WithTooltip);
        builder.AddAttribute(27, "with-markers", WithMarkers);
        builder.AddAttributeIfNotNull(28, "tooltip-placement", TooltipPlacement?.ToHtmlValue());
        builder.AddAttributeIfNotNull(29, "tooltip-distance", TooltipDistance);
        builder.AddAttribute(33, "autofocus", AutoFocus);
        AddWithHintAndLabelAttributes(builder, 14);

        // Add value binding - handle both single and range mode; the element's live value is a JS number, which
        // Blazor's built-in change reader cannot carry, so the handlers listen to the "numericchange" alias of the
        // change event that delivers it as an invariant-culture string ("min,max" in range mode)
        if (Range)
        {
            // For range mode, set min-value and max-value
            builder.AddAttributeIfNotNull(30, "min-value", MinValue);
            builder.AddAttributeIfNotNull(31, "max-value", MaxValue);
            builder.AddAttribute(32, Constants.NumericChangeEventAttribute, EventCallback.Factory.Create<ChangeEventArgs>(this, HandleRangeValueChange));
        }
        else
        {
            // For single value mode, bind the value
            builder.AddAttributeIfNotNull(30, "value", CurrentValue);
            builder.AddAttribute(31, Constants.NumericChangeEventAttribute, EventCallback.Factory.Create<ChangeEventArgs>(this, HandleValueChangeAsync));
        }

        builder.SetUpdatesAttributeName("value");

        // Add common event handlers; the input event carries the same JS number, so OnInput is bound to its alias
        AddCommonEventHandlers(builder, 40, includeInputHandler: false);
        AddNumericInputHandler(builder, 46);

        // Add slider-specific event handlers; OnValueChange is invoked by the single-value change handler above
        builder.AddAttributeIfHasDelegate(52, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(53, __sliderReference => Element = __sliderReference);

        // Add reference labels; the wrapper takes no box of its own, so each label is an item of the slot's flex row
        if (ReferenceContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "reference");
            builder.AddAttribute(62, "style", Constants.TransparentSlotWrapperStyle);
            builder.AddContent(63, ReferenceContent);
            builder.CloseElement();
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70);

        builder.CloseElement();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Formats with the invariant culture, the form <see cref="TryParseValueFromString"/> reads back (the base class would
    /// use the current culture, e.g. "2,5").
    /// </remarks>
    protected override string? FormatValueAsString(decimal? value) => value?.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out decimal? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = null;
            validationErrorMessage = null;
            return true;
        }

        if (ChangeEventArgsExtensions.TryParseJsNumber(value, out var decimalValue))
        {
            result = decimalValue;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = $"The value '{value}' is not a valid number.";
        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Range mode needs no sync: min-value and max-value map to the live minValue/maxValue properties.
    /// </remarks>
    protected override string? LiveValuePropertyName => Range ? null : "value";

    /// <inheritdoc />
    protected override object? GetLiveValue() => (double?)CurrentValue;

    #endregion

    #region ------ Internals ------

    // handles the single-value change event, whose value the numericchange alias delivers as a JS-formatted number;
    // records the element's live value (a JS number) before assigning the model and then reports the new value
    // through OnValueChange, and leaves the model unchanged (reporting nothing) when the value cannot be parsed
    private async Task HandleValueChangeAsync(ChangeEventArgs args)
    {
        var text = args.GetStringValue();
        if (string.IsNullOrEmpty(text))
        {
            MarkLiveValueSynced(null);
            CurrentValue = null;
        }
        else if (ChangeEventArgsExtensions.TryParseJsNumber(text, out var value))
        {
            MarkLiveValueSynced((double)value);
            CurrentValue = value;
        }
        else
        {
            return;
        }

        await OnValueChange.InvokeAsync(CurrentValue);
    }

    // handles the range-mode change event: assigns and reports each bound that parses
    private async Task HandleRangeValueChange(ChangeEventArgs args)
    {
        var values = ParseRangeValues(args);
        if (values is null) return;

        var (minValue, maxValue) = values.Value;

        if (minValue.HasValue)
        {
            MinValue = minValue;
            await MinValueChanged.InvokeAsync(minValue);
        }

        if (maxValue.HasValue)
        {
            MaxValue = maxValue;
            await MaxValueChanged.InvokeAsync(maxValue);
        }
    }

    // target of the fallback ValueExpression in range mode (see SetParametersAsync); never read or written, the
    // range selection lives in MinValue/MaxValue
    private readonly decimal? rangeModeValue = null;

    #endregion
}
