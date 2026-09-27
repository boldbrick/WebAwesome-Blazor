using System;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A standalone calendar for selecting a single date. Unlike <see cref="WaDateInput"/> it is not a
/// form-associated control; bind its value with <c>@bind-Value</c>.
/// Corresponds to the wa-date-picker Web Awesome component.
/// </summary>
/// <remarks>
/// This is a Pro component. The value travels as ISO <c>yyyy-MM-dd</c>, culture-free; an empty or invalid element
/// value binds as null.
/// </remarks>
public class WaDatePicker : WaDatePickerBase<DateOnly?>
{
    #region ------ Internals ------

    /// <inheritdoc />
    private protected override string? FormatValue(DateOnly? value)
        => value.HasValue ? WaWireFormat.FormatDate(value.Value) : null;

    /// <inheritdoc />
    private protected override DateOnly? ParseValue(string? value)
        => WaWireFormat.TryParseDate(value, out var date) ? date : null;

    #endregion
}
