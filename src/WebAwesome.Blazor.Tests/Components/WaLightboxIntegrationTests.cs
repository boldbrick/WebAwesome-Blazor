using System;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaLightbox and WaLightboxItem components (Web Awesome 3.15.0): the parameter to slot
/// mapping, the icon name shortcuts and their precedence behind the fragments (WaIconSugarTests pattern), the
/// navigation and zoom method guards with their recorded interop calls and the FocusAsync preventScroll argument
/// (WaCarouselIntegrationTests pattern), and the deserialization of the projected wa-item-change payload the JS
/// initializer builds (TypedEventArgsTests pattern). Attributes and defaults are covered by
/// RenderedAttributeParityTests, events by EventCallbackBindingParityTests.
/// </summary>
public class WaLightboxIntegrationTests : BunitContext
{
    public WaLightboxIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region ------ WaLightbox slots ------

    [Theory]
    [InlineData(nameof(WaLightbox.ChildContent), "")]
    [InlineData(nameof(WaLightbox.ActionsContent), "actions")]
    [InlineData(nameof(WaLightbox.CloseIconContent), "close-icon")]
    [InlineData(nameof(WaLightbox.NextIconContent), "next-icon")]
    [InlineData(nameof(WaLightbox.PreviousIconContent), "previous-icon")]
    [InlineData(nameof(WaLightbox.ZoomInIconContent), "zoom-in-icon")]
    [InlineData(nameof(WaLightbox.ZoomOutIconContent), "zoom-out-icon")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        // Arrange & Act
        var cut = Render<WaLightbox>(parameters => parameters.TryAdd(parameterName, SlotProbe.Fragment));

        // Assert
        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-lightbox")));
    }

    [Theory]
    [InlineData(nameof(WaLightbox.CloseIconName), "close-icon")]
    [InlineData(nameof(WaLightbox.NextIconName), "next-icon")]
    [InlineData(nameof(WaLightbox.PreviousIconName), "previous-icon")]
    [InlineData(nameof(WaLightbox.ZoomInIconName), "zoom-in-icon")]
    [InlineData(nameof(WaLightbox.ZoomOutIconName), "zoom-out-icon")]
    public void IconName_RendersWaIconIntoItsSlot(string parameterName, string slot)
    {
        // Arrange & Act
        var cut = Render<WaLightbox>(parameters => parameters.TryAdd(parameterName, "star"));

        // Assert
        var icon = Assert.Single(cut.FindAll("wa-lightbox > wa-icon"));
        Assert.Equal(slot, icon.GetAttribute("slot"));
        Assert.Equal("star", icon.GetAttribute("name"));
    }

    [Theory]
    [InlineData(nameof(WaLightbox.CloseIconContent), nameof(WaLightbox.CloseIconName), "close-icon")]
    [InlineData(nameof(WaLightbox.NextIconContent), nameof(WaLightbox.NextIconName), "next-icon")]
    [InlineData(nameof(WaLightbox.PreviousIconContent), nameof(WaLightbox.PreviousIconName), "previous-icon")]
    [InlineData(nameof(WaLightbox.ZoomInIconContent), nameof(WaLightbox.ZoomInIconName), "zoom-in-icon")]
    [InlineData(nameof(WaLightbox.ZoomOutIconContent), nameof(WaLightbox.ZoomOutIconName), "zoom-out-icon")]
    public void IconContent_WinsOverIconName(string contentParameter, string nameParameter, string slot)
    {
        // Arrange & Act
        var cut = Render<WaLightbox>(parameters =>
        {
            parameters.TryAdd(contentParameter, SlotProbe.Fragment);
            parameters.TryAdd(nameParameter, "star");
        });

        // Assert
        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-lightbox")));
        Assert.Empty(cut.FindAll("wa-icon"));
    }

    [Fact]
    public void WithoutIconNames_RendersNoIcons()
    {
        // Arrange & Act
        var cut = Render<WaLightbox>();

        // Assert
        Assert.Empty(cut.FindAll("wa-icon"));
    }

    #endregion

    #region ------ WaLightbox methods ------

    [Theory]
    [InlineData(NextMethod, "Cannot show the next item: component has not been rendered yet")]
    [InlineData(PreviousMethod, "Cannot show the previous item: component has not been rendered yet")]
    [InlineData(ZoomInMethod, "Cannot zoom in: component has not been rendered yet")]
    [InlineData(ZoomOutMethod, "Cannot zoom out: component has not been rendered yet")]
    public async Task Method_WithNullElement_ThrowsInvalidOperationException(string method, string message)
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var lightbox = runtime.CreateUnrendered<WaLightbox>();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Invoke(lightbox, method));

        // Assert
        Assert.Contains(message, exception.Message);
        Assert.Empty(runtime.Module.Invocations);
    }

    [Theory]
    [InlineData(NextMethod, "next")]
    [InlineData(PreviousMethod, "previous")]
    [InlineData(ZoomInMethod, "zoomIn")]
    [InlineData(ZoomOutMethod, "zoomOut")]
    public async Task Method_WithRenderedElement_InvokesTheElementMethod(string method, string elementMethod)
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var lightbox = runtime.CreateRendered<WaLightbox>();

        // Act
        await Invoke(lightbox, method);

        // Assert
        Assert.Empty(runtime.Module.AssertInvokedMethod(elementMethod));
    }

    #endregion

    #region ------ WaLightbox OnItemChange payload ------

    [Fact]
    public void ItemChangeArgs_ReadTheProjectedItems()
    {
        // Arrange - the shape the wa-item-change projection of the JS initializer builds
        const string payload = """{"index":1,"previousIndex":0,"item":{"src":"b.mp4","alt":"Clip","caption":"A clip","type":"video"},"previousItem":{"src":"a.jpg","alt":"Photo","caption":"","type":"image"}}""";

        // Act
        var args = JsonSerializer.Deserialize<WaLightboxItemChangeEventArgs>(payload, JsonSerializerOptions.Web)!;

        // Assert
        Assert.Equal(1, args.Index);
        Assert.Equal(0, args.PreviousIndex);
        Assert.Equal(WaLightboxItemType.Video, args.Item!.Type);
        Assert.Equal(("b.mp4", "Clip", "A clip"), (args.Item.Src, args.Item.Alt, args.Item.Caption));
        Assert.Equal(WaLightboxItemType.Image, args.PreviousItem!.Type);
        Assert.Equal(("a.jpg", "Photo", string.Empty), (args.PreviousItem.Src, args.PreviousItem.Alt, args.PreviousItem.Caption));
    }

    [Fact]
    public void ItemChangeArgs_WithNullPreviousItem_ReadNull()
    {
        // Arrange - the first item shown after opening has no previous item
        const string payload = """{"index":0,"previousIndex":0,"item":{"src":"a.jpg","alt":"","caption":"","type":"image"},"previousItem":null}""";

        // Act
        var args = JsonSerializer.Deserialize<WaLightboxItemChangeEventArgs>(payload, JsonSerializerOptions.Web)!;

        // Assert
        Assert.Equal("a.jpg", args.Item!.Src);
        Assert.Null(args.PreviousItem);
    }

    [Fact]
    public void ItemChangeArgs_WithUnknownItemType_Throws()
    {
        // Arrange - the value set of the type is closed
        const string payload = """{"index":0,"previousIndex":0,"item":{"src":"a","alt":"","caption":"","type":"audio"},"previousItem":null}""";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<WaLightboxItemChangeEventArgs>(payload, JsonSerializerOptions.Web));
    }

    [Theory]
    [InlineData("image", WaLightboxItemType.Image)]
    [InlineData("video", WaLightboxItemType.Video)]
    public void ItemType_ReadsItsWireForm(string wire, WaLightboxItemType expected)
    {
        // Arrange & Act
        var info = JsonSerializer.Deserialize<WaLightboxItemInfo>($"{{\"type\":\"{wire}\"}}", JsonSerializerOptions.Web)!;

        // Assert
        Assert.Equal(expected, info.Type);
    }

    #endregion

    #region ------ WaLightboxItem ------

    [Fact]
    public void Item_PlayIconName_RendersWaIconIntoPlayIconSlot()
    {
        // Arrange & Act
        var cut = Render<WaLightboxItem>(parameters => parameters.Add(p => p.PlayIconName, "circle-play"));

        // Assert
        var icon = cut.Find("wa-lightbox-item > wa-icon");
        Assert.Equal("play-icon", icon.GetAttribute("slot"));
        Assert.Equal("circle-play", icon.GetAttribute("name"));
    }

    [Fact]
    public void Item_WithoutPlayIconName_RendersNoIcon()
    {
        // Arrange & Act
        var cut = Render<WaLightboxItem>();

        // Assert
        Assert.Empty(cut.FindAll("wa-icon"));
    }

    [Fact]
    public void Item_PlayIconContent_WinsOverPlayIconName()
    {
        // Arrange & Act
        var cut = Render<WaLightboxItem>(parameters => parameters
            .Add(p => p.PlayIconContent, SlotProbe.Fragment)
            .Add(p => p.PlayIconName, "circle-play"));

        // Assert
        Assert.Equal("play-icon", SlotProbe.SlotOf(cut.Find("wa-lightbox-item")));
        Assert.Empty(cut.FindAll("wa-icon"));
    }

    [Fact]
    public void Item_ChildContent_RendersIntoTheDefaultSlot()
    {
        // Arrange & Act - the default slot holds the thumbnail
        var cut = Render<WaLightboxItem>(parameters => parameters.Add(p => p.ChildContent, SlotProbe.Fragment));

        // Assert
        Assert.Equal(string.Empty, SlotProbe.SlotOf(cut.Find("wa-lightbox-item")));
    }

    [Fact]
    public async Task Item_FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaLightboxItem>().FocusAsync());

        // Assert
        Assert.Contains("Cannot focus the lightbox item: component has not been rendered yet", exception.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Item_FocusAsync_WithRenderedElement_InvokesFocusWithPreventScroll(bool preventScroll)
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var item = runtime.CreateRendered<WaLightboxItem>();

        // Act
        await item.FocusAsync(preventScroll);

        // Assert
        var arguments = runtime.Module.AssertInvokedMethod("focus");
        Assert.Equal($"{{\"preventScroll\":{(preventScroll ? "true" : "false")}}}", JsonSerializer.Serialize(Assert.Single(arguments)));
    }

    [Fact]
    public async Task Item_FocusAsync_WithoutArgument_DoesNotPreventScroll()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var item = runtime.CreateRendered<WaLightboxItem>();

        // Act
        await item.FocusAsync();

        // Assert
        Assert.Equal("{\"preventScroll\":false}", JsonSerializer.Serialize(Assert.Single(runtime.Module.AssertInvokedMethod("focus"))));
    }

    #endregion

    #region ------ Internals ------

    private const string NextMethod = "next";
    private const string PreviousMethod = "previous";
    private const string ZoomInMethod = "zoomIn";
    private const string ZoomOutMethod = "zoomOut";

    private static Task Invoke(WaLightbox lightbox, string method) => method switch
    {
        NextMethod => lightbox.NextAsync(),
        PreviousMethod => lightbox.PreviousAsync(),
        ZoomInMethod => lightbox.ZoomInAsync(),
        ZoomOutMethod => lightbox.ZoomOutAsync(),
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
    };

    #endregion
}
