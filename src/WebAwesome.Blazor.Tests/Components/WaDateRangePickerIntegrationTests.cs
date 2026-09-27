using System;
using System.Linq;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for WaDateRangePicker (the range-mode wrapper of wa-date-picker since 3.12.0) and for how both range
/// wrappers relate to their single-date counterparts: always mode="range", the WaDateRange value in the element's
/// from/to wire form both ways (a single date as a half-filled range), and the same rendered event bindings as the
/// single-date wrapper of the same element, because both render through one base class (which is what lets the
/// browser suite prove the shared bindings once, on the single-date wrapper).
/// </summary>
public class WaDateRangePickerIntegrationTests : BunitContext
{
    public WaDateRangePickerIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void AlwaysRendersRangeMode()
    {
        var cut = Render<WaDateRangePicker>();

        Assert.Equal("range", cut.Find("wa-date-picker").GetAttribute("mode"));
    }

    [Fact]
    public void Value_RendersTheWireForm_InAnyCulture()
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);

        var cut = Render<WaDateRangePicker>(parameters => parameters.Add(p => p.Value, new WaDateRange(May11, May18)));

        Assert.Equal("2026-05-11/2026-05-18", cut.Find("wa-date-picker").GetAttribute("value"));
    }

    [Fact]
    public void ChangeEvent_WithRange_BindsTheRange()
    {
        WaDateRange? received = null;
        var cut = Render<WaDateRangePicker>(parameters => parameters.Add(p => p.ValueChanged, value => received = value));

        cut.Find("wa-date-picker").Change("2026-05-18/2026-05-11");

        Assert.Equal(new WaDateRange(May11, May18), received);
        Assert.Equal(new WaDateRange(May11, May18), cut.Instance.Value);
    }

    [Fact]
    public void ChangeEvent_WithFirstDateOnly_BindsHalfFilledRange()
    {
        WaDateRange? received = null;
        var cut = Render<WaDateRangePicker>(parameters => parameters.Add(p => p.ValueChanged, value => received = value));

        cut.Find("wa-date-picker").Change("2026-05-11");

        Assert.Equal(new WaDateRange(May11, null), received);
    }

    [Theory]
    [InlineData("")]
    [InlineData("11.05.2026/18.05.2026")]
    public void ChangeEvent_WithEmptyOrInvalidValue_BindsNull(string wireValue)
    {
        WaDateRange? received = new WaDateRange(May11, May18);
        var cut = Render<WaDateRangePicker>(parameters => parameters
            .Add(p => p.Value, new WaDateRange(May11, May18))
            .Add(p => p.ValueChanged, value => received = value));

        cut.Find("wa-date-picker").Change(wireValue);

        Assert.Null(received);
    }

    [Fact]
    public void MinRangeAndMaxRange_Render()
    {
        var cut = Render<WaDateRangePicker>(parameters => parameters.Add(p => p.MinRange, 3).Add(p => p.MaxRange, 10));

        Assert.Equal("3", cut.Find("wa-date-picker").GetAttribute("min-range"));
        Assert.Equal("10", cut.Find("wa-date-picker").GetAttribute("max-range"));
    }

    [Theory]
    [InlineData(typeof(WaDateRangeInput), typeof(WaDateInput))]
    [InlineData(typeof(WaDateRangePicker), typeof(WaDatePicker))]
    public void RangeWrapper_BindsTheSameEventsAsTheSingleDateWrapper(Type rangeWrapper, Type singleWrapper)
    {
        var range = RenderedWrapperCatalog.Observe(rangeWrapper);
        var single = RenderedWrapperCatalog.Observe(singleWrapper);

        Assert.Equal(single.Tag, range.Tag);
        Assert.Equal(single.BaselineHandlers.Order(), range.BaselineHandlers.Order());
        Assert.Equal(
            single.Callbacks.Select(c => $"{c.Name}: {string.Join(",", c.AddedHandlers.Order())}"),
            range.Callbacks.Select(c => $"{c.Name}: {string.Join(",", c.AddedHandlers.Order())}"));
    }

    #region ------ Internals ------

    private static readonly DateOnly May11 = new(2026, 5, 11);
    private static readonly DateOnly May18 = new(2026, 5, 18);

    #endregion
}
