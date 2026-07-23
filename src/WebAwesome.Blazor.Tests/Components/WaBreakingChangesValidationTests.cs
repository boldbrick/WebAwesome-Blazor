using System;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Validation tests for breaking changes in Web Awesome 3.0.0-beta.6 and 3.1.0 upgrades
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

        // Assert
        Assert.Equal("Test Group", component.Label);
        Assert.Equal(WaOrientation.Vertical, component.Orientation);
    }

    #endregion

    #region ------ 3.1.0 Breaking Changes ------

    [Fact]
    public void WaButtonGroup_DoesNotHaveVariantProperty()
    {
        // Arrange & Act
        var component = new WaButtonGroup();
        var type = component.GetType();

        // Assert - Variant property was removed in Web Awesome 3.1.0
        var variantProperty = type.GetProperty("Variant");
        Assert.Null(variantProperty);
    }

    [Fact]
    public void WaButton_KeepsFormProperty()
    {
        // Arrange & Act
        var component = new WaButton();

        // Assert - the form attribute left the 3.1.0 CEM (moved to native ElementInternals form
        // association) but remains functional, so the wrapper deliberately keeps the parameter
        component.Form = "external-form";
        Assert.Equal("external-form", component.Form);
    }

    #endregion

    #region ------ 3.2.0 Breaking Changes ------

    [Fact]
    public void WaQrCode_FillAndBackground_DefaultToNull()
    {
        // Arrange & Act - Web Awesome 3.2.0 changed both attribute defaults to '' (inherit theme),
        // so the wrapper now exposes them as nullable strings defaulting to null instead of the
        // previous non-nullable "black"/"white" defaults
        var component = new WaQrCode();

        // Assert
        Assert.Null(component.Fill);
        Assert.Null(component.Background);
    }

    [Fact]
    public void WaQrCode_FillAndBackground_CanBeSet()
    {
        // Arrange
        var component = new WaQrCode();

        // Act
        component.Fill = "#ff0000";
        component.Background = "transparent";

        // Assert
        Assert.Equal("#ff0000", component.Fill);
        Assert.Equal("transparent", component.Background);
    }

    [Fact]
    public void WaIcon_RotateProperty_DefaultsToZero()
    {
        // Arrange & Act
        var component = new WaIcon();

        // Assert
        Assert.Equal(0, component.Rotate);
    }

    [Fact]
    public void WaIcon_AnimationAndFlipProperties_DefaultToNull()
    {
        // Arrange & Act
        var component = new WaIcon();

        // Assert
        Assert.Null(component.Animation);
        Assert.Null(component.Flip);
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