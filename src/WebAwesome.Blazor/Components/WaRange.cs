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
    private protected override void AddValueAttribute(RenderTreeBuilder builder, WaAttributeMemory attributes, int sequence)
        => builder.AddNumberAttribute(attributes, sequence, "value", CurrentValue);

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
    private protected override Task HandleValueChangeAsync(ChangeEventArgs args)
    {
        if (!ChangeEventArgsExtensions.TryParseJsNumber(args.GetStringValue(), out var value)) return Task.CompletedTask;

        MarkLiveValueSynced((double)value);
        CurrentValue = value;
        return Task.CompletedTask;
    }

    // handles the range-mode change event; updates MinValue/MaxValue and reports each bound that changed
    private protected override async Task HandleRangeValueChangeAsync(ChangeEventArgs args)
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
