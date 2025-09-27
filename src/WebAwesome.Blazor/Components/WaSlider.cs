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

    [Parameter] public decimal Min { get; set; } = 0;
    [Parameter] public decimal Max { get; set; } = 100;
    [Parameter] public decimal Step { get; set; } = 1;
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

    [Parameter] public WaOrientation? Orientation { get; set; }
    [Parameter] public bool WithTooltip { get; set; }
    [Parameter] public bool WithMarkers { get; set; }
    [Parameter] public WaPlacement? TooltipPlacement { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<decimal?> OnValueChange { get; set; }

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
        if (OnValueChange.HasDelegate)
            builder.AddAttribute(50, "wa-change", OnValueChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(51, __sliderReference => Element = __sliderReference);

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

    #endregion
}
