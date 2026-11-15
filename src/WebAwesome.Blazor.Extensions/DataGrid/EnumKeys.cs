using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Computes the map keys of the typed enum factories (design section 11.4). The row data reaches the browser as JSON,
/// where an enum value is a number, or a string when the enum has a JSON converter attribute such as
/// <c>[JsonConverter(typeof(JsonStringEnumConverter))]</c>. The preset functions compare <c>String(value)</c> with the
/// keys, so a key is the JSON form of the value: the text of a JSON string, the digits of a JSON number.
/// </summary>
/// <remarks>
/// The serializer sees a converter attribute on the enum type or its members (<c>JsonStringEnumMemberName</c>). A
/// converter that only the serializer options of the app configure (for example in the options of the Blazor JS
/// interop) is not visible here; give such an enum the attribute.
/// </remarks>
internal static class EnumKeys
{
    /// <summary>
    /// The key of one enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <param name="value">The value</param>
    /// <returns>The JSON form of the value as a string</returns>
    /// <exception cref="ArgumentException">Thrown when the value does not serialize to a JSON string or number</exception>
    public static string Of<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var json = JsonSerializer.SerializeToElement(value, JsonSerializerOptions.Web);
        return json.ValueKind switch
        {
            JsonValueKind.String => json.GetString()!,
            JsonValueKind.Number => json.GetRawText(),
            _ => throw new ArgumentException($"The value {value} of {typeof(TEnum).Name} serializes to a JSON {json.ValueKind}, not to a string or a number.", nameof(value))
        };
    }

    /// <summary>
    /// The declared values of an enum, without duplicates (two names with one value count once), in the order of their
    /// numeric values.
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <returns>The values</returns>
    public static IReadOnlyList<TEnum> ValuesOf<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Distinct().ToArray();
    }

    /// <summary>
    /// A map from the key of each declared value to a projection of the value. A projection that returns null leaves
    /// the value out.
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <typeparam name="TResult">The projected type</typeparam>
    /// <param name="project">The projection</param>
    /// <returns>The map, in the order of the values</returns>
    public static Dictionary<string, TResult> Map<TEnum, TResult>(Func<TEnum, TResult?> project)
        where TEnum : struct, Enum
    {
        var map = new Dictionary<string, TResult>(StringComparer.Ordinal);
        foreach (var value in ValuesOf<TEnum>())
        {
            var result = project(value);
            if (result is not null) map[Of(value)] = result;
        }

        return map;
    }
}
