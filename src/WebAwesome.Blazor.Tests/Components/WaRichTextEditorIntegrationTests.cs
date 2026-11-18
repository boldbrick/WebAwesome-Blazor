using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaRichTextEditor component (Web Awesome 3.15.0), for what the render-based parity
/// checks cannot see: the actions slot, the events bound to the host's own focus and blur (the editor raises no
/// focusin or focusout), the relayed keydown and selectionchange handlers, the OnSuggestionsError payload, the
/// attributes that stay unrendered for an empty list, every public method's guard and recorded interop call, the
/// options object RunCommandAsync sends (only set options, explicit null for the removals, the heading level as a
/// number) and the return values of the methods that answer, read back under a Strict JS runtime. The
/// EditForm binding is WaRichTextEditorEditFormTests and the live value sync is LiveValueSyncTests; attributes and
/// defaults are RenderedAttributeParityTests, events EventCallbackBindingParityTests, the function members
/// (ImageProviderFunction, EmbedProviderFunction) ElementJsPropertyTests.
/// </summary>
public class WaRichTextEditorIntegrationTests : BunitContext
{
    public WaRichTextEditorIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region ------ Slots ------

    [Fact]
    public void ActionsContent_RendersIntoTheActionsSlot()
    {
        // Arrange & Act
        var cut = RenderEditor(parameters => parameters.Add(p => p.ActionsContent, SlotProbe.Fragment));

        // Assert
        Assert.Equal("actions", SlotProbe.SlotOf(cut.Find("wa-rich-text-editor")));
    }

    [Fact]
    public void ChildContent_RendersIntoTheDefaultSlot_AndWithoutActionsContent_NoActionsSlot()
    {
        // Arrange & Act
        var cut = RenderEditor(parameters => parameters.Add(p => p.ChildContent, SlotProbe.Fragment));

        // Assert
        Assert.Equal(string.Empty, SlotProbe.SlotOf(cut.Find("wa-rich-text-editor")));
        Assert.Empty(cut.FindAll("[slot='actions']"));
    }

    #endregion

    #region ------ Unrendered attributes ------

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HeadingLevels_NullOrEmpty_RendersNoAttribute(bool empty)
    {
        // Arrange & Act
        var cut = RenderEditor(parameters => parameters.Add(p => p.HeadingLevels, empty ? (IReadOnlyList<WaRichTextEditorHeadingLevel>?)Array.Empty<WaRichTextEditorHeadingLevel>() : null));

        // Assert
        Assert.False(cut.Find("wa-rich-text-editor").HasAttribute("heading-levels"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TextColors_NullOrEmpty_RendersNoAttribute(bool empty)
    {
        // Arrange & Act
        var cut = RenderEditor(parameters => parameters.Add(p => p.TextColors, empty ? (IReadOnlyList<string>?)Array.Empty<string>() : null));

        // Assert
        Assert.False(cut.Find("wa-rich-text-editor").HasAttribute("text-colors"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HighlightColors_NullOrEmpty_RendersNoAttribute(bool empty)
    {
        // Arrange & Act
        var cut = RenderEditor(parameters => parameters.Add(p => p.HighlightColors, empty ? (IReadOnlyList<string>?)Array.Empty<string>() : null));

        // Assert
        Assert.False(cut.Find("wa-rich-text-editor").HasAttribute("highlight-colors"));
    }

    #endregion

    #region ------ Event binding ------

    [Fact]
    public void OnFocusAndOnBlur_AreBoundToTheHostsFocusAndBlur()
    {
        // Arrange
        var focus = 0;
        var blur = 0;
        var cut = RenderEditor(parameters => parameters
            .Add(p => p.OnFocus, _ => focus++)
            .Add(p => p.OnBlur, _ => blur++));
        var element = cut.Find("wa-rich-text-editor");

        // Act
        element.Focus();
        element.Blur();

        // Assert - the editor dispatches focus and blur on the host itself, once for the whole editor
        Assert.Equal((1, 1), (focus, blur));
    }

    [Theory]
    [InlineData("onfocusin")]
    [InlineData("onfocusout")]
    public void OnFocusAndOnBlur_AreNotBoundToFocusInAndFocusOut(string handler)
    {
        // Arrange - a move between the content and the toolbar raises focusin and focusout inside the editor
        var cut = RenderEditor(parameters => parameters
            .Add(p => p.OnFocus, _ => { })
            .Add(p => p.OnBlur, _ => { }));

        // Act & Assert
        Assert.Throws<MissingEventHandlerException>(() => cut.Find("wa-rich-text-editor").TriggerEvent(handler, new FocusEventArgs()));
    }

    [Fact]
    public void OnKeyDown_IsBoundToTheRelayedKeyDown()
    {
        // Arrange
        KeyboardEventArgs? received = null;
        var cut = RenderEditor(parameters => parameters.Add(p => p.OnKeyDown, args => received = args));
        var element = cut.Find("wa-rich-text-editor");

        // Act
        element.TriggerEvent(RelayedAttribute("RelayedKeyDownEventAttribute"), new KeyboardEventArgs { Key = "Escape" });

        // Assert - the editor stops the propagation of Escape inside its shadow root, so the plain keydown is not bound
        Assert.Equal("Escape", received?.Key);
        Assert.Throws<MissingEventHandlerException>(() => element.TriggerEvent("onkeydown", new KeyboardEventArgs { Key = "a" }));
    }

    [Fact]
    public void OnSelectionChange_IsBoundToTheRelayedSelectionChange()
    {
        // Arrange - the element's selectionchange does not bubble out of the shadow root
        var selectionChanges = 0;
        var cut = RenderEditor(parameters => parameters.Add(p => p.OnSelectionChange, _ => selectionChanges++));
        var element = cut.Find("wa-rich-text-editor");

        // Act
        element.TriggerEvent(RelayedAttribute("RelayedSelectionChangeEventAttribute"), EventArgs.Empty);

        // Assert
        Assert.Equal(1, selectionChanges);
        Assert.Throws<MissingEventHandlerException>(() => element.TriggerEvent("onselectionchange", EventArgs.Empty));
    }

    [Fact]
    public void SuggestionsErrorArgs_ReadTheProjectedError()
    {
        // Arrange - the shape the wa-suggestions-error projection of the JS initializer builds
        const string payload = """{"error":"Network down","trigger":"@","query":"ada"}""";

        // Act
        var args = JsonSerializer.Deserialize<WaSuggestionsErrorEventArgs>(payload, JsonSerializerOptions.Web)!;

        // Assert
        Assert.Equal(("Network down", "@", "ada"), (args.Error, args.Trigger, args.Query));
    }

    [Fact]
    public void SuggestionsErrorArgs_WithNullError_ReadNull()
    {
        // Arrange
        const string payload = """{"error":null,"trigger":"#","query":""}""";

        // Act
        var args = JsonSerializer.Deserialize<WaSuggestionsErrorEventArgs>(payload, JsonSerializerOptions.Web)!;

        // Assert
        Assert.Null(args.Error);
        Assert.Equal(("#", string.Empty), (args.Trigger, args.Query));
    }

    #endregion

    #region ------ Method guards and recorded calls ------

    /// <summary>
    /// The public methods, one theory case each.
    /// </summary>
    public static TheoryData<string> Methods => new(MethodCases.Keys);

    [Theory]
    [MemberData(nameof(Methods))]
    public async Task Method_WithNullElement_ThrowsInvalidOperationException(string method)
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var editor = runtime.CreateUnrendered<WaRichTextEditor>();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => MethodCases[method].Call(editor));

        // Assert
        Assert.Contains($"Cannot {MethodCases[method].Action}: component has not been rendered yet", exception.Message);
        Assert.Empty(runtime.Module.Invocations);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    public async Task Method_WithRenderedElement_InvokesTheElementMethodWithItsArguments(string method)
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var editor = runtime.CreateRendered<WaRichTextEditor>();
        var methodCase = MethodCases[method];

        // Act
        await methodCase.Call(editor);

        // Assert
        Assert.Equal(methodCase.Arguments, runtime.Module.AssertInvokedMethod(methodCase.ElementMethod));
    }

    #endregion

    #region ------ RunCommandAsync options ------

    [Fact]
    public async Task RunCommandAsync_WithoutOptions_SendsAnEmptyOptionsObject()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Bold);

        // Assert
        Assert.Equal(["bold", new Dictionary<string, object?>()], runtime.Module.AssertInvokedMethod("runCommand"));
    }

    [Fact]
    public async Task RunCommandAsync_WithUnsetOptions_SendsOnlyTheSetOnes()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var options = new WaRichTextEditorCommandOptions { Href = "https://example.com", Rel = "nofollow" };

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Link, options);

        // Assert - no target, no other member
        Assert.Equal(["link", new Dictionary<string, object?> { ["href"] = "https://example.com", ["rel"] = "nofollow" }],
            runtime.Module.AssertInvokedMethod("runCommand"));
    }

    [Fact]
    public async Task RunCommandAsync_Heading_SendsTheLevelAsANumber()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Heading,
            new WaRichTextEditorCommandOptions { Level = WaRichTextEditorHeadingLevel.H4 });

        // Assert
        var payload = (Dictionary<string, object?>)runtime.Module.AssertInvokedMethod("runCommand")[1];
        Assert.Equal(4, Assert.IsType<int>(payload["level"]));
        Assert.Single(payload);
    }

    [Fact]
    public async Task RunCommandAsync_Align_SendsTheAlignmentName()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Align,
            new WaRichTextEditorCommandOptions { Align = WaRichTextEditorAlignment.Justify });

        // Assert
        Assert.Equal(["align", new Dictionary<string, object?> { ["align"] = "justify" }], runtime.Module.AssertInvokedMethod("runCommand"));
    }

    [Fact]
    public async Task RunCommandAsync_Image_SendsAddressAlignmentAndCaption()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var options = new WaRichTextEditorCommandOptions
        {
            Src = "a.png",
            Alt = "A",
            Title = "T",
            ImageAlign = WaRichTextEditorImageAlignment.WrapEnd,
            WithCaption = false,
        };

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Image, options);

        // Assert - a caption of false is a value, not an unset option
        var expected = new Dictionary<string, object?>
        {
            ["src"] = "a.png", ["alt"] = "A", ["title"] = "T", ["align"] = "wrap-end", ["caption"] = false,
        };
        Assert.Equal(["image", expected], runtime.Module.AssertInvokedMethod("runCommand"));
    }

    [Fact]
    public async Task RunCommandAsync_ClearImageAlign_SendsExplicitNullAndWinsOverImageAlign()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var options = new WaRichTextEditorCommandOptions { ImageAlign = WaRichTextEditorImageAlignment.Start, ClearImageAlign = true };

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Image, options);

        // Assert
        var payload = (Dictionary<string, object?>)runtime.Module.AssertInvokedMethod("runCommand")[1];
        Assert.True(payload.ContainsKey("align"));
        Assert.Null(payload["align"]);
    }

    [Fact]
    public async Task RunCommandAsync_Embed_SendsTheSizeAsNumbers()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var options = new WaRichTextEditorCommandOptions { Src = "https://example.com/v", Width = 640, Height = 360 };

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Embed, options);

        // Assert
        var payload = (Dictionary<string, object?>)runtime.Module.AssertInvokedMethod("runCommand")[1];
        Assert.Equal(640, Assert.IsType<int>(payload["width"]));
        Assert.Equal(360, Assert.IsType<int>(payload["height"]));
        Assert.Equal("https://example.com/v", payload["src"]);
    }

    [Fact]
    public async Task RunCommandAsync_Table_SendsRowsAndColumns()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Table,
            new WaRichTextEditorCommandOptions { Rows = 2, Columns = 5 });

        // Assert
        Assert.Equal(["table", new Dictionary<string, object?> { ["rows"] = 2, ["columns"] = 5 }], runtime.Module.AssertInvokedMethod("runCommand"));
    }

    [Fact]
    public async Task RunCommandAsync_ColorAndHighlight_SendTheColors()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var options = new WaRichTextEditorCommandOptions { Color = "red", Highlight = "yellow" };

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Color, options);

        // Assert
        Assert.Equal(["color", new Dictionary<string, object?> { ["color"] = "red", ["highlight"] = "yellow" }],
            runtime.Module.AssertInvokedMethod("runCommand"));
    }

    [Fact]
    public async Task RunCommandAsync_RemoveColor_SendsExplicitNullAndWinsOverColor()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var options = new WaRichTextEditorCommandOptions { Color = "red", RemoveColor = true };

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Color, options);

        // Assert - null removes the color, an absent member keeps it
        var payload = (Dictionary<string, object?>)runtime.Module.AssertInvokedMethod("runCommand")[1];
        Assert.True(payload.ContainsKey("color"));
        Assert.Null(payload["color"]);
        Assert.False(payload.ContainsKey("highlight"));
    }

    [Fact]
    public async Task RunCommandAsync_RemoveHighlight_SendsExplicitNullAndWinsOverTheOthers()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var options = new WaRichTextEditorCommandOptions { Highlight = "yellow", DefaultHighlight = true, RemoveHighlight = true };

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Color, options);

        // Assert
        var payload = (Dictionary<string, object?>)runtime.Module.AssertInvokedMethod("runCommand")[1];
        Assert.True(payload.ContainsKey("highlight"));
        Assert.Null(payload["highlight"]);
        Assert.False(payload.ContainsKey("color"));
    }

    [Fact]
    public async Task RunCommandAsync_DefaultHighlight_SendsTrueAndWinsOverHighlight()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var options = new WaRichTextEditorCommandOptions { Highlight = "yellow", DefaultHighlight = true };

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Color, options);

        // Assert
        Assert.Equal(["color", new Dictionary<string, object?> { ["highlight"] = true }], runtime.Module.AssertInvokedMethod("runCommand"));
    }

    [Fact]
    public async Task RunCommandAsync_Link_SendsTargetAndRelWhenSet()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var options = new WaRichTextEditorCommandOptions { Href = "https://example.com", Target = "_blank" };

        // Act
        await runtime.CreateRendered<WaRichTextEditor>().RunCommandAsync(WaRichTextEditorCommand.Link, options);

        // Assert
        Assert.Equal(["link", new Dictionary<string, object?> { ["href"] = "https://example.com", ["target"] = "_blank" }],
            runtime.Module.AssertInvokedMethod("runCommand"));
    }

    #endregion

    #region ------ Return values under a Strict JS runtime ------

    [Theory]
    [InlineData("undo", true)]
    [InlineData("undo", false)]
    [InlineData("redo", true)]
    [InlineData("redo", false)]
    public async Task UndoAndRedo_ReturnTheElementsAnswer(string method, bool answer)
    {
        // Arrange
        var module = SetupStrict();
        module.Setup<bool>("invokeMethod", i => Equals(i.Arguments[1], method)).SetResult(answer);
        var cut = RenderEditor();

        // Act
        var result = await cut.InvokeAsync(() => method == "undo" ? cut.Instance.UndoAsync() : cut.Instance.RedoAsync());

        // Assert
        Assert.Equal(answer, result);
        Assert.Equal(method, Assert.Single(module.Invocations).Arguments[1]);
    }

    [Fact]
    public async Task IsActiveAsync_PassesTheStateNameAndReturnsTheAnswer()
    {
        // Arrange
        var module = SetupStrict();
        module.Setup<bool>("invokeMethod", i => Equals(i.Arguments[1], "isActive")).SetResult(true);
        var cut = RenderEditor();

        // Act
        var result = await cut.InvokeAsync(() => cut.Instance.IsActiveAsync(WaRichTextEditorActiveState.ImageAlignWrapStart));

        // Assert
        Assert.True(result);
        var invocation = Assert.Single(module.Invocations);
        Assert.Equal(new object[] { "image-align-wrap-start" }, (object[])invocation.Arguments[2]!);
    }

    [Fact]
    public async Task RunCommandAsync_ReturnsTheElementsAnswer()
    {
        // Arrange
        var module = SetupStrict();
        module.Setup<bool>("invokeMethod", i => Equals(i.Arguments[1], "runCommand")).SetResult(true);
        var cut = RenderEditor();

        // Act
        var result = await cut.InvokeAsync(() => cut.Instance.RunCommandAsync(WaRichTextEditorCommand.TableAddRowBelow));

        // Assert
        Assert.True(result);
        Assert.Equal("table-add-row-below", ((object[])Assert.Single(module.Invocations).Arguments[2]!)[0]);
    }

    [Fact]
    public async Task GetSelectionAsync_ReturnsTheSelectionRange()
    {
        // Arrange
        var module = SetupStrict();
        module.Setup<WaRichTextEditorSelection>("invokeMethod", i => Equals(i.Arguments[1], "getSelection"))
            .SetResult(new WaRichTextEditorSelection(2, 7, false));
        var cut = RenderEditor();

        // Act
        var selection = await cut.InvokeAsync(() => cut.Instance.GetSelectionAsync());

        // Assert
        Assert.Equal(new WaRichTextEditorSelection(2, 7, false), selection);
    }

    [Fact]
    public void SelectionRecord_ReadsTheElementsAnswer()
    {
        // Arrange - the shape of the element's getSelection() result
        const string payload = """{"from":3,"to":3,"empty":true}""";

        // Act
        var selection = JsonSerializer.Deserialize<WaRichTextEditorSelection>(payload, JsonSerializerOptions.Web)!;

        // Assert
        Assert.Equal(new WaRichTextEditorSelection(3, 3, true), selection);
    }

    #endregion

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";

    private sealed record MethodCase(Func<WaRichTextEditor, Task> Call, string Action, string ElementMethod, object[] Arguments);

    // every public method: what the guard names, the element method it invokes and the arguments it passes
    private static readonly Dictionary<string, MethodCase> MethodCases = new(StringComparer.Ordinal)
    {
        ["BlurAsync"] = new(e => e.BlurAsync(), "blur", "blur", []),
        ["FocusAsync"] = new(e => e.FocusAsync(), "focus", "focus", []),
        ["DeleteSelectionAsync"] = new(e => e.DeleteSelectionAsync(), "delete the selection", "deleteSelection", []),
        ["SelectAllAsync"] = new(e => e.SelectAllAsync(), "select all", "selectAll", []),
        ["InsertHtmlAsync"] = new(e => e.InsertHtmlAsync("<b>x</b>"), "insert HTML", "insertHtml", ["<b>x</b>"]),
        ["InsertTextAsync"] = new(e => e.InsertTextAsync("x"), "insert text", "insertText", ["x"]),
        ["UndoAsync"] = new(e => e.UndoAsync(), "undo", "undo", []),
        ["RedoAsync"] = new(e => e.RedoAsync(), "redo", "redo", []),
        ["GetSelectionAsync"] = new(e => e.GetSelectionAsync(), "get the selection", "getSelection", []),
        ["IsActiveAsync"] = new(e => e.IsActiveAsync(WaRichTextEditorActiveState.Bold), "test the active state", "isActive", ["bold"]),
        ["RunCommandAsync"] = new(e => e.RunCommandAsync(WaRichTextEditorCommand.Underline), "run a command", "runCommand",
            ["underline", new Dictionary<string, object?>()]),
    };

    // the bUnit module interop of a Strict runtime: a call without a setup fails the test
    private BunitJSModuleInterop SetupStrict()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        return JSInterop.SetupModule(InteropModulePath);
    }

    private IRenderedComponent<WaRichTextEditor> RenderEditor(Action<ComponentParameterCollectionBuilder<WaRichTextEditor>>? configure = null)
    {
        string? value = null;
        return Render<WaRichTextEditor>(parameters =>
        {
            parameters.Add(p => p.ValueExpression, () => value);
            configure?.Invoke(parameters);
        });
    }

    // the relayed event attribute name of the internal Constants (InternalsVisibleTo applies to Debug builds only)
    private static string RelayedAttribute(string constantName)
        => (string)typeof(Constants).GetField(constantName, BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;

    #endregion
}
