using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// A row of the grid data with the HTML of its Razor templates (<see cref="WaDataGridRazorTemplates{TRow}"/>). It
/// serializes as the row itself plus the property <see cref="HtmlProperty"/>, which maps each template key to its HTML.
/// No column reads that property, so sort, filter, search and the CSV export see the row as it is.
/// </summary>
[JsonConverter(typeof(RazorTemplateRowJsonConverter))]
internal sealed class RazorTemplateRow
{
    /// <summary>
    /// The row property that holds the HTML of the templates; <c>templateHtmlProperty</c> in
    /// <c>data-grid-presets.js</c>.
    /// </summary>
    public const string HtmlProperty = "$wabTemplateHtml";

    /// <summary>
    /// The row of the app.
    /// </summary>
    public object Row { get; }

    /// <summary>
    /// The HTML of each template, by key.
    /// </summary>
    public IReadOnlyDictionary<string, string> Html { get; }

    #region ------ Constructors ------

    /// <summary>
    /// Creates the row.
    /// </summary>
    /// <param name="row">The row of the app</param>
    /// <param name="html">The HTML of each template, by key</param>
    public RazorTemplateRow(object row, IReadOnlyDictionary<string, string> html)
    {
        Row = row ?? throw new ArgumentNullException(nameof(row));
        Html = html ?? throw new ArgumentNullException(nameof(html));
    }

    #endregion
}

/// <summary>
/// Writes a <see cref="RazorTemplateRow"/>: the properties of the row as the given serializer options write them (the
/// options of the Blazor JS interop when the grid pushes its data), then the HTML of the templates. Reading is not
/// supported: the row goes to the browser only.
/// </summary>
internal sealed class RazorTemplateRowJsonConverter : JsonConverter<RazorTemplateRow>
{
    /// <inheritdoc />
    public override RazorTemplateRow Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("A data grid row with Razor template HTML is written to the browser only.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, RazorTemplateRow value, JsonSerializerOptions options)
    {
        using var document = JsonSerializer.SerializeToDocument(value.Row, value.Row.GetType(), options);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException($"A data grid row must serialize as a JSON object, but {value.Row.GetType().Name} serializes as {document.RootElement.ValueKind}.");

        writer.WriteStartObject();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.NameEquals(RazorTemplateRow.HtmlProperty)) continue;
            property.WriteTo(writer);
        }

        // the keys stay verbatim: they must equal the keys of the formatters
        writer.WriteStartObject(RazorTemplateRow.HtmlProperty);
        foreach (var (key, html) in value.Html)
        {
            writer.WriteString(key, html);
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
