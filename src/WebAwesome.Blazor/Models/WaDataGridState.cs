using System.Collections.Generic;

namespace WebAwesome.Blazor.Models;

#region ------ Data Grid State Models ------

/// <summary>
/// A serializable snapshot of the data grid's user-adjustable view state (column order, widths,
/// visibility, pinning, sort, filters, search, selection, and paging), as returned by
/// <c>GetStateAsync</c> and accepted by <c>SetStateAsync</c>. Safe to persist as JSON.
/// </summary>
public class WaDataGridState
{
    /// <summary>
    /// The state schema version. Always <c>1</c> for the current Web Awesome release.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// The user-applied column display order, as an array of column ids.
    /// </summary>
    public IReadOnlyList<string>? ColumnOrder { get; set; }

    /// <summary>
    /// Per-column widths in pixels, keyed by column id.
    /// </summary>
    public IReadOnlyDictionary<string, int>? ColumnWidths { get; set; }

    /// <summary>
    /// Per-column visibility, keyed by column id.
    /// </summary>
    public IReadOnlyDictionary<string, bool>? ColumnVisibility { get; set; }

    /// <summary>
    /// Which columns are pinned to which edge.
    /// </summary>
    public WaDataGridColumnPinning? ColumnPinning { get; set; }

    /// <summary>
    /// The active multi-column sort.
    /// </summary>
    public IReadOnlyList<WaDataGridSortDescriptor>? Sort { get; set; }

    /// <summary>
    /// The active column filters.
    /// </summary>
    public IReadOnlyList<WaDataGridFilterDescriptor>? Filters { get; set; }

    /// <summary>
    /// The active global search term.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// The <c>rowKey</c> values of the currently selected rows.
    /// </summary>
    public IReadOnlyList<object>? SelectedKeys { get; set; }

    /// <summary>
    /// The <c>rowKey</c> values of the currently expanded rows.
    /// </summary>
    public IReadOnlyList<object>? ExpandedKeys { get; set; }

    /// <summary>
    /// The current page index (0-based).
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// The current page size.
    /// </summary>
    public int? PageSize { get; set; }
}

/// <summary>
/// The column ids pinned to each edge of the data grid.
/// </summary>
public class WaDataGridColumnPinning
{
    /// <summary>
    /// The column ids pinned to the left edge.
    /// </summary>
    public IReadOnlyList<string>? Left { get; set; }

    /// <summary>
    /// The column ids pinned to the right edge.
    /// </summary>
    public IReadOnlyList<string>? Right { get; set; }
}

#endregion
