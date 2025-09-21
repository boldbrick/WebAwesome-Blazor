using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A range/slider input component for selecting numeric values within a specified range.
/// Corresponds to the wa-slider Web Awesome component.
/// </summary>
public class WaRange : WaInputBase<decimal>
{
    #region ------ Range Properties ------

    [Parameter] public decimal Min { get; set; } = 0;
    [Parameter] public decimal Max { get; set; } = 100;
    [Parameter] public decimal Step { get; set; } = 1;
    [Parameter] public WaOrientation? Orientation { get; set; }
    [Parameter] public bool WithTooltip { get; set; }
    [Parameter] public bool WithMarkers { get; set; }
    [Parameter] public string? TooltipPlacement { get; set; }
    [Parameter] public decimal? IndicatorOffset { get; set; }

    // Range selection (dual thumb)
    [Parameter] public bool Range { get; set; }
    [Parameter] public decimal? MinValue { get; set; }
    [Parameter] public decimal? MaxValue { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<decimal> OnMinValueChange { get; set; }
    [Parameter] public EventCallback<decimal> OnMaxValueChange { get; set; }

    #endregion

    #region ------ Content Slots ------

    /// <summary>
    /// Reference labels displayed below the slider
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

        // Add slider-specific attributes
        builder.AddAttribute(20, "min", Min);
        builder.AddAttribute(21, "max", Max);
        builder.AddAttribute(22, "step", Step);
        builder.AddAttributeIfNotNull(23, "orientation", Orientation?.ToHtmlValue());
        builder.AddAttribute(24, "with-tooltip", WithTooltip);
        builder.AddAttribute(25, "with-markers", WithMarkers);
        builder.AddAttributeIfNotNullOrEmpty(26, "tooltip-placement", TooltipPlacement);
        builder.AddAttributeIfNotNull(27, "indicator-offset", IndicatorOffset);

        // Range selection attributes
        builder.AddAttribute(30, "range", Range);
        if (Range)
        {
            builder.AddAttributeIfNotNull(31, "min-value", MinValue);
            builder.AddAttributeIfNotNull(32, "max-value", MaxValue);
        }
        else
        {
            builder.AddAttribute(33, "value", BindConverter.FormatValue(CurrentValue));
        }

        // Add value binding for single value mode
        if (!Range)
        {
            builder.AddAttribute(40, "onchange", EventCallback.Factory.CreateBinder<decimal>(this, __value => CurrentValue = __value, CurrentValue));
            builder.SetUpdatesAttributeName("value");
        }
        else
        {
            // TODO: Range mode requires custom event handling for min-value and max-value changes
            // This requires JavaScript interop to properly handle dual-thumb events
        }

        // Add common event handlers
        AddCommonEventHandlers(builder, 50);

        // Add element reference capture
        builder.AddElementReferenceCapture(60, __sliderReference => Element = __sliderReference);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70);

        // Add reference content slot
        if (ReferenceContent is not null)
        {
            builder.OpenElement(80, "span");
            builder.AddAttribute(81, "slot", "reference");
            builder.AddContent(82, ReferenceContent);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out decimal result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (decimal.TryParse(value, out result))
        {
            validationErrorMessage = null;
            return true;
        }

        result = default;
        validationErrorMessage = $"The {DisplayName ?? FieldIdentifier.FieldName} field must be a number.";
        return false;
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Sets a custom value formatter function for tooltips and screen readers.
    /// </summary>
    /// <remarks>
    /// TODO: This method requires JavaScript interop to call the underlying wa-slider's valueFormatter property.
    /// Implementation depends on the Web Awesome library being properly loaded in the page.
    /// </remarks>
    public void SetValueFormatter(string jsFunction)
    {
        // TODO: Implement JavaScript interop call
        // Should set Element.valueFormatter = jsFunction on the underlying wa-slider element
        throw new NotImplementedException("SetValueFormatter requires JavaScript interop implementation. " +
            "This should set the underlying wa-slider element's valueFormatter property.");
    }

    #endregion
}
