using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the WaPage component (wa-page, new in Web Awesome 3.0.0): each of its 14 named slot fragments and
/// ChildContent render into their slot of wa-page, and the navigation methods throw before the first render.
/// Its attributes and defaults (mobile-breakpoint, navigation-placement, nav-open, view,
/// disable-navigation-toggle) are covered by the CEM-driven RenderedAttributeParityTests.
/// </summary>
public class WaPageIntegrationTests : BunitContext
{
    public WaPageIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DisableSticky_RendersTheSectionTokens_AndNothingWhenUnset()
    {
        var unset = Render<WaPage>().Find("wa-page");
        var set = Render<WaPage>(p => p.Add(x => x.DisableSticky, WaPageSections.Header | WaPageSections.Aside)).Find("wa-page");

        Assert.False(unset.HasAttribute("disable-sticky"));
        Assert.Equal("header aside", set.GetAttribute("disable-sticky"));
    }

    [Theory]
    [InlineData(nameof(WaPage.ChildContent), "")]
    [InlineData(nameof(WaPage.Aside), "aside")]
    [InlineData(nameof(WaPage.Banner), "banner")]
    [InlineData(nameof(WaPage.Footer), "footer")]
    [InlineData(nameof(WaPage.Header), "header")]
    [InlineData(nameof(WaPage.MainFooter), "main-footer")]
    [InlineData(nameof(WaPage.MainHeader), "main-header")]
    [InlineData(nameof(WaPage.Menu), "menu")]
    [InlineData(nameof(WaPage.Navigation), "navigation")]
    [InlineData(nameof(WaPage.NavigationFooter), "navigation-footer")]
    [InlineData(nameof(WaPage.NavigationHeader), "navigation-header")]
    [InlineData(nameof(WaPage.NavigationToggle), "navigation-toggle")]
    [InlineData(nameof(WaPage.NavigationToggleIcon), "navigation-toggle-icon")]
    [InlineData(nameof(WaPage.SkipToContent), "skip-to-content")]
    [InlineData(nameof(WaPage.Subheader), "subheader")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        var cut = Render<WaPage>(parameters => parameters.TryAdd(parameterName, SlotProbe.Fragment));

        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-page")));
    }

    [Fact]
    public void DefaultRender_RendersNoSlotWrappers()
    {
        var cut = Render<WaPage>();

        Assert.Empty(cut.FindAll("[slot]"));
    }

    [Fact]
    public async Task HideNavigationAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaPage();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.HideNavigationAsync());

        Assert.Contains("Cannot hide navigation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ShowNavigationAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaPage();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.ShowNavigationAsync());

        Assert.Contains("Cannot show navigation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ToggleNavigationAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaPage();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.ToggleNavigationAsync());

        Assert.Contains("Cannot toggle navigation: component has not been rendered yet", exception.Message);
    }
}
