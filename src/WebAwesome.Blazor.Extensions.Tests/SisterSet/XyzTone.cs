using System;
using System.Text.Json.Serialization;
using WebAwesome.Blazor.Infrastructure;

namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// A test-only enum of a sister set, serialized as the strings its element uses.
/// </summary>
[JsonConverter(typeof(XyzToneJsonConverter))]
public enum XyzTone
{
    /// <summary>
    /// The calm tone, "calm".
    /// </summary>
    Calm,

    /// <summary>
    /// The loud tone, "very-loud".
    /// </summary>
    VeryLoud
}

/// <summary>
/// The JSON form of <see cref="XyzTone"/>, derived from the public <see cref="WaEnumJsonConverter{TEnum}"/> outside
/// WebAwesome.Blazor.
/// </summary>
public sealed class XyzToneJsonConverter : WaEnumJsonConverter<XyzTone>
{
    /// <inheritdoc />
    protected override string ToWire(XyzTone value) => value switch
    {
        XyzTone.Calm => "calm",
        XyzTone.VeryLoud => "very-loud",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}
