using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaDatePicker wrapper (new in WA 3.8.0). Unlike the date/time inputs it is not a
/// form-associated control, so it exposes manual Value/ValueChanged two-way binding rather than deriving from
/// WaInputBase. Covers defaults, parameter settability, enum mappings, the typed focus-day/view-change event
/// args, and imperative method guard clauses.
/// </summary>
public class WaDatePickerIntegrationTests
{
    #region ------ Defaults ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        var component = new WaDatePicker();

        Assert.Null(component.Element);
        Assert.Null(component.Value);
        Assert.Null(component.Mode);
        Assert.Null(component.View);
        Assert.Null(component.Min);
        Assert.Null(component.Max);
        Assert.Null(component.FirstDayOfWeek);
        Assert.Null(component.FocusedDate);
        Assert.Null(component.Locale);
        Assert.Null(component.Months);
        Assert.Null(component.PageBy);
        Assert.Null(component.Size);
        Assert.Null(component.WeekdayFormat);
        Assert.False(component.Disabled);
        Assert.False(component.Readonly);
        Assert.False(component.WithOutsideDays);
        Assert.False(component.WithWeekNumbers);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void CoreAttributes_CanBeSet()
    {
        var component = new WaDatePicker
        {
            Value = "2026-07-24",
            Mode = WaDateSelectionMode.Range,
            View = WaDatePickerView.Months,
            Min = "2026-01-01",
            Max = "2026-12-31",
            FocusedDate = "2026-07-24",
            Locale = "en-US",
            Months = 2,
            Size = WaSize.Large,
            Readonly = true
        };

        Assert.Equal("2026-07-24", component.Value);
        Assert.Equal(WaDateSelectionMode.Range, component.Mode);
        Assert.Equal(WaDatePickerView.Months, component.View);
        Assert.Equal("2026-01-01", component.Min);
        Assert.Equal("2026-12-31", component.Max);
        Assert.Equal("2026-07-24", component.FocusedDate);
        Assert.Equal("en-US", component.Locale);
        Assert.Equal(2, component.Months);
        Assert.Equal(WaSize.Large, component.Size);
        Assert.True(component.Readonly);
    }

    #endregion

    #region ------ Enum Mappings ------

    [Fact]
    public void View_MapsToHtmlValue()
    {
        Assert.Equal("days", WaDatePickerView.Days.ToHtmlValue());
        Assert.Equal("months", WaDatePickerView.Months.ToHtmlValue());
        Assert.Equal("years", WaDatePickerView.Years.ToHtmlValue());
    }

    #endregion

    #region ------ Binding &amp; Events ------

    [Fact]
    public async Task ValueChanged_CanBeWired()
    {
        var component = new WaDatePicker();
        string? received = null;
        component.ValueChanged = EventCallback.Factory.Create<string?>(new object(), v => received = v);

        await component.ValueChanged.InvokeAsync("2026-07-24");

        Assert.True(component.ValueChanged.HasDelegate);
        Assert.Equal("2026-07-24", received);
    }

    [Fact]
    public async Task OnFocusDay_CanBeWired_AndCarriesIsoDate()
    {
        var component = new WaDatePicker();
        WaDatePickerFocusDayEventArgs? received = null;
        component.OnFocusDay = EventCallback.Factory.Create<WaDatePickerFocusDayEventArgs>(new object(), a => received = a);

        await component.OnFocusDay.InvokeAsync(new WaDatePickerFocusDayEventArgs { Date = "2026-07-24" });

        Assert.True(component.OnFocusDay.HasDelegate);
        Assert.Equal("2026-07-24", received?.Date);
    }

    [Fact]
    public async Task OnViewChange_CanBeWired_AndCarriesViewAndDate()
    {
        var component = new WaDatePicker();
        WaDatePickerViewChangeEventArgs? received = null;
        component.OnViewChange = EventCallback.Factory.Create<WaDatePickerViewChangeEventArgs>(new object(), a => received = a);

        await component.OnViewChange.InvokeAsync(new WaDatePickerViewChangeEventArgs { View = "years", Date = "2026-01-01" });

        Assert.True(component.OnViewChange.HasDelegate);
        Assert.Equal("years", received?.View);
        Assert.Equal("2026-01-01", received?.Date);
    }

    [Fact]
    public void OnInput_CanBeWired()
    {
        var component = new WaDatePicker();
        component.OnInput = EventCallback.Factory.Create<ChangeEventArgs>(component, () => { });
        Assert.True(component.OnInput.HasDelegate);
    }

    #endregion

    #region ------ Public Methods (Guard Clauses) ------

    [Fact]
    public async Task ClearAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDatePicker();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ClearAsync());
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDatePicker();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    [Fact]
    public async Task GoToDateAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDatePicker();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.GoToDateAsync("2026-07-24"));
    }

    [Fact]
    public async Task GoToTodayAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDatePicker();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.GoToTodayAsync());
    }

    #endregion
}
