using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WebAwesome.Blazor.Base;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Guards the contract between the handlers the wrappers render and the custom event types the JS initializer
/// (WebAwesome.Blazor.lib.module.js) registers with Blazor.registerCustomEventType, in both directions. Blazor
/// only delivers an event to .NET when it knows the event type: its built-in types (the EventHandler attributes of
/// Microsoft.AspNetCore.Components.Web.EventHandlers, e.g. onchange, onfocus) or a registered custom type. An
/// unregistered custom event is dropped silently in the browser, so every custom event a wrapper renders must be
/// registered; conversely every registration must be bound by some wrapper and dispatched by the element (a
/// CEM event), or it is a leftover of a removed binding. The wrapper side is the rendered output of
/// RenderedWrapperCatalog, not a source scan, so bindings made through constants, computed sequence numbers or
/// base-class helpers are seen. The numericchange/numericinput aliases and the relayed events (wablazor-*) are
/// checked against the render-tree attribute constants in Base\Constants.cs both ways; a relayed event must be
/// bound only on the elements the initializer relays it for, and always with Blazor's stopPropagation.
/// </summary>
public class EventBindingRegistrationTests
{
    /// <summary>
    /// Every event handler a wrapper renders must be a Blazor built-in event or registered by the JS initializer.
    /// </summary>
    [Fact]
    public void AllRenderedCustomEvents_AreRegisteredInJsInitializer()
    {
        var registered = JsInitializerEventRegistrations.Current.AllRegisteredNames.ToHashSet(StringComparer.Ordinal);
        var builtIn = BlazorBuiltInEventNames();
        var misses = new List<string>();
        var handlerCount = 0;

        foreach (var wrapper in RenderedWrapperCatalog.All)
        {
            foreach (var handler in wrapper.AllHandlers)
            {
                handlerCount++;
                var eventName = RenderedWrapperCatalog.EventNameOf(handler);
                if (eventName == null || builtIn.Contains(eventName) || registered.Contains(eventName)) continue;

                misses.Add($"{wrapper.ComponentType.Name} ({wrapper.Tag}): renders '{handler}', but '{eventName}' is neither a Blazor built-in event nor registered in {JsInitializerEventRegistrations.FileName}");
            }
        }

        // guard the harness itself: a catalog that silently rendered nothing would pass vacuously
        Assert.True(handlerCount > 0, "The rendered wrappers bind no event handlers at all");

        AssertNoMisses(misses, "Rendered custom events without registerCustomEventType registration");
    }

    /// <summary>
    /// Every event the JS initializer registers must be bound by at least one wrapper and be an event the
    /// element it is bound on dispatches (CEM-declared), unless allowlisted with a reason.
    /// </summary>
    [Fact]
    public void AllJsRegistrations_AreBoundAndDispatchedByTheElement()
    {
        SkipUnlessParityEnabled();

        var registrations = JsInitializerEventRegistrations.Current;
        var bindings = RenderedBindings();
        var misses = new List<string>();

        foreach (var name in registrations.AllRegisteredNames.Where(n => !UnboundRegistrationAllowlist.ContainsKey(n)))
        {
            var tags = bindings.Where(b => b.EventName == name).Select(b => b.Tag).Distinct().ToList();
            if (tags.Count == 0)
            {
                misses.Add($"'{name}' is registered, but no wrapper binds it");
                continue;
            }

            // an alias is dispatched as its browser event, a relay as the element event it relays
            var dispatched = registrations.ResolveAlias(name);
            foreach (var tag in tags.Where(t => !IsDeclaredEvent(t, dispatched)))
                misses.Add($"'{name}' is registered and bound on '{tag}', whose CEM entry does not declare '{dispatched}'");

            if (!registrations.RelayedEvents.TryGetValue(name, out var relay)) continue;

            // the initializer dispatches a relayed event only on its hosts, so a binding elsewhere never fires
            foreach (var tag in tags.Where(t => !relay.Hosts.Contains(t)))
                misses.Add($"'{name}' is bound on '{tag}', but {JsInitializerEventRegistrations.RelayedEventsMap} relays '{relay.Event}' only for {string.Join(", ", relay.Hosts)}");
            foreach (var host in relay.Hosts.Where(h => !tags.Contains(h)))
                misses.Add($"{JsInitializerEventRegistrations.RelayedEventsMap} relays '{relay.Event}' as '{name}' for '{host}', but no wrapper of '{host}' binds it");
        }

        AssertNoMisses(misses, "JS initializer registrations that no wrapper binds or no element dispatches");
    }

    /// <summary>
    /// No registration may reuse a Blazor built-in event name: Blazor.registerCustomEventType rejects a name
    /// that is already registered, which would abort the registration of all later names.
    /// </summary>
    [Fact]
    public void NoJsRegistration_ShadowsBlazorBuiltInEvent()
    {
        var builtIn = BlazorBuiltInEventNames();

        var misses = JsInitializerEventRegistrations.Current.AllRegisteredNames
            .Where(builtIn.Contains)
            .Select(name => $"'{name}' is registered as a custom event, but Blazor already has a built-in '{name}' event")
            .ToList();

        AssertNoMisses(misses, "JS initializer registrations shadowing Blazor built-in events");
    }

    /// <summary>
    /// Every alias event attribute constant (Constants.*EventAttribute) must name a key of the JS
    /// numericValueEventAliases or relayedEvents map, and every key must have a constant; an alias must listen to
    /// a Blazor built-in event, whose reader it replaces, and a relayed name must be private: neither a Blazor
    /// built-in event nor a wa-* name a page listener could mistake for the element's own event.
    /// </summary>
    [Fact]
    public void AliasEventAttributeConstants_MatchJsAliases()
    {
        var registrations = JsInitializerEventRegistrations.Current;
        var aliases = registrations.NumericValueEventAliases;
        var relays = registrations.RelayedEvents;
        var constants = AliasEventAttributeConstants();
        Assert.NotEmpty(constants);

        var misses = new List<string>();

        foreach (var (field, attribute) in constants)
        {
            var eventName = RenderedWrapperCatalog.EventNameOf(attribute);
            if (eventName == null)
                misses.Add($"Constants.{field} = \"{attribute}\" lacks the \"on\" prefix of an event handler attribute");
            else if (!aliases.ContainsKey(eventName) && !relays.ContainsKey(eventName))
                misses.Add($"Constants.{field} = \"{attribute}\", but '{eventName}' is a key of neither {JsInitializerEventRegistrations.NumericValueEventAliasesMap} nor {JsInitializerEventRegistrations.RelayedEventsMap} in {JsInitializerEventRegistrations.FileName}");
        }

        var constantEvents = constants.Select(c => RenderedWrapperCatalog.EventNameOf(c.Attribute)).ToHashSet(StringComparer.Ordinal);
        var builtIn = BlazorBuiltInEventNames();

        foreach (var (alias, browserEventName) in aliases)
        {
            if (!constantEvents.Contains(alias))
                misses.Add($"{JsInitializerEventRegistrations.NumericValueEventAliasesMap} key '{alias}' has no Constants.*{AliasConstantSuffix}");
            if (!builtIn.Contains(browserEventName))
                misses.Add($"alias '{alias}' listens to '{browserEventName}', which is not a Blazor built-in event");
        }

        foreach (var (name, relay) in relays)
        {
            if (!constantEvents.Contains(name))
                misses.Add($"{JsInitializerEventRegistrations.RelayedEventsMap} key '{name}' has no Constants.*{AliasConstantSuffix}");
            if (name.StartsWith(WaEventPrefix, StringComparison.Ordinal) || name == relay.Event)
                misses.Add($"relayed name '{name}' is not private: page listeners of '{relay.Event}' or other wa-* events could take it for the element's own event");
            if (relay.Hosts.Count == 0)
                misses.Add($"relayed name '{name}' relays '{relay.Event}' for no element");
            if (relay.Source != RelaySourceHost && relay.Source != RelaySourceSubtree)
                misses.Add($"relayed name '{name}' has source '{relay.Source}', expected '{RelaySourceHost}' or '{RelaySourceSubtree}'");
        }

        AssertNoMisses(misses, "Alias event attribute constants out of step with the JS aliases");
    }

    /// <summary>
    /// Every relayed event handler a callback adds must come with Blazor's stopPropagation for it: the relayed
    /// event bubbles, so without it a wrapper of the same element further up the tree would receive it too (a
    /// nested WaIntersectionObserver would report its inner neighbour's intersections).
    /// </summary>
    [Fact]
    public void RelayedEventHandlers_StopPropagation()
    {
        var registrations = JsInitializerEventRegistrations.Current;

        // guard the harness itself: without a relayed binding the check passes vacuously
        var relayedCount = RenderedWrapperCatalog.All
            .SelectMany(w => w.AllHandlers)
            .Count(h => RenderedWrapperCatalog.EventNameOf(h) is { } e && registrations.RelayedEvents.ContainsKey(e));
        Assert.True(relayedCount > 0, "No rendered wrapper binds a relayed event");

        var misses = RenderedWrapperCatalog.All.SelectMany(w => RelayStopPropagationMisses(w, registrations)).ToList();

        AssertNoMisses(misses, "Relayed event handlers without stopPropagation");
    }

    /// <summary>
    /// The JS initializer must register every list the registration check reads.
    /// </summary>
    [Fact]
    public void JsInitializer_RegistersEveryList()
    {
        var path = Path.Combine(WrapperProjectDirectory(), "wwwroot", JsInitializerEventRegistrations.FileName);
        var source = File.ReadAllText(path);

        Assert.Contains($"of {JsInitializerEventRegistrations.EventNamesList})", source);
        Assert.Contains($"of {JsInitializerEventRegistrations.NativeCustomEventNamesList})", source);
        Assert.Contains($"Object.entries({JsInitializerEventRegistrations.NumericValueEventAliasesMap})", source);
        Assert.Contains($"Object.entries({JsInitializerEventRegistrations.RelayedEventsMap})", source);
    }

    /// <summary>
    /// Allowlisted unbound registrations must still be registered and still be unbound.
    /// </summary>
    [Fact]
    public void UnboundRegistrationAllowlist_IsNotStale()
    {
        var registered = JsInitializerEventRegistrations.Current.AllRegisteredNames.ToHashSet(StringComparer.Ordinal);
        var bound = RenderedBindings().Select(b => b.EventName).ToHashSet(StringComparer.Ordinal);

        Assert.All(UnboundRegistrationAllowlist, entry =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.Value), $"'{entry.Key}' has no reason");
            Assert.Contains(entry.Key, registered);
            Assert.DoesNotContain(entry.Key, bound);
        });
    }

    #region ------ Internals ------

    /// <summary>
    /// Collects the relayed event handlers of one rendered wrapper that lack Blazor's stopPropagation, or that the
    /// wrapper binds without any callback set.
    /// </summary>
    /// <param name="wrapper">The rendered wrapper</param>
    /// <param name="registrations">The JS initializer registrations</param>
    /// <returns>Descriptions of the misses</returns>
    internal static IEnumerable<string> RelayStopPropagationMisses(RenderedWrapper wrapper, JsInitializerEventRegistrations registrations)
    {
        bool IsRelayed(string handler) => RenderedWrapperCatalog.EventNameOf(handler) is { } e && registrations.RelayedEvents.ContainsKey(e);

        foreach (var callback in wrapper.Callbacks)
        {
            foreach (var handler in callback.AddedHandlers.Where(IsRelayed).Where(h => !callback.AddedStopPropagations.Contains(h)))
                yield return $"{wrapper.ComponentType.Name}.{callback.Name} binds the relayed '{handler}' without its stopPropagation (bind it with RenderTreeBuilderExtensions.AddRelayedEventIfHasDelegate)";
        }

        foreach (var handler in wrapper.BaselineHandlers.Where(IsRelayed))
            yield return $"{wrapper.ComponentType.Name} always binds the relayed '{handler}'; relayed events belong to an EventCallback, bound only when it has a delegate";
    }

    private const string AliasConstantSuffix = "EventAttribute";
    private const string WaEventPrefix = "wa-";
    private const string RelaySourceHost = "host";
    private const string RelaySourceSubtree = "subtree";

    // registrations deliberately not bound by any wrapper, each with the reason
    private static readonly Dictionary<string, string> UnboundRegistrationAllowlist = new(StringComparer.Ordinal);

    private sealed record RenderedBinding(string Tag, string EventName);

    private static List<RenderedBinding> RenderedBindings()
    {
        return RenderedWrapperCatalog.All
            .Where(w => w.Tag != null)
            .SelectMany(w => w.AllHandlers
                .Select(RenderedWrapperCatalog.EventNameOf)
                .Where(e => e != null)
                .Select(e => new RenderedBinding(w.Tag!, e!)))
            .ToList();
    }

    private static bool IsDeclaredEvent(string tag, string eventName)
    {
        if (!Surface.Components.TryGetValue(tag, out var component)) return false;

        // a native DOM event (a relayed keydown) is admitted like in the binding parity check
        var componentConfig = GetComponentConfig(tag);
        return (component.Events?.ContainsKey(eventName) == true && !componentConfig.CemOnlyEvents.Contains(eventName))
            || componentConfig.UndeclaredBoundEvents.Contains(eventName)
            || Config.NativeDomEvents.ContainsKey(eventName);
    }

    /// <summary>
    /// The event names (without the "on" prefix) Blazor handles without registration: the EventHandler
    /// attributes of Microsoft.AspNetCore.Components.Web.EventHandlers.
    /// </summary>
    /// <returns>The built-in event names</returns>
    internal static HashSet<string> BlazorBuiltInEventNames()
    {
        var names = typeof(EventHandlers).GetCustomAttributes<EventHandlerAttribute>()
            .Select(a => RenderedWrapperCatalog.EventNameOf(a.AttributeName))
            .Where(n => n != null)
            .Select(n => n!)
            .ToHashSet(StringComparer.Ordinal);
        Assert.NotEmpty(names);
        return names;
    }

    private static List<(string Field, string Attribute)> AliasEventAttributeConstants()
    {
        return typeof(Constants).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string) && f.Name.EndsWith(AliasConstantSuffix, StringComparison.Ordinal))
            .Select(f => (f.Name, (string)f.GetRawConstantValue()!))
            .ToList();
    }

    #endregion
}
