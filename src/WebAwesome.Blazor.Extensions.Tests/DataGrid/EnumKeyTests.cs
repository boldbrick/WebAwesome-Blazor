using System;
using System.Text.Json.Serialization;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions.DataGrid;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// The typed enum factories (design section 11.4): the row data reaches the browser as JSON, where an enum value is a
/// number unless the enum has a JSON converter attribute, and the preset functions compare String(value) with the map
/// keys. So each key must be the JSON form of the value, or a map keyed by "Active" would never match the value 2.
/// </summary>
public class EnumKeyTests
{
    [Fact]
    public void Badge_OfAPlainEnum_IsKeyedByTheNumbers()
    {
        var formatter = WaDataGridFormatters.Badge<OrderState>(state => state switch
        {
            OrderState.Active => WaVariant.Success,
            OrderState.Failed => WaVariant.Danger,
            _ => WaVariant.Neutral,
        }, state => state == OrderState.Failed ? null : $"State {state}");

        Assert.Equal(
            """{"variants":{"1":"neutral","2":"success","5":"danger"},"labels":{"1":"State New","2":"State Active"}}""",
            ArgsOf(formatter));
    }

    [Fact]
    public void Badge_OfAStringEnum_IsKeyedByTheNamesTheConverterWrites()
    {
        var formatter = WaDataGridFormatters.Badge<NamedState>(state => state == NamedState.InProgress ? WaVariant.Brand : WaVariant.Neutral,
            icon: state => state == NamedState.InProgress ? "spinner" : null);

        Assert.Equal("""{"variants":{"Open":"neutral","InProgress":"brand"},"icons":{"InProgress":{"name":"spinner"}}}""", ArgsOf(formatter));
        Assert.Equal(["wa-badge", "wa-icon"], formatter.Tags);
    }

    [Fact]
    public void Lookup_OfAnEnumWithMemberNames_IsKeyedByTheMemberNames()
    {
        var formatter = WaDataGridFormatters.Lookup<RenamedState>(state => state.ToString().ToUpperInvariant());

        Assert.Equal("""{"labels":{"to-do":"TODO","in-progress":"INPROGRESS"}}""", ArgsOf(formatter));
    }

    [Fact]
    public void Lookup_OfAnEnum_CountsAnAliasOnce()
    {
        var formatter = WaDataGridFormatters.Lookup<AliasedState>(state => state.ToString());

        // Current and Latest share the value 1; the first name wins
        Assert.Equal("""{"labels":{"0":"Old","1":"Current"}}""", ArgsOf(formatter));
    }

    [Fact]
    public void Ordinal_OfAnEnum_WithoutAnOrder_UsesTheDeclaredValues()
    {
        Assert.Equal("""{"values":["1","2","5"]}""", ArgsOf(WaDataGridComparators.Ordinal<OrderState>()));
    }

    [Fact]
    public void Ordinal_OfAnEnum_UsesTheGivenOrderAndTheJsonForm()
    {
        Assert.Equal("""{"values":["5","1"]}""", ArgsOf(WaDataGridComparators.Ordinal(OrderState.Failed, OrderState.New)));
        Assert.Equal("""{"values":["InProgress","Open"]}""", ArgsOf(WaDataGridComparators.Ordinal(NamedState.InProgress, NamedState.Open)));
    }

    [Fact]
    public void Ordinal_OfAnEnum_RejectsADuplicate()
    {
        Assert.Throws<ArgumentException>(() => WaDataGridComparators.Ordinal(OrderState.New, OrderState.New));
    }

    #region ------ Internals ------

    // values with gaps, as a database enum often has
    private enum OrderState
    {
        New = 1,
        Active = 2,
        Failed = 5,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    private enum NamedState
    {
        Open,
        InProgress,
    }

    [JsonConverter(typeof(JsonStringEnumConverter<RenamedState>))]
    private enum RenamedState
    {
        [JsonStringEnumMemberName("to-do")]
        ToDo,

        [JsonStringEnumMemberName("in-progress")]
        InProgress,
    }

    private enum AliasedState
    {
        Old = 0,
        Current = 1,
        Latest = 1,
    }

    private static string ArgsOf(WaJsFunction function) => function.Args!.Value.GetRawText();

    #endregion
}
