using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using System.Collections.Generic;
using System.Text.Json;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The JSON of the data grid column model as the JS interop sends it to the element's <c>columns</c> property. A test
/// <see cref="JSRuntime"/> records the argument JSON of the call, which is the exact text Blazor passes to JavaScript,
/// so the checks see the interop's own serializer options (which have no global ignore condition) and not the web
/// defaults of a test.
/// </summary>
public class WaDataGridColumnJsonTests
{
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
            CellClass = "region"
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

    // the camelCase names of every JSON-expressible DataGridColumn member, in the declaration order of the model
    private static readonly string[] ExpectedMemberNames =
    [
        "field", "id", "label", "align", "headerAlign", "sortable", "sortDescFirst", "sortFn", "sortUndefined",
        "searchable", "filterable", "filterType", "filterOptions", "hidden", "hideable", "resizable", "movable",
        "pinnable", "pinned", "width", "minWidth", "maxWidth", "flex", "footer", "aggregation", "cellClass"
    ];

    // the JSON of the columns argument of a setProperty call, as the interop sends it
    private static string SentColumnsJson(params WaDataGridColumn[] columns)
    {
        var runtime = new RecordingJSRuntime();
        IReadOnlyList<WaDataGridColumn> value = columns;

        // the call never completes (no JavaScript answers it); the argument JSON is recorded synchronously
        _ = runtime.InvokeVoidAsync("setProperty", "columns", value);

        Assert.NotNull(runtime.LastArgsJson);
        using var document = JsonDocument.Parse(runtime.LastArgsJson);
        return document.RootElement[1].GetRawText();
    }

    // a JS runtime without JavaScript: it records the argument JSON of each call
    private sealed class RecordingJSRuntime : JSRuntime
    {
        public string? LastArgsJson { get; private set; }

        protected override void BeginInvokeJS(long taskId, string identifier, string? argsJson, JSCallResultType resultType, long targetInstanceId)
        {
            LastArgsJson = argsJson;
        }

        protected override void EndInvokeDotNet(DotNetInvocationInfo invocationInfo, in DotNetInvocationResult invocationResult)
        {
        }
    }

    #endregion
}
