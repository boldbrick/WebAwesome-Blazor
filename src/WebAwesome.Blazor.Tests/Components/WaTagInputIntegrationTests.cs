using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaTagInput component (Web Awesome 3.13.0): the label/hint/start/end/clear-icon
/// slots and the StartIconName/EndIconName fragment-wins-over-name precedence (WaIconSugarTests pattern), the
/// "onchange" value binder reading the element's tag array into Value/ValueChanged with no push-back afterwards,
/// the live "value" property push on first render and after a C#-side Value change (LiveValueSyncTests pattern),
/// and the FocusAsync/BlurAsync guards and recorded interop calls (WaCarouselIntegrationTests pattern). Its
/// attributes and their defaults are covered by RenderedAttributeParityTests, events by
/// EventCallbackBindingParityTests, and enum values by EnumValueParityTests, all against the CEM.
/// TryParseValueFromString/FormatValueAsString mirror the element's delimiter parsing, but the wrapper's own
/// change handler assigns CurrentValue directly (never CurrentValueAsString) and renders no "value" attribute, so
/// neither override is reachable from the wrapper's own code paths without reflecting into InputBase's protected
/// members; that round trip is therefore not covered here (see the plan, Phase 2).
/// </summary>
public class WaTagInputIntegrationTests : BunitContext
{
    public WaTagInputIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region ------ Slots ------

    [Theory]
    [InlineData(nameof(WaTagInput.MarkupLabel), "label")]
    [InlineData(nameof(WaTagInput.MarkupHint), "hint")]
    [InlineData(nameof(WaTagInput.StartContent), "start")]
    [InlineData(nameof(WaTagInput.EndContent), "end")]
    [InlineData(nameof(WaTagInput.ClearIconContent), "clear-icon")]
    public void SlotContent_RendersIntoItsSlot(string parameterName, string slot)
    {
        // Arrange & Act
        IReadOnlyList<string>? value = null;
        var cut = Render<WaTagInput>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .TryAdd(parameterName, SlotProbe.Fragment));

        // Assert
        Assert.Equal(slot, SlotProbe.SlotOf(cut.Find("wa-tag-input")));
    }

    [Fact]
    public void IconNames_RenderWaIconsIntoStartAndEndSlots()
    {
        // Arrange & Act
        IReadOnlyList<string>? value = null;
        var cut = Render<WaTagInput>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .Add(p => p.StartIconName, "tag")
            .Add(p => p.EndIconName, "circle-xmark"));

        // Assert
        Assert.Equal("tag", cut.Find("wa-tag-input > wa-icon[slot='start']").GetAttribute("name"));
        Assert.Equal("circle-xmark", cut.Find("wa-tag-input > wa-icon[slot='end']").GetAttribute("name"));
    }

    [Fact]
    public void WithoutIconNames_RendersNoIcons()
    {
        // Arrange & Act
        IReadOnlyList<string>? value = null;
        var cut = Render<WaTagInput>(parameters => parameters.Add(p => p.ValueExpression, () => value));

        // Assert
        Assert.Empty(cut.FindAll("wa-icon"));
    }

    [Fact]
    public void StartContent_WinsOverStartIconName()
    {
        // Arrange & Act
        IReadOnlyList<string>? value = null;
        var cut = Render<WaTagInput>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .Add(p => p.StartContent, SlotProbe.Fragment)
            .Add(p => p.StartIconName, "tag"));

        // Assert
        Assert.Equal("start", SlotProbe.SlotOf(cut.Find("wa-tag-input")));
        Assert.Empty(cut.FindAll("wa-icon[slot='start']"));
    }

    [Fact]
    public void EndContent_WinsOverEndIconName()
    {
        // Arrange & Act
        IReadOnlyList<string>? value = null;
        var cut = Render<WaTagInput>(parameters => parameters
            .Add(p => p.ValueExpression, () => value)
            .Add(p => p.EndContent, SlotProbe.Fragment)
            .Add(p => p.EndIconName, "circle-xmark"));

        // Assert
        Assert.Equal("end", SlotProbe.SlotOf(cut.Find("wa-tag-input")));
        Assert.Empty(cut.FindAll("wa-icon[slot='end']"));
    }

    #endregion

    #region ------ Value binder ------

    [Fact]
    public void ElementChange_WithStringArray_UpdatesValue()
    {
        // Arrange
        var bound = new BoundValue(null);
        var cut = RenderBound(bound);

        // Act - Blazor's change reader delivers the element's array value as string[]
        cut.Find("wa-tag-input").Change(new[] { "urgent", "review" });

        // Assert
        Assert.Equal(new[] { "urgent", "review" }, bound.Value);
    }

    [Fact]
    public void ElementChange_WithJsonArrayPayload_UpdatesValue()
    {
        // Arrange - a custom event type (not the built-in "change" reader) delivers a JsonElement array instead
        var bound = new BoundValue(null);
        var cut = RenderBound(bound);
        var payload = JsonSerializer.Deserialize<JsonElement>("[\"urgent\",\"review\"]");

        // Act
        cut.Find("wa-tag-input").TriggerEvent("onchange", new ChangeEventArgs { Value = payload });

        // Assert
        Assert.Equal(new[] { "urgent", "review" }, bound.Value);
    }

    [Fact]
    public void ElementChange_IsNotPushedBack()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var bound = new BoundValue(["urgent"]);
        var cut = RenderBound(bound);
        var pushesOnFirstRender = SyncInvocations(module).Length;

        // Act - the user adds a tag, then the parent re-renders with it as @bind-Value would
        cut.Find("wa-tag-input").Change(new[] { "urgent", "review" });
        cut.Render(p => p.Add(c => c.Value, bound.Value));

        // Assert
        Assert.Equal(pushesOnFirstRender, SyncInvocations(module).Length);
    }

    #endregion

    #region ------ Live value push ------

    [Fact]
    public void FirstRender_WithNonEmptyValue_PushesTheTagsAsArray()
    {
        // Arrange & Act
        var module = JSInterop.SetupModule(InteropModulePath);
        RenderBound(new BoundValue(["urgent", "review"]));

        // Assert
        var invocation = Assert.Single(SyncInvocations(module));
        Assert.Equal("value", invocation.Arguments[1]);
        Assert.Equal(new[] { "urgent", "review" }, invocation.Arguments[2]);
    }

    [Fact]
    public void FirstRender_WithNullValue_PushesNothing()
    {
        // Arrange & Act
        var module = JSInterop.SetupModule(InteropModulePath);
        RenderBound(new BoundValue(null));

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void FirstRender_WithEmptyValue_PushesNothing()
    {
        // Arrange & Act
        var module = JSInterop.SetupModule(InteropModulePath);
        RenderBound(new BoundValue(Array.Empty<string>()));

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void ParentChange_SyncsTheLivePropertyOnce()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound(new BoundValue(["urgent"]));
        var pushesOnFirstRender = SyncInvocations(module).Length;

        // Act - a C#-side change, as @bind-Value would apply it
        cut.Render(p => p.Add(c => c.Value, (IReadOnlyList<string>?)new[] { "urgent", "review" }));

        // Assert
        var pushes = SyncInvocations(module).Skip(pushesOnFirstRender).ToArray();
        var invocation = Assert.Single(pushes);
        Assert.Equal("value", invocation.Arguments[1]);
        Assert.Equal(new[] { "urgent", "review" }, invocation.Arguments[2]);
    }

    [Fact]
    public void ParentRerenderWithEqualValue_DoesNotSync()
    {
        // Arrange - the wrapper keeps a stable array while the bound list keeps its content
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound(new BoundValue(["urgent"]));
        var pushesOnFirstRender = SyncInvocations(module).Length;

        // Act - a new list instance with the same content
        cut.Render(p => p.Add(c => c.Value, (IReadOnlyList<string>?)new[] { "urgent" }));

        // Assert
        Assert.Equal(pushesOnFirstRender, SyncInvocations(module).Length);
    }

    #endregion

    #region ------ FocusAsync / BlurAsync ------

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaTagInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
        Assert.Contains("Cannot focus: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaTagInput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
        Assert.Contains("Cannot blur: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task FocusAsync_WithRenderedElement_InvokesFocus()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var component = runtime.CreateRendered<WaTagInput>();

        // Act
        await component.FocusAsync();

        // Assert
        Assert.Empty(runtime.Module.AssertInvokedMethod("focus"));
    }

    [Fact]
    public async Task BlurAsync_WithRenderedElement_InvokesBlur()
    {
        // Arrange
        using var runtime = new RecordingJSRuntime();
        var component = runtime.CreateRendered<WaTagInput>();

        // Act
        await component.BlurAsync();

        // Assert
        Assert.Empty(runtime.Module.AssertInvokedMethod("blur"));
    }

    #endregion

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
    private const string SyncPropertyIdentifier = "syncProperty";

    // holds the bound value the way a parent's @bind-Value field would
    private sealed class BoundValue(IReadOnlyList<string>? value)
    {
        public IReadOnlyList<string>? Value { get; set; } = value;
    }

    private static JSRuntimeInvocation[] SyncInvocations(BunitJSModuleInterop module)
        => module.Invocations.Where(i => i.Identifier == SyncPropertyIdentifier).ToArray();

    private IRenderedComponent<WaTagInput> RenderBound(BoundValue bound)
    {
        return Render<WaTagInput>(p => p
            .Add(c => c.Value, bound.Value)
            .Add(c => c.ValueChanged, value => bound.Value = value)
            .Add(c => c.ValueExpression, () => bound.Value));
    }

    #endregion
}
