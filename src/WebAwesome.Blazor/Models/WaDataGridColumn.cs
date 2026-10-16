using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Models;

/// <summary>
/// A single column definition for <c>wa-data-grid</c>, serialized to the underlying element's
/// <c>columns</c> JavaScript property.
/// </summary>
/// <remarks>
/// Covers every member of Web Awesome's <c>DataGridColumn</c> interface. The function-valued members take a
/// <see cref="WaJsFunction"/> reference, which the browser resolves into the function before the grid receives the
/// column: <see cref="Formatter"/>, <see cref="AggregatedFormatter"/>, <see cref="ComparatorFunction"/>,
/// <see cref="ValueFunction"/> and <see cref="FilterFunction"/>, and the function forms <see cref="FooterFunction"/>,
/// <see cref="AggregationFunction"/> and <see cref="CellClassFunction"/> of <see cref="Footer"/>,
/// <see cref="Aggregation"/> and <see cref="CellClass"/>. Set either the static or the function form of one member:
/// <see cref="WaDataGrid"/> throws <see cref="InvalidOperationException"/> when both are set.
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
    /// Static footer text. A visible column with a footer adds a pinned footer row. For content computed from the rows,
    /// use <see cref="FooterFunction"/>; set one of the two.
    /// </summary>
    [JsonIgnore]
    public string? Footer { get; set; }

    /// <summary>
    /// A built-in aggregation applied on grouped rows. For a custom aggregation, use <see cref="AggregationFunction"/>;
    /// set one of the two.
    /// </summary>
    [JsonIgnore]
    public WaDataGridAggregation? Aggregation { get; set; }

    /// <summary>
    /// A static CSS class (or space-separated classes) applied to every cell in the column. For a class per cell, use
    /// <see cref="CellClassFunction"/>; set one of the two.
    /// </summary>
    [JsonIgnore]
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

    /// <summary>
    /// A JavaScript function that computes the cell value from the row, the column's <c>value</c>. Its signature is
    /// <c>(row, ctx) =&gt; unknown</c>. Unlike <see cref="Formatter"/>, which only changes what the cell shows, the
    /// computed value drives sort, filter, search, copy and the CSV export. It takes precedence over
    /// <see cref="Field"/>, so give the column an <see cref="Id"/>. <c>ctx</c> holds the members of
    /// <see cref="Formatter"/>. Null reads the value from <see cref="Field"/>.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaJsFunction? ValueFunction { get; set; }

    /// <summary>
    /// A JavaScript predicate for the column filter, the column's <c>filterFn</c>. Its signature is
    /// <c>(value, filterValue, row, ctx) =&gt; boolean</c>: the cell value, the active filter value and the row; true
    /// keeps the row. It applies only when <see cref="Filterable"/> is true, takes precedence over
    /// <see cref="FilterType"/>, and runs on the client only. <c>ctx</c> holds the members of <see cref="Formatter"/>.
    /// Null uses the filter of <see cref="FilterType"/>.
    /// </summary>
    [JsonPropertyName("filterFn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WaJsFunction? FilterFunction { get; set; }

    /// <summary>
    /// A JavaScript function that renders the footer cell of the column, the function form of the column's
    /// <c>footer</c>. Its signature is <c>(rows, ctx) =&gt; string | Node | TemplateResult</c>: the filtered rows across
    /// every page (the top-level rows of tree data, the data rows of a grouped grid, the loaded page in server mode). The
    /// footer renders in the grid's shadow root: declare its Web Awesome elements with <see cref="WaJsFunction.Uses"/>,
    /// and style its classes with <see cref="WaDataGrid.ShadowStyles"/>. <c>ctx</c> holds the members of
    /// <see cref="Formatter"/> except <c>rowKey</c>. Set this or <see cref="Footer"/>, not both: the grid throws
    /// <see cref="InvalidOperationException"/>.
    /// </summary>
    [JsonIgnore]
    public WaJsFunction? FooterFunction { get; set; }

    /// <summary>
    /// A JavaScript function that aggregates the column on group rows (with the grid's <c>GroupBy</c>), the function form
    /// of the column's <c>aggregation</c>. Its signature is <c>(values, rows, ctx) =&gt; unknown</c>: the values and the
    /// data rows of the group. The aggregate renders through <see cref="AggregatedFormatter"/>, else through
    /// <see cref="Formatter"/>. <c>ctx</c> holds the members of <see cref="Formatter"/> except <c>rowKey</c>. Set this or
    /// <see cref="Aggregation"/>, not both: the grid throws <see cref="InvalidOperationException"/>.
    /// </summary>
    [JsonIgnore]
    public WaJsFunction? AggregationFunction { get; set; }

    /// <summary>
    /// A JavaScript function that returns the CSS class (or space-separated classes) of one cell, the function form of
    /// the column's <c>cellClass</c>. Its signature is <c>(value, row, ctx) =&gt; string</c>. The cells are in the
    /// grid's shadow root, so style the classes with <see cref="WaDataGrid.ShadowStyles"/>. <c>ctx</c> holds the members
    /// of <see cref="Formatter"/>. Set this or <see cref="CellClass"/>, not both: the grid throws
    /// <see cref="InvalidOperationException"/>.
    /// </summary>
    [JsonIgnore]
    public WaJsFunction? CellClassFunction { get; set; }

    #region ------ Internals ------

    /// <summary>
    /// Throws when the static and the function form of one upstream member are both set, because both write the same
    /// member of the column.
    /// </summary>
    /// <param name="columnIndex">The position of the column in the list, for the message</param>
    /// <exception cref="InvalidOperationException">Thrown when both forms of one member are set</exception>
    internal void ThrowIfBothFormsAreSet(int columnIndex)
    {
        ThrowIfBoth(columnIndex, Footer is not null, nameof(Footer), FooterFunction is not null, nameof(FooterFunction));
        ThrowIfBoth(columnIndex, Aggregation is not null, nameof(Aggregation), AggregationFunction is not null, nameof(AggregationFunction));
        ThrowIfBoth(columnIndex, CellClass is not null, nameof(CellClass), CellClassFunction is not null, nameof(CellClassFunction));
    }

    private void ThrowIfBoth(int columnIndex, bool staticSet, string staticName, bool functionSet, string functionName)
    {
        if (!staticSet || !functionSet) return;

        var column = Id ?? Field ?? $"#{columnIndex}";
        throw new InvalidOperationException(
            $"{nameof(WaDataGridColumn)} \"{column}\": set either {staticName} or {functionName}, not both; they write the same column member.");
    }

    // the upstream footer: the static text or the function (one JSON key for two members)
    [JsonInclude]
    [JsonPropertyName("footer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private object? FooterValue => (object?)FooterFunction ?? Footer;

    // the upstream aggregation: the built-in name or the function
    [JsonInclude]
    [JsonPropertyName("aggregation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private object? AggregationValue => (object?)AggregationFunction ?? Aggregation;

    // the upstream cellClass: the static classes or the function
    [JsonInclude]
    [JsonPropertyName("cellClass")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private object? CellClassValue => (object?)CellClassFunction ?? CellClass;

    #endregion
}
