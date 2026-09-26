using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
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
/// the live property through the interop module's "syncProperty". A theory over all 13 single-value controls that
/// override LiveValuePropertyName (WaInput, WaTextArea, WaNumberInput, WaColorPicker, WaDateInput, WaKnownDate,
/// WaOtpInput, WaRadioGroup, WaSlider, WaRange, WaTimeInput, WaCheckbox, WaSwitch) asserts with a mocked interop
/// module that nothing is pushed on first render or on a re-render with the same value, that a parent-side change
/// is pushed exactly once with the live property name and the JS-typed value, and that a value that came from the
/// element is never pushed back. The controls whose attribute already maps to the live property (WaSelect and
/// WaCombobox in single-value mode, WaRating) and range-mode WaSlider/WaRange never sync; multiple selection is
/// MultipleSelectionBindingTests. Also covers the SetRangeTextAsync read-back on WaInput and WaTextArea, which updates
/// the bound value and the EditContext because Web Awesome dispatches no event for setRangeText. This is the C#
/// half only: the browser proof that the pushed property reaches the element is value-sync-binding.spec.js.
/// </summary>
public class LiveValueSyncTests : FormControlTestBase
{
    /// <summary>
    /// The names of the live-syncing controls, one theory case each.
    /// </summary>
    public static TheoryData<string> LiveSyncingControls => new(Cases.Keys);

    #region ------ All live-syncing controls ------

    [Theory]
    [MemberData(nameof(LiveSyncingControls))]
    public void FirstRender_DoesNotSync(string control)
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);

        // Act
        Cases[control].Render(this);

        // Assert - the attribute delivers the initial value
        Assert.Empty(SyncInvocations(module));
    }

    [Theory]
    [MemberData(nameof(LiveSyncingControls))]
    public void ParentChange_SyncsTheLivePropertyOnce(string control)
    {
        // Arrange
        var liveSync = Cases[control];
        var module = JSInterop.SetupModule(InteropModulePath);
        var run = liveSync.Render(this);

        // Act
        run.RenderChangedValue();

        // Assert - the live property name and the JS-typed value (a double for wa-slider, a bool for checked)
        var invocation = Assert.Single(SyncInvocations(module));
        Assert.Equal(liveSync.LiveProperty, invocation.Arguments[1]);
        Assert.Equal(liveSync.SyncedValue, invocation.Arguments[2]);
        Assert.IsType(liveSync.SyncedValue.GetType(), invocation.Arguments[2]);
    }

    [Theory]
    [MemberData(nameof(LiveSyncingControls))]
    public void ParentRerenderWithSameValue_DoesNotSync(string control)
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var run = Cases[control].Render(this);

        // Act
        run.RenderInitialValue();

        // Assert - the last synced value is tracked, so there is no interop call per render
        Assert.Empty(SyncInvocations(module));
    }

    [Theory]
    [MemberData(nameof(LiveSyncingControls))]
    public void ElementChange_IsNotPushedBack(string control)
    {
        // Arrange - WaCheckbox and WaSwitch read the real checked state back, under Strict (CheckedReadBack)
        var liveSync = Cases[control];
        var module = liveSync.ReadsCheckedBack ? JSInterop.SetupCheckedReadBack(elementChecked: true) : JSInterop.SetupModule(InteropModulePath);
        var run = liveSync.Render(this);

        // Act - the user commits a value, then the parent re-renders with it as @bind-Value would
        run.ChangeFromElement();
        run.RenderBoundValue();

        // Assert
        Assert.True(run.BoundIsElementValue(), $"{control}: the element's value did not reach the binding");
        Assert.Empty(SyncInvocations(module));
    }

    #endregion

    #region ------ Specific scenarios ------

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

    [Fact]
    public void WaRange_RangeMode_NeverSyncs()
    {
        // Arrange - min-value/max-value map to the live minValue/maxValue properties
        var module = JSInterop.SetupModule(InteropModulePath);
        var bound = 0m;
        var cut = Render<WaRange>(p => p
            .Add(c => c.ValueExpression, () => bound)
            .Add(c => c.Range, true)
            .Add(c => c.MinValue, 10m)
            .Add(c => c.MaxValue, 90m));

        // Act
        cut.Render(p => p.Add(c => c.MinValue, 20m).Add(c => c.MaxValue, 80m));

        // Assert
        Assert.Empty(SyncInvocations(module));
    }

    [Fact]
    public void WaSelect_ParentChange_NeverSyncs()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var cut = RenderBound<WaSelect, string?>(new BoundValue<string?>("apple"));

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
        var cut = RenderBound<WaCombobox, string?>(new BoundValue<string?>("apple"));

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
        var cut = RenderBound<WaRating, decimal>(new BoundValue<decimal>(3m));

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
        var module = SetupRangeTextModule(TextAfterReplacement);
        var model = new TextModel { Text = "Hello" };
        EditContext? editContext = null;
        var cut = RenderControlForm<WaInput, string?>(model, model.Text, value => model.Text = value, () => model.Text,
            onEditContext: context => editContext = context);
        var input = cut.FindComponent<WaInput>().Instance;

        // Act
        await cut.InvokeAsync(() => input.SetRangeTextAsync(", world", 5, 5, "end"));

        // Assert
        Assert.Equal(TextAfterReplacement, model.Text);
        Assert.NotNull(editContext);
        Assert.True(editContext!.IsModified(() => model.Text));
        AssertSetRangeTextBeforeReadBack(module);
    }

    [Fact]
    public async Task WaTextArea_SetRangeTextAsync_ReadsValueBack_AndNotifiesEditContext()
    {
        // Arrange
        var module = SetupRangeTextModule(TextAfterReplacement);
        var model = new TextModel { Text = "Hello" };
        EditContext? editContext = null;
        var cut = RenderControlForm<WaTextArea, string?>(model, model.Text, value => model.Text = value, () => model.Text,
            onEditContext: context => editContext = context);
        var textArea = cut.FindComponent<WaTextArea>().Instance;

        // Act
        await cut.InvokeAsync(() => textArea.SetRangeTextAsync(", world", 5, 5, WaTextAreaSelectMode.End));

        // Assert
        Assert.Equal(TextAfterReplacement, model.Text);
        Assert.NotNull(editContext);
        Assert.True(editContext!.IsModified(() => model.Text));
        AssertSetRangeTextBeforeReadBack(module);
    }

    [Fact]
    public async Task WaInput_SetRangeTextAsync_ReadBackValue_IsNotPushedBack()
    {
        // Arrange
        var module = SetupRangeTextModule(TextAfterReplacement);
        var bound = new BoundValue<string?>("Hello");
        var cut = RenderBound<WaInput, string?>(bound);

        // Act - the parent re-renders with the read-back value, as @bind-Value would
        await cut.InvokeAsync(() => cut.Instance.SetRangeTextAsync(", world"));
        cut.Render(p => p.Add(c => c.Value, bound.Value));

        // Assert
        Assert.Equal(TextAfterReplacement, bound.Value);
        Assert.Empty(SyncInvocations(module));
    }

    #endregion

    #region ------ Internals ------

    private const string SyncPropertyIdentifier = "syncProperty";
    private const string GetPropertyIdentifier = "getProperty";
    private const string InvokeMethodIdentifier = "invokeMethod";
    private const string ValueProperty = "value";
    private const string CheckedProperty = "checked";
    private const string TextAfterReplacement = "Hello, world";

    // every single-value control overriding WaInputBase.LiveValuePropertyName: the value bound first, the value a
    // parent then binds and the JS-typed value it syncs, and a user edit from the element with the value it binds
    private static readonly Dictionary<string, LiveSyncCase> Cases = new LiveSyncCase[]
    {
        new LiveSyncCase<WaInput, string?>("wa-input", ValueProperty, "Ada", "Grace", "Grace", e => e.Change("Linus"), "Linus"),
        new LiveSyncCase<WaTextArea, string?>("wa-textarea", ValueProperty, "Ada", "Grace", "Grace", e => e.Change("Linus"), "Linus"),
        new LiveSyncCase<WaNumberInput, decimal?>("wa-number-input", ValueProperty, 1.5m, 2.5m, "2.5", e => e.Change("3.5"), 3.5m),
        new LiveSyncCase<WaColorPicker, string>("wa-color-picker", ValueProperty, "#ff0000", "#00ff00", "#00ff00", e => e.Change("#0000ff"), "#0000ff"),
        new LiveSyncCase<WaDateInput, DateOnly?>("wa-date-input", ValueProperty, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 2), "2026-02-02", e => e.Change("2026-03-03"), new DateOnly(2026, 3, 3)),
        new LiveSyncCase<WaKnownDate, string?>("wa-known-date", ValueProperty, "1990-01-01", "1991-02-02", "1991-02-02", e => e.Change("1992-03-03"), "1992-03-03"),
        new LiveSyncCase<WaTimeInput, string?>("wa-time-input", ValueProperty, "09:00:00", "10:30:00", "10:30:00", e => e.Change("11:45:00"), "11:45:00"),
        new LiveSyncCase<WaOtpInput, string?>("wa-otp-input", ValueProperty, "123456", "654321", "654321", e => e.Change("111111"), "111111"),
        new LiveSyncCase<WaRadioGroup, string?>("wa-radio-group", ValueProperty, "a", "b", "b", e => e.Change("c"), "c"),
        new LiveSyncCase<WaSlider, decimal?>("wa-slider", ValueProperty, 50m, 75m, 75d, e => e.NumericChange("60"), 60m),
        new LiveSyncCase<WaRange, decimal>("wa-slider", ValueProperty, 50m, 75m, 75d, e => e.NumericChange("60"), 60m),
        new LiveSyncCase<WaCheckbox, bool>("wa-checkbox", CheckedProperty, false, true, true, e => e.Change(bool.TrueString), true, readsCheckedBack: true),
        new LiveSyncCase<WaSwitch, bool>("wa-switch", CheckedProperty, false, true, true, e => e.Change(bool.TrueString), true, readsCheckedBack: true),
    }.ToDictionary(c => c.Name, StringComparer.Ordinal);

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

    // one live-syncing control: how to render it bound, and what it must sync
    private abstract class LiveSyncCase
    {
        public abstract string Name { get; }

        public abstract string LiveProperty { get; }

        public abstract object SyncedValue { get; }

        public abstract bool ReadsCheckedBack { get; }

        public abstract LiveSyncRun Render(LiveValueSyncTests test);
    }

    // the operations of one rendered, bound control
    private sealed record LiveSyncRun(Action RenderInitialValue, Action RenderChangedValue, Action RenderBoundValue,
        Action ChangeFromElement, Func<bool> BoundIsElementValue);

    private sealed class LiveSyncCase<TComponent, TValue> : LiveSyncCase
        where TComponent : InputBase<TValue>
    {
        public LiveSyncCase(string tag, string liveProperty, TValue initial, TValue changed, object syncedValue,
            Action<IElement> elementChange, TValue elementValue, bool readsCheckedBack = false)
        {
            this.tag = tag;
            this.initial = initial;
            this.changed = changed;
            this.elementChange = elementChange;
            this.elementValue = elementValue;
            LiveProperty = liveProperty;
            SyncedValue = syncedValue;
            ReadsCheckedBack = readsCheckedBack;
        }

        public override string Name => typeof(TComponent).Name;

        public override string LiveProperty { get; }

        public override object SyncedValue { get; }

        public override bool ReadsCheckedBack { get; }

        public override LiveSyncRun Render(LiveValueSyncTests test)
        {
            var bound = new BoundValue<TValue>(initial);
            var cut = test.RenderBound<TComponent, TValue>(bound);

            return new LiveSyncRun(
                () => cut.Render(p => p.Add(c => c.Value, initial)),
                () => cut.Render(p => p.Add(c => c.Value, changed)),
                () => cut.Render(p => p.Add(c => c.Value, bound.Value)),
                () => elementChange(cut.Find(tag)),
                () => EqualityComparer<TValue>.Default.Equals(bound.Value, elementValue));
        }

        private readonly string tag;
        private readonly TValue initial;
        private readonly TValue changed;
        private readonly Action<IElement> elementChange;
        private readonly TValue elementValue;
    }

    private static JSRuntimeInvocation[] SyncInvocations(BunitJSModuleInterop module)
        => module.Invocations.Where(i => i.Identifier == SyncPropertyIdentifier).ToArray();

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
        module.Setup<string>(GetPropertyIdentifier, i => Equals(i.Arguments[1], ValueProperty)).SetResult(valueAfterReplacement);
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
