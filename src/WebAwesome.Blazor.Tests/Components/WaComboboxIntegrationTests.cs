using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for WaCombobox component, mirroring the WaSelect test suite conventions:
/// default values, parameter settability, enum ToHtmlValue mappings, multiple-selection support,
/// slot content, and event callback wiring
/// </summary>
public class WaComboboxIntegrationTests
{
    #region ------ Defaults ------

    [Fact]
    public void Constructor_WithDefaultValues_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var component = new WaCombobox();

        // Assert
        Assert.Null(component.Element);
        Assert.Null(component.Placeholder);
        Assert.Null(component.Appearance);
        Assert.False(component.Pill);
        Assert.False(component.WithClear);
        Assert.False(component.Multiple);
        Assert.False(component.AllowCustomValue);
        Assert.Null(component.MaxOptionsVisible);
        Assert.Null(component.Placement);
        Assert.False(component.Open);
        Assert.False(component.WithHint);
        Assert.False(component.WithLabel);
        Assert.Null(component.SelectedValues);
    }

    #endregion

    #region ------ Parameter Setting ------

    [Fact]
    public void Placeholder_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaCombobox();

        // Act
        component.Placeholder = "Select a fruit";

        // Assert
        Assert.Equal("Select a fruit", component.Placeholder);
    }

    [Fact]
    public void Pill_WithClear_Multiple_AllowCustomValue_CanBeSetTogether()
    {
        // Arrange
        var component = new WaCombobox();

        // Act
        component.Pill = true;
        component.WithClear = true;
        component.Multiple = true;
        component.AllowCustomValue = true;

        // Assert
        Assert.True(component.Pill);
        Assert.True(component.WithClear);
        Assert.True(component.Multiple);
        Assert.True(component.AllowCustomValue);
    }

    [Fact]
    public void MaxOptionsVisible_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaCombobox();

        // Act
        component.MaxOptionsVisible = 3;

        // Assert
        Assert.Equal(3, component.MaxOptionsVisible);
    }

    [Fact]
    public void Open_WithHint_WithLabel_CanBeSetTogether()
    {
        // Arrange
        var component = new WaCombobox();

        // Act
        component.Open = true;
        component.WithHint = true;
        component.WithLabel = true;

        // Assert
        Assert.True(component.Open);
        Assert.True(component.WithHint);
        Assert.True(component.WithLabel);
    }

    #endregion

    #region ------ Enum Mappings ------

    [Fact]
    public void Appearance_CanBeSetToEachValue()
    {
        // Arrange
        var component = new WaCombobox();

        // Act & Assert
        component.Appearance = WaInputAppearance.Outlined;
        Assert.Equal(WaInputAppearance.Outlined, component.Appearance);
        Assert.Equal("outlined", component.Appearance?.ToHtmlValue());

        component.Appearance = WaInputAppearance.Filled;
        Assert.Equal("filled", component.Appearance?.ToHtmlValue());

        component.Appearance = WaInputAppearance.FilledOutlined;
        Assert.Equal("filled-outlined", component.Appearance?.ToHtmlValue());
    }

    [Fact]
    public void Placement_CanBeSetAndMapsToHtmlValue()
    {
        // Arrange
        var component = new WaCombobox();

        // Act
        component.Placement = WaPlacement.BottomStart;

        // Assert
        Assert.Equal(WaPlacement.BottomStart, component.Placement);
        Assert.Equal("bottom-start", component.Placement?.ToHtmlValue());
    }

    #endregion

    #region ------ Multiple Selection Support ------

    [Fact]
    public void SelectedValues_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaCombobox { Multiple = true };
        var values = new[] { "apple", "banana" };

        // Act
        component.SelectedValues = values;

        // Assert
        Assert.Equal(values, component.SelectedValues);
    }

    [Fact]
    public async Task SelectedValuesChanged_CanBeWired()
    {
        // Arrange
        var component = new WaCombobox();
        var receiver = new object();
        string[]? received = null;
        var callback = EventCallback.Factory.Create<string[]?>(receiver, values => received = values);

        // Act
        component.SelectedValuesChanged = callback;
        await component.SelectedValuesChanged.InvokeAsync(new[] { "apple" });

        // Assert
        Assert.True(component.SelectedValuesChanged.HasDelegate);
        Assert.NotNull(received);
        Assert.Equal(new[] { "apple" }, received);
    }

    #endregion

    #region ------ Slots ------

    [Fact]
    public void StartAndEndIconNames_CanBeSetAndRetrieved()
    {
        // Arrange
        var component = new WaCombobox();

        // Act
        component.StartIconName = "search";
        component.EndIconName = "chevron-down";

        // Assert
        Assert.Equal("search", component.StartIconName);
        Assert.Equal("chevron-down", component.EndIconName);
    }

    [Fact]
    public void StartAndEndContent_DefaultToNullAndCanBeSet()
    {
        // Arrange
        var component = new WaCombobox();
        RenderFragment fragment = builder => { };

        // Assert - defaults
        Assert.Null(component.StartContent);
        Assert.Null(component.EndContent);

        // Act
        component.StartContent = fragment;
        component.EndContent = fragment;

        // Assert
        Assert.Same(fragment, component.StartContent);
        Assert.Same(fragment, component.EndContent);
    }

    [Fact]
    public void ClearIconContentAndExpandIconContent_DefaultToNullAndCanBeSet()
    {
        // Arrange
        var component = new WaCombobox();
        RenderFragment fragment = builder => { };

        // Assert - defaults
        Assert.Null(component.ClearIconContent);
        Assert.Null(component.ExpandIconContent);

        // Act
        component.ClearIconContent = fragment;
        component.ExpandIconContent = fragment;

        // Assert
        Assert.Same(fragment, component.ClearIconContent);
        Assert.Same(fragment, component.ExpandIconContent);
    }

    [Fact]
    public void ChildContent_DefaultsToNullAndCanBeSet()
    {
        // Arrange
        var component = new WaCombobox();
        RenderFragment fragment = builder => { };

        // Assert
        Assert.Null(component.ChildContent);

        // Act
        component.ChildContent = fragment;

        // Assert
        Assert.Same(fragment, component.ChildContent);
    }

    #endregion

    #region ------ Events ------

    [Fact]
    public void OnClear_CanBeWired()
    {
        // Arrange
        var component = new WaCombobox();
        var callback = EventCallback.Factory.Create(component, () => { });

        // Act
        component.OnClear = callback;

        // Assert
        Assert.True(component.OnClear.HasDelegate);
    }

    [Fact]
    public void OverlayEvents_CanAllBeWired()
    {
        // Arrange
        var component = new WaCombobox();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnShow = callback;
        component.OnHide = callback;
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;
        component.OnInvalid = callback;

        // Assert
        Assert.True(component.OnShow.HasDelegate);
        Assert.True(component.OnHide.HasDelegate);
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
        Assert.True(component.OnInvalid.HasDelegate);
    }

    #endregion

    #region ------ Public Methods (Guard Clauses) ------

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaCombobox();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.BlurAsync());

        Assert.Contains("Cannot blur: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaCombobox();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.FocusAsync());

        Assert.Contains("Cannot focus: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task HideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaCombobox();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.HideAsync());

        Assert.Contains("Cannot hide: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ShowAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaCombobox();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.ShowAsync());

        Assert.Contains("Cannot show: component has not been rendered yet", exception.Message);
    }

    #endregion
}
