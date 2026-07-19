using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
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
    /// Whether this slider supports range selection (dual-thumb mode)
    /// </summary>
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
    /// Callback for when MinValue changes in range selection mode
    /// </summary>
    [Parameter] public EventCallback<decimal?> MinValueChanged { get; set; }

    /// <summary>
    /// Callback for when MaxValue changes in range selection mode
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
    /// The placement of the tooltip in reference to the slider's thumb.
    /// </summary>
    [Parameter] public WaPlacement? TooltipPlacement { get; set; }

    /// <summary>
    /// The distance in pixels from which to offset the tooltip from the slider's thumb.
    /// </summary>
    [Parameter] public int? TooltipDistance { get; set; }

    /// <summary>
    /// Automatically focuses the slider when the page loads.
    /// </summary>
    [Parameter] public bool AutoFocus { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the slider's value changes.
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
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-slider");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add slider-specific attributes
        builder.AddAttribute(20, "min", Min);
        builder.AddAttribute(21, "max", Max);
        builder.AddAttribute(22, "step", Step);
        builder.AddAttributeIfNotNull(23, "indicator-offset", IndicatorOffset);
        builder.AddAttribute(24, "range", Range);
        builder.AddAttributeIfNotNull(25, "orientation", Orientation?.ToHtmlValue());
        builder.AddAttribute(26, "with-tooltip", WithTooltip);
        builder.AddAttribute(27, "with-markers", WithMarkers);
        builder.AddAttributeIfNotNull(28, "tooltip-placement", TooltipPlacement?.ToHtmlValue());
        builder.AddAttributeIfNotNull(29, "tooltip-distance", TooltipDistance);
        builder.AddAttribute(33, "autofocus", AutoFocus);

        // Add value binding - handle both single and range mode
        if (Range)
        {
            // For range mode, set min-value and max-value
            builder.AddAttributeIfNotNull(30, "min-value", MinValue);
            builder.AddAttributeIfNotNull(31, "max-value", MaxValue);
            builder.AddAttribute(32, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleRangeValueChange));
        }
        else
        {
            // For single value mode, use standard binding
            builder.AddAttribute(30, "value", CurrentValue);
            builder.AddAttribute(31, "onchange", EventCallback.Factory.CreateBinder<decimal?>(this, __value => CurrentValue = __value, CurrentValue));
        }

        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add slider-specific event handlers
        builder.AddAttributeIfHasDelegate(50, "wa-change", OnValueChange);
        builder.AddAttributeIfHasDelegate(52, "wa-invalid", OnInvalid);

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
    protected override bool TryParseValueFromString(string? value, out decimal? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = null;
            validationErrorMessage = null;
            return true;
        }

        if (decimal.TryParse(value, out var decimalValue))
        {
            result = decimalValue;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = $"The value '{value}' is not a valid number.";
        return false;
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Handles change events for range selection mode
    /// </summary>
    private async Task HandleRangeValueChange(ChangeEventArgs args)
    {
        // In range mode, the web component would typically fire events with both min and max values
        // This is a simplified implementation - in practice, you'd need JS interop to get both values
        if (args.Value is string stringValue)
        {
            // Parse range values (format might be "min,max" or handled differently by the component)
            // This is a placeholder implementation
            var parts = stringValue.Split(',');
            if (parts.Length == 2)
            {
                if (decimal.TryParse(parts[0], out var minVal))
                {
                    MinValue = minVal;
                    await MinValueChanged.InvokeAsync(minVal);
                }

                if (decimal.TryParse(parts[1], out var maxVal))
                {
                    MaxValue = maxVal;
                    await MaxValueChanged.InvokeAsync(maxVal);
                }
            }
        }
    }

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
}
