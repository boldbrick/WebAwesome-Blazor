using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace WebAwesome.Blazor.Infrastructure;

/// <summary>
/// Reads the value of a <see cref="ChangeEventArgs"/> that a custom element sends, in the forms that reach .NET: a
/// string, a JSON element (through a custom event type) or an array. The form controls of WebAwesome.Blazor read their
/// change and input events with these helpers, and a sister wrapper set can use them for its own controls.
/// </summary>
/// <remarks>
/// A multi-value control binds the array value of its <c>change</c> event as <c>WaSelect</c> does: an explicit
/// <c>onchange</c> handler (<c>EventCallback.Factory.Create&lt;ChangeEventArgs&gt;</c>) reads
/// <c>args.GetStringArrayValue()</c>, records it as the live value and assigns the model.
/// </remarks>
public static class ChangeEventArgsExtensions
{
    /// <summary>
    /// Reads the event value as a string. Blazor's own change-event reader, which gives a string, runs only for the
    /// built-in "change" and "input" event names. For a custom event type, <see cref="ChangeEventArgs.Value"/> arrives
    /// as a <see cref="JsonElement"/>, which Blazor's binders cannot convert; this reads a JSON string as its text, JSON
    /// null as null and any other JSON value as its raw text. Any other value is converted with the invariant culture.
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
    /// Reads the event value as a list of strings, for an element whose value is a string array (a multiple-selection
    /// wa-select or wa-combobox, wa-tag-input). The array reaches .NET as a string or object array, or, through a
    /// custom event type, as a JSON array; a JSON item that is no string gives its raw text. A plain string is split at
    /// commas, and an empty string or null gives an empty list.
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
    /// Parses a number that JavaScript formatted (invariant culture, possibly in exponent notation, for example
    /// "1e-7"), as an element sends its numeric value.
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
