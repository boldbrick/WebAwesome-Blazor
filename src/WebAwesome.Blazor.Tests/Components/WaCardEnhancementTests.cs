using System;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for WaCard component enhancements in Web Awesome 3.0.0-beta.6
/// </summary>
public class WaCardEnhancementTests
{
    [Fact]
    public void WaCard_OrientationProperty_DefaultsToNull()
    {
        // Arrange & Act
        var component = new WaCard();

        // Assert
        Assert.Null(component.Orientation);
    }

    [Fact]
    public void WaCard_OrientationProperty_CanBeSetToHorizontal()
    {
        // Arrange
        var component = new WaCard();

        // Act
        component.Orientation = WaOrientation.Horizontal;

        // Assert
        Assert.Equal(WaOrientation.Horizontal, component.Orientation);
    }

    [Fact]
    public void WaCard_OrientationProperty_CanBeSetToVertical()
    {
        // Arrange
        var component = new WaCard();

        // Act
        component.Orientation = WaOrientation.Vertical;

        // Assert
        Assert.Equal(WaOrientation.Vertical, component.Orientation);
    }

    [Fact]
    public void WaCard_HeaderActionsContentProperty_DefaultsToNull()
    {
        // Arrange & Act
        var component = new WaCard();

        // Assert
        Assert.Null(component.HeaderActionsContent);
    }

    [Fact]
    public void WaCard_FooterActionsContentProperty_DefaultsToNull()
    {
        // Arrange & Act
        var component = new WaCard();

        // Assert
        Assert.Null(component.FooterActionsContent);
    }

    [Fact]
    public void WaCard_WithAllEnhancedProperties_WorksCorrectly()
    {
        // Arrange
        var component = new WaCard();

        // Act
        component.Orientation = WaOrientation.Horizontal;
        component.Appearance = WaAppearance.Filled;
        component.WithHeader = true;
        component.WithFooter = true;

        // Assert
        Assert.Equal(WaOrientation.Horizontal, component.Orientation);
        Assert.Equal(WaAppearance.Filled, component.Appearance);
        Assert.True(component.WithHeader);
        Assert.True(component.WithFooter);
    }

    [Fact]
    public void WaCard_HasAllRequiredRenderFragmentProperties()
    {
        // Arrange & Act
        var component = new WaCard();
        var type = component.GetType();

        // Assert - Verify all RenderFragment properties exist
        Assert.NotNull(type.GetProperty("ChildContent"));
        Assert.NotNull(type.GetProperty("HeaderContent"));
        Assert.NotNull(type.GetProperty("FooterContent"));
        Assert.NotNull(type.GetProperty("MediaContent"));
        Assert.NotNull(type.GetProperty("HeaderActionsContent"));
        Assert.NotNull(type.GetProperty("FooterActionsContent"));
    }

    [Fact]
    public void WaCard_ExistingPropertiesStillWork()
    {
        // Arrange
        var component = new WaCard();

        // Act
        component.Appearance = WaAppearance.Text;
        component.WithHeader = true;
        component.WithFooter = true;
        component.WithMedia = true;

        // Assert
        Assert.Equal(WaAppearance.Text, component.Appearance);
        Assert.True(component.WithHeader);
        Assert.True(component.WithFooter);
        Assert.True(component.WithMedia);
    }
}