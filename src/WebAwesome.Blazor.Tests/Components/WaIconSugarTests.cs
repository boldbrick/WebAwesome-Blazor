using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for the icon convenience ("icon sugar") parameters added to components with icon-shaped slots.
/// </summary>
public class WaIconSugarTests
{
    [Fact]
    public void WaButton_IconNameProperties_DefaultToNull()
    {
        // Arrange & Act
        var component = new WaButton();

        // Assert
        Assert.Null(component.StartIconName);
        Assert.Null(component.EndIconName);
    }

    [Fact]
    public void WaButton_IconNameProperties_CanBeSetAlongsideExistingProperties()
    {
        // Arrange
        var component = new WaButton();

        // Act
        component.StartIconName = "star";
        component.EndIconName = "arrow-right";
        component.Variant = WaVariant.Brand;
        component.Pill = true;

        // Assert
        Assert.Equal("star", component.StartIconName);
        Assert.Equal("arrow-right", component.EndIconName);
        Assert.Equal(WaVariant.Brand, component.Variant);
        Assert.True(component.Pill);
    }

    [Fact]
    public void WaButton_StartContentFragment_CanCoexistWithStartIconNameProperty()
    {
        // Arrange
        var component = new WaButton();

        // Act
        component.StartContent = builder => { };
        component.StartIconName = "star";

        // Assert - both are settable; BuildRenderTree prefers the fragment at render time
        Assert.NotNull(component.StartContent);
        Assert.Equal("star", component.StartIconName);
    }

    [Fact]
    public void WaCallout_IconNameProperty_DefaultsToNull()
    {
        // Arrange & Act
        var component = new WaCallout();

        // Assert
        Assert.Null(component.IconName);
    }

    [Fact]
    public void WaCallout_IconNameProperty_CanBeSetAlongsideExistingProperties()
    {
        // Arrange
        var component = new WaCallout();

        // Act
        component.IconName = "info-circle";
        component.Variant = WaVariant.Warning;
        component.Size = WaSize.Large;

        // Assert
        Assert.Equal("info-circle", component.IconName);
        Assert.Equal(WaVariant.Warning, component.Variant);
        Assert.Equal(WaSize.Large, component.Size);
    }

    [Fact]
    public void WaDetails_IconNameProperties_DefaultToNull()
    {
        // Arrange & Act
        var component = new WaDetails();

        // Assert
        Assert.Null(component.ExpandIconName);
        Assert.Null(component.CollapseIconName);
    }

    [Fact]
    public void WaDetails_IconNameProperties_CanBeSetAlongsideExistingProperties()
    {
        // Arrange
        var component = new WaDetails();

        // Act
        component.ExpandIconName = "chevron-down";
        component.CollapseIconName = "chevron-up";
        component.Summary = "More info";
        component.Open = true;

        // Assert
        Assert.Equal("chevron-down", component.ExpandIconName);
        Assert.Equal("chevron-up", component.CollapseIconName);
        Assert.Equal("More info", component.Summary);
        Assert.True(component.Open);
    }

    [Fact]
    public void WaDetails_CollapseIconFragment_CanCoexistWithCollapseIconNameProperty()
    {
        // Arrange
        var component = new WaDetails();

        // Act
        component.CollapseIcon = builder => { };
        component.CollapseIconName = "chevron-up";

        // Assert - both are settable; BuildRenderTree prefers the fragment at render time
        Assert.NotNull(component.CollapseIcon);
        Assert.Equal("chevron-up", component.CollapseIconName);
    }
}
