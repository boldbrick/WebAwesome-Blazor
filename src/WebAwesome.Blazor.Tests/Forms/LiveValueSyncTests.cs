using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// Verifies the model-to-element live value sync of the WaInputBase form controls (Web Awesome 3.12.0 bindings,
/// GitHub issue #1): Web Awesome 3 maps the value/checked attribute to defaultValue/defaultChecked, so after the
/// user has interacted the attribute no longer reaches the element, and the wrappers push a C#-side change into
/// the live property through the interop module's "syncProperty". Asserted with a mocked interop module: nothing
/// is pushed on first render, a parent-side change is pushed exactly once with the live property name and the
/// JS-typed value, a value that came from the element is never pushed back, and the controls whose attribute
/// already maps to the live property (WaSelect, WaCombobox, WaRating) never sync. Also covers the
/// SetRangeTextAsync read-back on WaInput and WaTextArea, which updates the bound value and the EditContext
/// because Web Awesome dispatches no event for setRangeText.
/// </summary>
public class LiveValueSyncTests : FormControlTestBase
{
    #region ------ WaInput ("value", string) ------

    [Fact]
    public void WaInput_FirstRender_DoesNotSync()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);

        // Act
        RenderBound<WaInput, string?>("Ada");

        // Assert - the attribute delivers the initial value
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaInput_ParentChange_SyncsValueOnce()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound<WaInput, string?>("Ada");

        // Act
        cut.Render(p => p.Add(c => c.Value, "Grace"));

        // Assert
        var invocation = Assert.Single(SyncInvocations(module));
        Assert.Equal("value", invocation.Arguments[1]);
        Assert.Equal("Grace", invocation.Arguments[2]);
    }

    [Fact]
    public void WaInput_ParentRerenderWithSameValue_DoesNotSync()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound<WaInput, string?>("Ada");

        // Act
        cut.Render(p => p.Add(c => c.Value, "Ada"));

        // Assert - the last synced value is tracked, so there is no interop call per render
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaInput_ElementChange_IsNotPushedBack()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var bound = new BoundValue<string?>("Ada");
        var cut = RenderBound<WaInput, string?>(bound);

        // Act - the user commits a value, then the parent re-renders with it as @bind-Value would
        cut.Find("wa-input").Change("Grace");
        cut.Render(p => p.Add(c => c.Value, bound.Value));

        // Assert
        Assert.Equal("Grace", bound.Value);
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaInput_ResetAfterUserEdit_IsPushed()
    {
        // Arrange - the reset-after-submit scenario of issue #1
        var module = JSInterop.SetupModule(InteropModulePath);
        var bound = new BoundValue<string?>("Ada");
        var cut = RenderBound<WaInput, string?>(bound);
        cut.Find("wa-input").Change("Grace");

        // Act
        cut.Render(p => p.Add(c => c.Value, string.Empty));

        // Assert
        var invocation = Assert.Single(SyncInvocations(module));
        Assert.Equal("value", invocation.Arguments[1]);
        Assert.Equal(string.Empty, invocation.Arguments[2]);
    }

    #endregion

    #region ------ WaSlider ("value", number) ------

    [Fact]
    public void WaSlider_FirstRender_DoesNotSync()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);

        // Act
        RenderBound<WaSlider, decimal?>(50m);

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaSlider_ParentChange_SyncsValueOnceAsNumber()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound<WaSlider, decimal?>(50m);

        // Act
        cut.Render(p => p.Add(c => c.Value, 75m));

        // Assert - wa-slider's live value is a JS number, so a double is sent, not a string
        var invocation = Assert.Single(SyncInvocations(module));
        Assert.Equal("value", invocation.Arguments[1]);
        Assert.Equal(75d, Assert.IsType<double>(invocation.Arguments[2]));
    }

    [Fact]
    public void WaSlider_ElementChange_IsNotPushedBack()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var bound = new BoundValue<decimal?>(50m);
        var cut = RenderBound<WaSlider, decimal?>(bound);

        // Act
        cut.Find("wa-slider").NumericChange("60");
        cut.Render(p => p.Add(c => c.Value, bound.Value));

        // Assert
        Assert.Equal(60m, bound.Value);
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaSlider_RangeMode_NeverSyncs()
    {
        // Arrange - min-value/max-value map to the live minValue/maxValue properties
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = Render<WaSlider>(p => p
            .Add(c => c.Range, true)
            .Add(c => c.MinValue, 10m)
            .Add(c => c.MaxValue, 90m));

        // Act
        cut.Render(p => p.Add(c => c.MinValue, 20m).Add(c => c.MaxValue, 80m));

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    #endregion

    #region ------ WaCheckbox ("checked", bool) ------

    [Fact]
    public void WaCheckbox_FirstRender_DoesNotSync()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);

        // Act
        RenderBound<WaCheckbox, bool>(false);

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaCheckbox_ParentChange_SyncsCheckedOnceAsBool()
    {
        // Arrange - e.g. a "select all" button
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound<WaCheckbox, bool>(false);

        // Act
        cut.Render(p => p.Add(c => c.Value, true));

        // Assert
        var invocation = Assert.Single(SyncInvocations(module));
        Assert.Equal("checked", invocation.Arguments[1]);
        Assert.True(Assert.IsType<bool>(invocation.Arguments[2]));
    }

    [Fact]
    public void WaCheckbox_ElementChange_IsNotPushedBack()
    {
        // Arrange - the wrapper reads the real checked state back through getProperty
        var module = JSInterop.SetupModule(InteropModulePath);
        module.Setup<bool>(GetPropertyIdentifier, i => Equals(i.Arguments[1], "checked")).SetResult(true);
        var bound = new BoundValue<bool>(false);
        var cut = RenderBound<WaCheckbox, bool>(bound);

        // Act
        cut.Find("wa-checkbox").Change(bool.TrueString);
        cut.Render(p => p.Add(c => c.Value, bound.Value));

        // Assert
        Assert.True(bound.Value);
        Assert.Empty(SyncInvocations(module));
    }

    #endregion

    #region ------ WaRadioGroup ("value", string) ------

    [Fact]
    public void WaRadioGroup_FirstRender_DoesNotSync()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);

        // Act
        RenderBound<WaRadioGroup, string?>("a");

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaRadioGroup_ParentChange_SyncsValueOnce()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound<WaRadioGroup, string?>("a");

        // Act
        cut.Render(p => p.Add(c => c.Value, "b"));

        // Assert
        var invocation = Assert.Single(SyncInvocations(module));
        Assert.Equal("value", invocation.Arguments[1]);
        Assert.Equal("b", invocation.Arguments[2]);
    }

    [Fact]
    public void WaRadioGroup_ElementChange_IsNotPushedBack()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var bound = new BoundValue<string?>("a");
        var cut = RenderBound<WaRadioGroup, string?>(bound);

        // Act
        cut.Find("wa-radio-group").Change("c");
        cut.Render(p => p.Add(c => c.Value, bound.Value));

        // Assert
        Assert.Equal("c", bound.Value);
        Assert.Empty(SyncInvocations(module));
    }

    #endregion

    #region ------ Controls whose attribute is the live property ------

    [Fact]
    public void WaSelect_ParentChange_NeverSyncs()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound<WaSelect, string?>("apple");

        // Act
        cut.Render(p => p.Add(c => c.Value, "pear"));

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaCombobox_ParentChange_NeverSyncs()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound<WaCombobox, string?>("apple");

        // Act
        cut.Render(p => p.Add(c => c.Value, "pear"));

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaRating_ParentChange_NeverSyncs()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound<WaRating, decimal>(3m);

        // Act
        cut.Render(p => p.Add(c => c.Value, 4m));

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    #endregion

    #region ------ SetRangeTextAsync read-back ------

    [Fact]
    public async Task WaInput_SetRangeTextAsync_ReadsValueBack_AndNotifiesEditContext()
    {
        // Arrange
        var module = SetupRangeTextModule("Hello, world");
        var model = new TextModel { Text = "Hello" };
        EditContext? editContext = null;
        var cut = RenderControlForm<WaInput, string?>(model, model.Text, value => model.Text = value, () => model.Text,
            onEditContext: context => editContext = context);
        var input = cut.FindComponent<WaInput>().Instance;

        // Act
        await cut.InvokeAsync(() => input.SetRangeTextAsync(", world", 5, 5, "end"));

        // Assert
        Assert.Equal("Hello, world", model.Text);
        Assert.NotNull(editContext);
        Assert.True(editContext!.IsModified(() => model.Text));
        AssertSetRangeTextBeforeReadBack(module);
    }

    [Fact]
    public async Task WaTextArea_SetRangeTextAsync_ReadsValueBack_AndNotifiesEditContext()
    {
        // Arrange
        var module = SetupRangeTextModule("Hello, world");
        var model = new TextModel { Text = "Hello" };
        EditContext? editContext = null;
        var cut = RenderControlForm<WaTextArea, string?>(model, model.Text, value => model.Text = value, () => model.Text,
            onEditContext: context => editContext = context);
        var textArea = cut.FindComponent<WaTextArea>().Instance;

        // Act
        await cut.InvokeAsync(() => textArea.SetRangeTextAsync(", world", 5, 5, WaTextAreaSelectMode.End));

        // Assert
        Assert.Equal("Hello, world", model.Text);
        Assert.NotNull(editContext);
        Assert.True(editContext!.IsModified(() => model.Text));
        AssertSetRangeTextBeforeReadBack(module);
    }

    [Fact]
    public async Task WaInput_SetRangeTextAsync_ReadBackValue_IsNotPushedBack()
    {
        // Arrange
        var module = SetupRangeTextModule("Hello, world");
        var bound = new BoundValue<string?>("Hello");
        var cut = RenderBound<WaInput, string?>(bound);

        // Act - the parent re-renders with the read-back value, as @bind-Value would
        await cut.InvokeAsync(() => cut.Instance.SetRangeTextAsync(", world"));
        cut.Render(p => p.Add(c => c.Value, bound.Value));

        // Assert
        Assert.Equal("Hello, world", bound.Value);
        Assert.Empty(SyncInvocations(module));
    }

    #endregion

    #region ------ Internals ------

    private const string SyncPropertyIdentifier = "syncProperty";
    private const string GetPropertyIdentifier = "getProperty";
    private const string InvokeMethodIdentifier = "invokeMethod";

    private class TextModel
    {
        public string? Text { get; set; }
    }

    // holds a bound value the way a parent's @bind-Value field would
    private sealed class BoundValue<TValue>
    {
        public BoundValue(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; set; }
    }

    private static JSRuntimeInvocation[] SyncInvocations(BunitJSModuleInterop module)
        => module.Invocations.Where(i => i.Identifier == SyncPropertyIdentifier).ToArray();

    private IRenderedComponent<TComponent> RenderBound<TComponent, TValue>(TValue initialValue)
        where TComponent : InputBase<TValue>
        => RenderBound<TComponent, TValue>(new BoundValue<TValue>(initialValue));

    private IRenderedComponent<TComponent> RenderBound<TComponent, TValue>(BoundValue<TValue> bound)
        where TComponent : InputBase<TValue>
    {
        return Render<TComponent>(p => p
            .Add(c => c.Value, bound.Value)
            .Add(c => c.ValueChanged, value => bound.Value = value)
            .Add(c => c.ValueExpression, () => bound.Value));
    }

    private BunitJSModuleInterop SetupRangeTextModule(string valueAfterReplacement)
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid(InvokeMethodIdentifier, i => Equals(i.Arguments[1], "setRangeText")).SetVoidResult();
        module.Setup<string>(GetPropertyIdentifier, i => Equals(i.Arguments[1], "value")).SetResult(valueAfterReplacement);
        return module;
    }

    private static void AssertSetRangeTextBeforeReadBack(BunitJSModuleInterop module)
    {
        var identifiers = module.Invocations.Select(i => i.Identifier).ToList();
        var setRangeText = identifiers.IndexOf(InvokeMethodIdentifier);
        var readBack = identifiers.IndexOf(GetPropertyIdentifier);
        Assert.True(setRangeText >= 0 && readBack > setRangeText, $"Expected setRangeText then getProperty, got: {string.Join(", ", identifiers)}");
    }

    #endregion
}
