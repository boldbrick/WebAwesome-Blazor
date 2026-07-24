using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaDateInput wrapper (Pro; new in WA 3.8.0): defaults, parameter settability,
/// enum mappings, slot content, event wiring, and imperative method guard clauses.
/// </summary>
public class WaDateInputIntegrationTests
{
    #region ------ Defaults ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        var component = new WaDateInput();

        Assert.Null(component.Element);
        Assert.Null(component.Appearance);
        Assert.Equal(WaDateSelectionMode.Single, component.Mode);
        Assert.Null(component.Min);
        Assert.Null(component.Max);
        Assert.Null(component.MinRange);
        Assert.Null(component.MaxRange);
        Assert.False(component.DisablePast);
        Assert.False(component.DisableFuture);
        Assert.Null(component.FirstDayOfWeek);
        Assert.Null(component.Months);
        Assert.Null(component.PageBy);
        Assert.Null(component.WeekdayFormat);
        Assert.False(component.Open);
        Assert.False(component.WithClear);
        Assert.False(component.WithOutsideDays);
        Assert.False(component.WithWeekNumbers);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void CoreAttributes_CanBeSet()
    {
        var component = new WaDateInput
        {
            Mode = WaDateSelectionMode.Range,
            Min = "2026-01-01",
            Max = "2026-12-31",
            MinRange = 2,
            MaxRange = 30,
            DisablePast = true,
            DisabledDates = "2026-07-04 2026-12-25",
            DisabledDaysOfWeek = "sat sun",
            Months = 2,
            Today = "2026-07-24"
        };

        Assert.Equal(WaDateSelectionMode.Range, component.Mode);
        Assert.Equal("2026-01-01", component.Min);
        Assert.Equal("2026-12-31", component.Max);
        Assert.Equal(2, component.MinRange);
        Assert.Equal(30, component.MaxRange);
        Assert.True(component.DisablePast);
        Assert.Equal("2026-07-04 2026-12-25", component.DisabledDates);
        Assert.Equal("sat sun", component.DisabledDaysOfWeek);
        Assert.Equal(2, component.Months);
        Assert.Equal("2026-07-24", component.Today);
    }

    #endregion

    #region ------ Enum Mappings ------

    [Fact]
    public void Mode_MapsToHtmlValue()
    {
        Assert.Equal("single", WaDateSelectionMode.Single.ToHtmlValue());
        Assert.Equal("range", WaDateSelectionMode.Range.ToHtmlValue());
    }

    [Fact]
    public void FirstDayOfWeek_MapsToHtmlValue()
    {
        Assert.Equal("auto", WaFirstDayOfWeek.Auto.ToHtmlValue());
        Assert.Equal("mon", WaFirstDayOfWeek.Mon.ToHtmlValue());
        Assert.Equal("sun", WaFirstDayOfWeek.Sun.ToHtmlValue());
    }

    [Fact]
    public void PageBy_And_WeekdayFormat_MapToHtmlValue()
    {
        Assert.Equal("months", WaDatePageBy.Months.ToHtmlValue());
        Assert.Equal("single", WaDatePageBy.Single.ToHtmlValue());
        Assert.Equal("narrow", WaWeekdayFormat.Narrow.ToHtmlValue());
        Assert.Equal("long", WaWeekdayFormat.Long.ToHtmlValue());
    }

    #endregion

    #region ------ Slots ------

    [Fact]
    public void SlotContent_CanBeSet()
    {
        var component = new WaDateInput();
        RenderFragment fragment = builder => { };

        component.StartContent = fragment;
        component.EndContent = fragment;
        component.ClearIconContent = fragment;
        component.ExpandIconContent = fragment;
        component.FooterContent = fragment;
        component.PreviousIconContent = fragment;
        component.NextIconContent = fragment;

        Assert.Same(fragment, component.StartContent);
        Assert.Same(fragment, component.EndContent);
        Assert.Same(fragment, component.ClearIconContent);
        Assert.Same(fragment, component.ExpandIconContent);
        Assert.Same(fragment, component.FooterContent);
        Assert.Same(fragment, component.PreviousIconContent);
        Assert.Same(fragment, component.NextIconContent);
    }

    #endregion

    #region ------ Events ------

    [Fact]
    public void PopupEvents_CanAllBeWired()
    {
        var component = new WaDateInput();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });
        var plain = EventCallback.Factory.Create(component, () => { });

        component.OnClear = plain;
        component.OnShow = callback;
        component.OnHide = callback;
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;
        component.OnInvalid = callback;

        Assert.True(component.OnClear.HasDelegate);
        Assert.True(component.OnShow.HasDelegate);
        Assert.True(component.OnHide.HasDelegate);
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
        Assert.True(component.OnInvalid.HasDelegate);
    }

    #endregion

    #region ------ Public Methods (Guard Clauses) ------

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
    }

    [Fact]
    public async Task ClearAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ClearAsync());
    }

    [Fact]
    public async Task ShowAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ShowAsync());
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaDateInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.HideAsync());
    }

    #endregion
}
