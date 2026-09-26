using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// An experimental standalone calendar for selecting a date range (a start and an end date). Renders wa-date-picker
/// in range mode; for a single date, use <see cref="WaDatePicker"/>. Not a form-associated control; bind its value
/// with <c>@bind-Value</c>.
/// Corresponds to the wa-date-picker Web Awesome component with <c>mode="range"</c>.
/// </summary>
/// <remarks>
/// This is a Pro component. The value travels in the element's wire form, <c>yyyy-MM-dd/yyyy-MM-dd</c>, converted
/// culture-free (see <see cref="WaDateRange"/>). After the first click of a new range the element reports that one
/// date, which binds as a half-filled range (<see cref="WaDateRange.From"/> set, <see cref="WaDateRange.To"/> null);
/// an empty or invalid element value binds as null.
/// </remarks>
public class WaDateRangePicker : WaDatePickerBase<WaDateRange?>
{
    /// <summary>
    /// The Web Awesome default of <see cref="MinRange"/> (0, no limit): what the element holds while the parameter is null, and
    /// what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const int DefaultMinRange = 0;

    /// <summary>
    /// Minimum range length in days, counting both ends; null (or 0) sets no minimum.
    /// </summary>
    [Parameter] public int? MinRange { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="MaxRange"/> (0, no limit): what the element holds while the parameter is null, and
    /// what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const int DefaultMaxRange = 0;

    /// <summary>
    /// Maximum range length in days, counting both ends; null (or 0) sets no maximum.
    /// </summary>
    [Parameter] public int? MaxRange { get; set; }

    #region ------ Internals ------

    /// <inheritdoc />
    private protected override string? FormatValue(WaDateRange? value)
        => value is { IsEmpty: false } range ? WaWireFormat.FormatDateRange(range) : null;

    /// <inheritdoc />
    private protected override WaDateRange? ParseValue(string? value)
        => WaWireFormat.TryParseDateRange(value, out var range) ? range : null;

    /// <inheritdoc />
    private protected override void AddSelectionModeAttributes(RenderTreeBuilder builder, int sequence)
    {
        builder.AddAttribute(sequence + 0, Constants.ModeAttribute, Constants.RangeModeValue);
        builder.AddAttributeIfNotNull(WaAttributeMemory.Of(this), sequence + 1, Constants.MinRangeAttribute, MinRange, DefaultMinRange);
        builder.AddAttributeIfNotNull(WaAttributeMemory.Of(this), sequence + 2, Constants.MaxRangeAttribute, MaxRange, DefaultMaxRange);
    }

    #endregion
}
