using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Reverse direction of ApiSurfaceParityTests.AllEvents_AreExposedAsEventCallbacks (wrapper to CEM):
/// every "onwa-*" event binding in a wrapper's render tree must be an event the Custom Elements Manifest
/// declares for the wa-* element that render tree opens. A binding to an event the element never
/// dispatches compiles, renders and registers fine, but its callback can never fire - the 3.12.0 sweep
/// removed or rewired twelve such bindings (e.g. WaCopyButton "onwa-success", WaDialog
/// "onwa-initial-focus", WaCheckbox "onwa-change") and rebound two that used the wrong name for a native
/// event (WaZoomableFrame "onwa-load"/"onwa-error"). Found by a source scan like
/// EventBindingRegistrationTests; each binding is attributed to the nearest preceding
/// OpenElement(..., "wa-*") in the same file. Deliberate exceptions are allowlisted per component in
/// parity-config.json "undeclaredBoundEvents", each with an ignoreReasons entry keyed
/// "undeclaredBoundEvents:&lt;tag&gt;:&lt;event&gt;"; stale entries fail. Inert until parity-config.json
/// sets "enabled": true, like the other parity tests.
/// </summary>
public class BoundEventCemParityTests
{
    /// <summary>
    /// Every "onwa-*" binding must be a CEM event of the element it is bound on.
    /// </summary>
    [Fact]
    public void AllBoundWaEvents_AreDeclaredByRenderedElement()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();
        var bindingCount = 0;

        foreach (var file in EventBindingRegistrationTests.WrapperSourceFiles())
        {
            var fileName = Path.GetFileName(file);

            foreach (var binding in BoundWaEvents(File.ReadAllText(file)))
            {
                bindingCount++;
                if (IsDeclared(binding, Surface.Components)) continue;
                if (binding.Tag != null && GetComponentConfig(binding.Tag).UndeclaredBoundEvents.Contains(binding.EventName)) continue;

                misses.Add(DescribeUndeclared(fileName, binding, Surface.Components));
            }
        }

        // guard the scan itself: a regex that silently stopped matching would pass vacuously
        Assert.True(bindingCount > 0, "The source scan found no \"onwa-*\" event bindings at all");

        AssertNoMisses(misses, "Bound wa-* events the rendered element's CEM entry does not declare");
    }

    /// <summary>
    /// Every allowlisted undeclared bound event must carry a rationale in ignoreReasons.
    /// </summary>
    [Fact]
    public void UndeclaredBoundEvents_HaveReasons()
    {
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var eventName in componentConfig.UndeclaredBoundEvents)
            {
                var key = $"{ReasonKeyPrefix}:{tag}:{eventName}";
                if (!Config.IgnoreReasons.TryGetValue(key, out var reason) || string.IsNullOrWhiteSpace(reason))
                    misses.Add($"{tag}: undeclaredBoundEvents entry '{eventName}' has no ignoreReasons entry '{key}'");
            }
        }

        Assert.True(misses.Count == 0,
            $"Allowlisted undeclared bound events without a reason ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Every allowlisted undeclared bound event must still be bound and still be undeclared by the CEM.
    /// </summary>
    [Fact]
    public void UndeclaredBoundEvents_AreNotStale()
    {
        if (!Config.Enabled) return;

        var undeclared = EventBindingRegistrationTests.WrapperSourceFiles()
            .SelectMany(file => BoundWaEvents(File.ReadAllText(file)))
            .Where(binding => binding.Tag != null && !IsDeclared(binding, Surface.Components))
            .Select(binding => (binding.Tag!, binding.EventName))
            .ToHashSet();
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var eventName in componentConfig.UndeclaredBoundEvents)
            {
                if (!undeclared.Contains((tag, eventName)))
                    misses.Add($"{tag}: undeclaredBoundEvents entry '{eventName}' suppresses no miss (not bound, or now declared by the CEM) and must be removed");
            }
        }

        AssertNoMisses(misses, "Stale undeclaredBoundEvents entries");
    }

    #region ------ Internals ------

    private const string ReasonKeyPrefix = "undeclaredBoundEvents";

    // an "onwa-*" attribute bound through any AddAttribute* overload, whatever the sequence expression
    private static readonly Regex WaEventBindingRegex = new(
        "AddAttribute\\w*\\(\\s*[^,()]+,\\s*\"on(wa-[a-z-]+)\"", RegexOptions.Compiled);

    // the wa-* element a render tree opens (one per wrapper file)
    private static readonly Regex WaOpenElementRegex = new(
        "OpenElement\\(\\s*[^,()]+,\\s*\"(wa-[a-z-]+)\"", RegexOptions.Compiled);

    /// <summary>
    /// A "onwa-*" event binding found in wrapper source.
    /// </summary>
    /// <param name="Tag">The wa-* element the binding is attributed to, or null when no element precedes it</param>
    /// <param name="EventName">The bound event name without the "on" prefix, e.g. "wa-show"</param>
    internal sealed record BoundWaEvent(string? Tag, string EventName);

    /// <summary>
    /// Finds the "onwa-*" event bindings in a wrapper source file and attributes each to the nearest preceding
    /// wa-* OpenElement.
    /// </summary>
    /// <param name="source">Wrapper source text</param>
    /// <returns>The bindings in source order</returns>
    internal static IEnumerable<BoundWaEvent> BoundWaEvents(string source)
    {
        var elements = WaOpenElementRegex.Matches(source);

        foreach (Match binding in WaEventBindingRegex.Matches(source))
        {
            var tag = elements.LastOrDefault(e => e.Index < binding.Index)?.Groups[1].Value;
            yield return new BoundWaEvent(tag, binding.Groups[1].Value);
        }
    }

    /// <summary>
    /// Determines whether the CEM declares the bound event for the element it is bound on.
    /// </summary>
    /// <param name="binding">The event binding</param>
    /// <param name="components">Expected surface of all custom elements</param>
    /// <returns>true when the element is known and declares the event</returns>
    internal static bool IsDeclared(BoundWaEvent binding, IReadOnlyDictionary<string, ComponentSurface> components)
    {
        return binding.Tag != null
            && components.TryGetValue(binding.Tag, out var component)
            && component.Events != null
            && component.Events.ContainsKey(binding.EventName);
    }

    private static string DescribeUndeclared(string fileName, BoundWaEvent binding, IReadOnlyDictionary<string, ComponentSurface> components)
    {
        if (binding.Tag == null)
            return $"{fileName}: event '{binding.EventName}' is bound before any wa-* OpenElement, so its element cannot be determined";

        if (!components.TryGetValue(binding.Tag, out var component))
            return $"{fileName}: event '{binding.EventName}' is bound on '{binding.Tag}', which is not in the expected surface";

        var declared = component.Events == null || component.Events.Count == 0
            ? "none"
            : string.Join(", ", component.Events.Keys);
        return $"{binding.Tag} ({fileName}): bound event '{binding.EventName}' is not a CEM event of the element (declared: {declared})";
    }

    #endregion
}
