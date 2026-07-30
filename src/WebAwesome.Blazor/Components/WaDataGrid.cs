using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A data grid that displays tabular data with sorting, selection, filtering, pinning, tree data,
/// grouping with aggregation, column footers, expandable rows, pagination, CSV export, and full
/// keyboard navigation. Corresponds to the wa-data-grid Web Awesome Pro component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Data"/> and <see cref="Columns"/> are JavaScript properties on the underlying element,
/// not HTML attributes, and there is no declarative JSON child form. The wrapper pushes both through
/// <see cref="WebAwesomeJSInterop.SetPropertyAsync"/> on first render and whenever either parameter
/// changes.
/// </para>
/// <para>
/// The upstream <c>dataSource</c> property (an async request-to-response callback) cannot be marshaled
/// across JS interop and is not exposed. Use server mode instead: set <see cref="Server"/>, handle
/// <see cref="OnDataRequest"/> to fetch the current page (its args carry the active sort, filters,
/// search term, page, and page size), then assign <see cref="Data"/>, <see cref="Total"/>, and
/// <see cref="Loading"/> yourself.
/// </para>
/// <para>
/// The upstream <c>child-rows</c> property accepts either a field name or a function that computes a
/// row's children; only the field-name (string) form is exposed as <see cref="ChildRows"/>.
/// </para>
/// <para>
/// Rich per-cell rendering (the column <c>formatter</c>/<c>aggregatedFormatter</c> Lit templates) has no
/// Blazor equivalent and is not exposed by <see cref="WaDataGridColumn"/>; only its JSON-expressible
/// members are supported.
/// </para>
/// </remarks>
public class WaDataGrid : ComponentBase
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used to invoke methods and set properties on the underlying element.
    /// </summary>
    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// The row objects to display. Pushed to the element's <c>data</c> JavaScript property; see the
    /// class remarks for why this isn't an HTML attribute.
    /// </summary>
    [Parameter] public IReadOnlyList<object>? Data { get; set; }

    /// <summary>
    /// The column definitions. Pushed to the element's <c>columns</c> JavaScript property; see the
    /// class remarks for why this isn't an HTML attribute.
    /// </summary>
    [Parameter] public IReadOnlyList<WaDataGridColumn>? Columns { get; set; }

    /// <summary>
    /// An accessible label for the grid.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// The grid's visual appearance.
    /// </summary>
    [Parameter] public WaDataGridAppearance? Appearance { get; set; }

    /// <summary>
    /// The grid's size. Controls the font scale of grid text and form controls, plus row height and
    /// cell padding.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// Renders alternating row background colors.
    /// </summary>
    [Parameter] public bool Striped { get; set; }

    /// <summary>
    /// Shows a global search box that filters across all columns.
    /// </summary>
    [Parameter] public bool WithSearch { get; set; }

    /// <summary>
    /// Shows a per-column header menu (kebab button) with pin, sort, hide, and autosize actions.
    /// </summary>
    [Parameter] public bool WithColumnMenu { get; set; }

    /// <summary>
    /// Shows a toolbar menu for toggling column visibility.
    /// </summary>
    [Parameter] public bool WithColumnsMenu { get; set; }

    /// <summary>
    /// Keeps a sorted column always sorted, alternating between ascending and descending. By default,
    /// a sorted column's third click clears its sort (the ascending, descending, unsorted cycle).
    /// </summary>
    [Parameter] public bool WithoutSortRemoval { get; set; }

    /// <summary>
    /// When true, a column's first sort click sorts descending instead of ascending.
    /// </summary>
    [Parameter] public bool SortDescFirst { get; set; }

    /// <summary>
    /// The maximum number of columns that can participate in a multi-column sort. Zero (the default)
    /// means no limit.
    /// </summary>
    [Parameter] public int? MaxMultiSort { get; set; }

    /// <summary>
    /// Enables row selection. A bare attribute (or <see cref="WaDataGridSelectable.Multiple"/>) means
    /// multiple selection.
    /// </summary>
    [Parameter] public WaDataGridSelectable? Selectable { get; set; }

    /// <summary>
    /// The field used as a stable row id for selection. Required in practice when <see cref="Selectable"/>
    /// is set.
    /// </summary>
    [Parameter] public string? RowKey { get; set; }

    /// <summary>
    /// Enables client-side pagination and the pager footer.
    /// </summary>
    [Parameter] public bool Paginate { get; set; }

    /// <summary>
    /// The current page index (0-based).
    /// </summary>
    [Parameter] public int? Page { get; set; }

    /// <summary>
    /// The number of rows per page.
    /// </summary>
    [Parameter] public int? PageSize { get; set; }

    /// <summary>
    /// Switches the grid to server mode: client-side sorting, filtering, and pagination are disabled
    /// and the grid emits <see cref="OnDataRequest"/> whenever it needs data. Fetch in your handler,
    /// then set <see cref="Data"/>, <see cref="Total"/>, and <see cref="Loading"/>.
    /// </summary>
    [Parameter] public bool Server { get; set; }

    /// <summary>
    /// How long (in milliseconds) to wait after a search or filter keystroke before requesting data in
    /// server mode. Client-side filtering is always immediate; sort and page changes are never
    /// debounced.
    /// </summary>
    [Parameter] public int? FilterDebounce { get; set; }

    /// <summary>
    /// When filtering tree data, keeps a parent visible when any descendant matches (the filter runs
    /// leaf-up). By default a non-matching parent is removed with its entire subtree.
    /// </summary>
    [Parameter] public bool FilterFromLeafRows { get; set; }

    /// <summary>
    /// Groups rows by column id: a single id, or a space/comma-separated list for multi-level grouping.
    /// Ignored for tree data and in server mode.
    /// </summary>
    [Parameter] public string? GroupBy { get; set; }

    /// <summary>
    /// Provides each row's child rows for tree data, as a field name (dot paths allowed). Rows with
    /// children get an expand toggle; expanded children render indented and join sorting, filtering,
    /// and selection.
    /// </summary>
    /// <remarks>
    /// Upstream also accepts a function form (<c>(row) =&gt; Row[]</c>); only the field-name string form
    /// can be authored declaratively and is exposed here.
    /// </remarks>
    [Parameter] public string? ChildRows { get; set; }

    /// <summary>
    /// Enables drag-to-resize for columns (can be overridden per column).
    /// </summary>
    [Parameter] public bool Resizable { get; set; }

    /// <summary>
    /// Enables drag-to-reorder for columns (can be overridden per column with <c>movable</c>).
    /// </summary>
    [Parameter] public bool Reorderable { get; set; }

    /// <summary>
    /// Enables column pinning (and the pin actions in the column menu). Can be overridden per column
    /// with <c>pinnable</c>.
    /// </summary>
    [Parameter] public bool Pinnable { get; set; }

    /// <summary>
    /// The total row count in server mode. Drives the pager.
    /// </summary>
    [Parameter] public int? Total { get; set; }

    /// <summary>
    /// Whether a server-mode request is in flight.
    /// </summary>
    [Parameter] public bool Loading { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when a data cell is clicked, or Enter is pressed on the active data cell.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridCellClickEventArgs> OnCellClick { get; set; }

    /// <summary>
    /// Emitted when a data cell is right-clicked (or Shift+F10 / the menu key is pressed on the active
    /// cell). See the args' remarks about the native context menu.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridCellContextMenuEventArgs> OnCellContextMenu { get; set; }

    /// <summary>
    /// Emitted when a column is reordered (live during drag; check <c>Finished</c>).
    /// </summary>
    [Parameter] public EventCallback<WaDataGridColumnMoveEventArgs> OnColumnMove { get; set; }

    /// <summary>
    /// Emitted when the user pins or unpins a column through the built-in controls. Programmatic
    /// <c>PinColumnAsync</c> calls don't emit.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridColumnPinEventArgs> OnColumnPin { get; set; }

    /// <summary>
    /// Emitted when a column is resized (live during drag; check <c>Finished</c>).
    /// </summary>
    [Parameter] public EventCallback<WaDataGridColumnResizeEventArgs> OnColumnResize { get; set; }

    /// <summary>
    /// Emitted when the user shows or hides a column through the built-in menus. Programmatic
    /// <c>ToggleColumnAsync</c> calls don't emit.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridColumnVisibilityChangeEventArgs> OnColumnVisibilityChange { get; set; }

    /// <summary>
    /// Emitted in server mode when a <c>dataSource</c> request rejects. Since <c>dataSource</c> itself
    /// isn't supported by the wrapper, this only fires for consumers who set the JS property directly.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridDataErrorEventArgs> OnDataError { get; set; }

    /// <summary>
    /// Emitted in server mode when the grid needs data for the current sort, filters, and page. Fetch
    /// the requested page, then assign <see cref="Data"/>, <see cref="Total"/>, and
    /// <see cref="Loading"/>.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridDataRequestEventArgs> OnDataRequest { get; set; }

    /// <summary>
    /// Emitted when the global search or a column filter changes.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridFilterChangeEventArgs> OnFilterChange { get; set; }

    /// <summary>
    /// Emitted when the current page or page size changes.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridPageChangeEventArgs> OnPageChange { get; set; }

    /// <summary>
    /// Emitted when a row collapses (a detail panel or a tree row's children).
    /// </summary>
    [Parameter] public EventCallback<WaDataGridRowEventArgs> OnRowCollapse { get; set; }

    /// <summary>
    /// Emitted when a row expands (a detail panel or a tree row's children).
    /// </summary>
    [Parameter] public EventCallback<WaDataGridRowEventArgs> OnRowExpand { get; set; }

    /// <summary>
    /// Emitted when the row selection changes.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridRowSelectEventArgs> OnRowSelect { get; set; }

    /// <summary>
    /// Emitted when the sort order changes.
    /// </summary>
    [Parameter] public EventCallback<WaDataGridSortChangeEventArgs> OnSortChange { get; set; }

    #endregion

    #region ------ Content Slots ------

    /// <summary>
    /// Content shown when there are no rows to display.
    /// </summary>
    [Parameter] public RenderFragment? EmptyContent { get; set; }

    /// <summary>
    /// Content shown in the loading overlay (server mode).
    /// </summary>
    [Parameter] public RenderFragment? LoadingContent { get; set; }

    /// <summary>
    /// Content shown when an active search or filter matches no rows (falls back to a localized
    /// message).
    /// </summary>
    [Parameter] public RenderFragment? NoResultsContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-data-grid");

        // Common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Data grid attributes ('data' and 'columns' are JS properties, pushed separately - see OnAfterRenderAsync/OnParametersSetAsync)
        builder.AddAttributeIfNotNullOrEmpty(10, "label", Label);
        builder.AddAttributeIfNotNull(11, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttributeIfNotNull(12, "size", Size?.ToHtmlValue());
        builder.AddAttribute(13, "striped", Striped);
        builder.AddAttribute(14, "with-search", WithSearch);
        builder.AddAttribute(15, "with-column-menu", WithColumnMenu);
        builder.AddAttribute(16, "with-columns-menu", WithColumnsMenu);
        builder.AddAttribute(17, "without-sort-removal", WithoutSortRemoval);
        builder.AddAttribute(18, "sort-desc-first", SortDescFirst);
        builder.AddAttributeIfNotNull(19, "max-multi-sort", MaxMultiSort);
        builder.AddAttributeIfNotNull(20, "selectable", Selectable?.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(21, "row-key", RowKey);
        builder.AddAttribute(22, "paginate", Paginate);
        builder.AddAttributeIfNotNull(23, "page", Page);
        builder.AddAttributeIfNotNull(24, "page-size", PageSize);
        builder.AddAttribute(25, "server", Server);
        builder.AddAttributeIfNotNull(26, "filter-debounce", FilterDebounce);
        builder.AddAttribute(27, "filter-from-leaf-rows", FilterFromLeafRows);
        builder.AddAttributeIfNotNullOrEmpty(28, "group-by", GroupBy);
        builder.AddAttributeIfNotNullOrEmpty(29, "child-rows", ChildRows);
        builder.AddAttribute(30, "resizable", Resizable);
        builder.AddAttribute(31, "reorderable", Reorderable);
        builder.AddAttribute(32, "pinnable", Pinnable);
        builder.AddAttributeIfNotNull(33, "total", Total);
        builder.AddAttribute(34, "loading", Loading);

        // Event handlers
        builder.AddAttributeIfHasDelegate(60, "onwa-cell-click", OnCellClick);
        builder.AddAttributeIfHasDelegate(61, "onwa-cell-contextmenu", OnCellContextMenu);
        builder.AddAttributeIfHasDelegate(62, "onwa-column-move", OnColumnMove);
        builder.AddAttributeIfHasDelegate(63, "onwa-column-pin", OnColumnPin);
        builder.AddAttributeIfHasDelegate(64, "onwa-column-resize", OnColumnResize);
        builder.AddAttributeIfHasDelegate(65, "onwa-column-visibility-change", OnColumnVisibilityChange);
        builder.AddAttributeIfHasDelegate(66, "onwa-data-error", OnDataError);
        builder.AddAttributeIfHasDelegate(67, "onwa-data-request", OnDataRequest);
        builder.AddAttributeIfHasDelegate(68, "onwa-filter-change", OnFilterChange);
        builder.AddAttributeIfHasDelegate(69, "onwa-page-change", OnPageChange);
        builder.AddAttributeIfHasDelegate(70, "onwa-row-collapse", OnRowCollapse);
        builder.AddAttributeIfHasDelegate(71, "onwa-row-expand", OnRowExpand);
        builder.AddAttributeIfHasDelegate(72, "onwa-row-select", OnRowSelect);
        builder.AddAttributeIfHasDelegate(73, "onwa-sort-change", OnSortChange);

        // Element reference capture
        builder.AddElementReferenceCapture(90, __dataGridReference => Element = __dataGridReference);

        // Slots
        if (EmptyContent is not null)
        {
            builder.OpenElement(100, "div");
            builder.AddAttribute(101, "slot", "empty");
            builder.AddContent(102, EmptyContent);
            builder.CloseElement();
        }

        if (LoadingContent is not null)
        {
            builder.OpenElement(105, "div");
            builder.AddAttribute(106, "slot", "loading");
            builder.AddContent(107, LoadingContent);
            builder.CloseElement();
        }

        if (NoResultsContent is not null)
        {
            builder.OpenElement(110, "div");
            builder.AddAttribute(111, "slot", "no-results");
            builder.AddContent(112, NoResultsContent);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (Element != null)
        {
            await PushDataAndColumnsAsync();
        }

        await base.OnParametersSetAsync();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await PushDataAndColumnsAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Resizes one column to fit its widest rendered cell content (the double-click-handle behavior).
    /// </summary>
    public async Task AutoSizeColumnAsync(string columnId)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot auto-size column: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "autoSizeColumn", columnId);
    }

    /// <summary>
    /// Resizes every resizable column to fit its content.
    /// </summary>
    public async Task AutoSizeColumnsAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot auto-size columns: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "autoSizeColumns");
    }

    /// <summary>
    /// Collapses every row.
    /// </summary>
    public async Task CollapseAllRowsAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot collapse rows: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "collapseAllRows");
    }

    /// <summary>
    /// Collapses the row with the given key (its <c>rowKey</c> value).
    /// </summary>
    /// <param name="key">The row's key, either a string or a number.</param>
    public async Task CollapseRowAsync(object key)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot collapse row: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "collapseRow", key);
    }

    /// <summary>
    /// Copies the selected rows (or every processed row when nothing is selected) to the clipboard,
    /// honoring the active sort, filters, and column visibility/order. Also wired to Ctrl+C when the
    /// grid has focus.
    /// </summary>
    /// <param name="options">Copy options; all fields are optional.</param>
    /// <returns>The number of rows copied.</returns>
    public async Task<int> CopySelectedRowsAsync(WaDataGridCopyOptions? options = null)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot copy selected rows: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<int>(Element.Value, "copySelectedRows", (object?)options ?? new object());
    }

    /// <summary>
    /// Expands every row (all detail panels, or every branch of a tree).
    /// </summary>
    public async Task ExpandAllRowsAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot expand rows: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "expandAllRows");
    }

    /// <summary>
    /// Expands the row with the given key (its <c>rowKey</c> value).
    /// </summary>
    /// <param name="key">The row's key, either a string or a number.</param>
    public async Task ExpandRowAsync(object key)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot expand row: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "expandRow", key);
    }

    /// <summary>
    /// Exports the current rows as a CSV file (browser download), respecting the active sort, filters,
    /// search, and column visibility/order. In server mode, only the currently loaded page is exported.
    /// </summary>
    /// <param name="options">Export options; all fields are optional.</param>
    public async Task ExportDataAsCsvAsync(WaDataGridCsvExportOptions? options = null)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot export data: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "exportDataAsCsv", (object?)options ?? new object());
    }

    /// <summary>
    /// Focuses the grid by focusing the active (roving-tabindex) cell.
    /// </summary>
    /// <param name="preventScroll">When true, the browser does not scroll the newly focused cell into view.</param>
    public async Task FocusAsync(bool preventScroll = false)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus grid: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus", new { preventScroll });
    }

    /// <summary>
    /// Returns which edge a column is pinned to (<c>left</c> or <c>right</c>), or null when it isn't
    /// pinned.
    /// </summary>
    public async Task<string?> GetColumnPinAsync(string columnId)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get column pin: component has not been rendered yet.");

        var result = await JSInterop.InvokeMethodAsync<object?>(Element.Value, "getColumnPin", columnId);
        return result switch
        {
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            string side => side,
            _ => null
        };
    }

    /// <summary>
    /// Returns the current rows as a CSV string, honoring the active sort, filters, search, and column
    /// visibility/order. Every page and tree depth is included; server mode exports only the loaded
    /// page.
    /// </summary>
    /// <param name="options">CSV options; all fields are optional.</param>
    public async Task<string> GetDataAsCsvAsync(WaDataGridCsvOptions? options = null)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get data as CSV: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<string>(Element.Value, "getDataAsCsv", (object?)options ?? new object());
    }

    /// <summary>
    /// Every data row in the current result set, in display order, after sorting, filtering, and
    /// search, across all pages and tree depths (parents before their children). Group header rows are
    /// excluded. In server mode this is the currently loaded page.
    /// </summary>
    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object>>> GetProcessedRowsAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get processed rows: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<IReadOnlyList<IReadOnlyDictionary<string, object>>>(Element.Value, "getProcessedRows");
    }

    /// <summary>
    /// Returns a serializable snapshot of column order, widths, visibility, sort, filters, search,
    /// selection, and paging.
    /// </summary>
    public async Task<WaDataGridState> GetStateAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get state: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<WaDataGridState>(Element.Value, "getState");
    }

    /// <summary>
    /// The data rows currently displayed, in display order, after sorting, filtering, expansion, and
    /// pagination. Group header rows are excluded.
    /// </summary>
    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object>>> GetVisibleRowsAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot get visible rows: component has not been rendered yet.");

        return await JSInterop.InvokeMethodAsync<IReadOnlyList<IReadOnlyDictionary<string, object>>>(Element.Value, "getVisibleRows");
    }

    /// <summary>
    /// Pins a column to the <c>left</c> or <c>right</c> edge, or unpins it when <paramref name="side"/>
    /// is null.
    /// </summary>
    public async Task PinColumnAsync(string columnId, string? side)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot pin column: component has not been rendered yet.");

        object jsSide = side is null ? false : side;
        await JSInterop.InvokeMethodAsync(Element.Value, "pinColumn", columnId, jsSide);
    }

    /// <summary>
    /// Re-runs the current server request (server mode only), even if its parameters haven't changed.
    /// </summary>
    public async Task ReloadAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reload: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "reload");
    }

    /// <summary>
    /// Resets column order, widths, visibility, and pinning to the column definitions' defaults,
    /// leaving sort, filters, search, selection, and paging untouched (the columns menu's "Reset
    /// columns" action).
    /// </summary>
    public async Task ResetColumnsAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reset columns: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "resetColumns");
    }

    /// <summary>
    /// Resets all user-adjusted view state (order, widths, visibility, pinning, sort, filters, search,
    /// expansion) to the column defaults. Selection and paging are left alone.
    /// </summary>
    public async Task ResetStateAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot reset state: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "resetState");
    }

    /// <summary>
    /// Scrolls the row at the given display index into view (pairs with virtualization).
    /// </summary>
    /// <param name="index">The row's display index.</param>
    /// <param name="align">Where to align the row: <c>start</c>, <c>center</c>, or <c>end</c>.</param>
    public async Task ScrollToIndexAsync(int index, string? align = null)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot scroll to index: component has not been rendered yet.");

        if (align is null)
        {
            await JSInterop.InvokeMethodAsync(Element.Value, "scrollToIndex", index);
        }
        else
        {
            await JSInterop.InvokeMethodAsync(Element.Value, "scrollToIndex", index, new { align });
        }
    }

    /// <summary>
    /// Restores a previously captured state. Unknown column ids are ignored; omitted keys are left
    /// unchanged.
    /// </summary>
    public async Task SetStateAsync(WaDataGridState state)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set state: component has not been rendered yet.");

        if (state == null)
            throw new ArgumentNullException(nameof(state));

        await JSInterop.InvokeMethodAsync(Element.Value, "setState", state);
    }

    /// <summary>
    /// Distributes column widths to fill the available horizontal space, honoring each column's
    /// min/max.
    /// </summary>
    public async Task SizeColumnsToFitAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot size columns to fit: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "sizeColumnsToFit");
    }

    /// <summary>
    /// Shows or hides a column by its id (the column's <c>id</c>, or <c>field</c> when no id is set).
    /// </summary>
    /// <param name="columnId">The column id.</param>
    /// <param name="visible">The desired visibility; toggles the current visibility when omitted.</param>
    public async Task ToggleColumnAsync(string columnId, bool? visible = null)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot toggle column: component has not been rendered yet.");

        if (visible.HasValue)
        {
            await JSInterop.InvokeMethodAsync(Element.Value, "toggleColumn", columnId, visible.Value);
        }
        else
        {
            await JSInterop.InvokeMethodAsync(Element.Value, "toggleColumn", columnId);
        }
    }

    #endregion

    #region ------ Internals ------

    private string GetCombinedCssClass()
    {
        return string.IsNullOrEmpty(Class) ? string.Empty : Class;
    }

    private async Task PushDataAndColumnsAsync()
    {
        if (Element == null) return;

        await JSInterop.SetPropertyAsync(Element.Value, "data", Data ?? Array.Empty<object>());
        await JSInterop.SetPropertyAsync(Element.Value, "columns", Columns ?? Array.Empty<WaDataGridColumn>());
    }

    #endregion
}
