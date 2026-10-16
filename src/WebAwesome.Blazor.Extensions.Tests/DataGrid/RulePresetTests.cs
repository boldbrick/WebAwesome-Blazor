using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions.DataGrid;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// The rule and member presets (design section 7): class rules for cells and rows with every condition, footer
/// aggregates, the collation comparator, the formatted search, the filter options from labels, the detail fields and
/// the selectable field. The rule presets in data-grid-presets.js (compileRule) read exactly this JSON.
/// </summary>
public class RulePresetTests
{
    [Fact]
    public void CellRules_WriteEachConditionInTheRuleOrder()
    {
        var rules = WaDataGridClassRules.Cell(
            new WaDataGridClassRule("eq", WaDataGridCondition.EqualTo("ACT")),
            new WaDataGridClassRule("eq-number", WaDataGridCondition.EqualTo(2.5)),
            new WaDataGridClassRule("eq-bool", WaDataGridCondition.EqualTo(true)),
            new WaDataGridClassRule("in", WaDataGridCondition.In("a", "b", "a")),
            new WaDataGridClassRule("lt", WaDataGridCondition.LessThan(0)),
            new WaDataGridClassRule("gt-day", WaDataGridCondition.GreaterThan(new DateOnly(2026, 1, 31))),
            new WaDataGridClassRule("between", WaDataGridCondition.Between(1, 9.5)),
            new WaDataGridClassRule("instant", WaDataGridCondition.Between(new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.FromHours(1)), new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero))),
            new WaDataGridClassRule("null", WaDataGridCondition.IsNull),
            new WaDataGridClassRule("false", WaDataGridCondition.IsFalse),
            new WaDataGridClassRule("match", WaDataGridCondition.Matches("^ERR-\\d+", ignoreCase: true)) { Field = "code" });

        Assert.Equal("wab.dataGrid.cellClassRules", rules.Reference);
        Assert.Empty(rules.Tags);
        Assert.Equal(
            """{"rules":[{"class":"eq","when":"equals","value":"ACT"},{"class":"eq-number","when":"equals","value":"2.5"},{"class":"eq-bool","when":"isTrue"},{"class":"in","when":"in","values":["a","b"]},{"class":"lt","when":"lessThan","value":0,"type":"number"},{"class":"gt-day","when":"greaterThan","value":"2026-01-31","type":"date"},{"class":"between","when":"between","min":1,"max":9.5,"type":"number"},{"class":"instant","when":"between","min":"2026-01-01T08:00:00.000\u002B01:00","max":"2026-01-02T00:00:00.000\u002B00:00","type":"date"},{"class":"null","when":"isNull"},{"class":"false","when":"isFalse"},{"class":"match","when":"matches","pattern":"^ERR-\\d\u002B","ignoreCase":true,"field":"code"}]}""",
            ArgsOf(rules));
    }

    [Fact]
    public void Conditions_OfEnumValues_UseTheJsonFormOfTheValues()
    {
        var rules = WaDataGridClassRules.Cell(
            new WaDataGridClassRule("number", WaDataGridCondition.EqualTo(PlainState.Active)),
            new WaDataGridClassRule("names", WaDataGridCondition.In(NamedState.Open, NamedState.InProgress)));

        Assert.Equal(
            """{"rules":[{"class":"number","when":"equals","value":"2"},{"class":"names","when":"in","values":["Open","InProgress"]}]}""",
            ArgsOf(rules));
    }

    [Fact]
    public void RowRules_NeedAFieldOnEachRule()
    {
        var rules = WaDataGridClassRules.Row(new WaDataGridClassRule("late overdue", WaDataGridCondition.LessThan(new DateOnly(2026, 10, 1))) { Field = "due" });

        Assert.Equal("wab.dataGrid.rowClassRules", rules.Reference);
        Assert.Equal("""{"rules":[{"class":"late overdue","when":"lessThan","value":"2026-10-01","type":"date","field":"due"}]}""", ArgsOf(rules));
        Assert.Throws<ArgumentException>(() => WaDataGridClassRules.Row(new WaDataGridClassRule("x", WaDataGridCondition.IsTrue)));
    }

    [Fact]
    public void Rules_RejectInvalidInput()
    {
        Assert.Throws<ArgumentNullException>(() => WaDataGridClassRules.Cell(null!));
        Assert.Throws<ArgumentException>(() => WaDataGridClassRules.Cell());
        Assert.Throws<ArgumentException>(() => WaDataGridClassRules.Cell(new WaDataGridClassRule("x", WaDataGridCondition.IsTrue) { Field = "a..b" }));
        Assert.Throws<ArgumentException>(() => new WaDataGridClassRule(" x", WaDataGridCondition.IsTrue));
        Assert.Throws<ArgumentException>(() => new WaDataGridClassRule("", WaDataGridCondition.IsTrue));
        Assert.Throws<ArgumentNullException>(() => new WaDataGridClassRule("x", null!));
        Assert.Throws<ArgumentException>(() => WaDataGridCondition.LessThan(double.NaN));
        Assert.Throws<ArgumentException>(() => WaDataGridCondition.EqualTo(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => WaDataGridCondition.Between(2, 1));
        Assert.Throws<ArgumentException>(() => WaDataGridCondition.Between(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)));
        Assert.Throws<ArgumentException>(() => WaDataGridCondition.In());
        Assert.Throws<ArgumentException>(() => WaDataGridCondition.In("a", null!));
        Assert.Throws<ArgumentException>(() => WaDataGridCondition.Matches(""));
    }

    [Fact]
    public void Footers_WriteTheAggregateTheFormatAndTheLabel()
    {
        var sum = WaDataGridFooters.Sum(WaDataGridFormatters.Currency("EUR", culture: English), "Total", "-", English);

        Assert.Equal(
            """{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.footerAggregate"},"args":{"culture":"en-US","nullText":"-","aggregate":"sum","format":{"name":"wab.dataGrid.currency","args":{"culture":"en-US","currency":"EUR"}},"label":"Total"}}""",
            sum.ToString());
        Assert.Equal("""{"culture":"en-US","aggregate":"average"}""", ArgsOf(WaDataGridFooters.Average(culture: English)));
        Assert.Equal("""{"culture":"en-US","aggregate":"min"}""", ArgsOf(WaDataGridFooters.Min(culture: English)));
        Assert.Equal("""{"culture":"en-US","aggregate":"max"}""", ArgsOf(WaDataGridFooters.Max(culture: English)));
        Assert.Equal("""{"culture":"en-US","aggregate":"count","label":"Rows"}""", ArgsOf(WaDataGridFooters.Count(label: "Rows", culture: English)));
    }

    [Fact]
    public void Collation_WritesTheCultureAndTheOptions()
    {
        Assert.Equal("""{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.collation"},"args":{"culture":"cs-CZ"}}""",
            WaDataGridComparators.Collation(CultureInfo.GetCultureInfo("cs-CZ")).ToString());
        Assert.Equal("""{"culture":"en-US","sensitivity":"base","numeric":true,"ignorePunctuation":true}""",
            ArgsOf(WaDataGridComparators.Collation(English, WaDataGridCollationSensitivity.Base, numeric: true, ignorePunctuation: true)));
    }

    [Fact]
    public void SearchFormatted_TakesTheLabelsOfTheLabeledSearchableColumns()
    {
        var search = WaDataGridSearch.Formatted(
            new WaDataGridColumn { Field = "region", Formatter = WaDataGridFormatters.Lookup(new Dictionary<string, string> { ["EU"] = "Europe" }, "Other") },
            new WaDataGridColumn { Field = "state", Formatter = WaDataGridFormatters.Badge<PlainState>(_ => WaVariant.Neutral, state => state.ToString()) },
            new WaDataGridColumn { Field = "hidden", Searchable = false, Formatter = WaDataGridFormatters.Lookup(new Dictionary<string, string> { ["x"] = "X" }) },
            new WaDataGridColumn { Field = "kind", Formatter = WaDataGridFormatters.Tag() },
            new WaDataGridColumn { Field = "amount", Formatter = WaDataGridFormatters.Number(culture: English) },
            new WaDataGridColumn { Id = "computed", Formatter = WaDataGridFormatters.Lookup(new Dictionary<string, string> { ["y"] = "Y" }) },
            new WaDataGridColumn { Field = "plain" });

        Assert.Equal("wab.dataGrid.searchFormatted", search.Reference);
        Assert.Equal(
            """{"columns":[{"field":"region","labels":{"EU":"Europe"},"fallback":"Other"},{"field":"state","labels":{"1":"New","2":"Active"}}]}""",
            ArgsOf(search));
        Assert.Throws<ArgumentNullException>(() => WaDataGridSearch.Formatted((IEnumerable<WaDataGridColumn>)null!));
    }

    [Fact]
    public void FilterOptions_ComeFromLabelsAnEnumOrAFormatter()
    {
        Assert.Equal([new WaDataGridFilterOption("EU", "Europe"), new WaDataGridFilterOption("NA", "North America")],
            WaDataGridFilterOptions.FromLabels(new Dictionary<string, string> { ["EU"] = "Europe", ["NA"] = "North America" }));
        Assert.Equal([new WaDataGridFilterOption("Open", "open"), new WaDataGridFilterOption("InProgress", "in progress")],
            WaDataGridFilterOptions.FromEnum<NamedState>(state => state == NamedState.Open ? "open" : "in progress"));
        Assert.Equal([new WaDataGridFilterOption("1"), new WaDataGridFilterOption("2")], WaDataGridFilterOptions.FromEnum<PlainState>());

        var badge = WaDataGridFormatters.Badge(new Dictionary<string, WaVariant> { ["a"] = WaVariant.Brand }, labels: new Dictionary<string, string> { ["a"] = "Alpha" });
        Assert.Equal([new WaDataGridFilterOption("a", "Alpha")], WaDataGridFilterOptions.FromFormatter(badge));
        Assert.Throws<ArgumentException>(() => WaDataGridFilterOptions.FromFormatter(WaDataGridFormatters.Number()));
        Assert.Throws<ArgumentException>(() => WaDataGridFilterOptions.FromFormatter(WaDataGridFormatters.Tag()));
        Assert.Throws<ArgumentNullException>(() => WaDataGridFilterOptions.FromLabels(null!));
    }

    [Fact]
    public void DetailFields_WriteTheLinesWithTheirFormats()
    {
        var detail = WaDataGridRows.DetailFields(
        [
            new WaDataGridDetailField("Name", "customer.name"),
            new WaDataGridDetailField("Due", "due") { Format = WaDataGridFormatters.Date(WaDataGridDateStyle.Long, culture: English) }
        ], "-");

        Assert.Equal("wab.dataGrid.detailFields", detail.Reference);
        Assert.Equal(
            """{"nullText":"-","fields":[{"label":"Name","field":"customer.name"},{"label":"Due","field":"due","format":{"name":"wab.dataGrid.date","args":{"culture":"en-US","dateStyle":"long"}}}]}""",
            ArgsOf(detail));
        Assert.Throws<ArgumentException>(() => WaDataGridRows.DetailFields());
        Assert.Throws<ArgumentException>(() => new WaDataGridDetailField("Name", "a b"));
        Assert.Throws<ArgumentException>(() => new WaDataGridDetailField("", "a"));
        Assert.Throws<ArgumentException>(() => WaDataGridRows.DetailFields(new WaDataGridDetailField("Tag", "tag") { Format = WaDataGridFormatters.Tag() }));
    }

    [Fact]
    public void SelectableField_WritesTheField()
    {
        Assert.Equal("""{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.selectableField"},"args":{"field":"canSelect"}}""",
            WaDataGridRows.SelectableField("canSelect").ToString());
        Assert.Throws<ArgumentException>(() => WaDataGridRows.SelectableField("can select"));
    }

    #region ------ Internals ------

    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    private enum PlainState
    {
        New = 1,
        Active = 2,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    private enum NamedState
    {
        Open,
        InProgress,
    }

    private static string ArgsOf(WaJsFunction function) => function.Args!.Value.GetRawText();

    #endregion
}
