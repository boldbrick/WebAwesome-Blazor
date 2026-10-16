using System;
using System.Collections.Generic;
using System.Linq;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Global search presets for <see cref="WaDataGrid.SearchFunction"/> (design section 7.4).
/// </summary>
public static class WaDataGridSearch
{
    /// <summary>
    /// The global search that also finds the display texts of coded columns. A column with a lookup, badge, tag, tags
    /// or icon preset (<see cref="WaDataGridFormatters"/>) and labels shows a label but holds a code, so the built-in
    /// search for "Active" does not find "ACT". This search matches a row when a raw value contains the term (without
    /// case, as the built-in search) or when the label of a value of such a column does.
    /// </summary>
    /// <param name="columns">The columns of the grid; only the columns with a field, a labeled preset and a search that
    /// is not turned off (<see cref="WaDataGridColumn.Searchable"/>) add labels</param>
    /// <returns>The reference for <see cref="WaDataGrid.SearchFunction"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when columns is null</exception>
    /// <remarks>Build the reference from the same column list the grid gets, so the labels match the cells.</remarks>
    public static WaJsFunction Formatted(IEnumerable<WaDataGridColumn> columns)
    {
        if (columns is null) throw new ArgumentNullException(nameof(columns));

        var searched = new List<Dictionary<string, object>>();
        foreach (var column in columns)
        {
            if (column?.Field is null || column.Searchable == false) continue;
            if (PresetArgs.LabelsOf(column.Formatter) is not { } labels) continue;

            var item = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [FieldKey] = column.Field,
                [LabelsKey] = labels.Labels,
            };
            if (labels.Fallback is not null) item[FallbackKey] = labels.Fallback;
            searched.Add(item);
        }

        return new PresetArgs()
            .Set(ColumnsKey, searched)
            .ToFunction(PresetNames.SearchFormatted);
    }

    /// <summary>
    /// The global search that also finds the display texts of coded columns (see
    /// <see cref="Formatted(IEnumerable{WaDataGridColumn})"/>).
    /// </summary>
    /// <param name="columns">The columns of the grid</param>
    /// <returns>The reference for <see cref="WaDataGrid.SearchFunction"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when columns is null</exception>
    public static WaJsFunction Formatted(params WaDataGridColumn[] columns)
    {
        return Formatted((IEnumerable<WaDataGridColumn>)columns);
    }

    #region ------ Internals ------

    // the option names of the search preset
    private const string ColumnsKey = "columns";
    private const string FieldKey = "field";
    private const string LabelsKey = "labels";
    private const string FallbackKey = "fallback";

    #endregion
}
