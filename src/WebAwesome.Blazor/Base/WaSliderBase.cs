using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Base class of the wrappers of wa-slider (<see cref="WaSlider"/> with a nullable value, <see cref="WaRange"/> with
/// a non-nullable one): declares the element's parameters, content and methods once. The value binding and the
/// range-mode callbacks, which differ between the two, are declared by each wrapper.
/// </summary>
/// <typeparam name="TValue">The type of value bound to the slider</typeparam>
public abstract class WaSliderBase<TValue> : WaLabeledInputBase<TValue>
{
    /// <summary>
    /// Makes the slider read-only, allowing its value to be seen but not edited.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Min"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const decimal DefaultMin = 0m;

    /// <summary>
    /// The minimum value allowed.
    /// </summary>
    [Parameter] public decimal Min { get; set; } = DefaultMin;

    /// <summary>
    /// The Web Awesome default of <see cref="Max"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const decimal DefaultMax = 100m;

    /// <summary>
    /// The maximum value allowed.
    /// </summary>
    [Parameter] public decimal Max { get; set; } = DefaultMax;

    /// <summary>
    /// The Web Awesome default of <see cref="Step"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const decimal DefaultStep = 1m;

    /// <summary>
    /// The granularity the value must adhere to when incrementing and decrementing.
    /// </summary>
    [Parameter] public decimal Step { get; set; } = DefaultStep;

    /// <summary>
    /// The starting value from which to draw the slider's fill, which is based on its current value.
    /// </summary>
    [Parameter] public decimal? IndicatorOffset { get; set; }

    /// <summary>
    /// Converts the slider to a range slider with two thumbs, whose selection is <see cref="MinValue"/> and
    /// <see cref="MaxValue"/>.
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
    /// The side of the slider's thumb on which the tooltip is shown. When null, the attribute is omitted and Web
    /// Awesome's default (top) applies.
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
    /// Invoked when the form control has been checked for validity and its constraints are not satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    /// <summary>
    /// One or more reference labels shown below the slider (e.g. one <c>&lt;span&gt;</c> per label), rendered into the
    /// element's "reference" slot; the labels are spread evenly along the track.
    /// </summary>
    [Parameter] public RenderFragment? ReferenceContent { get; set; }

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

    #region ------ Internals ------

    /// <summary>
    /// Parses the range-mode change payload the numericchange alias delivers, "&lt;minValue&gt;,&lt;maxValue&gt;"
    /// (JS-formatted numbers).
    /// </summary>
    /// <param name="args">The change event arguments</param>
    /// <returns>Each bound, or null when it cannot be parsed; null when the payload is not a pair</returns>
    internal static (decimal? Min, decimal? Max)? ParseRangeValues(ChangeEventArgs args)
    {
        var parts = args.GetStringValue()?.Split(RangeValueSeparator);
        if (parts is not { Length: 2 }) return null;

        return (ParseBound(parts[0]), ParseBound(parts[1]));
    }

    private static decimal? ParseBound(string text) => ChangeEventArgsExtensions.TryParseJsNumber(text, out var value) ? value : null;

    // separates the min and max value in the range-mode change payload built by the JS initializer
    private const char RangeValueSeparator = ',';

    #endregion
}
