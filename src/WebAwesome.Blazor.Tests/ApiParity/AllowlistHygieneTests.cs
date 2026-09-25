using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// One rule for every allowlist and override of parity-config.json (review H2): each entry carries its own reason,
/// keyed per component and entry ("ignoredEvents:wa-checkbox:change"), never a reason shared by name across
/// components, because a shared "change" reason is what the dead OnCheckedChange/OnValueChange bindings hid behind.
/// A reason without an entry fails too, so a removed entry takes its reason with it. Entries that record a known
/// defect awaiting an owner decision keep their reason in "knownDefects" instead of "ignoreReasons"; their number is
/// pinned here and printed in the test output, so the list cannot grow unnoticed. Whether an entry is still needed
/// is checked by the test class that reads the list (the *_AreNotStale tests), because that is where its condition
/// is known. The hard-coded allowlists of the test classes (UnboundRegistrationAllowlist, UnusedExportAllowlist)
/// follow the same rule next to their checks.
/// </summary>
public class AllowlistHygieneTests
{
    /// <summary>
    /// Every entry of every allowlist and override must have a non-blank reason of its own, in exactly one of
    /// ignoreReasons and knownDefects, and no entry may be listed twice.
    /// </summary>
    [Fact]
    public void EveryAllowlistEntry_HasItsOwnReason()
    {
        var misses = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in ParityAllowlists.Entries(Config))
        {
            if (!seen.Add(entry.Key))
            {
                misses.Add($"'{entry.Key}': listed twice");
                continue;
            }

            if (Config.IgnoreReasons.ContainsKey(entry.Key) && Config.KnownDefects.ContainsKey(entry.Key))
                misses.Add($"'{entry.Key}': has a reason in both {IgnoreReasonsName} and {KnownDefectsName}; keep one");
            else if (!HasReason(entry.Key))
                misses.Add($"'{entry.Key}': has no reason; add {IgnoreReasonsName}[\"{entry.Key}\"] (or {KnownDefectsName}[\"{entry.Key}\"] for a known defect)");
        }

        AssertNoMisses(misses, "Allowlist entries without a reason of their own");
    }

    /// <summary>
    /// Every key of ignoreReasons and knownDefects must be the key of an allowlist entry: a reason keyed by a bare
    /// name, or left behind by a removed entry, applies to nothing.
    /// </summary>
    [Fact]
    public void EveryReason_BelongsToAnAllowlistEntry()
    {
        var keys = ParityAllowlists.Entries(Config).Select(e => e.Key).ToHashSet(StringComparer.Ordinal);

        var misses = Config.IgnoreReasons.Keys.Select(k => (Map: IgnoreReasonsName, Key: k))
            .Concat(Config.KnownDefects.Keys.Select(k => (Map: KnownDefectsName, Key: k)))
            .Where(r => !keys.Contains(r.Key))
            .Select(r => $"{r.Map}[\"{r.Key}\"]: no allowlist entry has this key ('<list>:<tag>:<entry>' or '<list>:<entry>'); remove it or restore its entry")
            .ToList();

        AssertNoMisses(misses, "Reasons without an allowlist entry");
    }

    /// <summary>
    /// Known defects are structurally apart: no ignoreReasons entry may carry the KNOWN DEFECT marker (it belongs in
    /// knownDefects), and the number of knownDefects entries must equal the pinned count, so adding or fixing one is
    /// a visible change of this test. The entries are printed in the test output.
    /// </summary>
    [Fact]
    public void KnownDefects_AreKeptApartAndCounted()
    {
        var misses = Config.IgnoreReasons
            .Where(r => r.Value.Contains(KnownDefectMarker, StringComparison.OrdinalIgnoreCase))
            .Select(r => $"{IgnoreReasonsName}[\"{r.Key}\"] is marked {KnownDefectMarker}; move it to {KnownDefectsName}")
            .ToList();

        var defects = Config.KnownDefects.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
        var output = TestContext.Current.TestOutputHelper;
        output?.WriteLine($"{defects.Count} known defects recorded in parity-config.json {KnownDefectsName}:");
        foreach (var key in defects)
            output?.WriteLine($"  {key}");

        if (defects.Count != ExpectedKnownDefectCount)
        {
            misses.Add($"{KnownDefectsName} has {defects.Count} entries, the pinned count is {ExpectedKnownDefectCount}; " +
                $"update {nameof(ExpectedKnownDefectCount)} in {nameof(AllowlistHygieneTests)} deliberately when a defect is recorded or fixed:" +
                Environment.NewLine + string.Join(Environment.NewLine, defects.Select(k => $"  {k}")));
        }

        AssertNoMisses(misses, "Known defects not kept apart or not counted");
    }

    /// <summary>
    /// Every list property of ParityConfig and ComponentParityConfig must be registered in ParityAllowlists, so a
    /// new allowlist cannot bypass the reason rule.
    /// </summary>
    [Fact]
    public void AllowlistRegistry_CoversEveryConfigList()
    {
        var topLevel = JsonNames(typeof(ParityConfig)).Except(NonListMembers, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
        var component = JsonNames(typeof(ComponentParityConfig)).ToHashSet(StringComparer.Ordinal);

        Assert.True(topLevel.SetEquals(ParityAllowlists.TopLevelListNames),
            $"ParityAllowlists does not register exactly the top-level lists of ParityConfig: unregistered " +
            $"{string.Join(", ", topLevel.Except(ParityAllowlists.TopLevelListNames))}; unknown {string.Join(", ", ParityAllowlists.TopLevelListNames.Except(topLevel))}");
        Assert.True(component.SetEquals(ParityAllowlists.ComponentListNames),
            $"ParityAllowlists does not register exactly the lists of ComponentParityConfig: unregistered " +
            $"{string.Join(", ", component.Except(ParityAllowlists.ComponentListNames))}; unknown {string.Join(", ", ParityAllowlists.ComponentListNames.Except(component))}");
    }

    /// <summary>
    /// Every "components" entry must name a custom element of the expected surface; the lists of a removed element
    /// would otherwise linger without anything checking them.
    /// </summary>
    [Fact]
    public void ComponentConfigs_NameCemElements()
    {
        SkipUnlessParityEnabled();

        var misses = Config.Components.Keys
            .Where(tag => !Surface.Components.ContainsKey(tag))
            .Select(tag => $"components entry '{tag}' is not a custom element of Web Awesome {Surface.Version} and must be removed")
            .ToList();

        AssertNoMisses(misses, "Stale components entries");
    }

    #region ------ Internals ------

    private const string IgnoreReasonsName = "ignoreReasons";
    private const string KnownDefectsName = "knownDefects";
    private const string KnownDefectMarker = "KNOWN DEFECT";

    // the known defects recorded for an owner decision in 3.12.0: 44 inherited WaInputBase parameters thirteen
    // elements ignore (extraRenderedAttributes) and the four non-bubbling wa-color-picker popup events
    // (sourceVerifiedEvents); change it only together with parity-config.json knownDefects
    private const int ExpectedKnownDefectCount = 48;

    // ParityConfig members that are no allowlist
    private static readonly string[] NonListMembers = { "enabled", "targetWaVersion", "components", IgnoreReasonsName, KnownDefectsName };

    private static IEnumerable<string> JsonNames(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name)
            .OfType<string>();
    }

    #endregion
}
