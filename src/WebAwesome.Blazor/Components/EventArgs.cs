using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Components;

#region ------ Carousel Events ------

/// <summary>
/// Event arguments for carousel slide change events
/// </summary>
public class WaSlideChangeEventArgs : EventArgs
{
    /// <summary>
    /// Zero-based index of the active slide
    /// </summary>
    /// <remarks>
    /// The wa-slide-change event's detail also carries the slide element itself; DOM elements
    /// cannot be marshaled into Blazor <see cref="ElementReference"/>s from event payloads,
    /// so only the index is exposed.
    /// </remarks>
    public int Index { get; set; }
}

#endregion

#region ------ Tab Events ------

/// <summary>
/// Event arguments for tab change events
/// </summary>
public class WaTabChangeEventArgs : EventArgs
{
    /// <summary>
    /// Name of the tab panel that was shown or hidden
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

#endregion

#region ------ Rating Events ------

/// <summary>
/// Event arguments for rating hover events
/// </summary>
public class WaRatingHoverEventArgs : EventArgs
{
    /// <summary>
    /// Hover phase: 'start', 'move', or 'end'
    /// </summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// The potential rating value during hover
    /// </summary>
    public decimal Value { get; set; }
}

#endregion

#region ------ Details Events ------

/// <summary>
/// Event arguments for details toggle events
/// </summary>
public class WaDetailsToggleEventArgs : EventArgs
{
    /// <summary>
    /// Whether the details element is open after the toggle.
    /// </summary>
    public bool IsOpen { get; set; }
}

#endregion

#region ------ Split Panel Events ------

/// <summary>
/// Event arguments for split panel reposition events
/// </summary>
public class WaSplitPanelRepositionEventArgs : EventArgs
{
    /// <summary>
    /// The new position of the divider from the primary panel's edge, as a percentage between 0 and 100.
    /// </summary>
    public decimal Position { get; set; }

    /// <summary>
    /// The new position of the divider from the primary panel's edge, in pixels.
    /// </summary>
    public int PositionInPixels { get; set; }
}

#endregion

#region ------ Pagination Events ------

/// <summary>
/// Event arguments for pagination page-change events, shared by <c>wa-before-page-change</c> and
/// <c>wa-page-change</c>.
/// </summary>
public class WaPaginationPageChangeEventArgs : EventArgs
{
    /// <summary>
    /// The page that becomes (or, for the "before" event, will become) the active page.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The current page size.
    /// </summary>
    public int PageSize { get; set; }
}

#endregion

#region ------ Observer Events ------

/// <summary>
/// Event arguments for mutation events
/// </summary>
public class MutationEventArgs : EventArgs
{
    /// <summary>
    /// Array of MutationRecord objects describing the mutations
    /// </summary>
    public object[]? MutationRecords { get; set; }
}

/// <summary>
/// Event arguments for resize events
/// </summary>
public class ResizeEventArgs : EventArgs
{
    /// <summary>
    /// Array of ResizeObserverEntry objects describing the size changes
    /// </summary>
    public object[]? ResizeObserverEntries { get; set; }
}

#endregion

#region ------ Utility Events ------

/// <summary>
/// Event arguments for include error events
/// </summary>
public class IncludeErrorEventArgs : EventArgs
{
    /// <summary>
    /// HTTP status code of the failed request (e.g., 404)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Error message describing the failure
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Event arguments for zoom change events
/// </summary>
public class ZoomChangeEventArgs : EventArgs
{
    /// <summary>
    /// The new zoom level (1.0 = 100%)
    /// </summary>
    public double ZoomLevel { get; set; }

    /// <summary>
    /// The previous zoom level
    /// </summary>
    public double PreviousZoomLevel { get; set; }
}

#endregion

#region ------ Tree Events ------

/// <summary>
/// Event arguments for tree selection change events
/// </summary>
/// <remarks>
/// <see cref="Selection"/> is left as raw deserialized objects (one per selected <c>&lt;wa-tree-item&gt;</c>),
/// mirroring <see cref="MutationEventArgs.MutationRecords"/> and <see cref="ResizeEventArgs.ResizeObserverEntries"/>:
/// there is no supported way to marshal arbitrary DOM elements from a custom event's <c>detail</c> payload into
/// live <see cref="ElementReference"/>s (those are only produced by Blazor itself, via <c>@ref</c>/element
/// reference capture). Consumers needing to act on specific items should track selection via each
/// <c>WaTreeItem</c>'s own <c>Selected</c> parameter/<c>OnSelectedChange</c>-style wiring instead.
/// </remarks>
public class WaTreeSelectionChangeEventArgs : EventArgs
{
    /// <summary>
    /// Selection data projected from the wa-selection-change event's detail, one entry per
    /// selected tree item; each entry carries the item's id and trimmed text content.
    /// </summary>
    public object[]? Selection { get; set; }
}

#endregion

#region ------ Intersection Observer Events ------

/// <summary>
/// Event arguments for intersection observer events
/// </summary>
public class WaIntersectionEventArgs : EventArgs
{
    /// <summary>
    /// Whether the target element is intersecting with the root
    /// </summary>
    public bool IsIntersecting { get; set; }

    /// <summary>
    /// The ratio of intersection between 0.0 and 1.0
    /// </summary>
    /// <remarks>
    /// The wa-intersect event's detail carries the full IntersectionObserverEntry, whose
    /// target is a DOM element; DOM elements cannot be marshaled into Blazor
    /// <see cref="ElementReference"/>s from event payloads, so only the scalar fields are exposed.
    /// </remarks>
    public double IntersectionRatio { get; set; }
}

#endregion

#region ------ Combobox Events ------

/// <summary>
/// Event arguments for the combobox create event
/// </summary>
public class WaCreateEventArgs : EventArgs
{
    /// <summary>
    /// The text the user typed that will be used to create a new option.
    /// </summary>
    public string InputValue { get; set; } = string.Empty;
}

#endregion

#region ------ Date Picker Events ------

/// <summary>
/// Event arguments for the date picker's focused-day change event.
/// </summary>
public class WaDatePickerFocusDayEventArgs : EventArgs
{
    /// <summary>
    /// The newly focused day as an ISO date string (<c>YYYY-MM-DD</c>).
    /// </summary>
    /// <remarks>
    /// The wa-focus-day event's detail carries a live JavaScript <c>Date</c>; it is projected to an
    /// ISO date string by the interop module, since <c>Date</c> objects do not marshal into Blazor.
    /// </remarks>
    public string? Date { get; set; }
}

/// <summary>
/// Event arguments for the date picker's view change event.
/// </summary>
public class WaDatePickerViewChangeEventArgs : EventArgs
{
    /// <summary>
    /// The view the picker switched to: <c>days</c>, <c>months</c>, or <c>years</c>.
    /// </summary>
    public string? View { get; set; }

    /// <summary>
    /// The anchor date of the new view as an ISO date string (<c>YYYY-MM-DD</c>).
    /// </summary>
    public string? Date { get; set; }
}

#endregion

#region ------ Video Playlist Events ------

/// <summary>
/// Event arguments for the video playlist active-video change event.
/// </summary>
public class WaVideoChangeEventArgs : EventArgs
{
    /// <summary>
    /// Zero-based index of the previously active video.
    /// </summary>
    public int PreviousIndex { get; set; }

    /// <summary>
    /// Zero-based index of the newly active video.
    /// </summary>
    public int CurrentIndex { get; set; }

    /// <summary>
    /// Title of the incoming video.
    /// </summary>
    /// <remarks>
    /// The wa-video-change event's detail also carries the incoming video element itself; DOM
    /// elements cannot be marshaled into Blazor from event payloads, so only its title is exposed.
    /// </remarks>
    public string? VideoTitle { get; set; }
}

#endregion

#region ------ Random Content Events ------

/// <summary>
/// Event arguments for the random-content content-change event, raised whenever the displayed
/// selection changes (including on first render, on RandomizeAsync, and on each autoplay tick).
/// </summary>
/// <remarks>
/// The wa-content-change event's detail carries the live child elements now shown; DOM elements
/// cannot be marshaled into Blazor <see cref="ElementReference"/>s from event payloads, so
/// <see cref="Items"/> is left as raw deserialized objects (each projected to its id and trimmed
/// text content), mirroring <see cref="WaTreeSelectionChangeEventArgs.Selection"/>, and
/// <see cref="Count"/> exposes how many children are now shown.
/// </remarks>
public class WaContentChangeEventArgs : EventArgs
{
    /// <summary>
    /// Number of children currently shown.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Selection data projected from the wa-content-change event's detail, one entry per shown
    /// child; each entry carries the element's id and trimmed text content.
    /// </summary>
    public object[]? Items { get; set; }
}

#endregion

#region ------ Data Grid Events ------

/// <summary>
/// Event arguments for the data grid's cell-click event.
/// </summary>
public class WaDataGridCellClickEventArgs : EventArgs
{
    /// <summary>
    /// The id of the clicked cell's column.
    /// </summary>
    public string Column { get; set; } = string.Empty;

    /// <summary>
    /// The cell's accessor value.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The row object the cell belongs to.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Row { get; set; }

    /// <summary>
    /// The row's display index (post sort, filter, and pagination).
    /// </summary>
    public int RowIndex { get; set; }
}

/// <summary>
/// Event arguments for the data grid's cell-contextmenu event.
/// </summary>
/// <remarks>
/// The wa-cell-contextmenu event's detail also carries the originating <c>PointerEvent</c> or
/// <c>KeyboardEvent</c> (<c>originalEvent</c>); DOM events cannot be marshaled into Blazor from custom
/// event payloads, so it is omitted here. Upstream documents the event as cancelable (calling
/// <c>event.preventDefault()</c> suppresses the browser's native context menu); Blazor dispatches
/// custom registered events asynchronously, so a .NET <see cref="EventCallback{TValue}"/> handler cannot
/// synchronously veto the native menu the way a same-thread JavaScript listener can. Consumers who need
/// to suppress the native menu should do so with their own small, targeted JS interop.
/// </remarks>
public class WaDataGridCellContextMenuEventArgs : EventArgs
{
    /// <summary>
    /// The id of the cell's column.
    /// </summary>
    public string Column { get; set; } = string.Empty;

    /// <summary>
    /// The cell's accessor value.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The row object the cell belongs to.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Row { get; set; }

    /// <summary>
    /// The row's display index (post sort, filter, and pagination).
    /// </summary>
    public int RowIndex { get; set; }
}

/// <summary>
/// Event arguments for the data grid's column-move event.
/// </summary>
public class WaDataGridColumnMoveEventArgs : EventArgs
{
    /// <summary>
    /// The id of the column being moved.
    /// </summary>
    public string Column { get; set; } = string.Empty;

    /// <summary>
    /// The column's new index in the display order.
    /// </summary>
    public int ToIndex { get; set; }

    /// <summary>
    /// The full new column order, as an array of column ids.
    /// </summary>
    public IReadOnlyList<string>? ColumnOrder { get; set; }

    /// <summary>
    /// False during a live drag; true once the drag (or keyboard move) settles.
    /// </summary>
    public bool Finished { get; set; }
}

/// <summary>
/// Event arguments for the data grid's column-pin event.
/// </summary>
public class WaDataGridColumnPinEventArgs : EventArgs
{
    /// <summary>
    /// The id of the column whose pin state changed.
    /// </summary>
    public string Column { get; set; } = string.Empty;

    /// <summary>
    /// The side the column is now pinned to (<c>left</c> or <c>right</c>), or null when unpinned.
    /// </summary>
    public string? Side { get; set; }
}

/// <summary>
/// Event arguments for the data grid's column-resize event.
/// </summary>
public class WaDataGridColumnResizeEventArgs : EventArgs
{
    /// <summary>
    /// The id of the column being resized.
    /// </summary>
    public string Column { get; set; } = string.Empty;

    /// <summary>
    /// The column's new width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// False during a live drag; true once the drag (or keyboard resize) settles.
    /// </summary>
    public bool Finished { get; set; }
}

/// <summary>
/// Event arguments for the data grid's column-visibility-change event.
/// </summary>
public class WaDataGridColumnVisibilityChangeEventArgs : EventArgs
{
    /// <summary>
    /// The id of the column whose visibility changed.
    /// </summary>
    public string Column { get; set; } = string.Empty;

    /// <summary>
    /// Whether the column is now visible.
    /// </summary>
    public bool Visible { get; set; }
}

/// <summary>
/// Event arguments for the data grid's data-error event, emitted in server mode when a
/// <c>dataSource</c> request rejects.
/// </summary>
public class WaDataGridDataErrorEventArgs : EventArgs
{
    /// <summary>
    /// The error message the data source rejected with.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// The request that failed (without its abort signal).
    /// </summary>
    public WaDataGridRequestSnapshot? Request { get; set; }
}

/// <summary>
/// Event arguments for the data grid's data-request event, emitted in server mode when the grid
/// needs data for the current sort, filters, and page.
/// </summary>
/// <remarks>
/// The wa-data-request event's detail also carries an <c>AbortSignal</c>; abort signals cannot be
/// marshaled into Blazor from custom event payloads, so it is omitted here.
/// </remarks>
public class WaDataGridDataRequestEventArgs : EventArgs
{
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
    /// The requested page index (0-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The requested page size.
    /// </summary>
    public int PageSize { get; set; }
}

/// <summary>
/// Event arguments for the data grid's filter-change event.
/// </summary>
public class WaDataGridFilterChangeEventArgs : EventArgs
{
    /// <summary>
    /// The active global search term.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// The active column filters.
    /// </summary>
    public IReadOnlyList<WaDataGridFilterDescriptor>? Filters { get; set; }
}

/// <summary>
/// Event arguments for the data grid's page-change event.
/// </summary>
public class WaDataGridPageChangeEventArgs : EventArgs
{
    /// <summary>
    /// The current page index (0-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The current page size.
    /// </summary>
    public int PageSize { get; set; }
}

/// <summary>
/// Event arguments for the data grid's row-expand and row-collapse events.
/// </summary>
public class WaDataGridRowEventArgs : EventArgs
{
    /// <summary>
    /// The row that expanded or collapsed.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Row { get; set; }
}

/// <summary>
/// Event arguments for the data grid's row-select event.
/// </summary>
public class WaDataGridRowSelectEventArgs : EventArgs
{
    /// <summary>
    /// The <c>rowKey</c> values of the currently selected rows.
    /// </summary>
    public IReadOnlyList<object>? SelectedKeys { get; set; }

    /// <summary>
    /// The selected row objects.
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object>>? SelectedRows { get; set; }
}

/// <summary>
/// Event arguments for the data grid's sort-change event.
/// </summary>
public class WaDataGridSortChangeEventArgs : EventArgs
{
    /// <summary>
    /// The active multi-column sort.
    /// </summary>
    public IReadOnlyList<WaDataGridSortDescriptor>? Sort { get; set; }
}

#endregion
