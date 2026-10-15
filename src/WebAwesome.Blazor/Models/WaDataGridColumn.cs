using System.Collections.Generic;
using System.Text.Json.Serialization;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Models;

/// <summary>
/// A single column definition for <c>wa-data-grid</c>, serialized to the underlying element's
/// <c>columns</c> JavaScript property.
/// </summary>
/// <remarks>
/// Covers the JSON-expressible members of Web Awesome's <c>DataGridColumn</c> interface, and the function-valued
/// members <see cref="Formatter"/>, <see cref="AggregatedFormatter"/> and <see cref="ComparatorFunction"/> as
/// <see cref="WaJsFunction"/> references, which the browser resolves into the functions. Not yet exposed: <c>value</c>
/// (a computed cell-value accessor), <c>filterFn</c> (a custom predicate), and the function forms of
/// <see cref="Footer"/>, <see cref="Aggregation"/>, and <see cref="CellClass"/> (only their string forms are
/// exposed).
/// </remarks>
public class WaDataGridColumn
{
    /// <summary>
    /// Dot-path accessor into the row object, e.g. <c>user.name</c>. Doubles as the column id when
    /// <see cref="Id"/> is omitted.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Field { get; set; }

    /// <summary>
    /// Explicit column id. Required for columns without a <see cref="Field"/> (e.g. an actions or
    /// computed column).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    /// <summary>
    /// Header text.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>
    /// Horizontal alignment of the cell content. Null leaves the element's default, start.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaDataGridAlign? Align { get; set; }

    /// <summary>
    /// Horizontal alignment of the header content. Defaults
    /// to <see cref="Align"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaDataGridAlign? HeaderAlign { get; set; }

    /// <summary>
    /// Whether the column can be sorted. Defaults to true for columns with a <see cref="Field"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Sortable { get; set; }

    /// <summary>
    /// When true, this column's first sort click sorts descending (defaults to the grid-level
    /// <c>sort-desc-first</c>).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SortDescFirst { get; set; }

    /// <summary>
    /// The built-in comparison strategy used when sorting this column; null leaves the element's default,
    /// <see cref="WaDataGridSortFn.Alphanumeric"/>. Ignored when <see cref="ComparatorFunction"/> is set.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaDataGridSortFn? SortFn { get; set; }

    /// <summary>
    /// Where null and undefined values sort (<see cref="WaDataGridSortUndefined"/>). Omit to leave them in place,
    /// which is the element's default.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaDataGridSortUndefined? SortUndefined { get; set; }

    /// <summary>
    /// Whether the global search box matches this column. Defaults to true for columns with a
    /// <see cref="Field"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Searchable { get; set; }

    /// <summary>
    /// Whether the column shows a per-column filter input.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Filterable { get; set; }

    /// <summary>
    /// How the column's filter matches; null leaves the element's default, <see cref="WaDataGridFilterType.Text"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaDataGridFilterType? FilterType { get; set; }

    /// <summary>
    /// The options shown in a <see cref="WaDataGridFilterType.Set"/>, <see cref="WaDataGridFilterType.IncludesAny"/>
    /// or <see cref="WaDataGridFilterType.IncludesAll"/> filter picker, replacing the facets the grid computes from its
    /// rows. Each option filters by its value, displays its label (defaults to the value), and shows its count next to
    /// it when provided. Options render in the order given. Required for a value picker in server mode, where the grid
    /// only holds one page and can't derive the distinct values itself; also useful client-side to control ordering or
    /// include values no loaded row has. Null leaves the computed facets.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<WaDataGridFilterOption>? FilterOptions { get; set; }

    /// <summary>
    /// Whether the column starts hidden.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Hidden { get; set; }

    /// <summary>
    /// Whether the user can toggle the column's visibility in the columns menu. Defaults to true.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Hideable { get; set; }

    /// <summary>
    /// Whether the column can be resized. Overrides the grid-level <c>resizable</c> setting.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Resizable { get; set; }

    /// <summary>
    /// Whether the column can be drag-reordered. Defaults to the grid-level <c>reorderable</c> setting.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Movable { get; set; }

    /// <summary>
    /// Whether the column can be pinned to an edge. Defaults to the grid-level <c>pinnable</c> setting.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Pinnable { get; set; }

    /// <summary>
    /// Pins the column to an edge initially. The user can still unpin it
    /// (when <see cref="Pinnable"/>); use the grid's <c>PinColumnAsync</c> method for programmatic control.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaDataGridPinSide? Pinned { get; set; }

    /// <summary>
    /// Initial column width in pixels. Ignored when <see cref="Flex"/> is set.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }

    /// <summary>
    /// Minimum column width in pixels (resize and flex clamp).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MinWidth { get; set; }

    /// <summary>
    /// Maximum column width in pixels (resize and flex clamp).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Flex-grow ratio. When set, the column shares leftover horizontal space proportionally with other
    /// flex columns (respecting <see cref="MinWidth"/>/<see cref="MaxWidth"/>) instead of using a fixed
    /// <see cref="Width"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Flex { get; set; }

    /// <summary>
    /// Static footer text. The function form (aggregating the filtered rows across every page) is not
    /// marshalable and is not supported.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Footer { get; set; }

    /// <summary>
    /// A built-in aggregation applied on grouped rows. The custom function form is not marshalable and is not
    /// supported.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaDataGridAggregation? Aggregation { get; set; }

    /// <summary>
    /// A static CSS class (or space-separated classes) applied to every cell in the column. The
    /// per-cell function form is not marshalable and is not supported.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CellClass { get; set; }

    /// <summary>
    /// A JavaScript function that renders the cell content, the column's <c>formatter</c>. Its signature is
    /// <c>(value, row, ctx) =&gt; string | Node | TemplateResult</c>: a string renders as escaped text, a <c>Node</c> or
    /// a Lit <c>TemplateResult</c> as rich content. It runs on every render of the cell, so a new <c>Node</c> replaces
    /// the previous one; return the same <c>Node</c> to keep its DOM state. It changes only what the cell shows: sort,
    /// filter and search use the raw value, and CSV export and copy use its output only when it is a string. Declare
    /// the Web Awesome elements it renders with <see cref="WaJsFunction.Uses"/>. <c>ctx</c> holds <c>args</c>,
    /// <c>element</c> and <c>grid</c> (the grid element), <c>column</c> (the column id) and <c>rowKey</c> (the row's
    /// value of the grid's <c>RowKey</c> field). Null renders the raw value.
    /// </summary>
    [JsonPropertyName("formatter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaJsFunction? Formatter { get; set; }

    /// <summary>
    /// A JavaScript function that renders the aggregated cell of a group row (with the grid's <c>GroupBy</c> and the
    /// column's <see cref="Aggregation"/>), the column's <c>aggregatedFormatter</c>. Its signature is
    /// <c>(value, rows, ctx) =&gt; string | Node | TemplateResult</c>: the aggregate value and the group's data rows.
    /// <c>ctx</c> holds the members of <see cref="Formatter"/> except <c>rowKey</c>. Null renders the aggregate through
    /// <see cref="Formatter"/> (with the group's first row), or as text.
    /// </summary>
    [JsonPropertyName("aggregatedFormatter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaJsFunction? AggregatedFormatter { get; set; }

    /// <summary>
    /// A JavaScript function that compares two cell values for client-side sorting, the column's <c>comparator</c>. Its
    /// signature is <c>(a, b, rowA, rowB, ctx) =&gt; number</c>: a negative, zero or positive number for ascending
    /// order (the grid applies the descending direction itself). It takes precedence over <see cref="SortFn"/>.
    /// <c>ctx</c> holds the members of <see cref="Formatter"/> except <c>rowKey</c>. Null uses the built-in sort.
    /// </summary>
    [JsonPropertyName("comparator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaJsFunction? ComparatorFunction { get; set; }
}
