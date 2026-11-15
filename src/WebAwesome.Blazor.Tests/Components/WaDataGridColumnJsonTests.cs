using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            ComparatorFunction = WaJsFunction.Registered("myapp.byRegion"),
            ValueFunction = WaJsFunction.Registered("myapp.regionName"),
            FilterFunction = WaJsFunction.Registered("myapp.regionFilter")
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
    public void FunctionForms_SendTheirDescriptorsUnderTheKeysOfTheStaticForms()
    {
        var json = SentColumnsJson(new WaDataGridColumn
        {
            Id = "total",
            ValueFunction = WaJsFunction.Registered("myapp.lineTotal"),
            FilterFunction = WaJsFunction.Registered("myapp.atLeast"),
            FooterFunction = WaJsFunction.Registered("myapp.sum").Uses("wa-badge"),
            AggregationFunction = WaJsFunction.Registered("myapp.range"),
            CellClassFunction = WaJsFunction.Registered("myapp.sign").WithArgs(new { Threshold = 0 })
        });

        Assert.Equal(
            "[{\"id\":\"total\","
            + "\"footer\":{\"$wab\":\"fn\",\"ref\":{\"kind\":\"registered\",\"name\":\"myapp.sum\"},\"uses\":[\"wa-badge\"]},"
            + "\"aggregation\":{\"$wab\":\"fn\",\"ref\":{\"kind\":\"registered\",\"name\":\"myapp.range\"}},"
            + "\"cellClass\":{\"$wab\":\"fn\",\"ref\":{\"kind\":\"registered\",\"name\":\"myapp.sign\"},\"args\":{\"threshold\":0}},"
            + "\"value\":{\"$wab\":\"fn\",\"ref\":{\"kind\":\"registered\",\"name\":\"myapp.lineTotal\"}},"
            + "\"filterFn\":{\"$wab\":\"fn\",\"ref\":{\"kind\":\"registered\",\"name\":\"myapp.atLeast\"}}}]",
            json);
    }

    [Fact]
    public void StaticForms_SendTheirValuesUnderTheSameKeys()
    {
        var json = SentColumnsJson(new WaDataGridColumn
        {
            Field = "amount",
            Footer = "Total",
            Aggregation = WaDataGridAggregation.UniqueCount,
            CellClass = "money"
        });

        Assert.Equal("[{\"field\":\"amount\",\"footer\":\"Total\",\"aggregation\":\"uniqueCount\",\"cellClass\":\"money\"}]", json);
    }

    [Theory]
    [MemberData(nameof(BothFormsCases))]
    public void Column_WithBothFormsOfOneMember_Throws(WaDataGridColumn column, string staticForm, string functionForm)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Render<WaDataGrid>(p => p.Add(c => c.Columns, [new WaDataGridColumn { Field = "name" }, column])));

        Assert.Contains("\"amount\"", exception.Message);
        Assert.Contains(staticForm, exception.Message);
        Assert.Contains(functionForm, exception.Message);
    }

    [Fact]
    public void Column_WithBothFormsOfOneMember_InAParameterSet_Throws()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setDataGridColumns", _ => true).SetVoidResult();
        var grid = Render<WaDataGrid>(p => p.Add(c => c.Columns, [new WaDataGridColumn { Field = "amount", Footer = "Total" }]));

        var conflicting = new WaDataGridColumn { Field = "amount", Footer = "Total", FooterFunction = WaJsFunction.Registered("myapp.sum") };
        Assert.Throws<InvalidOperationException>(() => grid.Render(p => p.Add(c => c.Columns, [conflicting])));

        // the conflicting columns never reach the element
        Assert.Single(module.Invocations, i => i.Identifier == "setDataGridColumns");
    }

    /// <summary>
    /// A column with the static and the function form of one member, with the names of both forms.
    /// </summary>
    public static TheoryData<WaDataGridColumn, string, string> BothFormsCases() => new()
    {
        { new WaDataGridColumn { Field = "amount", Footer = "Total", FooterFunction = WaJsFunction.Registered("myapp.sum") }, nameof(WaDataGridColumn.Footer), nameof(WaDataGridColumn.FooterFunction) },
        { new WaDataGridColumn { Field = "amount", Aggregation = WaDataGridAggregation.Sum, AggregationFunction = WaJsFunction.Registered("myapp.range") }, nameof(WaDataGridColumn.Aggregation), nameof(WaDataGridColumn.AggregationFunction) },
        { new WaDataGridColumn { Field = "amount", CellClass = "money", CellClassFunction = WaJsFunction.Registered("myapp.sign") }, nameof(WaDataGridColumn.CellClass), nameof(WaDataGridColumn.CellClassFunction) },
    };

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

    // the camelCase names of every DataGridColumn member the model binds, in the declaration order of the model (a member
    // with a static and a function form keeps the position of its static form)
    private static readonly string[] ExpectedMemberNames =
    [
        "field", "id", "label", "align", "headerAlign", "sortable", "sortDescFirst", "sortFn", "sortUndefined",
        "searchable", "filterable", "filterType", "filterOptions", "hidden", "hideable", "resizable", "movable",
        "pinnable", "pinned", "width", "minWidth", "maxWidth", "flex", "footer", "aggregation", "cellClass",
        "formatter", "aggregatedFormatter", "comparator", "value", "filterFn"
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
