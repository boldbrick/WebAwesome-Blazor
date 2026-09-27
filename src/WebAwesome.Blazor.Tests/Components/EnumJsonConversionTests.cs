using System;
using System.Collections.Generic;
using System.Text.Json;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The enums that travel as JSON (event args deserialized from the JS initializer's payloads, the data grid column
/// model pushed as a JS property) convert to and from exactly Web Awesome's strings, with the web defaults Blazor's
/// event dispatch and JS interop use; an unknown string fails instead of silently becoming a default member.
/// </summary>
public class EnumJsonConversionTests
{
    [Theory]
    [InlineData("days", WaDatePickerView.Days)]
    [InlineData("months", WaDatePickerView.Months)]
    [InlineData("years", WaDatePickerView.Years)]
    public void ViewChangeArgs_ReadTheView(string view, WaDatePickerView expected)
    {
        var args = JsonSerializer.Deserialize<WaDatePickerViewChangeEventArgs>($"{{\"view\":\"{view}\",\"date\":null}}", JsonSerializerOptions.Web);

        Assert.Equal(expected, args!.View);
    }

    [Fact]
    public void ViewChangeArgs_ReadNull_AsNoView()
    {
        var args = JsonSerializer.Deserialize<WaDatePickerViewChangeEventArgs>("{\"view\":null,\"date\":null}", JsonSerializerOptions.Web);

        Assert.Null(args!.View);
    }

    [Fact]
    public void UnknownValue_Fails()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<WaDatePickerViewChangeEventArgs>("{\"view\":\"decades\"}", JsonSerializerOptions.Web));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<WaDatePickerViewChangeEventArgs>("{\"view\":\"Days\"}", JsonSerializerOptions.Web));
    }

    [Theory]
    [InlineData("start", WaRatingHoverPhase.Start)]
    [InlineData("move", WaRatingHoverPhase.Move)]
    [InlineData("end", WaRatingHoverPhase.End)]
    public void RatingHoverArgs_ReadThePhase(string phase, WaRatingHoverPhase expected)
    {
        var args = JsonSerializer.Deserialize<WaRatingHoverEventArgs>($"{{\"phase\":\"{phase}\",\"value\":2.5}}", JsonSerializerOptions.Web);

        Assert.Equal(expected, args!.Phase);
        Assert.Equal(2.5m, args.Value);
    }

    [Theory]
    [InlineData("\"left\"", WaDataGridPinSide.Left)]
    [InlineData("\"right\"", WaDataGridPinSide.Right)]
    [InlineData("null", null)]
    public void ColumnPinArgs_ReadTheSide(string side, WaDataGridPinSide? expected)
    {
        var args = JsonSerializer.Deserialize<WaDataGridColumnPinEventArgs>($"{{\"column\":\"name\",\"side\":{side}}}", JsonSerializerOptions.Web);

        Assert.Equal(expected, args!.Side);
    }

    [Fact]
    public void ColumnModel_WritesTheElementStrings()
    {
        var column = new WaDataGridColumn
        {
            Field = "price",
            Align = WaDataGridAlign.End,
            HeaderAlign = WaDataGridAlign.Center,
            SortFn = WaDataGridSortFn.AlphanumericCaseSensitive,
            FilterType = WaDataGridFilterType.ExactMatch,
            Pinned = WaDataGridPinSide.Left,
            Aggregation = WaDataGridAggregation.UniqueCount
        };

        var json = JsonSerializer.SerializeToElement(column, JsonSerializerOptions.Web);

        Assert.Equal("end", json.GetProperty("align").GetString());
        Assert.Equal("center", json.GetProperty("headerAlign").GetString());
        Assert.Equal("alphanumericCaseSensitive", json.GetProperty("sortFn").GetString());
        Assert.Equal("equals", json.GetProperty("filterType").GetString());
        Assert.Equal("left", json.GetProperty("pinned").GetString());
        Assert.Equal("uniqueCount", json.GetProperty("aggregation").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("sortUndefined").ValueKind);
    }

    [Theory]
    [InlineData(WaDataGridSortUndefined.First, "\"first\"")]
    [InlineData(WaDataGridSortUndefined.Last, "\"last\"")]
    [InlineData(WaDataGridSortUndefined.Lower, "-1")]
    [InlineData(WaDataGridSortUndefined.Higher, "1")]
    public void SortUndefined_WritesStringsAndNumbers_AsTheUnionTypesThem(WaDataGridSortUndefined value, string expected)
    {
        var json = JsonSerializer.SerializeToElement(new WaDataGridColumn { SortUndefined = value }, JsonSerializerOptions.Web);

        Assert.Equal(expected, json.GetProperty("sortUndefined").GetRawText());
        Assert.Equal(value, JsonSerializer.Deserialize<WaDataGridColumn>(json, JsonSerializerOptions.Web)!.SortUndefined);
    }

    [Fact]
    public void EveryConvertedEnum_RoundTripsEveryMember()
    {
        foreach (var enumType in ConvertedEnums)
        {
            foreach (var member in Enum.GetValues(enumType))
            {
                var json = JsonSerializer.Serialize(member, enumType, JsonSerializerOptions.Web);
                Assert.Equal(member, JsonSerializer.Deserialize(json, enumType, JsonSerializerOptions.Web));
            }
        }
    }

    #region ------ Internals ------

    private static readonly IReadOnlyList<Type> ConvertedEnums =
    [
        typeof(WaDatePickerView), typeof(WaRatingHoverPhase), typeof(WaMutationType), typeof(WaDataGridPinSide),
        typeof(WaDataGridAlign), typeof(WaDataGridSortFn), typeof(WaDataGridSortUndefined), typeof(WaDataGridFilterType),
        typeof(WaDataGridAggregation)
    ];

    #endregion
}
