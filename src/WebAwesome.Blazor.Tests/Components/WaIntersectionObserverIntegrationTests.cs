using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaIntersectionObserver component focusing on validation
/// </summary>
public class WaIntersectionObserverIntegrationTests
{
    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var component = new WaIntersectionObserver();

        // Assert
        Assert.Null(component.Element);
        Assert.Null(component.Threshold);
        Assert.Null(component.Root);
        Assert.Null(component.RootMargin);
        Assert.Null(component.IntersectClass);
        Assert.False(component.Disabled);
    }

    [Fact]
    public void ThresholdProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaIntersectionObserver();
        const string testThreshold = "0.0 0.5 1.0";

        // Act
        component.Threshold = testThreshold;

        // Assert
        Assert.Equal(testThreshold, component.Threshold);
    }

    [Fact]
    public void RootProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaIntersectionObserver();
        const string testRoot = "viewport-element";

        // Act
        component.Root = testRoot;

        // Assert
        Assert.Equal(testRoot, component.Root);
    }

    [Fact]
    public void RootMarginProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaIntersectionObserver();
        const string testRootMargin = "10px 20px";

        // Act
        component.RootMargin = testRootMargin;

        // Assert
        Assert.Equal(testRootMargin, component.RootMargin);
    }

    [Fact]
    public void IntersectClassProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaIntersectionObserver();
        const string testClass = "intersecting";

        // Act
        component.IntersectClass = testClass;

        // Assert
        Assert.Equal(testClass, component.IntersectClass);
    }

    [Fact]
    public void DisabledProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaIntersectionObserver();

        // Act
        component.Disabled = true;

        // Assert
        Assert.True(component.Disabled);
    }

    [Fact]
    public void ThresholdProperty_WithMultipleValues_HandlesCorrectly()
    {
        // Arrange
        var component = new WaIntersectionObserver();
        const string complexThreshold = "0.0 0.25 0.5 0.75 1.0";

        // Act
        component.Threshold = complexThreshold;

        // Assert
        Assert.Equal(complexThreshold, component.Threshold);
    }

    [Fact]
    public void RootMarginProperty_WithComplexCSSValues_HandlesCorrectly()
    {
        // Arrange
        var component = new WaIntersectionObserver();
        const string complexMargin = "10px 20px 30px 40px";

        // Act
        component.RootMargin = complexMargin;

        // Assert
        Assert.Equal(complexMargin, component.RootMargin);
    }

    [Fact]
    public void AllProperties_WhenSetTogether_WorkCorrectly()
    {
        // Arrange
        var component = new WaIntersectionObserver();

        // Act
        component.Threshold = "0.0 0.5 1.0";
        component.Root = "custom-viewport";
        component.RootMargin = "10px";
        component.IntersectClass = "visible";
        component.Disabled = false;

        // Assert
        Assert.Equal("0.0 0.5 1.0", component.Threshold);
        Assert.Equal("custom-viewport", component.Root);
        Assert.Equal("10px", component.RootMargin);
        Assert.Equal("visible", component.IntersectClass);
        Assert.False(component.Disabled);
    }
}