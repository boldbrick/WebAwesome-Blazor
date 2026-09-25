using System;
using System.Collections.Generic;
using System.Linq;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// The registry of every allowlist and override of parity-config.json. It enumerates each entry with the key its
/// reason is filed under - "&lt;list&gt;:&lt;tag&gt;:&lt;entry&gt;" for a per-component list, "&lt;list&gt;:&lt;entry&gt;" for a
/// top-level one, where the entry is the list item or the map key - so AllowlistHygieneTests can apply one rule to
/// all of them: every entry has its own reason, every reason belongs to an entry, and known defects are kept apart.
/// A list added to ParityConfig or ComponentParityConfig must be registered here, which AllowlistHygieneTests
/// enforces.
/// </summary>
internal static class ParityAllowlists
{
    /// <summary>
    /// The JSON names of every registered top-level allowlist or override.
    /// </summary>
    public static IReadOnlyCollection<string> TopLevelListNames => TopLevelLists.Select(l => l.List).ToList();

    /// <summary>
    /// The JSON names of every registered per-component allowlist or override.
    /// </summary>
    public static IReadOnlyCollection<string> ComponentListNames => ComponentLists.Select(l => l.List).ToList();

    /// <summary>
    /// Enumerates every entry of every allowlist and override of a parity configuration, top-level lists first,
    /// then the components in configuration order; duplicates within one list are returned as often as they occur.
    /// </summary>
    /// <param name="config">The parity configuration</param>
    /// <returns>The entries</returns>
    public static IEnumerable<AllowlistEntry> Entries(ParityConfig config)
    {
        foreach (var (list, names) in TopLevelLists)
        {
            foreach (var name in names(config))
                yield return new AllowlistEntry(list, null, name);
        }

        foreach (var (tag, componentConfig) in config.Components)
        {
            foreach (var (list, names) in ComponentLists)
            {
                foreach (var name in names(componentConfig))
                    yield return new AllowlistEntry(list, tag, name);
            }
        }
    }

    #region ------ Internals ------

    private static readonly (string List, Func<ParityConfig, IEnumerable<string>> Names)[] TopLevelLists =
    {
        ("globalIgnoredAttributes", c => c.GlobalIgnoredAttributes),
        ("ignoredComponents", c => c.IgnoredComponents),
        ("componentClassOverrides", c => c.ComponentClassOverrides.Keys),
        ("nativeElementMethods", c => c.NativeElementMethods),
        ("nativeDomEvents", c => c.NativeDomEvents.Keys),
        ("bubblingEventAliases", c => c.BubblingEventAliases.Keys),
        ("unreachableEnumUnionValues", c => c.UnreachableEnumUnionValues.Keys),
    };

    private static readonly (string List, Func<ComponentParityConfig, IEnumerable<string>> Names)[] ComponentLists =
    {
        ("attributeOverrides", c => c.AttributeOverrides.Keys),
        ("ignoredAttributes", c => c.IgnoredAttributes),
        ("eventOverrides", c => c.EventOverrides.Keys),
        ("ignoredEvents", c => c.IgnoredEvents),
        ("methodOverrides", c => c.MethodOverrides.Keys),
        ("ignoredMethods", c => c.IgnoredMethods),
        ("extraElementMethods", c => c.ExtraElementMethods),
        ("ignoredEnumValues", c => c.IgnoredEnumValues.Keys),
        ("unreachableUnionValues", c => c.UnreachableUnionValues.Keys),
        ("ignoredBoolUnionAttributes", c => c.IgnoredBoolUnionAttributes),
        ("tokenListAttributes", c => c.TokenListAttributes.Keys),
        ("unresolvedEnumAttributes", c => c.UnresolvedEnumAttributes),
        ("extraRenderedAttributes", c => c.ExtraRenderedAttributes),
        ("unrenderedAttributes", c => c.UnrenderedAttributes),
        ("attributePrerequisites", c => c.AttributePrerequisites.Keys),
        ("trueFalseAttributes", c => c.TrueFalseAttributes),
        ("wrapperDefaultAttributes", c => c.WrapperDefaultAttributes),
        ("undeclaredBoundEvents", c => c.UndeclaredBoundEvents),
        ("derivedEventCallbacks", c => c.DerivedEventCallbacks.Keys),
        ("unboundEventCallbacks", c => c.UnboundEventCallbacks),
        ("cemOnlyEvents", c => c.CemOnlyEvents),
        ("sourceVerifiedEvents", c => c.SourceVerifiedEvents),
    };

    #endregion
}

/// <summary>
/// One entry of a parity-config.json allowlist or override.
/// </summary>
/// <param name="List">JSON name of the list, e.g. "ignoredEvents"</param>
/// <param name="Tag">Tag of the component the entry belongs to, or null for a top-level list</param>
/// <param name="Name">The list item or map key, e.g. "change"</param>
internal sealed record AllowlistEntry(string List, string? Tag, string Name)
{
    /// <summary>
    /// The key the entry's reason is filed under in ignoreReasons or knownDefects.
    /// </summary>
    public string Key => Tag == null ? $"{List}:{Name}" : $"{List}:{Tag}:{Name}";
}
