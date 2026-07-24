using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaAccordionItem wrapper (new in WA 3.8.0): defaults, parameter settability,
/// slot content, and imperative method guard clauses.
/// </summary>
public class WaAccordionItemIntegrationTests
{
    #region ------ Defaults ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        var component = new WaAccordionItem();

        Assert.Null(component.Element);
        Assert.Null(component.Label);
        Assert.False(component.Expanded);
        Assert.False(component.Disabled);
        Assert.Null(component.ChildContent);
        Assert.Null(component.LabelContent);
        Assert.Null(component.IconContent);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void LabelExpandedDisabled_CanBeSet()
    {
        var component = new WaAccordionItem
        {
            Label = "Section 1",
            Expanded = true,
            Disabled = true
        };

        Assert.Equal("Section 1", component.Label);
        Assert.True(component.Expanded);
        Assert.True(component.Disabled);
    }

    [Fact]
    public void SlotContent_CanBeSet()
    {
        var component = new WaAccordionItem();
        RenderFragment fragment = builder => { };

        component.LabelContent = fragment;
        component.IconContent = fragment;
        component.ChildContent = fragment;

        Assert.Same(fragment, component.LabelContent);
        Assert.Same(fragment, component.IconContent);
        Assert.Same(fragment, component.ChildContent);
    }

    #endregion

    #region ------ Public Methods (Guard Clauses) ------

    [Fact]
    public async Task ExpandAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordionItem();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ExpandAsync());
    }

    [Fact]
    public async Task CollapseAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordionItem();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.CollapseAsync());
    }

    [Fact]
    public async Task ToggleAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordionItem();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.ToggleAsync());
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaAccordionItem();
        await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
    }

    #endregion
}
