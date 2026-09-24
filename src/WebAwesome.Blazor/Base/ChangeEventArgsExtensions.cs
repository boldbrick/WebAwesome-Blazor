using Microsoft.AspNetCore.Components;
using System;
using System.Globalization;
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
    /// Parses a number formatted by JavaScript (invariant culture, possibly in exponent notation).
    /// </summary>
    /// <param name="text">The text to parse</param>
    /// <param name="value">The parsed number</param>
    /// <returns>true when the text is a valid number</returns>
    public static bool TryParseJsNumber(string? text, out decimal value)
        => decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
