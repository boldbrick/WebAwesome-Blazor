using System;
using System.Collections.Generic;
using System.Linq;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Row presets for the grid members of <see cref="WaDataGrid"/> (design section 7.5): a detail panel with fields for
/// <see cref="WaDataGrid.RowDetailFunction"/>, and the selectable rows by a field for
/// <see cref="WaDataGrid.SelectableRowsFunction"/>.
/// </summary>
public static class WaDataGridRows
{
    /// <summary>
    /// A detail panel that lists fields of the row, a label and a value per line (a <c>dl</c> element). Output: an
    /// element.
    /// </summary>
    /// <param name="fields">The lines, in their order</param>
    /// <returns>The reference for <see cref="WaDataGrid.RowDetailFunction"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when fields is null</exception>
    /// <exception cref="ArgumentException">Thrown when there is no field, a field is null, or a format is not a text
    /// preset</exception>
    public static WaJsFunction DetailFields(params WaDataGridDetailField[] fields)
    {
        return DetailFields((IReadOnlyList<WaDataGridDetailField>)fields);
    }

    /// <summary>
    /// A detail panel that lists fields of the row (see <see cref="DetailFields(WaDataGridDetailField[])"/>).
    /// </summary>
    /// <param name="fields">The lines, in their order</param>
    /// <param name="nullText">The text of a null or empty value; empty by default</param>
    /// <returns>The reference for <see cref="WaDataGrid.RowDetailFunction"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when fields is null</exception>
    /// <exception cref="ArgumentException">Thrown when there is no field, a field is null, or a format is not a text
    /// preset</exception>
    public static WaJsFunction DetailFields(IReadOnlyList<WaDataGridDetailField> fields, string? nullText = null)
    {
        if (fields is null) throw new ArgumentNullException(nameof(fields));
        if (fields.Count == 0) throw new ArgumentException("Give at least one field.", nameof(fields));
        if (fields.Any(field => field is null)) throw new ArgumentException("A field must not be null.", nameof(fields));

        var lines = fields
            .Select(field =>
            {
                var line = new Dictionary<string, object>(StringComparer.Ordinal) { [LabelKey] = field.Label, [FieldKey] = field.Field };
                if (PresetArgs.TextFormatOrNull(field.Format, nameof(fields)) is { } format) line[FormatKey] = format;
                return line;
            })
            .ToList();

        return new PresetArgs()
            .SetNullText(nullText)
            .Set(FieldsKey, lines)
            .ToFunction(PresetNames.DetailFields);
    }

    /// <summary>
    /// Only the rows whose field holds a true value can be selected, for example <c>"canSelect"</c>.
    /// </summary>
    /// <param name="field">The path of the row field</param>
    /// <returns>The reference for <see cref="WaDataGrid.SelectableRowsFunction"/></returns>
    /// <exception cref="ArgumentException">Thrown when the field is not a field path</exception>
    public static WaJsFunction SelectableField(string field)
    {
        CellTemplate.ValidateFieldPath(field, nameof(field));

        return new PresetArgs()
            .Set(FieldKey, field)
            .ToFunction(PresetNames.SelectableField);
    }

    #region ------ Internals ------

    // the option names of the row presets
    private const string FieldsKey = "fields";
    private const string LabelKey = "label";
    private const string FieldKey = "field";
    private const string FormatKey = "format";

    #endregion
}
