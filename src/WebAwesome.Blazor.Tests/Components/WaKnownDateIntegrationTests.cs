using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaKnownDate wrapper (new in WA 3.8.0): defaults, parameter settability,
/// enum mappings, event wiring, and imperative method guard clauses.
/// </summary>
public class WaKnownDateIntegrationTests
{
    #region ------ Defaults ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        var component = new WaKnownDate();

        Assert.Null(component.Element);
        Assert.Null(component.Appearance);
        Assert.Null(component.Locale);
        Assert.Null(component.Min);
        Assert.Null(component.Max);
        Assert.False(component.Pill);
        Assert.False(component.WithHint);
        Assert.False(component.WithLabel);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void CoreAttributes_CanBeSet()
    {
        var component = new WaKnownDate
        {
            Locale = "en-GB",
            Min = "1900-01-01",
            Max = "2026-12-31",
            Pill = true
        };

        Assert.Equal("en-GB", component.Locale);
        Assert.Equal("1900-01-01", component.Min);
        Assert.Equal("2026-12-31", component.Max);
        Assert.True(component.Pill);
    }

    [Fact]
    public void Appearance_MapsToHtmlValue()
    {
        var component = new WaKnownDate { Appearance = WaInputAppearance.FilledOutlined };
        Assert.Equal("filled-outlined", component.Appearance?.ToHtmlValue());
    }

    #endregion

    #region ------ Events ------

    [Fact]
    public void OnInvalid_CanBeWired()
    {
        var component = new WaKnownDate();
        component.OnInvalid = EventCallback.Factory.Create<EventArgs>(component, () => { });
        Assert.True(component.OnInvalid.HasDelegate);
    }

    #endregion

    #region ------ Public Methods (Guard Clauses) ------

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaKnownDate();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaKnownDate();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
    }

    #endregion
}
