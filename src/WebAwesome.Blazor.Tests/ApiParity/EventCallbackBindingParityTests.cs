using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Render-based event-binding parity: every wrapper component is rendered with bUnit (see
/// RenderedWrapperCatalog), once plain and once per EventCallback parameter with a no-op delegate on just that
/// parameter, and the event handlers the renderer registers on the root element are compared with the Custom
/// Elements Manifest events of the tag the wrapper actually renders. The expected event of a callback follows the
/// naming convention ("wa-after-hide" -> OnAfterHide, "blur" -> OnBlur) plus the component's "eventOverrides";
/// native DOM events the CEM does not list are admitted through "nativeDomEvents", and a handler bound to a JS
/// alias ("onnumericchange") counts as its browser event. Because the check inspects the render tree, it sees
/// bindings made through constants, variables or base-class helpers, bindings of secondary wrappers (WaRange
/// renders wa-slider), and handlers under a name without the "on" prefix, which Blazor's browser renderer
/// rejects. It catches a callback bound to the wrong event, a deleted binding, a binding to an event the element
/// never dispatches, and a CEM event no callback binds. Deliberate deviations are allowlisted in
/// parity-config.json ("derivedEventCallbacks", "unboundEventCallbacks", "undeclaredBoundEvents",
/// "nativeDomEvents", "ignoredEvents", "eventOverrides"), each with a reason, and stale entries fail. Skipped until parity-config.json sets
/// "enabled": true, like the other parity tests.
/// </summary>
public class EventCallbackBindingParityTests
{
    /// <summary>
    /// Every EventCallback parameter must add exactly the handler(s) of its expected event(s) to the root
    /// element, and every handler the wrapper binds without any callback must be an event of the element.
    /// </summary>
    [Fact]
    public void AllEventCallbacks_BindTheirCemEvent()
    {
        SkipUnlessParityEnabled();

        var misses = RenderedWrapperCatalog.All.SelectMany(w => BindingMisses(w, JsInitializerEventRegistrations.Current)).ToList();

        AssertNoMisses(misses, "Event callbacks not bound to the element event they stand for");
    }

    /// <summary>
    /// Every CEM event of a rendered element must be bound by some callback of each wrapper rendering it; an
    /// event listed in "ignoredEvents" must be bound by the wrapper's value handling instead.
    /// </summary>
    [Fact]
    public void AllCemEvents_AreBoundByEveryWrapperOfTheElement()
    {
        SkipUnlessParityEnabled();

        var misses = RenderedWrapperCatalog.All.SelectMany(w => UnboundCemEvents(w, JsInitializerEventRegistrations.Current)).ToList();

        AssertNoMisses(misses, "CEM events no callback of the wrapper binds");
    }

    /// <summary>
    /// Every event-binding allowlist entry must carry a rationale in ignoreReasons.
    /// </summary>
    [Fact]
    public void EventBindingAllowlists_HaveReasons()
    {
        var keys = new List<string>();
        keys.AddRange(Config.NativeDomEvents.Keys.Select(e => $"{NativeDomEventsKey}:{e}"));

        foreach (var (tag, componentConfig) in Config.Components)
        {
            keys.AddRange(componentConfig.DerivedEventCallbacks.Keys.Select(c => $"{DerivedEventCallbacksKey}:{tag}:{c}"));
            keys.AddRange(componentConfig.UnboundEventCallbacks.Select(c => $"{UnboundEventCallbacksKey}:{tag}:{c}"));
            keys.AddRange(componentConfig.UndeclaredBoundEvents.Select(e => $"{UndeclaredBoundEventsKey}:{tag}:{e}"));
        }

        var misses = keys
            .Where(key => !HasReason(key))
            .Select(key => $"allowlist entry has no ignoreReasons entry '{key}'")
            .ToList();

        AssertNoMisses(misses, "Event-binding allowlist entries without a reason");
    }

    /// <summary>
    /// Every event-binding allowlist entry must still suppress a miss for some rendered wrapper; an "ignoredEvents"
    /// entry must be a CEM event of a rendered element that no EventCallback binds, and an "eventOverrides" entry
    /// must differ from the convention and map an event of the element to an EventCallback a wrapper of it has.
    /// </summary>
    [Fact]
    public void EventBindingAllowlists_AreNotStale()
    {
        SkipUnlessParityEnabled();

        var misses = StaleAllowlistEntries(RenderedWrapperCatalog.All, JsInitializerEventRegistrations.Current).ToList();

        AssertNoMisses(misses, "Stale event-binding allowlist entries");
    }

    #region ------ Internals ------

    private const string NativeDomEventsKey = "nativeDomEvents";
    private const string DerivedEventCallbacksKey = "derivedEventCallbacks";
    private const string UnboundEventCallbacksKey = "unboundEventCallbacks";
    private const string UndeclaredBoundEventsKey = "undeclaredBoundEvents";
    private const string CallbackPrefix = "On";
    private const string WaEventPrefix = "wa-";
    private const string BindingCallbackSuffix = "Changed";

    /// <summary>
    /// How one EventCallback parameter relates to element events.
    /// </summary>
    private enum CallbackKind
    {
        // named after the event it binds (convention, eventOverrides or nativeDomEvents)
        Direct,

        // a two-way binding callback (XChanged next to an X parameter), raised by the value handling
        ValueBinding,

        // raised from the handler of another event (derivedEventCallbacks)
        Derived,

        // deliberately unbound (unboundEventCallbacks)
        Unbound
    }

    /// <summary>
    /// Collects the binding misses of one rendered wrapper: callbacks that bind other events than expected (or
    /// none), handlers bound under a name Blazor cannot dispatch, and always-bound handlers for events the
    /// element does not dispatch.
    /// </summary>
    /// <param name="wrapper">The rendered wrapper</param>
    /// <param name="registrations">The JS initializer registrations (for alias resolution)</param>
    /// <returns>Descriptions of the misses</returns>
    internal static IEnumerable<string> BindingMisses(RenderedWrapper wrapper, JsInitializerEventRegistrations registrations)
    {
        var name = wrapper.ComponentType.Name;

        if (wrapper.Error != null)
        {
            yield return $"{name}: failed to render: {wrapper.Error}";
            yield break;
        }

        foreach (var callback in wrapper.Callbacks.Where(c => c.Error != null))
            yield return $"{name}.{callback.Name}: failed to render with the callback set: {callback.Error}";

        if (!TryGetElement(wrapper, out var tag, out var component))
        {
            if (wrapper.Callbacks.Count > 0 || wrapper.BaselineHandlers.Count > 0)
                yield return $"{name}: binds events on its root element '{wrapper.Tag ?? "(none)"}', which is not a custom element of the expected surface, so its events cannot be checked";
            yield break;
        }

        var componentConfig = GetComponentConfig(tag);
        var elementEvents = ElementEvents(component, componentConfig);

        foreach (var handler in wrapper.BaselineHandlers.OrderBy(h => h, StringComparer.Ordinal))
        {
            var eventName = RenderedWrapperCatalog.EventNameOf(handler);
            if (eventName == null)
                yield return $"{tag} ({name}): handler bound under '{handler}', which lacks the \"on\" prefix - Blazor's browser renderer rejects it";
            else if (!IsAdmittedEvent(registrations.ResolveAlias(eventName), elementEvents))
                yield return $"{tag} ({name}): always binds '{handler}', but '{registrations.ResolveAlias(eventName)}' is not an event of the element (declared: {Describe(elementEvents)})";
        }

        foreach (var callback in wrapper.Callbacks.Where(c => c.Error == null))
        {
            foreach (var miss in CallbackMisses(wrapper, tag, componentConfig, elementEvents, callback, registrations))
                yield return miss;
        }
    }

    /// <summary>
    /// Collects the CEM events of the rendered element that no callback of the wrapper binds, and the
    /// "ignoredEvents" the wrapper's value handling does not bind.
    /// </summary>
    /// <param name="wrapper">The rendered wrapper</param>
    /// <param name="registrations">The JS initializer registrations (for alias resolution)</param>
    /// <returns>Descriptions of the misses</returns>
    internal static IEnumerable<string> UnboundCemEvents(RenderedWrapper wrapper, JsInitializerEventRegistrations registrations)
    {
        if (wrapper.Error != null || !TryGetElement(wrapper, out var tag, out var component)) yield break;

        var componentConfig = GetComponentConfig(tag);
        var baseline = ResolvedEvents(wrapper.BaselineHandlers, registrations);
        var bound = wrapper.Callbacks
            .SelectMany(c => ResolvedEvents(c.AddedHandlers, registrations))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var eventName in ElementEvents(component, componentConfig).CemEvents)
        {
            if (componentConfig.IgnoredEvents.Contains(eventName))
            {
                if (!baseline.Contains(eventName))
                    yield return $"{tag} ({wrapper.ComponentType.Name}): event '{eventName}' is in ignoredEvents (folded into the value binding), but the wrapper does not bind it";
            }
            else if (!bound.Contains(eventName))
            {
                yield return $"{tag} ({wrapper.ComponentType.Name}): CEM event '{eventName}' is bound by no EventCallback (expected '{ExpectedCallbackName(eventName, componentConfig)}')";
            }
        }
    }

    /// <summary>
    /// Returns the allowlist entries that suppress no miss for any rendered wrapper.
    /// </summary>
    /// <param name="wrappers">The rendered wrappers</param>
    /// <param name="registrations">The JS initializer registrations (for alias resolution)</param>
    /// <returns>Descriptions of the stale entries</returns>
    internal static IEnumerable<string> StaleAllowlistEntries(IReadOnlyList<RenderedWrapper> wrappers, JsInitializerEventRegistrations registrations)
    {
        var usedNative = new HashSet<string>(StringComparer.Ordinal);
        var usedUndeclared = new HashSet<(string Tag, string Event)>();
        var usedCallbacks = new HashSet<(string Tag, string Callback)>();
        var boundByCallback = new HashSet<(string Tag, string Event)>();
        var renderedTags = new HashSet<string>(StringComparer.Ordinal);

        foreach (var wrapper in wrappers)
        {
            if (wrapper.Error != null || !TryGetElement(wrapper, out var tag, out var component)) continue;

            renderedTags.Add(tag);
            foreach (var callback in wrapper.Callbacks)
                boundByCallback.UnionWith(ResolvedEvents(callback.AddedHandlers, registrations).Select(e => (tag, e)));

            var componentConfig = GetComponentConfig(tag);
            var declared = component.Events?.Keys.Except(componentConfig.CemOnlyEvents).ToHashSet(StringComparer.Ordinal)
                ?? new HashSet<string>(StringComparer.Ordinal);
            var events = ResolvedEvents(wrapper.BaselineHandlers, registrations)
                .Concat(wrapper.Callbacks.SelectMany(c => ResolvedEvents(c.AddedHandlers, registrations)));

            foreach (var eventName in events.Where(e => !declared.Contains(e)))
            {
                if (componentConfig.UndeclaredBoundEvents.Contains(eventName)) usedUndeclared.Add((tag, eventName));
                else usedNative.Add(eventName);
            }

            foreach (var callback in wrapper.Callbacks)
                usedCallbacks.Add((tag, callback.Name));
        }

        foreach (var eventName in Config.NativeDomEvents.Keys.Where(e => !usedNative.Contains(e)))
            yield return $"nativeDomEvents entry '{eventName}' admits no binding (no wrapper binds it on an element whose CEM entry lacks it) and must be removed";

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var eventName in componentConfig.UndeclaredBoundEvents.Where(e => !usedUndeclared.Contains((tag, e))))
                yield return $"{tag}: undeclaredBoundEvents entry '{eventName}' suppresses no miss (not bound, or now declared by the CEM) and must be removed";

            foreach (var callback in componentConfig.DerivedEventCallbacks.Keys.Where(c => !usedCallbacks.Contains((tag, c))))
                yield return $"{tag}: derivedEventCallbacks entry '{callback}' names no EventCallback of a wrapper rendering the element and must be removed";

            foreach (var callback in componentConfig.UnboundEventCallbacks.Where(c => !usedCallbacks.Contains((tag, c))))
                yield return $"{tag}: unboundEventCallbacks entry '{callback}' names no EventCallback of a wrapper rendering the element and must be removed";

            var elementEvents = Surface.Components.TryGetValue(tag, out var component) ? ElementEvents(component, componentConfig) : null;

            foreach (var eventName in componentConfig.IgnoredEvents)
            {
                if (elementEvents == null || !elementEvents.CemEvents.Contains(eventName))
                    yield return $"{tag}: ignoredEvents entry '{eventName}' is no CEM event of the element and must be removed";
                else if (!renderedTags.Contains(tag))
                    yield return $"{tag}: ignoredEvents entry '{eventName}' belongs to an element no wrapper renders and must be removed";
                else if (boundByCallback.Contains((tag, eventName)))
                    yield return $"{tag}: ignoredEvents entry '{eventName}' is now bound by an EventCallback, which the regular checks own, and must be removed";
            }

            foreach (var (eventName, callback) in componentConfig.EventOverrides)
            {
                if (callback == ConventionalCallbackName(eventName))
                    yield return $"{tag}: eventOverrides entry '{eventName}' -> '{callback}' equals the naming convention and must be removed";
                else if (elementEvents == null || !(elementEvents.CemEvents.Contains(eventName) || elementEvents.UndeclaredEvents.Contains(eventName)))
                    yield return $"{tag}: eventOverrides entry '{eventName}' -> '{callback}' names no event of the element and must be removed";
                else if (!usedCallbacks.Contains((tag, callback)))
                    yield return $"{tag}: eventOverrides entry '{eventName}' -> '{callback}' names no EventCallback of a wrapper rendering the element and must be removed";
            }
        }
    }

    /// <summary>
    /// The events a wrapper of an element may bind.
    /// </summary>
    /// <param name="CemEvents">CEM events of the element, without its cemOnlyEvents</param>
    /// <param name="UndeclaredEvents">Allowlisted undeclaredBoundEvents of the element</param>
    private sealed record ElementEventSet(IReadOnlySet<string> CemEvents, IReadOnlySet<string> UndeclaredEvents);

    private static ElementEventSet ElementEvents(ComponentSurface component, ComponentParityConfig componentConfig)
    {
        var cemEvents = (component.Events?.Keys ?? Enumerable.Empty<string>())
            .Except(componentConfig.CemOnlyEvents)
            .ToHashSet(StringComparer.Ordinal);
        return new ElementEventSet(cemEvents, componentConfig.UndeclaredBoundEvents.ToHashSet(StringComparer.Ordinal));
    }

    private static bool IsAdmittedEvent(string eventName, ElementEventSet events)
    {
        return events.CemEvents.Contains(eventName)
            || events.UndeclaredEvents.Contains(eventName)
            || Config.NativeDomEvents.ContainsKey(eventName);
    }

    private static IEnumerable<string> CallbackMisses(RenderedWrapper wrapper, string tag, ComponentParityConfig componentConfig,
        ElementEventSet elementEvents, RenderedCallback callback, JsInitializerEventRegistrations registrations)
    {
        var label = $"{tag} ({wrapper.ComponentType.Name}.{callback.Name})";

        foreach (var handler in callback.AddedHandlers.Where(h => RenderedWrapperCatalog.EventNameOf(h) == null))
            yield return $"{label}: bound under '{handler}', which lacks the \"on\" prefix - Blazor's browser renderer rejects it";

        var observed = ResolvedEvents(callback.AddedHandlers, registrations);
        var baseline = ResolvedEvents(wrapper.BaselineHandlers, registrations);

        switch (Classify(wrapper, componentConfig, callback.Name))
        {
            case CallbackKind.Unbound:
                if (observed.Count > 0)
                    yield return $"{label}: listed in unboundEventCallbacks but binds {Describe(observed)}";
                break;

            case CallbackKind.Derived:
                var sourceEvent = componentConfig.DerivedEventCallbacks[callback.Name];
                if (observed.Any(e => e != sourceEvent))
                    yield return $"{label}: raised from '{sourceEvent}' (derivedEventCallbacks) but binds {Describe(observed)}";
                if (!observed.Contains(sourceEvent) && !baseline.Contains(sourceEvent))
                    yield return $"{label}: raised from '{sourceEvent}' (derivedEventCallbacks), but no '{sourceEvent}' handler is bound";
                if (!IsAdmittedEvent(sourceEvent, elementEvents))
                    yield return $"{label}: raised from '{sourceEvent}' (derivedEventCallbacks), which is not an event of the element";
                break;

            case CallbackKind.ValueBinding:
                foreach (var eventName in observed.Where(e => !IsAdmittedEvent(e, elementEvents)))
                    yield return $"{label}: binds '{eventName}', which is not an event of the element (declared: {Describe(elementEvents)})";
                break;

            default:
                var expected = ExpectedEvents(callback.Name, componentConfig, elementEvents);
                if (expected.Count == 0)
                {
                    yield return observed.Count == 0
                        ? $"{label}: stands for no event of the element (declared: {Describe(elementEvents)}) and binds nothing"
                        : $"{label}: binds {Describe(observed)}, but its name stands for no event of the element (declared: {Describe(elementEvents)})";
                }
                else if (!observed.SetEquals(expected))
                {
                    yield return observed.Count == 0
                        ? $"{label}: binds nothing, expected {Describe(expected)}"
                        : $"{label}: binds {Describe(observed)}, expected {Describe(expected)}";
                }
                break;
        }
    }

    private static CallbackKind Classify(RenderedWrapper wrapper, ComponentParityConfig componentConfig, string callbackName)
    {
        if (componentConfig.UnboundEventCallbacks.Contains(callbackName)) return CallbackKind.Unbound;
        if (componentConfig.DerivedEventCallbacks.ContainsKey(callbackName)) return CallbackKind.Derived;
        if (IsValueBindingCallback(wrapper.ComponentType, callbackName)) return CallbackKind.ValueBinding;
        return CallbackKind.Direct;
    }

    // Blazor's two-way binding convention: @bind-X pairs parameter X with the callback XChanged
    private static bool IsValueBindingCallback(Type componentType, string callbackName)
    {
        return callbackName.EndsWith(BindingCallbackSuffix, StringComparison.Ordinal)
            && FindParameter(componentType, callbackName[..^BindingCallbackSuffix.Length]) != null;
    }

    // the events whose conventional (or overridden) callback name is this callback
    private static HashSet<string> ExpectedEvents(string callbackName, ComponentParityConfig componentConfig, ElementEventSet elementEvents)
    {
        var expected = elementEvents.CemEvents
            .Concat(elementEvents.UndeclaredEvents)
            .Where(e => ExpectedCallbackName(e, componentConfig) == callbackName)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var (eventName, nativeCallback) in Config.NativeDomEvents)
        {
            if (nativeCallback == callbackName) expected.Add(eventName);
        }

        return expected;
    }

    /// <summary>
    /// Returns the name of the EventCallback expected to carry an event.
    /// </summary>
    /// <param name="eventName">Event name, e.g. "wa-after-hide" or "blur"</param>
    /// <param name="componentConfig">Parity configuration of the element</param>
    /// <returns>The eventOverrides entry, or "On" plus the PascalCase event name without the "wa-" prefix</returns>
    internal static string ExpectedCallbackName(string eventName, ComponentParityConfig componentConfig)
    {
        return componentConfig.EventOverrides.TryGetValue(eventName, out var over) ? over : ConventionalCallbackName(eventName);
    }

    // "On" plus the PascalCase event name without the "wa-" prefix
    private static string ConventionalCallbackName(string eventName)
    {
        var baseName = eventName.StartsWith(WaEventPrefix, StringComparison.Ordinal) ? eventName[WaEventPrefix.Length..] : eventName;
        return CallbackPrefix + ToPascalCase(baseName);
    }

    private static HashSet<string> ResolvedEvents(IEnumerable<string> handlers, JsInitializerEventRegistrations registrations)
    {
        return handlers
            .Select(RenderedWrapperCatalog.EventNameOf)
            .Where(e => e != null)
            .Select(e => registrations.ResolveAlias(e!))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool TryGetElement(RenderedWrapper wrapper, out string tag, out ComponentSurface component)
    {
        tag = wrapper.Tag ?? string.Empty;
        component = null!;

        if (wrapper.Tag == null || Config.IgnoredComponents.Contains(wrapper.Tag)) return false;
        if (!Surface.Components.TryGetValue(wrapper.Tag, out var found)) return false;

        component = found;
        return true;
    }

    private static string Describe(IEnumerable<string> events)
    {
        var list = events.OrderBy(e => e, StringComparer.Ordinal).ToList();
        return list.Count == 0 ? "none" : string.Join(", ", list.Select(e => $"'{e}'"));
    }

    private static string Describe(ElementEventSet events) => Describe(events.CemEvents.Concat(events.UndeclaredEvents));

    #endregion
}
