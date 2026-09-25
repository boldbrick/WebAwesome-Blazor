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
public class WaSlider : WaInputBase<decimal?>
{
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
    /// The starting value from which to draw the slider's fill, which is based on its current value.
    /// </summary>
    [Parameter] public decimal? IndicatorOffset { get; set; }

    #endregion

    #region ------ Range Selection Mode ------

    /// <summary>
    /// Whether this slider supports range selection (dual-thumb mode).
    /// </summary>
    /// <remarks>
    /// In range mode the selection is bound through <see cref="MinValue"/> and <see cref="MaxValue"/>
    /// (<c>@bind-MinValue</c>/<c>@bind-MaxValue</c>); <c>@bind-Value</c> is neither needed nor used, and
    /// <see cref="OnValueChange"/> is not invoked. When no <c>ValueExpression</c> is supplied (no
    /// <c>@bind-Value</c>), the slider falls back to an internal placeholder field, so no field of an enclosing
    /// <c>EditForm</c> model is associated with it: the range values are not validated or tracked by the edit
    /// context. The slider must be in range mode on its first render to use the fallback; single-value mode keeps
    /// requiring <c>@bind-Value</c> like any other input.
    /// </remarks>
    [Parameter] public bool Range { get; set; }

    /// <summary>
    /// The minimum value in range selection mode
    /// </summary>
    [Parameter] public decimal? MinValue { get; set; }

    /// <summary>
    /// The maximum value in range selection mode
    /// </summary>
    [Parameter] public decimal? MaxValue { get; set; }

    /// <summary>
    /// Invoked with the new minimum value when the user commits a change in range selection mode (the element's
    /// change event); the <c>@bind-MinValue</c> counterpart of <see cref="MinValue"/>.
    /// </summary>
    [Parameter] public EventCallback<decimal?> MinValueChanged { get; set; }

    /// <summary>
    /// Invoked with the new maximum value when the user commits a change in range selection mode (the element's
    /// change event); the <c>@bind-MaxValue</c> counterpart of <see cref="MaxValue"/>.
    /// </summary>
    [Parameter] public EventCallback<decimal?> MaxValueChanged { get; set; }

    #endregion

    #region ------ Visual Properties ------

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
    /// The side of the slider's thumb on which the tooltip is shown. When null, the attribute is omitted and Web Awesome's
    /// default (top) applies.
    /// </summary>
    [Parameter] public WaTooltipSide? TooltipPlacement { get; set; }

    /// <summary>
    /// The distance in pixels from which to offset the tooltip from the slider's thumb.
    /// </summary>
    [Parameter] public int? TooltipDistance { get; set; }

    /// <summary>
    /// Automatically focuses the slider when the page loads.
    /// </summary>
    [Parameter] public bool AutoFocus { get; set; }

    /// <summary>
    /// Only required for SSR. Set to true when slotting in a hint element so the server-rendered markup includes
    /// the hint before the component hydrates on the client.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    /// <summary>
    /// Only required for SSR. Set to true when slotting in a label element so the server-rendered markup includes
    /// the label before the component hydrates on the client.
    /// </summary>
    [Parameter] public bool WithLabel { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked with the new value when the user commits a change in single-value mode (the element's change
    /// event), after the bound value has been updated. Not invoked in range mode, see
    /// <see cref="MinValueChanged"/> and <see cref="MaxValueChanged"/>.
    /// </summary>
    [Parameter] public EventCallback<decimal?> OnValueChange { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints are not satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// Reference labels to display below the slider
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

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
    /// the base class validates it, so <c>@bind-Value</c> is not required there (see <see cref="Range"/>).
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
        builder.AddAttribute(34, "with-hint", WithHint);
        builder.AddAttribute(35, "with-label", WithLabel);

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

        // Add child content (reference labels)
        if (ChildContent is not null)
        {
            builder.AddContent(60, ChildContent);
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

    #region ------ Public Methods ------

    /// <summary>
    /// Sets the custom value formatting function for tooltips and screen readers.
    /// </summary>
    /// <param name="jsFunction">JavaScript function string that formats slider values for display</param>
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

    /// <summary>
    /// Removes focus from the slider.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Sets focus on the slider.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Decrements the slider's value by <see cref="Step"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task StepDownAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot step down: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "stepDown");
    }

    /// <summary>
    /// Increments the slider's value by <see cref="Step"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task StepUpAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot step up: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "stepUp");
    }

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

    // handles the range-mode change event, whose value the numericchange alias delivers as "<minValue>,<maxValue>"
    // (JS-formatted numbers)
    private async Task HandleRangeValueChange(ChangeEventArgs args)
    {
        var parts = args.GetStringValue()?.Split(RangeValueSeparator);
        if (parts is not { Length: 2 }) return;

        if (ChangeEventArgsExtensions.TryParseJsNumber(parts[0], out var minVal))
        {
            MinValue = minVal;
            await MinValueChanged.InvokeAsync(minVal);
        }

        if (ChangeEventArgsExtensions.TryParseJsNumber(parts[1], out var maxVal))
        {
            MaxValue = maxVal;
            await MaxValueChanged.InvokeAsync(maxVal);
        }
    }

    // separates the min and max value in the range-mode change payload built by the JS initializer
    private const char RangeValueSeparator = ',';

    // target of the fallback ValueExpression in range mode (see SetParametersAsync); never read or written, the
    // range selection lives in MinValue/MaxValue
    private readonly decimal? rangeModeValue = null;

    #endregion
}
