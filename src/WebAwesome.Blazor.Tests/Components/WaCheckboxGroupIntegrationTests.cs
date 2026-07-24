using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the WaCheckboxGroup wrapper (new in WA 3.9.0): defaults, parameter
/// settability, enum mappings, and slot content. WaCheckboxGroup is a grouping wrapper, not a
/// form control - it exposes no value binding and no events.
/// </summary>
public class WaCheckboxGroupIntegrationTests
{
    #region ------ Defaults ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var component = new WaCheckboxGroup();

        // Assert
        Assert.Null(component.Element);
        Assert.Null(component.Label);
        Assert.Null(component.Hint);
        Assert.Null(component.Orientation);
        Assert.Null(component.Size);
        Assert.False(component.Required);
        Assert.False(component.WithHint);
        Assert.False(component.WithLabel);
        Assert.Null(component.ChildContent);
        Assert.Null(component.MarkupLabel);
        Assert.Null(component.MarkupHint);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void LabelAndHint_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaCheckboxGroup();

        // Act
        component.Label = "Interests";
        component.Hint = "Choose as many as you like.";

        // Assert
        Assert.Equal("Interests", component.Label);
        Assert.Equal("Choose as many as you like.", component.Hint);
    }

    [Fact]
    public void Required_WithHint_WithLabel_CanBeSetTogether()
    {
        // Arrange
        var component = new WaCheckboxGroup();

        // Act
        component.Required = true;
        component.WithHint = true;
        component.WithLabel = true;

        // Assert
        Assert.True(component.Required);
        Assert.True(component.WithHint);
        Assert.True(component.WithLabel);
    }

    #endregion

    #region ------ Enum Mappings ------

    [Fact]
    public void Orientation_CanBeSetAndMapsToHtmlValue()
    {
        // Arrange
        var component = new WaCheckboxGroup();

        // Act & Assert
        component.Orientation = WaOrientation.Horizontal;
        Assert.Equal("horizontal", component.Orientation?.ToHtmlValue());

        component.Orientation = WaOrientation.Vertical;
        Assert.Equal("vertical", component.Orientation?.ToHtmlValue());
    }

    [Fact]
    public void Size_CanBeSetAndMapsToHtmlValue()
    {
        // Arrange
        var component = new WaCheckboxGroup();

        // Act & Assert - the group size is applied to all grouped items
        component.Size = WaSize.Small;
        Assert.Equal("small", component.Size?.ToHtmlValue());

        component.Size = WaSize.Large;
        Assert.Equal("large", component.Size?.ToHtmlValue());
    }

    #endregion

    #region ------ Slots ------

    [Fact]
    public void ChildContent_DefaultsToNullAndCanBeSet()
    {
        // Arrange
        var component = new WaCheckboxGroup();
        RenderFragment fragment = builder => { };

        // Assert - default
        Assert.Null(component.ChildContent);

        // Act
        component.ChildContent = fragment;

        // Assert
        Assert.Same(fragment, component.ChildContent);
    }

    [Fact]
    public void MarkupLabelAndMarkupHint_DefaultToNullAndCanBeSet()
    {
        // Arrange
        var component = new WaCheckboxGroup();
        RenderFragment fragment = builder => { };

        // Assert - defaults
        Assert.Null(component.MarkupLabel);
        Assert.Null(component.MarkupHint);

        // Act
        component.MarkupLabel = fragment;
        component.MarkupHint = fragment;

        // Assert
        Assert.Same(fragment, component.MarkupLabel);
        Assert.Same(fragment, component.MarkupHint);
    }

    #endregion
}
