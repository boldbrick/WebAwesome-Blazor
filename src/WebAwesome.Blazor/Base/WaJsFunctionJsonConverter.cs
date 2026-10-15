using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Writes a <see cref="WaJsFunction"/> as its JSON descriptor, with any serializer options. The descriptor only goes
/// to the browser, so reading one is not supported.
/// </summary>
internal sealed class WaJsFunctionJsonConverter : JsonConverter<WaJsFunction>
{
    /// <inheritdoc />
    public override WaJsFunction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("A JavaScript function reference is written to the browser only; it cannot be read back.");

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, WaJsFunction value, JsonSerializerOptions options)
        => value.WriteDescriptor(writer);
}
