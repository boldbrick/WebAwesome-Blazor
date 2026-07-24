using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaTimeInput wrapper (new in WA 3.8.0): defaults, parameter settability,
/// enum mappings, slot content, event wiring, and imperative method guard clauses.
/// </summary>
public class WaTimeInputIntegrationTests
{
    #region ------ Defaults ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        var component = new WaTimeInput();

        Assert.Null(component.Element);
        Assert.Null(component.Appearance);
        Assert.Null(component.HourFormat);
        Assert.Null(component.Min);
        Assert.Null(component.Max);
        Assert.Null(component.Step);
        Assert.Null(component.Placement);
        Assert.Null(component.Distance);
        Assert.False(component.Open);
        Assert.False(component.Pill);
        Assert.False(component.WithClear);
        Assert.False(component.WithNow);
        Assert.False(component.WithHint);
        Assert.False(component.WithLabel);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void CoreAttributes_CanBeSet()
    {
        var component = new WaTimeInput
        {
            Min = "09:00:00",
            Max = "17:00:00",
            Step = "any",
            Distance = 8,
            Open = true,
            Pill = true,
            WithClear = true,
            WithNow = true
        };

        Assert.Equal("09:00:00", component.Min);
        Assert.Equal("17:00:00", component.Max);
        Assert.Equal("any", component.Step);
        Assert.Equal(8, component.Distance);
        Assert.True(component.Open);
        Assert.True(component.Pill);
        Assert.True(component.WithClear);
        Assert.True(component.WithNow);
    }

    #endregion

    #region ------ Enum Mappings ------

    [Fact]
    public void HourFormat_MapsToHtmlValue()
    {
        Assert.Equal("auto", WaTimeHourFormat.Auto.ToHtmlValue());
        Assert.Equal("12", WaTimeHourFormat.Twelve.ToHtmlValue());
        Assert.Equal("24", WaTimeHourFormat.TwentyFour.ToHtmlValue());
    }

    [Fact]
    public void Placement_MapsToHtmlValue()
    {
        var component = new WaTimeInput { Placement = WaPlacement.TopEnd };
        Assert.Equal("top-end", component.Placement?.ToHtmlValue());
    }

    #endregion

    #region ------ Slots ------

    [Fact]
    public void SlotContent_CanBeSet()
    {
        var component = new WaTimeInput();
        RenderFragment fragment = builder => { };

        component.StartContent = fragment;
        component.EndContent = fragment;
        component.ClearIconContent = fragment;
        component.ExpandIconContent = fragment;
        component.FooterContent = fragment;

        Assert.Same(fragment, component.StartContent);
        Assert.Same(fragment, component.EndContent);
        Assert.Same(fragment, component.ClearIconContent);
        Assert.Same(fragment, component.ExpandIconContent);
        Assert.Same(fragment, component.FooterContent);
    }

    #endregion

    #region ------ Events ------

    [Fact]
    public void PopupEvents_CanAllBeWired()
    {
        var component = new WaTimeInput();
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
        var component = new WaTimeInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaTimeInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
    }

    [Fact]
    public async Task ShowAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaTimeInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ShowAsync());
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaTimeInput();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.HideAsync());
    }

    #endregion
}
