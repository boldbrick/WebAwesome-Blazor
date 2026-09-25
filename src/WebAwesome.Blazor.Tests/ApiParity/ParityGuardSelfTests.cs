using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using WebAwesome.Blazor.Components;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Demonstrates on synthetic data that the guards added for 3.12.0 catch the defect classes they were
/// written for, independently of the (fixed) wrapper code they pass against: the enum-value checks
/// (forward and reverse) catch the pre-3.12.0 WaFormat { Auto, Relative, Numeric } mapping of
/// wa-relative-time "format", the bool-vs-literal-union check catches the pre-3.12.0 bool Numeric
/// parameter, the token-list check catches an invalid token in any [Flags] combination (WaTrigger on wa-tooltip
/// trigger), and the render-based event-binding check (EventCallbackBindingParityTests) catches a swapped
/// event name, a deleted binding, an event name held in a constant, a swap inside a base-class helper, a
/// handler bound without the "on" prefix and a derived callback bound to an event the element never
/// dispatches (the "onwa-change" bindings the 3.12.0 sweep rewired). The synthetic wrappers render real Web
/// Awesome tags, so they are checked against the real expected surface and configuration. Each guard is also
/// shown not to flag the fixed shape.
/// </summary>
public class ParityGuardSelfTests
{
    #region ------ Enum values: forward and reverse ------

    [Fact]
    public void EnumValueGuard_FlagsPreFixWaFormatMapping_InBothDirections()
    {
        // Arrange - the pre-3.12.0 WaRelativeTime.Format enum against the real 'long' | 'short' | 'narrow' union
        var binding = CreateBinding(typeof(PreFixFormat), typeof(PreFixFormatMapping), FormatUnion);

        // Act
        var outOfUnion = EnumValueParityTests.OutOfUnionValues(binding).Select(v => v.Member).ToList();
        var unreachable = EnumValueParityTests.UnreachableUnionValues(binding).ToList();

        // Assert - no member emits a valid style, and no valid style can be requested
        Assert.Equal(new[] { "Auto", "Relative", "Numeric" }, outOfUnion);
        Assert.Equal(new[] { "long", "short", "narrow" }, unreachable);
    }

    [Fact]
    public void EnumValueGuard_FlagsMissingEnumMember_InReverseDirectionOnly()
    {
        // Arrange - an enum whose values are all valid but that cannot express 'narrow'
        var binding = CreateBinding(typeof(PartialFormat), typeof(PartialFormatMapping), FormatUnion);

        // Act
        var outOfUnion = EnumValueParityTests.OutOfUnionValues(binding).ToList();
        var unreachable = EnumValueParityTests.UnreachableUnionValues(binding).ToList();

        // Assert - only the reverse check notices the gap
        Assert.Empty(outOfUnion);
        Assert.Equal(new[] { "narrow" }, unreachable);
    }

    [Fact]
    public void EnumValueGuard_AcceptsFixedWaRelativeTimeFormat()
    {
        // Arrange - the fixed enum and its real ToHtmlValue from the wrapper assembly
        var toHtmlValue = typeof(WaRelativeTimeFormat).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Single(m => m.Name == ToHtmlValueMethodName
                && m.IsStatic
                && m.GetParameters() is { Length: 1 } p
                && p[0].ParameterType == typeof(WaRelativeTimeFormat));
        var binding = new EnumValueParityTests.EnumAttributeBinding("wa-relative-time", "format", typeof(WaRelativeTime),
            typeof(WaRelativeTime).GetProperty(nameof(WaRelativeTime.Format))!, typeof(WaRelativeTimeFormat), FormatUnion, toHtmlValue);

        // Act & Assert
        Assert.Empty(EnumValueParityTests.OutOfUnionValues(binding));
        Assert.Empty(EnumValueParityTests.UnreachableUnionValues(binding));
    }

    [Fact]
    public void EnumValueGuard_ChecksEveryFlagCombinationTokenByToken()
    {
        // Arrange - the real WaTrigger flags against the wa-tooltip trigger tokens
        var tokens = GetComponentConfig(TooltipTag).TokenListAttributes[TriggerAttribute];
        var binding = new EnumValueParityTests.EnumAttributeBinding(TooltipTag, TriggerAttribute, typeof(WaTooltip),
            typeof(WaTooltip).GetProperty(nameof(WaTooltip.Trigger))!, typeof(WaTrigger), tokens,
            typeof(WaTrigger).Assembly.GetTypes().SelectMany(t => t.GetMethods())
                .Single(m => m.Name == ToHtmlValueMethodName && m.IsStatic && m.GetParameters() is { Length: 1 } p && p[0].ParameterType == typeof(WaTrigger)),
            IsTokenList: true);

        // Act
        var values = EnumValueParityTests.EnumValuesToCheck(typeof(WaTrigger)).ToList();

        // Assert - four named flags plus the eleven unnamed combinations, all emitting valid tokens only
        Assert.Equal(15, values.Count);
        Assert.Contains(values, v => v.Name == "Click|Hover|Focus|Manual");
        Assert.Empty(EnumValueParityTests.OutOfUnionValues(binding));
        Assert.Empty(EnumValueParityTests.UnreachableUnionValues(binding));
    }

    [Fact]
    public void EnumValueGuard_FlagsInvalidTokenInFlagCombination()
    {
        // Arrange - a flag mapping that misspells one token, visible only in combinations containing it
        var binding = new EnumValueParityTests.EnumAttributeBinding(TooltipTag, TriggerAttribute, typeof(WaTooltip),
            typeof(WaTooltip).GetProperty(nameof(WaTooltip.Trigger))!, typeof(MisspeltTrigger), TriggerTokens,
            typeof(MisspeltTriggerMapping).GetMethod(ToHtmlValueMethodName)!, IsTokenList: true);

        // Act
        var outOfUnion = EnumValueParityTests.OutOfUnionValues(binding).Select(v => v.Member).ToList();

        // Assert
        Assert.Equal(new[] { "Hover", "Click|Hover" }, outOfUnion);
        Assert.Equal(new[] { "hover" }, EnumValueParityTests.UnreachableUnionValues(binding));
    }

    #endregion

    #region ------ Bool parameter bound to a string-literal union ------

    [Fact]
    public void BoolUnionGuard_FlagsPreFixBoolNumericParameter()
    {
        // Arrange - the pre-3.12.0 WaRelativeTime shape: numeric is 'always' | 'auto', the parameter a bool
        var component = CreateSurface(NumericAttribute, NumericUnionType);

        // Act
        var parameter = Assert.Single(EnumValueParityTests.LiteralUnionParameters(SyntheticTag, component, typeof(PreFixRelativeTime)));

        // Assert
        Assert.True(EnumValueParityTests.IsBoolType(parameter.Property.PropertyType));
        Assert.Contains("a bool parameter can emit none of its literals", EnumValueParityTests.DescribeBoolBinding(parameter));
    }

    [Fact]
    public void BoolUnionGuard_FlagsNullableBoolParameter()
    {
        // Arrange - bool? has the same defect: Blazor never emits either literal
        var component = CreateSurface(NumericAttribute, NumericUnionType);

        // Act
        var parameter = Assert.Single(EnumValueParityTests.LiteralUnionParameters(SyntheticTag, component, typeof(NullableBoolRelativeTime)));

        // Assert
        Assert.True(EnumValueParityTests.IsBoolType(parameter.Property.PropertyType));
    }

    [Fact]
    public void BoolUnionGuard_AcceptsFixedEnumNumericParameter()
    {
        // Arrange
        var component = CreateSurface(NumericAttribute, NumericUnionType);

        // Act
        var parameter = Assert.Single(EnumValueParityTests.LiteralUnionParameters(SyntheticTag, component, typeof(WaRelativeTime)));

        // Assert
        Assert.False(EnumValueParityTests.IsBoolType(parameter.Property.PropertyType));
    }

    [Fact]
    public void BoolUnionGuard_IgnoresUnionsWithBooleanMember()
    {
        // Assert - a union that admits boolean is not a pure string-literal union, so a bool parameter is fine
        Assert.Null(EnumValueParityTests.ParseStringLiteralUnion("boolean | 'auto'"));
        Assert.Equal(new[] { "always", "auto" }, EnumValueParityTests.ParseStringLiteralUnion(NumericUnionType));
    }

    #endregion

    #region ------ Rendered event bindings ------

    [Fact]
    public void BindingGuard_AcceptsFixedShape()
    {
        // Arrange
        var wrapper = RenderedWrapperCatalog.Observe(typeof(FixedDialog));

        // Act & Assert
        Assert.Empty(EventCallbackBindingParityTests.BindingMisses(wrapper, Registrations));
        Assert.Empty(EventCallbackBindingParityTests.UnboundCemEvents(wrapper, Registrations));
    }

    [Fact]
    public void BindingGuard_FlagsSwappedEventName()
    {
        // Arrange - review mutation M1: OnShow bound to "onwa-hide"
        var wrapper = RenderedWrapperCatalog.Observe(typeof(SwappedDialog));

        // Act
        var misses = EventCallbackBindingParityTests.BindingMisses(wrapper, Registrations).ToList();

        // Assert
        var miss = Assert.Single(misses);
        Assert.Contains("SwappedDialog.OnShow): binds 'wa-hide', expected 'wa-show'", miss);
        Assert.Contains(EventCallbackBindingParityTests.UnboundCemEvents(wrapper, Registrations), m => m.Contains("'wa-show'"));
    }

    [Fact]
    public void BindingGuard_FlagsDeletedBinding()
    {
        // Arrange - review mutation M2: the OnAfterHide binding deleted
        var wrapper = RenderedWrapperCatalog.Observe(typeof(DeletedBindingDialog));

        // Act
        var misses = EventCallbackBindingParityTests.BindingMisses(wrapper, Registrations).ToList();

        // Assert
        Assert.Contains("DeletedBindingDialog.OnAfterHide): binds nothing, expected 'wa-after-hide'", Assert.Single(misses));
        Assert.Contains("'wa-after-hide'", Assert.Single(EventCallbackBindingParityTests.UnboundCemEvents(wrapper, Registrations)));
    }

    [Fact]
    public void BindingGuard_FlagsEventNameHeldInConstant()
    {
        // Arrange - review mutation M3: a constant holding an event wa-drawer never dispatches
        var wrapper = RenderedWrapperCatalog.Observe(typeof(ConstantNameDrawer));

        // Act
        var misses = EventCallbackBindingParityTests.BindingMisses(wrapper, Registrations).ToList();

        // Assert
        Assert.Contains("ConstantNameDrawer.OnShow): binds 'wa-initial-focus', expected 'wa-show'", Assert.Single(misses));
    }

    [Fact]
    public void BindingGuard_FlagsBaseClassBinding()
    {
        // Arrange - a base-class helper (like WaInputBase.AddCommonEventHandlers) swapping focus and blur
        var wrapper = RenderedWrapperCatalog.Observe(typeof(BaseSwapButton));

        // Act
        var misses = EventCallbackBindingParityTests.BindingMisses(wrapper, Registrations).ToList();

        // Assert
        Assert.Equal(2, misses.Count);
        Assert.Contains(misses, m => m.Contains("BaseSwapButton.OnFocus): binds 'blur', expected 'focus'"));
        Assert.Contains(misses, m => m.Contains("BaseSwapButton.OnBlur): binds 'focus', expected 'blur'"));
    }

    [Fact]
    public void BindingGuard_FlagsHandlerWithoutOnPrefix()
    {
        // Arrange - the pre-fix WaAnimatedImage "load" binding, invisible to bUnit markup and TriggerEvent
        var wrapper = RenderedWrapperCatalog.Observe(typeof(BareNameImage));

        // Act
        var misses = EventCallbackBindingParityTests.BindingMisses(wrapper, Registrations).ToList();

        // Assert
        Assert.Contains(misses, m => m.Contains("BareNameImage.OnLoad): bound under 'load', which lacks the \"on\" prefix"));
        Assert.Contains(misses, m => m.Contains("BareNameImage.OnLoad): binds nothing, expected 'wa-load'"));
    }

    [Fact]
    public void BindingGuard_FlagsDerivedCallbackBoundToUndispatchedEvent()
    {
        // Arrange - the pre-3.12.0 WaCheckbox: OnCheckedChange bound to "onwa-change"
        var wrapper = RenderedWrapperCatalog.Observe(typeof(WaChangeCheckbox));

        // Act
        var misses = EventCallbackBindingParityTests.BindingMisses(wrapper, Registrations).ToList();

        // Assert
        Assert.Contains("WaChangeCheckbox.OnCheckedChange): raised from 'change' (derivedEventCallbacks) but binds 'wa-change'", Assert.Single(misses));
    }

    [Fact]
    public void BindingGuard_ResolvesNumericAliasToItsBrowserEvent()
    {
        // Arrange
        var registrations = JsInitializerEventRegistrations.Parse(SyntheticJsInitializer);

        // Act & Assert
        Assert.Equal("change", registrations.ResolveAlias("numericchange"));
        Assert.Equal("wa-show", registrations.ResolveAlias("wa-show"));
        Assert.Equal(new[] { "wa-show", "wa-hide" }, registrations.EventNames);
        Assert.Equal(new[] { "beforeinput" }, registrations.NativeCustomEventNames);
        Assert.Equal(new[] { "wa-show", "wa-hide", "beforeinput", "numericchange" }, registrations.AllRegisteredNames);
    }

    #endregion
    #region ------ CEM event corroboration ------

    [Fact]
    public void CorroborationGuard_FlagsInventedDataGridRequestEvent()
    {
        // Arrange - the real 3.12.0 surface: the CEM lists 'request', which wa-data-grid never dispatches
        var dataGrid = Surface.Components["wa-data-grid"];

        // Act
        var uncorroborated = CemEventCorroborationTests.UncorroboratedEvents("wa-data-grid", dataGrid, Surface.DeclaredEventTypes);

        // Assert
        Assert.Equal("request", Assert.Single(uncorroborated).EventName);
    }

    [Fact]
    public void CorroborationGuard_FlagsWaEventWithoutEventClass()
    {
        // Arrange - a JSDoc-declared wa-* event dist\events has no class for
        var component = new ComponentSurface
        {
            Events = new Dictionary<string, EventSurface> { ["wa-show"] = new(), ["wa-invented"] = new(), ["blur"] = new() },
            JsDocEvents = new List<string> { "wa-show", "wa-invented", "blur" }
        };

        // Act
        var uncorroborated = CemEventCorroborationTests.UncorroboratedEvents(SyntheticTag, component, new[] { "wa-show" }).ToList();

        // Assert - native events need no event class
        Assert.Equal("wa-invented", Assert.Single(uncorroborated).EventName);
        Assert.Contains("no event class", uncorroborated[0].Reason);
    }

    #endregion

    #region ------ Element method invocations ------

    [Fact]
    public void MethodScan_ReadsNestedGenericInvocations()
    {
        // Arrange - the WaDataGrid shape the old generic-argument pattern skipped (review mutation M4)
        const string source = """
            return await JSInterop.InvokeMethodAsync<IReadOnlyList<IReadOnlyDictionary<string, object>>>(Element.Value, "getVisibleRowz");
            await JSInterop.InvokeMethodAsync(Element.Value, "focus", new { preventScroll });
            """;

        // Act
        var names = ElementMethodInvocationTests.InvokedMethods(source).Select(i => i.MethodName);

        // Assert
        Assert.Equal(new[] { "getVisibleRowz", "focus" }, names);
    }

    [Fact]
    public void MethodScan_TracesHelperParameterToCallSiteLiterals()
    {
        // Arrange - the WaVideo forwarding helper
        const string source = """
            public Task PlayAsync() => InvokeVoidElementMethodAsync("play");
            public Task SeekAsync(double time) => InvokeVoidElementMethodAsync("seek", time);
            private Task InvokeVoidElementMethodAsync(string methodName, params object[] args)
                => JSInterop.InvokeMethodAsync(Element.Value, methodName, args);
            """;

        // Act
        var names = ElementMethodInvocationTests.InvokedMethods(source).Select(i => i.MethodName);

        // Assert
        Assert.Equal(new[] { "play", "seek" }, names);
    }

    [Fact]
    public void MethodScan_ReportsUntraceableMethodName()
    {
        // Arrange
        const string source = "await JSInterop.InvokeMethodAsync(Element.Value, computedName);";

        // Act
        var invocation = Assert.Single(ElementMethodInvocationTests.InvokedMethods(source));

        // Assert
        Assert.Null(invocation.MethodName);
        Assert.Equal("computedName", invocation.Expression);
    }

    [Fact]
    public void MethodScan_ResolvesBaseClassesAndComputedTagsToRenderedElements()
    {
        // Act - WaInputBase declares resetValidity, WaChartBase renders OpenElement(0, TagName)
        var inputTags = ElementMethodInvocationTests.RenderedTagsOf(new HashSet<string> { "WaInputBase" });
        var chartTags = ElementMethodInvocationTests.RenderedTagsOf(new HashSet<string> { "WaChartBase" });
        var unknownTags = ElementMethodInvocationTests.RenderedTagsOf(new HashSet<string> { "NoSuchWrapper" });

        // Assert
        Assert.Contains("wa-input", inputTags);
        Assert.Contains("wa-slider", inputTags);
        Assert.Contains("wa-bar-chart", chartTags);
        Assert.Empty(unknownTags);
    }

    #endregion

    #region ------ Internals ------

    private const string ToHtmlValueMethodName = "ToHtmlValue";
    private const string SyntheticTag = "wa-relative-time";
    private const string FormatAttribute = "format";
    private const string NumericAttribute = "numeric";
    private const string NumericUnionType = "'always' | 'auto'";

    private const string TooltipTag = "wa-tooltip";
    private const string TriggerAttribute = "trigger";

    private static readonly IReadOnlyList<string> FormatUnion = new[] { "long", "short", "narrow" };
    private static readonly IReadOnlyList<string> TriggerTokens = new[] { "click", "hover" };
    private static readonly JsInitializerEventRegistrations Registrations = JsInitializerEventRegistrations.Current;

    private const string SyntheticJsInitializer = """
        const eventNames = [
          'wa-show',
          'wa-hide',
        ];
        const nativeCustomEventNames = [
          'beforeinput',
        ];
        const numericValueEventAliases = {
          'numericchange': 'change',
        };
        """;

    private static EnumValueParityTests.EnumAttributeBinding CreateBinding(Type enumType, Type mappingType, IReadOnlyList<string> union)
    {
        var toHtmlValue = mappingType.GetMethod(ToHtmlValueMethodName)!;
        return new EnumValueParityTests.EnumAttributeBinding(SyntheticTag, FormatAttribute, typeof(PreFixRelativeTime),
            typeof(PreFixRelativeTime).GetProperty(nameof(PreFixRelativeTime.Numeric))!, enumType, union, toHtmlValue);
    }

    private static ComponentSurface CreateSurface(string attributeName, string attributeType)
    {
        return new ComponentSurface
        {
            Attributes = new Dictionary<string, AttributeSurface>
            {
                [attributeName] = new AttributeSurface { Type = attributeType }
            }
        };
    }

    // the pre-3.12.0 WaFormat enum and its mapping
    private enum PreFixFormat
    {
        Auto,
        Relative,
        Numeric
    }

    private static class PreFixFormatMapping
    {
        public static string ToHtmlValue(PreFixFormat format) => format.ToString().ToLowerInvariant();
    }

    private enum PartialFormat
    {
        Long,
        Short
    }

    private static class PartialFormatMapping
    {
        public static string ToHtmlValue(PartialFormat format) => format.ToString().ToLowerInvariant();
    }

    [Flags]
    private enum MisspeltTrigger
    {
        Click = 1,
        Hover = 2
    }

    private static class MisspeltTriggerMapping
    {
        public static string ToHtmlValue(MisspeltTrigger trigger)
        {
            var tokens = new List<string>();
            if (trigger.HasFlag(MisspeltTrigger.Click)) tokens.Add("click");
            if (trigger.HasFlag(MisspeltTrigger.Hover)) tokens.Add("hovers");
            return string.Join(' ', tokens);
        }
    }

    // the pre-3.12.0 WaRelativeTime parameter shape
    private class PreFixRelativeTime
    {
        [Parameter] public bool Numeric { get; set; }
    }

    private class NullableBoolRelativeTime
    {
        [Parameter] public bool? Numeric { get; set; }
    }


    /// <summary>
    /// wa-dialog with every event bound correctly (the fixed shape).
    /// </summary>
    private sealed class FixedDialog : ComponentBase
    {
        [Parameter] public EventCallback<EventArgs> OnShow { get; set; }
        [Parameter] public EventCallback<EventArgs> OnHide { get; set; }
        [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }
        [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "wa-dialog");
            AddIfSet(builder, 1, "onwa-show", OnShow);
            AddIfSet(builder, 2, "onwa-hide", OnHide);
            AddIfSet(builder, 3, "onwa-after-show", OnAfterShow);
            AddIfSet(builder, 4, "onwa-after-hide", OnAfterHide);
            builder.CloseElement();
        }
    }

    /// <summary>
    /// wa-dialog whose OnShow is bound to the hide event (review mutation M1).
    /// </summary>
    private sealed class SwappedDialog : ComponentBase
    {
        [Parameter] public EventCallback<EventArgs> OnShow { get; set; }
        [Parameter] public EventCallback<EventArgs> OnHide { get; set; }
        [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }
        [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "wa-dialog");
            AddIfSet(builder, 1, "onwa-hide", OnShow);
            AddIfSet(builder, 2, "onwa-hide", OnHide);
            AddIfSet(builder, 3, "onwa-after-show", OnAfterShow);
            AddIfSet(builder, 4, "onwa-after-hide", OnAfterHide);
            builder.CloseElement();
        }
    }

    /// <summary>
    /// wa-dialog whose OnAfterHide binding was deleted (review mutation M2).
    /// </summary>
    private sealed class DeletedBindingDialog : ComponentBase
    {
        [Parameter] public EventCallback<EventArgs> OnShow { get; set; }
        [Parameter] public EventCallback<EventArgs> OnHide { get; set; }
        [Parameter] public EventCallback<EventArgs> OnAfterShow { get; set; }
        [Parameter] public EventCallback<EventArgs> OnAfterHide { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "wa-dialog");
            AddIfSet(builder, 1, "onwa-show", OnShow);
            AddIfSet(builder, 2, "onwa-hide", OnHide);
            AddIfSet(builder, 3, "onwa-after-show", OnAfterShow);
            builder.CloseElement();
        }
    }

    /// <summary>
    /// wa-drawer whose OnShow is bound through a constant to an event the element never dispatches (review
    /// mutation M3).
    /// </summary>
    private sealed class ConstantNameDrawer : ComponentBase
    {
        [Parameter] public EventCallback<EventArgs> OnShow { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            const string deadShowEvent = "onwa-initial-focus";

            builder.OpenElement(0, "wa-drawer");
            AddIfSet(builder, 1, deadShowEvent, OnShow);
            builder.CloseElement();
        }
    }

    /// <summary>
    /// Base class whose shared handler helper swaps focus and blur, like a defect in WaInputBase would.
    /// </summary>
    private abstract class SwappingFocusBase : ComponentBase
    {
        [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
        [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

        protected void AddFocusHandlers(RenderTreeBuilder builder, int sequence)
        {
            AddIfSet(builder, sequence, "onblur", OnFocus);
            AddIfSet(builder, sequence + 1, "onfocus", OnBlur);
        }
    }

    /// <summary>
    /// wa-button inheriting the swapped focus handlers from its base class.
    /// </summary>
    private sealed class BaseSwapButton : SwappingFocusBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "wa-button");
            AddFocusHandlers(builder, 1);
            builder.CloseElement();
        }
    }

    /// <summary>
    /// wa-animated-image binding its load event under the bare name "load" (the pre-fix WaAnimatedImage).
    /// </summary>
    private sealed class BareNameImage : ComponentBase
    {
        [Parameter] public EventCallback OnLoad { get; set; }
        [Parameter] public EventCallback OnError { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "wa-animated-image");
            if (OnLoad.HasDelegate) builder.AddAttribute(1, "load", OnLoad);
            if (OnError.HasDelegate) builder.AddAttribute(2, "onwa-error", OnError);
            builder.CloseElement();
        }
    }

    /// <summary>
    /// wa-checkbox binding its derived OnCheckedChange to "onwa-change" (the pre-3.12.0 WaCheckbox).
    /// </summary>
    private sealed class WaChangeCheckbox : ComponentBase
    {
        [Parameter] public EventCallback<bool> OnCheckedChange { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "wa-checkbox");
            builder.AddAttribute(1, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => { }));
            if (OnCheckedChange.HasDelegate) builder.AddAttribute(2, "onwa-change", OnCheckedChange);
            builder.CloseElement();
        }
    }

    private static void AddIfSet<T>(RenderTreeBuilder builder, int sequence, string name, EventCallback<T> callback)
    {
        if (callback.HasDelegate) builder.AddAttribute(sequence, name, callback);
    }

    #endregion
}
