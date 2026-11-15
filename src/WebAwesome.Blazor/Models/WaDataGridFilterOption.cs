using System.Text.Json.Serialization;

namespace WebAwesome.Blazor.Models;

/// <summary>
/// One option of a data grid column's value picker (<see cref="WaDataGridColumn.FilterOptions"/>), serialized as an
/// entry of the column's <c>filterOptions</c>.
/// </summary>
/// <param name="Value">The value the option filters by; it is compared with the cell value as a string.</param>
/// <param name="Label">The text the picker displays; null displays <paramref name="Value"/>.</param>
/// <param name="Count">The count shown next to the label, for example the number of matching rows; null shows
/// none.</param>
public sealed record WaDataGridFilterOption(
    string Value,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Label = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? Count = null);
