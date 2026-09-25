using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// The custom event types the JS initializer (wwwroot\WebAwesome.Blazor.lib.module.js) registers with
/// Blazor.registerCustomEventType, read from its four registration lists: the wa-* "eventNames", the
/// native-named "nativeCustomEventNames", the "numericValueEventAliases" map (Blazor event name to the
/// browser event it listens to) and the "relayedEvents" map (private Blazor event name to the element event the
/// initializer relays under it, and the elements it relays it for). Unregistered custom events never reach .NET;
/// an alias is equivalent to its browser event and a relay to its element event, which is how the event-binding
/// checks treat a rendered "onnumericchange" or "onwablazor-show" handler.
/// </summary>
internal sealed class JsInitializerEventRegistrations
{
    /// <summary>
    /// File name of the JS initializer.
    /// </summary>
    public const string FileName = "WebAwesome.Blazor.lib.module.js";

    /// <summary>
    /// The registrations of the library's JS initializer; read once per test run.
    /// </summary>
    public static JsInitializerEventRegistrations Current => current.Value;

    /// <summary>
    /// wa-* event names registered with the default detail payload or a special payload.
    /// </summary>
    public IReadOnlyList<string> EventNames { get; }

    /// <summary>
    /// Native-named events Web Awesome re-dispatches that Blazor has no built-in reader for.
    /// </summary>
    public IReadOnlyList<string> NativeCustomEventNames { get; }

    /// <summary>
    /// Alias event names mapped to the browser event they listen to (browserEventName).
    /// </summary>
    public IReadOnlyDictionary<string, string> NumericValueEventAliases { get; }

    /// <summary>
    /// Private relayed event names mapped to the element event they relay and the elements they relay it for.
    /// </summary>
    public IReadOnlyDictionary<string, RelayedEvent> RelayedEvents { get; }

    /// <summary>
    /// Every registered event name: the two lists, the alias names and the relayed names.
    /// </summary>
    public IEnumerable<string> AllRegisteredNames => EventNames.Concat(NativeCustomEventNames).Concat(NumericValueEventAliases.Keys).Concat(RelayedEvents.Keys);

    /// <summary>
    /// Maps an alias event name to the browser event it listens to and a relayed event name to the element event it
    /// relays; any other name maps to itself.
    /// </summary>
    /// <param name="eventName">Event name as bound (without the "on" prefix)</param>
    /// <returns>The browser or element event name</returns>
    public string ResolveAlias(string eventName)
    {
        if (NumericValueEventAliases.TryGetValue(eventName, out var browserEventName)) return browserEventName;
        return RelayedEvents.TryGetValue(eventName, out var relay) ? relay.Event : eventName;
    }

    /// <summary>
    /// Parses the registrations from JS initializer source.
    /// </summary>
    /// <param name="source">Source text of the JS initializer</param>
    /// <returns>The parsed registrations</returns>
    public static JsInitializerEventRegistrations Parse(string source)
    {
        return new JsInitializerEventRegistrations(
            ParseList(source, EventNamesList),
            ParseList(source, NativeCustomEventNamesList),
            ParseAliases(source),
            ParseRelays(source));
    }

    #region ------ Constructors ------

    private JsInitializerEventRegistrations(IReadOnlyList<string> eventNames, IReadOnlyList<string> nativeCustomEventNames,
        IReadOnlyDictionary<string, string> numericValueEventAliases, IReadOnlyDictionary<string, RelayedEvent> relayedEvents)
    {
        EventNames = eventNames;
        NativeCustomEventNames = nativeCustomEventNames;
        NumericValueEventAliases = numericValueEventAliases;
        RelayedEvents = relayedEvents;
    }

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// Name of the wa-* event list in the JS initializer.
    /// </summary>
    internal const string EventNamesList = "eventNames";

    /// <summary>
    /// Name of the native-named custom event list in the JS initializer.
    /// </summary>
    internal const string NativeCustomEventNamesList = "nativeCustomEventNames";

    /// <summary>
    /// Name of the alias map in the JS initializer.
    /// </summary>
    internal const string NumericValueEventAliasesMap = "numericValueEventAliases";

    /// <summary>
    /// Name of the relay map in the JS initializer.
    /// </summary>
    internal const string RelayedEventsMap = "relayedEvents";

    private static readonly Lazy<JsInitializerEventRegistrations> current = new(() =>
    {
        var path = Path.Combine(ApiParityData.WrapperProjectDirectory(), "wwwroot", FileName);
        Assert.True(File.Exists(path), $"JS initializer not found: {path}");
        return Parse(File.ReadAllText(path));
    });

    private static readonly Regex QuotedNameRegex = new("'([^']+)'", RegexOptions.Compiled);
    private static readonly Regex AliasEntryRegex = new("'([^']+)'\\s*:\\s*'([^']+)'", RegexOptions.Compiled);
    private static readonly Regex RelayEntryRegex = new(
        "'([^']+)'\\s*:\\s*\\{\\s*event:\\s*'([^']+)',\\s*hosts:\\s*\\[([^\\]]*)\\],\\s*source:\\s*'([^']+)'\\s*\\}",
        RegexOptions.Compiled);

    private static IReadOnlyList<string> ParseList(string source, string listName)
    {
        var match = Regex.Match(source, $"const\\s+{listName}\\s*=\\s*\\[(.*?)\\];", RegexOptions.Singleline);
        Assert.True(match.Success, $"{FileName}: registration list '{listName}' not found");

        return QuotedNameRegex.Matches(match.Groups[1].Value).Select(m => m.Groups[1].Value).ToList();
    }

    private static IReadOnlyDictionary<string, string> ParseAliases(string source)
    {
        var match = Regex.Match(source, $"const\\s+{NumericValueEventAliasesMap}\\s*=\\s*\\{{(.*?)\\}};", RegexOptions.Singleline);
        Assert.True(match.Success, $"{FileName}: alias map '{NumericValueEventAliasesMap}' not found");

        return AliasEntryRegex.Matches(match.Groups[1].Value)
            .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value, StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, RelayedEvent> ParseRelays(string source)
    {
        var match = Regex.Match(source, $"const\\s+{RelayedEventsMap}\\s*=\\s*\\{{(.*?)\\}};", RegexOptions.Singleline);
        Assert.True(match.Success, $"{FileName}: relay map '{RelayedEventsMap}' not found");

        var body = match.Groups[1].Value;
        var entries = RelayEntryRegex.Matches(body);

        // every entry must parse; a reshaped entry would otherwise drop out of every check silently
        var entryCount = Regex.Matches(body, "'[^']+'\\s*:\\s*\\{").Count;
        Assert.True(entries.Count == entryCount,
            $"{FileName}: {entryCount - entries.Count} of the {entryCount} '{RelayedEventsMap}' entries do not have the shape " +
            "'<name>': { event: '<event>', hosts: ['<tag>', ...], source: '<source>' }");

        return entries.ToDictionary(
            m => m.Groups[1].Value,
            m => new RelayedEvent(
                m.Groups[2].Value,
                QuotedNameRegex.Matches(m.Groups[3].Value).Select(h => h.Groups[1].Value).ToList(),
                m.Groups[4].Value),
            StringComparer.Ordinal);
    }

    #endregion
}

/// <summary>
/// One entry of the JS initializer's relay map.
/// </summary>
/// <param name="Event">The element event the initializer relays, e.g. "wa-show"</param>
/// <param name="Hosts">Tags of the elements it relays the event for</param>
/// <param name="Source">"host" when only the host's own dispatch is relayed, "subtree" when the event may originate
/// anywhere inside the host</param>
internal sealed record RelayedEvent(string Event, IReadOnlyList<string> Hosts, string Source);
