using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// The custom event types the JS initializer (wwwroot\WebAwesome.Blazor.lib.module.js) registers with
/// Blazor.registerCustomEventType, read from its three registration lists: the wa-* "eventNames", the
/// native-named "nativeCustomEventNames" and the "numericValueEventAliases" map (Blazor event name to the
/// browser event it listens to). Unregistered custom events never reach .NET, and an alias is equivalent to
/// its browser event, which is how the event-binding checks treat a rendered "onnumericchange" handler.
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
    /// Every registered event name: the two lists and the alias names.
    /// </summary>
    public IEnumerable<string> AllRegisteredNames => EventNames.Concat(NativeCustomEventNames).Concat(NumericValueEventAliases.Keys);

    /// <summary>
    /// Maps an alias event name to the browser event it listens to; any other name maps to itself.
    /// </summary>
    /// <param name="eventName">Event name as bound (without the "on" prefix)</param>
    /// <returns>The browser event name</returns>
    public string ResolveAlias(string eventName)
    {
        return NumericValueEventAliases.TryGetValue(eventName, out var browserEventName) ? browserEventName : eventName;
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
            ParseAliases(source));
    }

    #region ------ Constructors ------

    private JsInitializerEventRegistrations(IReadOnlyList<string> eventNames, IReadOnlyList<string> nativeCustomEventNames,
        IReadOnlyDictionary<string, string> numericValueEventAliases)
    {
        EventNames = eventNames;
        NativeCustomEventNames = nativeCustomEventNames;
        NumericValueEventAliases = numericValueEventAliases;
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

    private static readonly Lazy<JsInitializerEventRegistrations> current = new(() =>
    {
        var path = Path.Combine(ApiParityData.WrapperProjectDirectory(), "wwwroot", FileName);
        Assert.True(File.Exists(path), $"JS initializer not found: {path}");
        return Parse(File.ReadAllText(path));
    });

    private static readonly Regex QuotedNameRegex = new("'([^']+)'", RegexOptions.Compiled);
    private static readonly Regex AliasEntryRegex = new("'([^']+)'\\s*:\\s*'([^']+)'", RegexOptions.Compiled);

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

    #endregion
}
