using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Serializes an enum as the exact string Web Awesome uses for it (its ToHtmlValue() form), both ways: event args
/// arriving from the JS initializer and models pushed to the element as JS properties. An unknown string fails
/// with a <see cref="JsonException"/> naming the enum, because the value set is closed (the CEM or .d.ts union).
/// </summary>
/// <typeparam name="TEnum">The enum type</typeparam>
internal abstract class WaEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    /// <inheritdoc />
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => ReadNumberText(ref reader),
            _ => null
        };

        if (text != null && byWireValue.TryGetValue(text, out var value)) return value;
        throw new JsonException($"'{text ?? reader.TokenType.ToString()}' is no {typeof(TEnum).Name} value.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        => writer.WriteStringValue(ToWire(value));

    #region ------ Constructors ------

    /// <summary>
    /// Builds the reverse lookup of every enum member's wire string.
    /// </summary>
    protected WaEnumJsonConverter()
    {
        byWireValue = Enum.GetValues<TEnum>().ToDictionary(ToWire, v => v, StringComparer.Ordinal);
    }

    #endregion

    #region ------ Internals ------

    private readonly Dictionary<string, TEnum> byWireValue;

    // the invariant text of an integral JSON number token, e.g. "-1"; null for any other number
    private static string? ReadNumberText(ref Utf8JsonReader reader)
        => reader.TryGetInt64(out var number) ? number.ToString(CultureInfo.InvariantCulture) : null;

    #endregion

    #region ------ Interface for descendants ------

    /// <summary>
    /// The wire string of an enum member.
    /// </summary>
    /// <param name="value">The enum member</param>
    /// <returns>Its ToHtmlValue() form</returns>
    protected abstract string ToWire(TEnum value);

    #endregion
}

/// <summary>
/// JSON form of <see cref="WaDatePickerView"/> (the wa-view-change detail).
/// </summary>
internal sealed class WaDatePickerViewJsonConverter : WaEnumJsonConverter<WaDatePickerView>
{
    /// <inheritdoc />
    protected override string ToWire(WaDatePickerView value) => value.ToHtmlValue();
}

/// <summary>
/// JSON form of <see cref="WaRatingHoverPhase"/> (the wa-hover detail).
/// </summary>
internal sealed class WaRatingHoverPhaseJsonConverter : WaEnumJsonConverter<WaRatingHoverPhase>
{
    /// <inheritdoc />
    protected override string ToWire(WaRatingHoverPhase value) => value.ToHtmlValue();
}

/// <summary>
/// JSON form of <see cref="WaMutationType"/> (the projected wa-mutation records).
/// </summary>
internal sealed class WaMutationTypeJsonConverter : WaEnumJsonConverter<WaMutationType>
{
    /// <inheritdoc />
    protected override string ToWire(WaMutationType value) => value.ToHtmlValue();
}

/// <summary>
/// JSON form of <see cref="WaDataGridPinSide"/> (the wa-column-pin detail and a column's pinned member).
/// </summary>
internal sealed class WaDataGridPinSideJsonConverter : WaEnumJsonConverter<WaDataGridPinSide>
{
    /// <inheritdoc />
    protected override string ToWire(WaDataGridPinSide value) => value.ToHtmlValue();
}

/// <summary>
/// JSON form of <see cref="WaDataGridAlign"/> (a column's align and headerAlign members).
/// </summary>
internal sealed class WaDataGridAlignJsonConverter : WaEnumJsonConverter<WaDataGridAlign>
{
    /// <inheritdoc />
    protected override string ToWire(WaDataGridAlign value) => value.ToHtmlValue();
}

/// <summary>
/// JSON form of <see cref="WaDataGridSortFn"/> (a column's sortFn member).
/// </summary>
internal sealed class WaDataGridSortFnJsonConverter : WaEnumJsonConverter<WaDataGridSortFn>
{
    /// <inheritdoc />
    protected override string ToWire(WaDataGridSortFn value) => value.ToHtmlValue();
}

/// <summary>
/// JSON form of <see cref="WaDataGridFilterType"/> (a column's filterType member).
/// </summary>
internal sealed class WaDataGridFilterTypeJsonConverter : WaEnumJsonConverter<WaDataGridFilterType>
{
    /// <inheritdoc />
    protected override string ToWire(WaDataGridFilterType value) => value.ToHtmlValue();
}

/// <summary>
/// JSON form of <see cref="WaDataGridAggregation"/> (a column's aggregation member).
/// </summary>
internal sealed class WaDataGridAggregationJsonConverter : WaEnumJsonConverter<WaDataGridAggregation>
{
    /// <inheritdoc />
    protected override string ToWire(WaDataGridAggregation value) => value.ToHtmlValue();
}

/// <summary>
/// JSON form of <see cref="WaDataGridSortUndefined"/> (a column's sortUndefined member): "first" and "last" as
/// strings, <see cref="WaDataGridSortUndefined.Lower"/> and <see cref="WaDataGridSortUndefined.Higher"/> as the
/// numbers -1 and 1, as the .d.ts union <c>false | -1 | 1 | 'first' | 'last'</c> types them.
/// </summary>
internal sealed class WaDataGridSortUndefinedJsonConverter : WaEnumJsonConverter<WaDataGridSortUndefined>
{
    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, WaDataGridSortUndefined value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case WaDataGridSortUndefined.Lower:
                writer.WriteNumberValue(LowerRank);
                break;
            case WaDataGridSortUndefined.Higher:
                writer.WriteNumberValue(HigherRank);
                break;
            default:
                base.Write(writer, value, options);
                break;
        }
    }

    /// <inheritdoc />
    protected override string ToWire(WaDataGridSortUndefined value) => value.ToHtmlValue();

    #region ------ Internals ------

    private const int LowerRank = -1;
    private const int HigherRank = 1;

    #endregion
}
