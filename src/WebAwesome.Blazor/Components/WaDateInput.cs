using System;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A date input with segmented text entry and a popup calendar, bound to a single date.
/// Corresponds to the wa-date-input Web Awesome component.
/// </summary>
/// <remarks>
/// <para>
/// This is a Pro component.
/// </para>
/// <para>
/// The value travels as ISO <c>yyyy-MM-dd</c>, the only form the element reads and writes; formatting and parsing are
/// culture-free. An empty element value binds as null; a value that is not an ISO date adds the validation message
/// "The {field} field must be a date." to the edit context.
/// </para>
/// </remarks>
public class WaDateInput : WaDateInputBase<DateOnly?>
{
    #region ------ Overrides ------

    /// <summary>
    /// Formats the value as ISO <c>yyyy-MM-dd</c>, culture-free, the form the element reads; null renders no value.
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The wire string</returns>
    protected override string? FormatValueAsString(DateOnly? value)
        => value.HasValue ? WaWireFormat.FormatDate(value.Value) : null;

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out DateOnly? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = null;
            validationErrorMessage = null;
            return true;
        }

        if (WaWireFormat.TryParseDate(value, out var date))
        {
            result = date;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = FormatValidationMessage(Constants.DateValidationMessageFormat);
        return false;
    }

    #endregion
}
