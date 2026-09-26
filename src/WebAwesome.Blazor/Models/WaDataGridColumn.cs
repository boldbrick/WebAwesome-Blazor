using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Models;

/// <summary>
/// A single column definition for <c>wa-data-grid</c>, serialized to the underlying element's
/// <c>columns</c> JavaScript property.
/// </summary>
/// <remarks>
/// Covers the JSON-expressible members of Web Awesome's <c>DataGridColumn</c> interface. Function-valued
/// members cannot cross the JS interop boundary and are intentionally omitted: <c>value</c> (a computed
/// cell-value accessor), <c>filterFn</c> and <c>comparator</c> (custom predicates), <c>formatter</c> and
/// <c>aggregatedFormatter</c> (Lit template or DOM cell renderers), and the function forms of
/// <see cref="Footer"/>, <see cref="Aggregation"/>, and <see cref="CellClass"/> (only their string forms
/// are exposed).
/// </remarks>
public class WaDataGridColumn
{
    /// <summary>
    /// Dot-path accessor into the row object, e.g. <c>user.name</c>. Doubles as the column id when
    /// <see cref="Id"/> is omitted.
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    /// Explicit column id. Required for columns without a <see cref="Field"/> (e.g. an actions or
    /// computed column).
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Header text.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Horizontal alignment of the cell content. Null leaves the element's default, start.
    /// </summary>
    public WaDataGridAlign? Align { get; set; }

    /// <summary>
    /// Horizontal alignment of the header content. Defaults
    /// to <see cref="Align"/>.
    /// </summary>
    public WaDataGridAlign? HeaderAlign { get; set; }

    /// <summary>
    /// Whether the column can be sorted. Defaults to true for columns with a <see cref="Field"/>.
    /// </summary>
    public bool? Sortable { get; set; }

    /// <summary>
    /// When true, this column's first sort click sorts descending (defaults to the grid-level
    /// <c>sort-desc-first</c>).
    /// </summary>
    public bool? SortDescFirst { get; set; }

    /// <summary>
    /// The built-in comparison strategy used when sorting this column; null leaves the element's default,
    /// <see cref="WaDataGridSortFn.Alphanumeric"/>. Ignored when a custom comparator is provided, which the wrapper
    /// does not support.
    /// </summary>
    public WaDataGridSortFn? SortFn { get; set; }

    /// <summary>
    /// Where null and undefined values sort (<see cref="WaDataGridSortUndefined"/>). Omit to leave them in place,
    /// which is the element's default.
    /// </summary>
    public WaDataGridSortUndefined? SortUndefined { get; set; }

    /// <summary>
    /// Whether the global search box matches this column. Defaults to true for columns with a
    /// <see cref="Field"/>.
    /// </summary>
    public bool? Searchable { get; set; }

    /// <summary>
    /// Whether the column shows a per-column filter input.
    /// </summary>
    public bool? Filterable { get; set; }

    /// <summary>
    /// How the column's filter matches; null leaves the element's default, <see cref="WaDataGridFilterType.Text"/>.
    /// </summary>
    public WaDataGridFilterType? FilterType { get; set; }

    /// <summary>
    /// Whether the column starts hidden.
    /// </summary>
    public bool? Hidden { get; set; }

    /// <summary>
    /// Whether the user can toggle the column's visibility in the columns menu. Defaults to true.
    /// </summary>
    public bool? Hideable { get; set; }

    /// <summary>
    /// Whether the column can be resized. Overrides the grid-level <c>resizable</c> setting.
    /// </summary>
    public bool? Resizable { get; set; }

    /// <summary>
    /// Whether the column can be drag-reordered. Defaults to the grid-level <c>reorderable</c> setting.
    /// </summary>
    public bool? Movable { get; set; }

    /// <summary>
    /// Whether the column can be pinned to an edge. Defaults to the grid-level <c>pinnable</c> setting.
    /// </summary>
    public bool? Pinnable { get; set; }

    /// <summary>
    /// Pins the column to an edge initially. The user can still unpin it
    /// (when <see cref="Pinnable"/>); use the grid's <c>PinColumnAsync</c> method for programmatic control.
    /// </summary>
    public WaDataGridPinSide? Pinned { get; set; }

    /// <summary>
    /// Initial column width in pixels. Ignored when <see cref="Flex"/> is set.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Minimum column width in pixels (resize and flex clamp).
    /// </summary>
    public int? MinWidth { get; set; }

    /// <summary>
    /// Maximum column width in pixels (resize and flex clamp).
    /// </summary>
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Flex-grow ratio. When set, the column shares leftover horizontal space proportionally with other
    /// flex columns (respecting <see cref="MinWidth"/>/<see cref="MaxWidth"/>) instead of using a fixed
    /// <see cref="Width"/>.
    /// </summary>
    public double? Flex { get; set; }

    /// <summary>
    /// Static footer text. The function form (aggregating the filtered rows across every page) is not
    /// marshalable and is not supported.
    /// </summary>
    public string? Footer { get; set; }

    /// <summary>
    /// A built-in aggregation applied on grouped rows. The custom function form is not marshalable and is not
    /// supported.
    /// </summary>
    public WaDataGridAggregation? Aggregation { get; set; }

    /// <summary>
    /// A static CSS class (or space-separated classes) applied to every cell in the column. The
    /// per-cell function form is not marshalable and is not supported.
    /// </summary>
    public string? CellClass { get; set; }
}
