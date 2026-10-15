using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text.Json;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The JSON of the data grid column model as <see cref="WaDataGrid"/> sends it to the element: the column JSON text is
/// the argument of the interop's <c>setDataGridColumns</c>, which parses it, resolves the function references and
/// assigns the element's <c>columns</c> property. The checks read that argument, so they see exactly what the browser
/// receives.
/// </summary>
public class WaDataGridColumnJsonTests : BunitContext
{
    public WaDataGridColumnJsonTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Column_WithOnlyAField_SendsOnlyThatMember()
    {
        var json = SentColumnsJson(new WaDataGridColumn { Field = "name" });

        Assert.Equal("[{\"field\":\"name\"}]", json);
    }

    [Fact]
    public void Column_WithEveryMemberSet_SendsEveryMember()
    {
        var column = new WaDataGridColumn
        {
            Field = "region",
            Id = "area",
            Label = "Region",
            Align = WaDataGridAlign.Start,
            HeaderAlign = WaDataGridAlign.Center,
            Sortable = false,
            SortDescFirst = false,
            SortFn = WaDataGridSortFn.Text,
            SortUndefined = WaDataGridSortUndefined.Last,
            Searchable = false,
            Filterable = true,
            FilterType = WaDataGridFilterType.Set,
            FilterOptions = [new("Europe")],
            Hidden = false,
            Hideable = false,
            Resizable = false,
            Movable = false,
            Pinnable = false,
            Pinned = WaDataGridPinSide.Right,
            Width = 140,
            MinWidth = 100,
            MaxWidth = 300,
            Flex = 1.5,
            Footer = "Total",
            Aggregation = WaDataGridAggregation.Count,
            CellClass = "region",
            Formatter = WaJsFunction.Registered("myapp.region"),
            AggregatedFormatter = WaJsFunction.Registered("myapp.regionCount"),
            ComparatorFunction = WaJsFunction.Registered("myapp.byRegion")
        };

        using var document = JsonDocument.Parse(SentColumnsJson(column));
        var sent = document.RootElement[0];

        // false and zero-like values are values, not unset members, so they are sent
        Assert.Equal(JsonValueKind.False, sent.GetProperty("sortable").ValueKind);
        Assert.Equal(JsonValueKind.False, sent.GetProperty("hidden").ValueKind);
        var names = new List<string>();
        foreach (var property in sent.EnumerateObject())
            names.Add(property.Name);

        Assert.Equal(ExpectedMemberNames, names);
    }

    [Fact]
    public void FunctionMembers_SendTheirDescriptorsUnderTheUpstreamKeys()
    {
        var json = SentColumnsJson(new WaDataGridColumn
        {
            Field = "state",
            Formatter = WaJsFunction.Module("./Pages/Orders.razor.js", "stateCell").WithArgs(new { ShowIcon = true }).Uses("wa-badge"),
            AggregatedFormatter = WaJsFunction.Registered("myapp.stateCount"),
            ComparatorFunction = WaJsFunction.Global("myApp.byState")
        });

        Assert.Equal(
            "[{\"field\":\"state\","
            + "\"formatter\":{\"$wab\":\"fn\",\"ref\":{\"kind\":\"module\",\"url\":\"./Pages/Orders.razor.js\",\"export\":\"stateCell\"},\"args\":{\"showIcon\":true},\"uses\":[\"wa-badge\"]},"
            + "\"aggregatedFormatter\":{\"$wab\":\"fn\",\"ref\":{\"kind\":\"registered\",\"name\":\"myapp.stateCount\"}},"
            + "\"comparator\":{\"$wab\":\"fn\",\"ref\":{\"kind\":\"global\",\"path\":\"myApp.byState\"}}}]",
            json);
    }

    [Fact]
    public void FilterOptions_SendValueLabelAndCount_AndLeaveOutUnsetOnes()
    {
        var json = SentColumnsJson(new WaDataGridColumn
        {
            Field = "status",
            FilterType = WaDataGridFilterType.Set,
            FilterOptions = [new("ACT", "Active", 12), new("SUS", Count: 0), new("CLS")]
        });

        Assert.Equal(
            "[{\"field\":\"status\",\"filterType\":\"set\",\"filterOptions\":"
            + "[{\"value\":\"ACT\",\"label\":\"Active\",\"count\":12},{\"value\":\"SUS\",\"count\":0},{\"value\":\"CLS\"}]}]",
            json);
    }

    [Fact]
    public void FilterOptions_EmptyList_IsSent()
    {
        // an empty list is a value (a picker with no options), unlike null, which leaves the computed facets
        var json = SentColumnsJson(new WaDataGridColumn { Field = "status", FilterOptions = [] });

        Assert.Equal("[{\"field\":\"status\",\"filterOptions\":[]}]", json);
    }

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";

    // the camelCase names of every DataGridColumn member the model binds, in the declaration order of the model
    private static readonly string[] ExpectedMemberNames =
    [
        "field", "id", "label", "align", "headerAlign", "sortable", "sortDescFirst", "sortFn", "sortUndefined",
        "searchable", "filterable", "filterType", "filterOptions", "hidden", "hideable", "resizable", "movable",
        "pinnable", "pinned", "width", "minWidth", "maxWidth", "flex", "footer", "aggregation", "cellClass",
        "formatter", "aggregatedFormatter", "comparator"
    ];

    // the column JSON of the grid's first push, as the interop sends it to setDataGridColumns
    private string SentColumnsJson(params WaDataGridColumn[] columns)
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setDataGridColumns", _ => true).SetVoidResult();

        Render<WaDataGrid>(parameters => parameters.Add(p => p.Columns, columns));

        var push = Assert.Single(module.Invocations, i => i.Identifier == "setDataGridColumns");
        return Assert.IsType<string>(push.Arguments[1]);
    }

    #endregion
}
