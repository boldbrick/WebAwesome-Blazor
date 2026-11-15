using System;
using System.Collections.Generic;
using System.Text.Json;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Models;

/// <summary>
/// The JSON descriptor of a <see cref="WaJsFunction"/> (the wire format the resolver in webawesome-functions.js reads),
/// its immutability and equality, and the checks C# can make before the browser resolves the reference.
/// </summary>
public class WaJsFunctionTests
{
    [Fact]
    public void Registered_WritesTheNameReference()
    {
        Assert.Equal("""{"$wab":"fn","ref":{"kind":"registered","name":"myapp.state"}}""", Json(WaJsFunction.Registered("myapp.state")));
    }

    [Fact]
    public void Module_WithoutAnExportName_WritesTheDefaultExport()
    {
        Assert.Equal(
            """{"$wab":"fn","ref":{"kind":"module","url":"./Pages/Orders.razor.js","export":"default"}}""",
            Json(WaJsFunction.Module("./Pages/Orders.razor.js")));
    }

    [Fact]
    public void Module_WritesTheExportName_AndTheCrossOriginOptInOnlyWhenSet()
    {
        Assert.Equal(
            """{"$wab":"fn","ref":{"kind":"module","url":"https://cdn.example.com/cells.js","export":"stateCell","allowCrossOrigin":true}}""",
            Json(WaJsFunction.Module("https://cdn.example.com/cells.js", "stateCell", allowCrossOrigin: true)));
        Assert.DoesNotContain("allowCrossOrigin", Json(WaJsFunction.Module("/js/cells.js", "stateCell")));
    }

    [Fact]
    public void Global_WritesThePath()
    {
        Assert.Equal("""{"$wab":"fn","ref":{"kind":"global","path":"myApp.grid.stateCell"}}""", Json(WaJsFunction.Global("myApp.grid.stateCell")));
    }

    [Fact]
    public void WithArgs_WritesTheArgumentsInCamelCase_AndUsesWritesTheTags()
    {
        var function = WaJsFunction.Module("./Pages/Orders.razor.js", "stateCell")
            .WithArgs(new { ShowIcon = true, Variants = new Dictionary<string, string> { ["ACT"] = "success" }, Max = 5 })
            .Uses("wa-badge", "wa-icon");

        Assert.Equal(
            """{"$wab":"fn","ref":{"kind":"module","url":"./Pages/Orders.razor.js","export":"stateCell"},"args":{"showIcon":true,"variants":{"ACT":"success"},"max":5},"uses":["wa-badge","wa-icon"]}""",
            Json(function));
    }

    [Fact]
    public void WithArgs_SerializesAtOnce_SoALaterChangeOfTheObjectDoesNotChangeTheReference()
    {
        var labels = new Dictionary<string, string> { ["ACT"] = "Active" };
        var function = WaJsFunction.Registered("myapp.lookup").WithArgs(labels);
        labels["ACT"] = "Changed";

        Assert.Equal("Active", function.Args!.Value.GetProperty("ACT").GetString());
    }

    [Fact]
    public void WithArgs_ReplacesEarlierArguments_AndUsesAddsToEarlierTags()
    {
        var function = WaJsFunction.Registered("myapp.cell")
            .WithArgs(new { a = 1 })
            .WithArgs(new { b = 2 })
            .Uses("wa-badge")
            .Uses("wa-icon", "wa-badge");

        Assert.Equal("""{"b":2}""", function.Args!.Value.GetRawText());
        Assert.Equal(["wa-badge", "wa-icon"], function.Tags);
    }

    [Fact]
    public void WithArgs_AndUses_ReturnCopies_AndLeaveTheOriginalUnchanged()
    {
        var original = WaJsFunction.Registered("myapp.cell");

        var withArgs = original.WithArgs(new { a = 1 });
        var withTags = original.Uses("wa-badge");

        Assert.NotSame(original, withArgs);
        Assert.NotSame(original, withTags);
        Assert.Null(original.Args);
        Assert.Empty(original.Tags);
        Assert.Equal("""{"$wab":"fn","ref":{"kind":"registered","name":"myapp.cell"}}""", Json(original));
    }

    [Fact]
    public void Properties_DescribeTheReference()
    {
        var module = WaJsFunction.Module("./cells.js", "stateCell");
        Assert.Equal(WaJsFunctionKind.Module, module.Kind);
        Assert.Equal("./cells.js", module.Reference);
        Assert.Equal("stateCell", module.ExportName);
        Assert.False(module.AllowCrossOrigin);

        var registered = WaJsFunction.Registered("myapp.cell");
        Assert.Equal(WaJsFunctionKind.Registered, registered.Kind);
        Assert.Equal("myapp.cell", registered.Reference);
        Assert.Null(registered.ExportName);

        Assert.Equal(WaJsFunctionKind.Global, WaJsFunction.Global("myApp.cell").Kind);
    }

    [Fact]
    public void Equality_ComparesTheDescriptor()
    {
        var first = WaJsFunction.Module("./cells.js", "stateCell").WithArgs(new { max = 5 }).Uses("wa-badge");
        var same = WaJsFunction.Module("./cells.js", "stateCell").WithArgs(new { max = 5 }).Uses("wa-badge");

        Assert.Equal(first, same);
        Assert.Equal(first.GetHashCode(), same.GetHashCode());
        Assert.NotEqual(first, same.WithArgs(new { max = 6 }));
        Assert.NotEqual(first, same.Uses("wa-icon"));
        Assert.NotEqual(first, WaJsFunction.Module("./cells.js", "otherCell").WithArgs(new { max = 5 }).Uses("wa-badge"));
        Assert.NotEqual(WaJsFunction.Registered("a.b"), WaJsFunction.Global("a.b"));
    }

    [Fact]
    public void ToString_ReturnsTheDescriptor()
    {
        var function = WaJsFunction.Global("myApp.cell").Uses("wa-tag");

        Assert.Equal(Json(function), function.ToString());
    }

    [Fact]
    public void Descriptor_CannotBeRead()
    {
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<WaJsFunction>(
            """{"$wab":"fn","ref":{"kind":"registered","name":"a"}}""", JsonSerializerOptions.Web));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" myapp.cell")]
    [InlineData("myapp.cell ")]
    [InlineData("myapp\u0000cell")]
    public void Registered_RejectsAnInvalidName(string name)
    {
        Assert.Throws<ArgumentException>(() => WaJsFunction.Registered(name));
    }

    [Theory]
    [InlineData("./Pages/Orders.razor.js")]
    [InlineData("/js/cells.js")]
    [InlineData("_content/MyLibrary/cells.js")]
    [InlineData("https://cdn.example.com/cells.js")]
    [InlineData("HTTP://example.com/cells.js")]
    [InlineData("//cdn.example.com/cells.js")]
    public void Module_AcceptsARelativeOrHttpUrl(string url)
    {
        Assert.Equal(url, WaJsFunction.Module(url).Reference);
    }

    [Theory]
    [InlineData("")]
    [InlineData("javascript:alert(1)")]
    [InlineData("JavaScript:alert(1)")]
    [InlineData("data:text/javascript,export default () => 1")]
    [InlineData("blob:https://example.com/0f1d")]
    [InlineData("file:///C:/cells.js")]
    [InlineData("java\tscript:alert(1)")]
    [InlineData(" ./cells.js")]
    [InlineData("./cells.js\n")]
    [InlineData("./my cells.js")]
    public void Module_RejectsAnUnsafeOrMalformedUrl(string url)
    {
        Assert.Throws<ArgumentException>(() => WaJsFunction.Module(url));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" stateCell")]
    public void Module_RejectsAnInvalidExportName(string exportName)
    {
        Assert.Throws<ArgumentException>(() => WaJsFunction.Module("./cells.js", exportName));
    }

    [Theory]
    [InlineData("stateCell")]
    [InlineData("myApp.grid.stateCell")]
    [InlineData("$app._cells.état")]
    public void Global_AcceptsADottedPathOfIdentifiers(string path)
    {
        Assert.Equal(path, WaJsFunction.Global(path).Reference);
    }

    [Theory]
    [InlineData("")]
    [InlineData("myApp..cell")]
    [InlineData(".cell")]
    [InlineData("myApp.")]
    [InlineData("myApp.1cell")]
    [InlineData("myApp.state-cell")]
    [InlineData("myApp.cell()")]
    [InlineData("myApp['cell']")]
    [InlineData("my App.cell")]
    public void Global_RejectsAnythingButADottedPathOfIdentifiers(string path)
    {
        Assert.Throws<ArgumentException>(() => WaJsFunction.Global(path));
    }

    [Theory]
    [InlineData("badge")]
    [InlineData("wa-")]
    [InlineData("WA-badge")]
    [InlineData("wa-Badge")]
    [InlineData("wa-badge wa-icon")]
    [InlineData("wa--badge")]
    [InlineData("sl-badge")]
    public void Uses_RejectsATagThatIsNotAWebAwesomeTagName(string tag)
    {
        Assert.Throws<ArgumentException>(() => WaJsFunction.Registered("myapp.cell").Uses(tag));
    }

    [Fact]
    public void Uses_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => WaJsFunction.Registered("myapp.cell").Uses(null!));
        Assert.Throws<ArgumentException>(() => WaJsFunction.Registered("myapp.cell").Uses("wa-badge", null!));
    }

    [Fact]
    public void WithArgs_RejectsNull_AndAValueThatCannotBeSerialized()
    {
        var cycle = new Node();
        cycle.Next = cycle;

        Assert.Throws<ArgumentNullException>(() => WaJsFunction.Registered("myapp.cell").WithArgs(null!));
        Assert.Throws<ArgumentException>(() => WaJsFunction.Registered("myapp.cell").WithArgs(cycle));
    }

    #region ------ Internals ------

    // the descriptor as the data grid sends it (the column JSON uses the web defaults)
    private static string Json(WaJsFunction function) => JsonSerializer.Serialize(function, JsonSerializerOptions.Web);

    // an object graph with a cycle, which JSON cannot express
    private sealed class Node
    {
        public Node? Next { get; set; }
    }

    #endregion
}
