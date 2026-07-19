using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaComparison component (wa-comparison) added in Web Awesome 3.0.0
/// </summary>
public class WaComparisonIntegrationTests
{
    #region ------ Default Values ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var component = new WaComparison();

        // Assert
        Assert.Null(component.Position);
        Assert.Null(component.Class);
        Assert.Null(component.Style);
        Assert.Null(component.ChildContent);
        Assert.Null(component.BeforeContent);
        Assert.Null(component.AfterContent);
        Assert.Null(component.HandleContent);
        Assert.Null(component.Element);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void PositionProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaComparison();

        // Act
        component.Position = 42.5m;

        // Assert
        Assert.Equal(42.5m, component.Position);
    }

    [Fact]
    public void ClassAndStyleProperties_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaComparison();

        // Act
        component.Class = "custom-comparison";
        component.Style = "width: 100%;";

        // Assert
        Assert.Equal("custom-comparison", component.Class);
        Assert.Equal("width: 100%;", component.Style);
    }

    #endregion

    #region ------ Slots ------

    [Fact]
    public void SlotProperties_CanBeAssignedRenderFragments()
    {
        // Arrange
        var component = new WaComparison();
        RenderFragment before = builder => { };
        RenderFragment after = builder => { };
        RenderFragment handle = builder => { };
        RenderFragment child = builder => { };

        // Act
        component.BeforeContent = before;
        component.AfterContent = after;
        component.HandleContent = handle;
        component.ChildContent = child;

        // Assert
        Assert.Same(before, component.BeforeContent);
        Assert.Same(after, component.AfterContent);
        Assert.Same(handle, component.HandleContent);
        Assert.Same(child, component.ChildContent);
    }

    #endregion

    #region ------ Events ------

    [Fact]
    public void OnChange_DefaultsToNoDelegate()
    {
        // Arrange & Act
        var component = new WaComparison();

        // Assert
        Assert.False(component.OnChange.HasDelegate);
    }

    [Fact]
    public void OnChange_CanBeWiredWithEventCallback()
    {
        // Arrange
        var component = new WaComparison();
        var invoked = false;
        var callback = EventCallback.Factory.Create<System.EventArgs>(component, () => invoked = true);

        // Act
        component.OnChange = callback;

        // Assert
        Assert.True(component.OnChange.HasDelegate);
        Assert.False(invoked);
    }

    #endregion

    #region ------ Additional Attributes ------

    [Fact]
    public void AdditionalAttributes_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaComparison();
        var attributes = new System.Collections.Generic.Dictionary<string, object>
        {
            ["data-testid"] = "comparison-1"
        };

        // Act
        component.AdditionalAttributes = attributes;

        // Assert
        Assert.Same(attributes, component.AdditionalAttributes);
    }

    #endregion
}
