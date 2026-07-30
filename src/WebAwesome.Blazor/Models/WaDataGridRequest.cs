using System.Collections.Generic;

namespace WebAwesome.Blazor.Models;

#region ------ Data Grid Request Models ------

/// <summary>
/// A single column's sort direction, as reported by the data grid's <c>sort</c> state and the
/// wa-sort-change / wa-data-request / wa-data-error events.
/// </summary>
public class WaDataGridSortDescriptor
{
    /// <summary>
    /// The sorted column's id.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Whether the column is sorted descending.
    /// </summary>
    public bool Desc { get; set; }
}

/// <summary>
/// A single column's active filter value, as reported by the data grid's <c>filters</c> state and the
/// wa-filter-change / wa-data-request / wa-data-error events.
/// </summary>
public class WaDataGridFilterDescriptor
{
    /// <summary>
    /// The filtered column's id.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The active filter value; its shape depends on the column's <c>filterType</c>.
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// A snapshot of the server request that failed, carried by the wa-data-error event (without its abort
/// signal, which cannot be marshaled into Blazor).
/// </summary>
public class WaDataGridRequestSnapshot
{
    /// <summary>
    /// The sort that was in effect for the failed request.
    /// </summary>
    public IReadOnlyList<WaDataGridSortDescriptor>? Sort { get; set; }

    /// <summary>
    /// The column filters that were in effect for the failed request.
    /// </summary>
    public IReadOnlyList<WaDataGridFilterDescriptor>? Filters { get; set; }

    /// <summary>
    /// The global search term that was in effect for the failed request.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// The requested page index (0-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The requested page size.
    /// </summary>
    public int PageSize { get; set; }
}

#endregion
