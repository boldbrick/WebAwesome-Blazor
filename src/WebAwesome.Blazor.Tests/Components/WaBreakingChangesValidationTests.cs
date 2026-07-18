using System;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Validation tests for breaking changes in Web Awesome 3.0.0-beta.6 upgrade
/// </summary>
public class WaBreakingChangesValidationTests
{
    #region ------ WaIcon Breaking Changes ------

    [Fact]
    public void WaIcon_AutoWidthProperty_DefaultsToFalse()
    {
        // Arrange & Act
        var component = new WaIcon();

        // Assert
        Assert.False(component.AutoWidth);
    }

    [Fact]
    public void WaIcon_SwapOpacityProperty_DefaultsToFalse()
    {
        // Arrange & Act
        var component = new WaIcon();

        // Assert
        Assert.False(component.SwapOpacity);
    }

    [Fact]
    public void WaIcon_AutoWidthAndSwapOpacity_CanBeSetTogether()
    {
        // Arrange
        var component = new WaIcon();

        // Act
        component.AutoWidth = true;
        component.SwapOpacity = true;

        // Assert
        Assert.True(component.AutoWidth);
        Assert.True(component.SwapOpacity);
    }

    [Fact]
    public void WaIcon_WithAllProperties_WorksCorrectly()
    {
        // Arrange
        var component = new WaIcon();

        // Act
        component.Name = "test-icon";
        component.Library = "custom";
        component.AutoWidth = true;
        component.SwapOpacity = true;

        // Assert
        Assert.Equal("test-icon", component.Name);
        Assert.Equal("custom", component.Library);
        Assert.True(component.AutoWidth);
        Assert.True(component.SwapOpacity);
    }

    #endregion

    #region ------ WaDetails Breaking Changes ------

    [Fact]
    public void WaDetails_IconPlacementProperty_DefaultsToEnd()
    {
        // Arrange & Act
        var component = new WaDetails();

        // Assert
        Assert.Equal(WaIconPlacement.End, component.IconPlacement);
    }

    [Fact]
    public void WaDetails_IconPlacementProperty_CanBeSetToStart()
    {
        // Arrange
        var component = new WaDetails();

        // Act
        component.IconPlacement = WaIconPlacement.Start;

        // Assert
        Assert.Equal(WaIconPlacement.Start, component.IconPlacement);
    }

    [Fact]
    public void WaDetails_WithIconPlacementAndOtherProperties_WorksCorrectly()
    {
        // Arrange
        var component = new WaDetails();

        // Act
        component.Summary = "Test Summary";
        component.Open = true;
        component.IconPlacement = WaIconPlacement.Start;

        // Assert
        Assert.Equal("Test Summary", component.Summary);
        Assert.True(component.Open);
        Assert.Equal(WaIconPlacement.Start, component.IconPlacement);
    }

    #endregion

    #region ------ WaButtonGroup Breaking Changes ------

    [Fact]
    public void WaButtonGroup_DoesNotHaveSizeProperty()
    {
        // Arrange & Act
        var component = new WaButtonGroup();
        var type = component.GetType();

        // Assert - Size property should not exist
        var sizeProperty = type.GetProperty("Size");
        Assert.Null(sizeProperty);
    }

    [Fact]
    public void WaButtonGroup_HasCorrectRemainingProperties()
    {
        // Arrange
        var component = new WaButtonGroup();

        // Act
        component.Label = "Test Group";
        component.Orientation = WaOrientation.Vertical;
        component.Variant = WaVariant.Brand;

        // Assert
        Assert.Equal("Test Group", component.Label);
        Assert.Equal(WaOrientation.Vertical, component.Orientation);
        Assert.Equal(WaVariant.Brand, component.Variant);
    }

    #endregion

    #region ------ Enum Breaking Changes ------

    [Fact]
    public void WaIconPlacement_HasCorrectValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(WaIconPlacement), WaIconPlacement.Start));
        Assert.True(Enum.IsDefined(typeof(WaIconPlacement), WaIconPlacement.End));
    }

    [Fact]
    public void WaIconPlacement_ToHtmlValue_ReturnsCorrectStrings()
    {
        // Assert
        Assert.Equal("start", WaIconPlacement.Start.ToHtmlValue());
        Assert.Equal("end", WaIconPlacement.End.ToHtmlValue());
    }

    #endregion
}