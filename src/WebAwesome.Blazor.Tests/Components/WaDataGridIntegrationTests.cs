using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaDataGrid (Pro) component introduced in Web Awesome 3.11.0.
/// Covers attribute emission for the 25 declarative attributes, confirms Data/Columns are NOT
/// rendered as attributes (they are pushed as JS properties via WebAwesomeJSInterop.SetPropertyAsync
/// during OnAfterRenderAsync/OnParametersSetAsync), event wiring for all 14 events, the three content
/// slots, and guard clauses for the imperative methods. bUnit cannot host a real wa-data-grid custom
/// element, so these tests observe the interop boundary (the setProperty/invokeMethod calls the
/// wrapper makes) rather than any real grid behavior in the browser.
/// </summary>
public class WaDataGridIntegrationTests : BunitContext
{
    public WaDataGridIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        var cut = Render<WaDataGrid>();

        var element = cut.Find("wa-data-grid");
        Assert.False(element.HasAttribute("label"));
        Assert.False(element.HasAttribute("appearance"));
        Assert.False(element.HasAttribute("size"));
        Assert.False(element.HasAttribute("max-multi-sort"));
        Assert.False(element.HasAttribute("selectable"));
        Assert.False(element.HasAttribute("row-key"));
        Assert.False(element.HasAttribute("page"));
        Assert.False(element.HasAttribute("page-size"));
        Assert.False(element.HasAttribute("filter-debounce"));
        Assert.False(element.HasAttribute("group-by"));
        Assert.False(element.HasAttribute("child-rows"));
        Assert.False(element.HasAttribute("total"));
        // boolean attributes are omitted when false
        Assert.False(element.HasAttribute("striped"));
        Assert.False(element.HasAttribute("with-search"));
        Assert.False(element.HasAttribute("with-column-menu"));
        Assert.False(element.HasAttribute("with-columns-menu"));
        Assert.False(element.HasAttribute("without-sort-removal"));
        Assert.False(element.HasAttribute("sort-desc-first"));
        Assert.False(element.HasAttribute("paginate"));
        Assert.False(element.HasAttribute("server"));
        Assert.False(element.HasAttribute("filter-from-leaf-rows"));
        Assert.False(element.HasAttribute("resizable"));
        Assert.False(element.HasAttribute("reorderable"));
        Assert.False(element.HasAttribute("pinnable"));
        Assert.False(element.HasAttribute("loading"));
        // data/columns are JS properties, not attributes
        Assert.False(element.HasAttribute("data"));
        Assert.False(element.HasAttribute("columns"));
    }

    [Fact]
    public void Attributes_WhenSet_RenderExpectedKebabCaseNames()
    {
        var cut = Render<WaDataGrid>(parameters => parameters
            .Add(p => p.Label, "Orders")
            .Add(p => p.Appearance, WaDataGridAppearance.Outlined)
            .Add(p => p.Size, WaSize.Small)
            .Add(p => p.Striped, true)
            .Add(p => p.WithSearch, true)
            .Add(p => p.WithColumnMenu, true)
            .Add(p => p.WithColumnsMenu, true)
            .Add(p => p.WithoutSortRemoval, true)
            .Add(p => p.SortDescFirst, true)
            .Add(p => p.MaxMultiSort, 3)
            .Add(p => p.Selectable, WaDataGridSelectable.Multiple)
            .Add(p => p.RowKey, "id")
            .Add(p => p.Paginate, true)
            .Add(p => p.Page, 2)
            .Add(p => p.PageSize, 25)
            .Add(p => p.Server, true)
            .Add(p => p.FilterDebounce, 300)
            .Add(p => p.FilterFromLeafRows, true)
            .Add(p => p.GroupBy, "category")
            .Add(p => p.ChildRows, "children")
            .Add(p => p.Resizable, true)
            .Add(p => p.Reorderable, true)
            .Add(p => p.Pinnable, true)
            .Add(p => p.Total, 500)
            .Add(p => p.Loading, true));

        var element = cut.Find("wa-data-grid");
        Assert.Equal("Orders", element.GetAttribute("label"));
        Assert.Equal("outlined", element.GetAttribute("appearance"));
        Assert.Equal("small", element.GetAttribute("size"));
        Assert.True(element.HasAttribute("striped"));
        Assert.True(element.HasAttribute("with-search"));
        Assert.True(element.HasAttribute("with-column-menu"));
        Assert.True(element.HasAttribute("with-columns-menu"));
        Assert.True(element.HasAttribute("without-sort-removal"));
        Assert.True(element.HasAttribute("sort-desc-first"));
        Assert.Equal("3", element.GetAttribute("max-multi-sort"));
        Assert.Equal("multiple", element.GetAttribute("selectable"));
        Assert.Equal("id", element.GetAttribute("row-key"));
        Assert.True(element.HasAttribute("paginate"));
        Assert.Equal("2", element.GetAttribute("page"));
        Assert.Equal("25", element.GetAttribute("page-size"));
        Assert.True(element.HasAttribute("server"));
        Assert.Equal("300", element.GetAttribute("filter-debounce"));
        Assert.True(element.HasAttribute("filter-from-leaf-rows"));
        Assert.Equal("category", element.GetAttribute("group-by"));
        Assert.Equal("children", element.GetAttribute("child-rows"));
        Assert.True(element.HasAttribute("resizable"));
        Assert.True(element.HasAttribute("reorderable"));
        Assert.True(element.HasAttribute("pinnable"));
        Assert.Equal("500", element.GetAttribute("total"));
        Assert.True(element.HasAttribute("loading"));
    }

    [Fact]
    public void Appearance_MapsToHtmlValue()
    {
        Assert.Equal("outlined", WaDataGridAppearance.Outlined.ToHtmlValue());
        Assert.Equal("plain", WaDataGridAppearance.Plain.ToHtmlValue());
    }

    [Fact]
    public void Selectable_MapsToHtmlValue()
    {
        Assert.Equal("none", WaDataGridSelectable.None.ToHtmlValue());
        Assert.Equal("single", WaDataGridSelectable.Single.ToHtmlValue());
        Assert.Equal("multiple", WaDataGridSelectable.Multiple.ToHtmlValue());
    }

    [Fact]
    public void Data_And_Columns_ArePushedAsJSPropertiesOnFirstRender()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setProperty", _ => true).SetVoidResult();

        var data = new List<object> { new { id = 1, name = "Ada" } };
        var columns = new List<WaDataGridColumn> { new() { Field = "name", Label = "Name" } };

        var cut = Render<WaDataGrid>(parameters => parameters
            .Add(p => p.Data, data)
            .Add(p => p.Columns, columns));

        var dataInvocation = Assert.Single(module.Invocations, i => i.Identifier == "setProperty" && Equals(i.Arguments[1], "data"));
        Assert.Same(data, dataInvocation.Arguments[2]);

        var columnsInvocation = Assert.Single(module.Invocations, i => i.Identifier == "setProperty" && Equals(i.Arguments[1], "columns"));
        Assert.Same(columns, columnsInvocation.Arguments[2]);
    }

    [Fact]
    public void Data_And_Columns_ArePushedAgainWhenParametersChange()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setProperty", _ => true).SetVoidResult();

        var cut = Render<WaDataGrid>(parameters => parameters
            .Add(p => p.Data, new List<object>()));

        cut.Render(parameters => parameters
            .Add(p => p.Data, new List<object> { new { id = 2 } }));

        var dataInvocations = module.Invocations.Where(i => i.Identifier == "setProperty" && Equals(i.Arguments[1], "data")).ToList();
        Assert.True(dataInvocations.Count >= 2);
    }

    [Fact]
    public void Events_WhenWired_ReceiveDomEvents()
    {
        var cellClick = 0;
        var cellContextMenu = 0;
        var columnMove = 0;
        var columnPin = 0;
        var columnResize = 0;
        var columnVisibilityChange = 0;
        var dataError = 0;
        var dataRequest = 0;
        var filterChange = 0;
        var pageChange = 0;
        var rowCollapse = 0;
        var rowExpand = 0;
        var rowSelect = 0;
        var sortChange = 0;

        var cut = Render<WaDataGrid>(parameters => parameters
            .Add(p => p.OnCellClick, _ => cellClick++)
            .Add(p => p.OnCellContextMenu, _ => cellContextMenu++)
            .Add(p => p.OnColumnMove, _ => columnMove++)
            .Add(p => p.OnColumnPin, _ => columnPin++)
            .Add(p => p.OnColumnResize, _ => columnResize++)
            .Add(p => p.OnColumnVisibilityChange, _ => columnVisibilityChange++)
            .Add(p => p.OnDataError, _ => dataError++)
            .Add(p => p.OnDataRequest, _ => dataRequest++)
            .Add(p => p.OnFilterChange, _ => filterChange++)
            .Add(p => p.OnPageChange, _ => pageChange++)
            .Add(p => p.OnRowCollapse, _ => rowCollapse++)
            .Add(p => p.OnRowExpand, _ => rowExpand++)
            .Add(p => p.OnRowSelect, _ => rowSelect++)
            .Add(p => p.OnSortChange, _ => sortChange++));

        var element = cut.Find("wa-data-grid");
        element.TriggerEvent("onwa-cell-click", new WaDataGridCellClickEventArgs());
        element.TriggerEvent("onwa-cell-contextmenu", new WaDataGridCellContextMenuEventArgs());
        element.TriggerEvent("onwa-column-move", new WaDataGridColumnMoveEventArgs());
        element.TriggerEvent("onwa-column-pin", new WaDataGridColumnPinEventArgs());
        element.TriggerEvent("onwa-column-resize", new WaDataGridColumnResizeEventArgs());
        element.TriggerEvent("onwa-column-visibility-change", new WaDataGridColumnVisibilityChangeEventArgs());
        element.TriggerEvent("onwa-data-error", new WaDataGridDataErrorEventArgs());
        element.TriggerEvent("onwa-data-request", new WaDataGridDataRequestEventArgs());
        element.TriggerEvent("onwa-filter-change", new WaDataGridFilterChangeEventArgs());
        element.TriggerEvent("onwa-page-change", new WaDataGridPageChangeEventArgs());
        element.TriggerEvent("onwa-row-collapse", new WaDataGridRowEventArgs());
        element.TriggerEvent("onwa-row-expand", new WaDataGridRowEventArgs());
        element.TriggerEvent("onwa-row-select", new WaDataGridRowSelectEventArgs());
        element.TriggerEvent("onwa-sort-change", new WaDataGridSortChangeEventArgs());

        Assert.Equal(1, cellClick);
        Assert.Equal(1, cellContextMenu);
        Assert.Equal(1, columnMove);
        Assert.Equal(1, columnPin);
        Assert.Equal(1, columnResize);
        Assert.Equal(1, columnVisibilityChange);
        Assert.Equal(1, dataError);
        Assert.Equal(1, dataRequest);
        Assert.Equal(1, filterChange);
        Assert.Equal(1, pageChange);
        Assert.Equal(1, rowCollapse);
        Assert.Equal(1, rowExpand);
        Assert.Equal(1, rowSelect);
        Assert.Equal(1, sortChange);
    }

    [Fact]
    public void Slots_WhenProvided_RenderIntoNamedSlots()
    {
        var cut = Render<WaDataGrid>(parameters => parameters
            .Add(p => p.EmptyContent, b => b.AddContent(0, "no rows"))
            .Add(p => p.LoadingContent, b => b.AddContent(0, "loading…"))
            .Add(p => p.NoResultsContent, b => b.AddContent(0, "no matches")));

        Assert.Equal("no rows", cut.Find("div[slot='empty']").TextContent);
        Assert.Equal("loading…", cut.Find("div[slot='loading']").TextContent);
        Assert.Equal("no matches", cut.Find("div[slot='no-results']").TextContent);
    }

    [Fact]
    public void Class_And_Style_AreApplied()
    {
        var cut = Render<WaDataGrid>(parameters => parameters
            .Add(p => p.Class, "custom-class")
            .Add(p => p.Style, "height: 30rem;"));

        var element = cut.Find("wa-data-grid");
        Assert.Equal("custom-class", element.GetAttribute("class"));
        Assert.Equal("height: 30rem;", element.GetAttribute("style"));
    }

    [Fact]
    public async Task AutoSizeColumnAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.AutoSizeColumnAsync("name"));
        Assert.Contains("component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GetStateAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.GetStateAsync());
    }

    [Fact]
    public async Task SetStateAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.SetStateAsync(new WaDataGridState()));
    }

    [Fact]
    public async Task SetStateAsync_WithNullState_ThrowsArgumentNullException()
    {
        var component = new WaDataGrid();
        var elementProperty = typeof(WaDataGrid).GetProperty("Element",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        elementProperty?.SetValue(component, new Microsoft.AspNetCore.Components.ElementReference("test-grid"));

        await Assert.ThrowsAsync<ArgumentNullException>(() => component.SetStateAsync(null!));
    }

    [Fact]
    public async Task PinColumnAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.PinColumnAsync("name", "left"));
    }

    [Fact]
    public async Task ToggleColumnAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ToggleColumnAsync("name"));
    }

    [Fact]
    public async Task CopySelectedRowsAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.CopySelectedRowsAsync());
    }

    [Fact]
    public async Task ExportDataAsCsvAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ExportDataAsCsvAsync());
    }

    [Fact]
    public async Task GetDataAsCsvAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.GetDataAsCsvAsync());
    }

    [Fact]
    public async Task ScrollToIndexAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ScrollToIndexAsync(0));
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    [Fact]
    public async Task GetColumnPinAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.GetColumnPinAsync("name"));
    }

    [Fact]
    public async Task GetProcessedRowsAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.GetProcessedRowsAsync());
    }

    [Fact]
    public async Task GetVisibleRowsAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.GetVisibleRowsAsync());
    }

    [Fact]
    public async Task CollapseAllRowsAsync_And_ExpandAllRowsAsync_WithNullElement_ThrowInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.CollapseAllRowsAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ExpandAllRowsAsync());
    }

    [Fact]
    public async Task CollapseRowAsync_And_ExpandRowAsync_WithNullElement_ThrowInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.CollapseRowAsync("row-1"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ExpandRowAsync("row-1"));
    }

    [Fact]
    public async Task ReloadAsync_And_ResetColumnsAsync_And_ResetStateAsync_And_SizeColumnsToFitAsync_And_AutoSizeColumnsAsync_WithNullElement_ThrowInvalidOperationException()
    {
        var component = new WaDataGrid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ReloadAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ResetColumnsAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ResetStateAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.SizeColumnsToFitAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.AutoSizeColumnsAsync());
    }

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
}
