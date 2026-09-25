using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Cross-checks the Custom Elements Manifest events, which the event-binding checks use as their oracle, against
/// the two stronger sources of the release (recorded in expected-api-surface.json by
/// tools\upgrade\Export-WaApiSurface.ps1): the component's own @event JSDoc in its dist\components .d.ts
/// ("jsDocEvents"), and, for wa-* events, the event classes of dist\events ("declaredEventTypes"). The CEM analyzer
/// also invents events, e.g. wa-data-grid "request", which carries the wa-data-request event type but is
/// dispatched nowhere; a wrapper bound to such an event would pass every CEM-based check and never fire. A CEM
/// event neither source confirms therefore fails unless it is allowlisted with a reason: in "cemOnlyEvents"
/// (keyed "cemOnlyEvents:&lt;tag&gt;:&lt;event&gt;") when the element never dispatches it, so the event-binding
/// checks do not rely on it, or in "sourceVerifiedEvents" (keyed "sourceVerifiedEvents:&lt;tag&gt;:&lt;event&gt;")
/// when the compiled source was verified to dispatch it although the JSDoc omits it, which must be re-verified on
/// every upgrade. Stale entries fail. Inert until parity-config.json sets "enabled": true, like the other parity tests.
/// </summary>
public class CemEventCorroborationTests
{
    /// <summary>
    /// Every CEM event must be declared by the component's @event JSDoc, and a wa-* event must also have an
    /// event class in dist\events, unless it is allowlisted in cemOnlyEvents.
    /// </summary>
    [Fact]
    public void AllCemEvents_AreCorroboratedByTheTypeDeclarations()
    {
        if (!Config.Enabled) return;

        Assert.True(Surface.DeclaredEventTypes is { Count: > 0 },
            "expected-api-surface.json records no declaredEventTypes; regenerate it with tools\\upgrade\\Export-WaApiSurface.ps1");

        var misses = RelevantComponents()
            .SelectMany(c => UncorroboratedEvents(c.Tag, c.Component, Surface.DeclaredEventTypes)
                .Where(e => !IsAllowlisted(GetComponentConfig(c.Tag), e.EventName))
                .Select(e => $"{c.Tag}: CEM event '{e.EventName}' {e.Reason}"))
            .ToList();

        AssertNoMisses(misses, "CEM events the type declarations do not confirm");
    }

    /// <summary>
    /// Every cemOnlyEvents and sourceVerifiedEvents entry needs a reason, must be a CEM event of the component,
    /// must still be unconfirmed by the type declarations, and may appear in only one of the two lists.
    /// </summary>
    [Fact]
    public void CorroborationAllowlists_HaveReasonsAndAreNotStale()
    {
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            var entries = componentConfig.CemOnlyEvents.Select(e => (List: CemOnlyEventsKey, Event: e))
                .Concat(componentConfig.SourceVerifiedEvents.Select(e => (List: SourceVerifiedEventsKey, Event: e)));

            foreach (var (list, eventName) in entries)
            {
                var key = $"{list}:{tag}:{eventName}";
                if (!Config.IgnoreReasons.TryGetValue(key, out var reason) || string.IsNullOrWhiteSpace(reason))
                    misses.Add($"{tag}: {list} entry '{eventName}' has no ignoreReasons entry '{key}'");

                if (componentConfig.CemOnlyEvents.Contains(eventName) && componentConfig.SourceVerifiedEvents.Contains(eventName))
                    misses.Add($"{tag}: '{eventName}' is listed in both {CemOnlyEventsKey} and {SourceVerifiedEventsKey}");

                if (!Config.Enabled) continue;

                if (!Surface.Components.TryGetValue(tag, out var component) || component.Events?.ContainsKey(eventName) != true)
                    misses.Add($"{tag}: {list} entry '{eventName}' is not a CEM event of the component and must be removed");
                else if (UncorroboratedEvents(tag, component, Surface.DeclaredEventTypes ?? new List<string>()).All(e => e.EventName != eventName))
                    misses.Add($"{tag}: {list} entry '{eventName}' is now confirmed by the type declarations and must be removed");
            }
        }

        AssertNoMisses(misses, "Invalid CEM event corroboration allowlist entries");
    }

    #region ------ Internals ------

    private const string CemOnlyEventsKey = "cemOnlyEvents";
    private const string SourceVerifiedEventsKey = "sourceVerifiedEvents";

    private static bool IsAllowlisted(ComponentParityConfig componentConfig, string eventName)
    {
        return componentConfig.CemOnlyEvents.Contains(eventName) || componentConfig.SourceVerifiedEvents.Contains(eventName);
    }
    private const string WaEventPrefix = "wa-";

    /// <summary>
    /// A CEM event the type declarations do not confirm.
    /// </summary>
    /// <param name="EventName">The CEM event name</param>
    /// <param name="Reason">Which source fails to confirm it</param>
    internal sealed record UncorroboratedEvent(string EventName, string Reason);

    /// <summary>
    /// Returns the CEM events of a component that its @event JSDoc does not declare, or (for wa-* events) that
    /// have no event class in dist\events.
    /// </summary>
    /// <param name="tag">Custom element tag name</param>
    /// <param name="component">Expected surface of the component</param>
    /// <param name="declaredEventTypes">Event names with an event class in dist\events</param>
    /// <returns>The unconfirmed events</returns>
    internal static IEnumerable<UncorroboratedEvent> UncorroboratedEvents(string tag, ComponentSurface component, IReadOnlyCollection<string> declaredEventTypes)
    {
        foreach (var eventName in (component.Events?.Keys ?? Enumerable.Empty<string>()).OrderBy(e => e, StringComparer.Ordinal))
        {
            if (component.JsDocEvents == null)
                yield return new UncorroboratedEvent(eventName, $"cannot be confirmed: the export found no .d.ts for {tag}");
            else if (!component.JsDocEvents.Contains(eventName))
                yield return new UncorroboratedEvent(eventName, "is not declared by the component's @event JSDoc");
            else if (eventName.StartsWith(WaEventPrefix, StringComparison.Ordinal) && !declaredEventTypes.Contains(eventName))
                yield return new UncorroboratedEvent(eventName, "has no event class in dist\\events");
        }
    }

    #endregion
}
