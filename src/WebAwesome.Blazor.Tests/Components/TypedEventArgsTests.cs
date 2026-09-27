using System.Text.Json;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The typed event payloads the JS initializer projects (lib.module.js specialArgs for wa-mutation, wa-resize,
/// wa-selection-change and wa-content-change) deserialize into their records with the web defaults Blazor's event
/// dispatch uses; the JSON is the exact shape the initializer builds.
/// </summary>
public class TypedEventArgsTests
{
    [Fact]
    public void MutationArgs_ReadTheProjectedRecords()
    {
        const string payload = """{"mutationRecords":[{"type":"attributes","attributeName":"data-state","oldValue":"off"},{"type":"childList","attributeName":null,"oldValue":null}]}""";

        var args = JsonSerializer.Deserialize<MutationEventArgs>(payload, JsonSerializerOptions.Web)!;

        Assert.Equal(2, args.MutationRecords!.Count);
        Assert.Equal(new WaMutationRecord { Type = WaMutationType.Attributes, AttributeName = "data-state", OldValue = "off" }, args.MutationRecords[0]);
        Assert.Equal(WaMutationType.ChildList, args.MutationRecords[1].Type);
        Assert.Null(args.MutationRecords[1].AttributeName);
    }

    [Fact]
    public void ResizeArgs_ReadTheContentRect()
    {
        const string payload = """{"resizeObserverEntries":[{"contentRect":{"x":1,"y":2,"width":300.5,"height":40,"top":2,"right":301.5,"bottom":42,"left":1}},{"contentRect":null}]}""";

        var args = JsonSerializer.Deserialize<ResizeEventArgs>(payload, JsonSerializerOptions.Web)!;

        var rect = args.ResizeObserverEntries![0].ContentRect!;
        Assert.Equal((1d, 2d, 300.5, 40d, 2d, 301.5, 42d, 1d), (rect.X, rect.Y, rect.Width, rect.Height, rect.Top, rect.Right, rect.Bottom, rect.Left));
        Assert.Null(args.ResizeObserverEntries[1].ContentRect);
    }

    [Fact]
    public void TreeSelectionArgs_ReadTheElementInfo()
    {
        const string payload = """{"selection":[{"id":"docs","textContent":"Documents"},{"id":null,"textContent":"Photos"}]}""";

        var args = JsonSerializer.Deserialize<WaTreeSelectionChangeEventArgs>(payload, JsonSerializerOptions.Web)!;

        Assert.Equal([new WaElementInfo { Id = "docs", TextContent = "Documents" }, new WaElementInfo { TextContent = "Photos" }], args.Selection!);
    }

    [Fact]
    public void ContentChangeArgs_ReadTheCountAndItems()
    {
        const string payload = """{"count":1,"items":[{"id":"tip-2","textContent":"Use keyboard shortcuts"}]}""";

        var args = JsonSerializer.Deserialize<WaContentChangeEventArgs>(payload, JsonSerializerOptions.Web)!;

        Assert.Equal(1, args.Count);
        Assert.Equal("tip-2", Assert.Single(args.Items!).Id);
    }
}
