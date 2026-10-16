using System;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// One line of <see cref="WaDataGridRows.DetailFields(WaDataGridDetailField[])"/>: a label and the value of a row field.
/// </summary>
public sealed record WaDataGridDetailField
{
    /// <summary>
    /// The label of the line.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// The path of the row field, for example <c>customer.email</c>.
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// The text format of the value, a text preset of <see cref="WaDataGridFormatters"/> such as
    /// <c>WaDataGridFormatters.Date()</c>; the value as text when null.
    /// </summary>
    public WaJsFunction? Format { get; init; }

    #region ------ Constructors ------

    /// <summary>
    /// Creates a line of a detail panel.
    /// </summary>
    /// <param name="label">The label of the line</param>
    /// <param name="field">The path of the row field</param>
    /// <exception cref="ArgumentException">Thrown when the label is empty or the field is not a field path</exception>
    public WaDataGridDetailField(string label, string field)
    {
        PresetArgs.ValidateLabel(label, nameof(label));
        CellTemplate.ValidateFieldPath(field, nameof(field));
        Label = label;
        Field = field;
    }

    #endregion
}
