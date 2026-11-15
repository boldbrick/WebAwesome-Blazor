using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Asserts the module call of the wrapper methods whose element call is more than a plain element method: the color
/// picker's hex string from getFormattedValue, the copy button's click on its button part, the dialog and drawer focus
/// by the element's open rule, the plain projection of a tree item's child items, and the show and hide methods that
/// complete when the action is cancelled. The C# half only (the module function and its arguments, with the fake
/// interop module); the browser proof is the method suite (tools\e2e, methods-*.spec.js).
/// </summary>
public class WaElementMethodCallTests
{
    #region ------ WaColorPicker.GetHexStringAsync ------

    [Theory]
    [InlineData(false, "hex")]
    [InlineData(true, "hexa")]
    public async Task WaColorPicker_GetHexStringAsync_FormatsTheCurrentValue(bool opacity, string expectedFormat)
    {
        // Arrange - getHexString(hue, saturation, brightness, alpha) converts its arguments, not the current value
        using var runtime = new RecordingJSRuntime();
        var picker = runtime.CreateRendered<WaColorPicker>();
        picker.Opacity = opacity;
        runtime.Module.NextResult = "#ff0000";

        // Act
        var hex = await picker.GetHexStringAsync();

        // Assert
        Assert.Equal(new object[] { expectedFormat }, runtime.Module.AssertInvokedMethod("getFormattedValue"));
        Assert.Equal("#ff0000", hex);
    }

    #endregion

    #region ------ WaCopyButton.CopyAsync ------

    [Fact]
    public async Task WaCopyButton_CopyAsync_ClicksTheButtonPart()
    {
        // Arrange - a click on the host does not reach the copy handler in the shadow root
        using var runtime = new RecordingJSRuntime();

        // Act
        await runtime.CreateRendered<WaCopyButton>().CopyAsync();

        // Assert
        Assert.Equal(new object?[] { "button" }, runtime.Module.AssertCalledFunction(ClickShadowPartFunction));
    }

    #endregion

    #region ------ WaDialog / WaDrawer.FocusAsync ------

    [Fact]
    public async Task WaDialog_FocusAsync_FocusesByTheOpenRule_OfTheDialogPanel()
    {
        // Arrange - the host takes no focus; the module focuses the autofocus element or the dialog property
        using var runtime = new RecordingJSRuntime();

        // Act
        await runtime.CreateRendered<WaDialog>().FocusAsync();

        // Assert
        Assert.Equal(new object?[] { "dialog" }, runtime.Module.AssertCalledFunction(FocusOverlayFunction));
    }

    [Fact]
    public async Task WaDrawer_FocusAsync_FocusesByTheOpenRule_OfTheDrawerPanel()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();

        // Act
        await runtime.CreateRendered<WaDrawer>().FocusAsync();

        // Assert
        Assert.Equal(new object?[] { "drawer" }, runtime.Module.AssertCalledFunction(FocusOverlayFunction));
    }

    #endregion

    #region ------ WaTreeItem.GetChildrenItemsAsync ------

    [Fact]
    public async Task WaTreeItem_GetChildrenItemsAsync_ReturnsThePlainProjection()
    {
        // Arrange - the live elements of getChildrenItems() do not serialize; the module projects them
        using var runtime = new RecordingJSRuntime();
        runtime.Module.NextResult = JsonDocument.Parse(TreeChildrenJson).RootElement;

        // Act
        var children = await runtime.CreateRendered<WaTreeItem>().GetChildrenItemsAsync();

        // Assert
        Assert.Empty(runtime.Module.AssertCalledFunction(GetTreeItemChildrenFunction));
        Assert.Equal(2, children.GetArrayLength());
        Assert.Equal("Apple", children[0].GetProperty("text").GetString());
        Assert.Equal(JsonValueKind.Null, children[1].GetProperty("id").ValueKind);
        Assert.True(children[1].GetProperty("disabled").GetBoolean());
    }

    #endregion

    #region ------ Show and hide that complete when cancelled ------

    /// <summary>
    /// Every wrapper show and hide whose element method waits for its after-event, with the element method and the
    /// cancel event the wrapper names.
    /// </summary>
    public static TheoryData<string, string, string> CancelableCalls => new()
    {
        { "WaDetails.ShowAsync", "show", "wa-show" },
        { "WaDetails.HideAsync", "hide", "wa-hide" },
        { "WaPopover.ShowAsync", "show", "wa-show" },
        { "WaPopover.HideAsync", "hide", "wa-hide" },
        { "WaTooltip.ShowAsync", "show", "wa-show" },
        { "WaTooltip.HideAsync", "hide", "wa-hide" },
        { "WaTimeInput.ShowAsync", "show", "wa-show" },
        { "WaTimeInput.HideAsync", "hide", "wa-hide" },
        { "WaCombobox.ShowAsync", "show", "wa-show" },
        { "WaCombobox.HideAsync", "hide", "wa-hide" },
        { "WaSelect.ShowAsync", "show", "wa-show" },
        { "WaColorPicker.ShowAsync", "show", "wa-show" },
        { "WaDateInput.ShowAsync", "show", "wa-show" },
        { "WaDateRangeInput.HideAsync", "hide", "wa-hide" },
    };

    [Theory]
    [MemberData(nameof(CancelableCalls))]
    public async Task ShowAndHide_CompleteWhenTheActionIsCancelled(string call, string method, string cancelEvent)
    {
        // Arrange - the element's Promise waits for the after-event, which a cancelled action never dispatches; the
        // module completes the call on a cancelled cancel event
        using var runtime = new RecordingJSRuntime();

        // Act
        await Calls[call](runtime);

        // Assert
        Assert.Equal(new object?[] { method, cancelEvent }, runtime.Module.AssertCalledFunction(InvokeCancelableMethodFunction));
    }

    #endregion

    #region ------ Internals ------

    private const string ClickShadowPartFunction = "clickShadowPart";
    private const string FocusOverlayFunction = "focusOverlay";
    private const string GetTreeItemChildrenFunction = "getTreeItemChildren";
    private const string InvokeCancelableMethodFunction = "invokeCancelableMethod";

    // the projection of two child items, as the module returns it
    private const string TreeChildrenJson =
        """
        [
          { "id": "apple", "text": "Apple", "selected": false, "expanded": false, "disabled": false, "lazy": false },
          { "id": null, "text": "Banana", "selected": false, "expanded": false, "disabled": true, "lazy": false }
        ]
        """;

    // the calls of CancelableCalls, by name
    private static readonly Dictionary<string, Func<RecordingJSRuntime, Task>> Calls = new(StringComparer.Ordinal)
    {
        ["WaDetails.ShowAsync"] = r => r.CreateRendered<WaDetails>().ShowAsync(),
        ["WaDetails.HideAsync"] = r => r.CreateRendered<WaDetails>().HideAsync(),
        ["WaPopover.ShowAsync"] = r => r.CreateRendered<WaPopover>().ShowAsync(),
        ["WaPopover.HideAsync"] = r => r.CreateRendered<WaPopover>().HideAsync(),
        ["WaTooltip.ShowAsync"] = r => r.CreateRendered<WaTooltip>().ShowAsync(),
        ["WaTooltip.HideAsync"] = r => r.CreateRendered<WaTooltip>().HideAsync(),
        ["WaTimeInput.ShowAsync"] = r => r.CreateRendered<WaTimeInput>().ShowAsync(),
        ["WaTimeInput.HideAsync"] = r => r.CreateRendered<WaTimeInput>().HideAsync(),
        ["WaCombobox.ShowAsync"] = r => r.CreateRendered<WaCombobox>().ShowAsync(),
        ["WaCombobox.HideAsync"] = r => r.CreateRendered<WaCombobox>().HideAsync(),
        ["WaSelect.ShowAsync"] = r => r.CreateRendered<WaSelect>().ShowAsync(),
        ["WaColorPicker.ShowAsync"] = r => r.CreateRendered<WaColorPicker>().ShowAsync(),
        ["WaDateInput.ShowAsync"] = r => r.CreateRendered<WaDateInput>().ShowAsync(),
        ["WaDateRangeInput.HideAsync"] = r => r.CreateRendered<WaDateRangeInput>().HideAsync(),
    };

    #endregion
}
