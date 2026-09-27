using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Helpers for reading <see cref="ChangeEventArgs"/> delivered through custom event types.
/// </summary>
internal static class ChangeEventArgsExtensions
{
    /// <summary>
    /// Reads the event value as a string. Blazor's dedicated change-event reader, which yields a string, only runs for
    /// the built-in "change"/"input" event names; for a custom event type (such as the numericchange/numericinput
    /// aliases registered by the JS initializer) <see cref="ChangeEventArgs.Value"/> is deserialized as a
    /// <see cref="JsonElement"/>, which Blazor's binders cannot convert.
    /// </summary>
    /// <param name="args">The change event arguments</param>
    /// <returns>The value as a string, or null when there is no value</returns>
    public static string? GetStringValue(this ChangeEventArgs args)
    {
        return args.Value switch
        {
            null => null,
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } => null,
            JsonElement element => element.GetRawText(),
            var other => Convert.ToString(other, CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Reads the event value as a list of strings: the value of a multiple-selection wa-select or wa-combobox is a
    /// string array, which reaches .NET as a string or object array (or, through a custom event type, a JSON array).
    /// A plain string is split at commas, the format the wrappers used before.
    /// </summary>
    /// <param name="args">The change event arguments</param>
    /// <returns>The values, empty when there is no value</returns>
    public static string[] GetStringArrayValue(this ChangeEventArgs args)
    {
        return args.Value switch
        {
            null => [],
            string[] values => values,
            JsonElement { ValueKind: JsonValueKind.Array } element => element.EnumerateArray()
                .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : item.GetRawText())
                .ToArray(),
            string text => text.Length == 0 ? [] : text.Split(MultipleValueSeparator, StringSplitOptions.RemoveEmptyEntries),
            IEnumerable items => items.Cast<object?>()
                .Select(item => Convert.ToString(item, CultureInfo.InvariantCulture) ?? string.Empty)
                .ToArray(),
            _ => args.GetStringValue() is { Length: > 0 } single ? [single] : []
        };
    }

    /// <summary>
    /// Parses a number formatted by JavaScript (invariant culture, possibly in exponent notation).
    /// </summary>
    /// <param name="text">The text to parse</param>
    /// <param name="value">The parsed number</param>
    /// <returns>true when the text is a valid number</returns>
    public static bool TryParseJsNumber(string? text, out decimal value)
        => decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    #region ------ Internals ------

    private const char MultipleValueSeparator = ',';

    #endregion
}
