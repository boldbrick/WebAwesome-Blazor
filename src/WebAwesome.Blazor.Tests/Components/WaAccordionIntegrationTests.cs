using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaAccordion wrapper (new in WA 3.8.0): defaults, parameter settability,
/// enum mappings, event wiring, and imperative method guard clauses.
/// </summary>
public class WaAccordionIntegrationTests
{
    #region ------ Defaults ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        var component = new WaAccordion();

        Assert.Null(component.Element);
        Assert.Equal(WaAppearance.Outlined, component.Appearance);
        Assert.Null(component.HeadingLevel);
        Assert.Equal(WaIconPlacement.End, component.IconPlacement);
        Assert.Equal(WaAccordionMode.Multiple, component.Mode);
        Assert.Null(component.ChildContent);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void HeadingLevel_CanBeSetAndRetrieved()
    {
        var component = new WaAccordion { HeadingLevel = "2" };
        Assert.Equal("2", component.HeadingLevel);
    }

    #endregion

    #region ------ Enum Mappings ------

    [Fact]
    public void Mode_CanBeSetToEachValue_AndMapsToHtmlValue()
    {
        Assert.Equal("single", WaAccordionMode.Single.ToHtmlValue());
        Assert.Equal("single-collapsible", WaAccordionMode.SingleCollapsible.ToHtmlValue());
        Assert.Equal("multiple", WaAccordionMode.Multiple.ToHtmlValue());
    }

    [Fact]
    public void Appearance_MapsToHtmlValue()
    {
        var component = new WaAccordion { Appearance = WaAppearance.Plain };
        Assert.Equal("plain", component.Appearance.ToHtmlValue());
    }

    #endregion

    #region ------ Events ------

    [Fact]
    public void ExpandCollapseEvents_CanAllBeWired()
    {
        var component = new WaAccordion();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        component.OnExpand = callback;
        component.OnCollapse = callback;
        component.OnAfterExpand = callback;
        component.OnAfterCollapse = callback;

        Assert.True(component.OnExpand.HasDelegate);
        Assert.True(component.OnCollapse.HasDelegate);
        Assert.True(component.OnAfterExpand.HasDelegate);
        Assert.True(component.OnAfterCollapse.HasDelegate);
    }

    #endregion

    #region ------ Public Methods (Guard Clauses) ------

    [Fact]
    public async Task ExpandAllAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordion();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ExpandAllAsync());
    }

    [Fact]
    public async Task CollapseAllAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordion();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.CollapseAllAsync());
    }

    #endregion
}
