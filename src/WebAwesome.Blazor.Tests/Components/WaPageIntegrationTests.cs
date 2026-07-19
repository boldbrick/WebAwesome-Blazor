using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaPage component (wa-page) added in Web Awesome 3.0.0
/// </summary>
public class WaPageIntegrationTests
{
    #region ------ Default Values ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var component = new WaPage();

        // Assert
        Assert.False(component.DisableNavigationToggle);
        Assert.Equal("768px", component.MobileBreakpoint);
        Assert.Null(component.NavigationPlacement);
        Assert.False(component.NavOpen);
        Assert.Null(component.View);
        Assert.Null(component.Element);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void DisableNavigationToggleProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaPage();

        // Act
        component.DisableNavigationToggle = true;

        // Assert
        Assert.True(component.DisableNavigationToggle);
    }

    [Fact]
    public void MobileBreakpointProperty_CanBeOverridden()
    {
        // Arrange
        var component = new WaPage();

        // Act
        component.MobileBreakpoint = "50em";

        // Assert
        Assert.Equal("50em", component.MobileBreakpoint);
    }

    [Fact]
    public void NavigationPlacementProperty_CanBeSetToStartAndEnd()
    {
        // Arrange
        var component = new WaPage();

        // Act & Assert
        component.NavigationPlacement = WaPageNavigationPlacement.Start;
        Assert.Equal(WaPageNavigationPlacement.Start, component.NavigationPlacement);

        component.NavigationPlacement = WaPageNavigationPlacement.End;
        Assert.Equal(WaPageNavigationPlacement.End, component.NavigationPlacement);
    }

    [Fact]
    public void NavOpenProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaPage();

        // Act
        component.NavOpen = true;

        // Assert
        Assert.True(component.NavOpen);
    }

    [Fact]
    public void ViewProperty_CanBeSetToMobileAndDesktop()
    {
        // Arrange
        var component = new WaPage();

        // Act & Assert
        component.View = WaPageView.Mobile;
        Assert.Equal(WaPageView.Mobile, component.View);

        component.View = WaPageView.Desktop;
        Assert.Equal(WaPageView.Desktop, component.View);
    }

    #endregion

    #region ------ Enum ToHtmlValue Mappings ------

    [Fact]
    public void WaPageNavigationPlacement_ToHtmlValue_ReturnsCorrectStrings()
    {
        // Assert
        Assert.Equal("start", WaPageNavigationPlacement.Start.ToHtmlValue());
        Assert.Equal("end", WaPageNavigationPlacement.End.ToHtmlValue());
    }

    [Fact]
    public void WaPageView_ToHtmlValue_ReturnsCorrectStrings()
    {
        // Assert
        Assert.Equal("mobile", WaPageView.Mobile.ToHtmlValue());
        Assert.Equal("desktop", WaPageView.Desktop.ToHtmlValue());
    }

    #endregion

    #region ------ Named Slots ------

    [Fact]
    public void AllNamedSlotProperties_Exist()
    {
        // Arrange & Act
        var type = typeof(WaPage);

        // Assert - all 14 named slots plus the default slot documented in the CEM change report
        Assert.NotNull(type.GetProperty("ChildContent"));
        Assert.NotNull(type.GetProperty("Aside"));
        Assert.NotNull(type.GetProperty("Banner"));
        Assert.NotNull(type.GetProperty("Footer"));
        Assert.NotNull(type.GetProperty("Header"));
        Assert.NotNull(type.GetProperty("MainFooter"));
        Assert.NotNull(type.GetProperty("MainHeader"));
        Assert.NotNull(type.GetProperty("Menu"));
        Assert.NotNull(type.GetProperty("Navigation"));
        Assert.NotNull(type.GetProperty("NavigationFooter"));
        Assert.NotNull(type.GetProperty("NavigationHeader"));
        Assert.NotNull(type.GetProperty("NavigationToggle"));
        Assert.NotNull(type.GetProperty("NavigationToggleIcon"));
        Assert.NotNull(type.GetProperty("SkipToContent"));
        Assert.NotNull(type.GetProperty("Subheader"));
    }

    [Fact]
    public void NamedSlotProperties_CanBeAssignedRenderFragments()
    {
        // Arrange
        var component = new WaPage();
        RenderFragment fragment = builder => { };

        // Act
        component.Aside = fragment;
        component.Banner = fragment;
        component.Footer = fragment;
        component.Header = fragment;
        component.MainFooter = fragment;
        component.MainHeader = fragment;
        component.Menu = fragment;
        component.Navigation = fragment;
        component.NavigationFooter = fragment;
        component.NavigationHeader = fragment;
        component.NavigationToggle = fragment;
        component.NavigationToggleIcon = fragment;
        component.SkipToContent = fragment;
        component.Subheader = fragment;

        // Assert
        Assert.Same(fragment, component.Aside);
        Assert.Same(fragment, component.Banner);
        Assert.Same(fragment, component.Footer);
        Assert.Same(fragment, component.Header);
        Assert.Same(fragment, component.MainFooter);
        Assert.Same(fragment, component.MainHeader);
        Assert.Same(fragment, component.Menu);
        Assert.Same(fragment, component.Navigation);
        Assert.Same(fragment, component.NavigationFooter);
        Assert.Same(fragment, component.NavigationHeader);
        Assert.Same(fragment, component.NavigationToggle);
        Assert.Same(fragment, component.NavigationToggleIcon);
        Assert.Same(fragment, component.SkipToContent);
        Assert.Same(fragment, component.Subheader);
    }

    #endregion

    #region ------ Public Methods (render-guard) ------

    [Fact]
    public async Task HideNavigationAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaPage();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.HideNavigationAsync());

        Assert.Contains("Cannot hide navigation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ShowNavigationAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaPage();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ShowNavigationAsync());

        Assert.Contains("Cannot show navigation: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ToggleNavigationAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaPage();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ToggleNavigationAsync());

        Assert.Contains("Cannot toggle navigation: component has not been rendered yet", exception.Message);
    }

    #endregion
}
