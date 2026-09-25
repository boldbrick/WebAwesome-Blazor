using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Tests.Components;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Render-based slot parity: every wrapper component is rendered with bUnit (RenderedWrapperCatalog), once plain,
/// once per RenderFragment parameter set to a SlotProbe marker, once per icon convenience parameter
/// ("&lt;Slot&gt;IconName") and once per sample value of its bool and string parameters, and the slots of the root
/// element's direct children are compared with the Custom Elements Manifest slots of the tag the wrapper actually
/// renders. Checked in both directions: (a) every slot a wrapper renders content into is declared by the element
/// (a misspelled slot name, or content for a slot the element lacks, never shows in the browser); (b) every CEM
/// slot is fed by a RenderFragment parameter of each wrapper of the element (ChildContent for the default slot),
/// unless allowlisted in "unreachableSlots"; (c) every RenderFragment parameter renders its content into the
/// element; (d) every icon convenience parameter renders a wa-icon into a slot a RenderFragment parameter also
/// feeds, and yields to that fragment when both are set (docs\technical.md, "Icon slot convenience"). Every
/// allowlist entry needs a reason (AllowlistHygieneTests), and stale entries fail. Skipped until parity-config.json
/// sets "enabled": true.
/// </summary>
public class SlotParityTests
{
    /// <summary>
    /// Every wrapper must render with each of its slot-relevant parameters set.
    /// </summary>
    [Fact]
    public void AllWrappers_RenderWithEverySlotSample()
    {
        SkipUnlessParityEnabled();

        var misses = AllRenders.Value
            .SelectMany(w => w.Renders.Where(r => r.Slots.Error != null)
                .Select(r => $"{w.ComponentType.Name} ({r.Label}): failed to render: {r.Slots.Error}"))
            .ToList();

        AssertNoMisses(misses, "Wrappers that fail to render with a slot sample");
    }

    /// <summary>
    /// (a) Every slot a wrapper renders content into (the slot attribute of a direct child of the root element, or
    /// the default slot for a child without one, including where a RenderFragment parameter's content lands) must be
    /// declared by the element's CEM entry.
    /// </summary>
    [Fact]
    public void AllRenderedSlots_AreDeclaredByTheElement()
    {
        SkipUnlessParityEnabled();

        var misses = new List<string>();

        foreach (var element in RenderedElements())
        {
            var reported = new HashSet<string>(StringComparer.Ordinal);

            foreach (var render in element.Wrapper.Renders.Where(r => r.Slots.Tag == element.Tag))
            {
                // a <script> child is data the element reads (wa-markdown's source), not slotted content
                var slots = render.Slots.Children.Where(c => c.LocalName != ScriptTag).Select(c => c.Slot);
                if (render.Slots.ProbeSlot != null) slots = slots.Append(render.Slots.ProbeSlot);

                foreach (var slot in slots.Distinct(StringComparer.Ordinal))
                {
                    if (element.Component.Slots.ContainsKey(CemSlotName(slot)) || !reported.Add(slot)) continue;

                    misses.Add($"{element.Tag} ({element.Name}): renders content into the {DescribeSlot(slot)} ({render.Label}), " +
                        $"which the element does not declare (declared: {string.Join(", ", element.Component.Slots.Keys.OrderBy(s => s, StringComparer.Ordinal))})");
                }
            }
        }

        AssertNoMisses(misses, "Rendered slots the element does not declare");
    }

    /// <summary>
    /// (b) Every CEM slot of a rendered element must be fed by a RenderFragment parameter of each wrapper rendering
    /// the element (ChildContent for the default slot), or be allowlisted in "unreachableSlots".
    /// </summary>
    [Fact]
    public void AllCemSlots_AreFedByAFragmentParameter()
    {
        SkipUnlessParityEnabled();

        var misses = new List<string>();

        foreach (var element in RenderedElements())
        {
            var reached = element.Wrapper.FragmentSlots();

            foreach (var slot in element.Component.Slots.Keys.OrderBy(s => s, StringComparer.Ordinal))
            {
                if (reached.ContainsKey(slot) || element.Config.UnreachableSlots.Contains(slot)) continue;

                misses.Add($"{element.Tag} ({element.Name}): no RenderFragment parameter renders into the {DescribeSlot(WrapperSlotName(slot))} " +
                    $"(\"{element.Component.Slots[slot]}\")");
            }
        }

        AssertNoMisses(misses, "CEM slots no RenderFragment parameter feeds");
    }

    /// <summary>
    /// (c) Every RenderFragment parameter must render its content inside the wrapper's root element, into a slot or
    /// into a &lt;script&gt; data child the element reads (wa-markdown's source).
    /// </summary>
    [Fact]
    public void AllFragmentParameters_RenderIntoTheElement()
    {
        SkipUnlessParityEnabled();

        var misses = RenderedElements()
            .SelectMany(e => e.Wrapper.Renders
                .Where(r => r.Kind == SampleKind.Fragment && r.Slots.Error == null && r.Slots.ProbeSlot == null && !r.Slots.ProbeInData)
                .Select(r => $"{e.Tag} ({e.Name}): {r.Parameter!.Name} renders nothing inside the element"))
            .ToList();

        AssertNoMisses(misses, "RenderFragment parameters whose content is not rendered");
    }

    /// <summary>
    /// (d) Every icon convenience parameter ("&lt;Slot&gt;IconName") must render one wa-icon with the given name into a
    /// slot some RenderFragment parameter also feeds, and render nothing while that fragment is set.
    /// </summary>
    [Fact]
    public void IconNameParameters_FollowTheIconSlotConvention()
    {
        SkipUnlessParityEnabled();

        var misses = new List<string>();

        foreach (var element in RenderedElements())
        {
            var fragments = element.Wrapper.FragmentSlots();

            foreach (var render in element.Wrapper.Renders.Where(r => r.Kind == SampleKind.IconName && r.Slots.Error == null))
            {
                var label = $"{element.Tag} ({element.Name}.{render.Parameter!.Name})";
                var icons = render.Slots.Children.Where(c => c.IconName == IconSample).ToList();

                if (icons.Count != 1)
                {
                    misses.Add($"{label}: renders {icons.Count} wa-icon children named \"{IconSample}\", expected one");
                    continue;
                }

                var slot = CemSlotName(icons[0].Slot);
                if (!fragments.TryGetValue(slot, out var fragment))
                {
                    misses.Add($"{label}: renders its wa-icon into the {DescribeSlot(icons[0].Slot)}, which no RenderFragment parameter feeds");
                    continue;
                }

                var both = element.Wrapper.Renders.SingleOrDefault(r => r.Kind == SampleKind.IconWithFragment && r.Parameter == render.Parameter);
                if (both == null || both.Slots.Error != null) continue;

                if (both.Slots.Children.Any(c => c.IconName == IconSample))
                    misses.Add($"{label}: still renders its wa-icon while {fragment} is set; the fragment must win");
            }
        }

        AssertNoMisses(misses, "Icon convenience parameters not following the icon slot convention");
    }

    /// <summary>
    /// Every "unreachableSlots" entry must still be a CEM slot of the element that some wrapper of the element feeds
    /// from no RenderFragment parameter.
    /// </summary>
    [Fact]
    public void UnreachableSlots_AreNotStale()
    {
        SkipUnlessParityEnabled();

        var elements = RenderedElements().ToList();
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            var ofTag = elements.Where(e => e.Tag == tag).ToList();

            foreach (var slot in componentConfig.UnreachableSlots)
            {
                if (!ofTag.Any(e => e.Component.Slots.ContainsKey(slot) && !e.Wrapper.FragmentSlots().ContainsKey(slot)))
                    misses.Add($"{tag}: unreachableSlots entry '{slot}' is no CEM slot of the element, or every wrapper of the element feeds it, and must be removed");
            }
        }

        AssertNoMisses(misses, "Stale unreachableSlots entries");
    }

    #region ------ Internals ------

    // the CEM's key of the default slot
    private const string DefaultSlotKey = "(default)";
    private const string IconNameSuffix = "IconName";
    private const string IconSample = "x-sample-icon";
    private const string ScriptTag = "script";

    private static readonly Lazy<IReadOnlyList<WrapperSlotRenders>> AllRenders = new(() =>
        RenderedWrapperCatalog.WrapperTypes.Select(RenderWrapper).ToList());

    /// <summary>
    /// What a slot render sets.
    /// </summary>
    private enum SampleKind
    {
        // nothing set
        Baseline,

        // one RenderFragment parameter set to the SlotProbe marker
        Fragment,

        // one icon convenience parameter set to IconSample
        IconName,

        // an icon convenience parameter together with the RenderFragment parameter feeding its slot
        IconWithFragment,

        // one bool or string parameter set to a sample value
        Value
    }

    /// <summary>
    /// One render of a wrapper for the slot checks.
    /// </summary>
    /// <param name="Kind">What the render sets</param>
    /// <param name="Parameter">The parameter the render is about, or null for the baseline</param>
    /// <param name="Label">Description for miss messages</param>
    /// <param name="Slots">The observed slots</param>
    private sealed record SlotRender(SampleKind Kind, PropertyInfo? Parameter, string Label, RenderedSlots Slots);

    /// <summary>
    /// All slot renders of one wrapper.
    /// </summary>
    /// <param name="ComponentType">The wrapper component type</param>
    /// <param name="Tag">The tag of the baseline render, or null</param>
    /// <param name="Renders">The renders</param>
    private sealed record WrapperSlotRenders(Type ComponentType, string? Tag, IReadOnlyList<SlotRender> Renders)
    {
        // the CEM slot names the RenderFragment parameters render into, mapped to the (first) parameter feeding each
        public IReadOnlyDictionary<string, string> FragmentSlots()
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var render in Renders.Where(r => r.Kind == SampleKind.Fragment && r.Slots.ProbeSlot != null))
                result.TryAdd(CemSlotName(render.Slots.ProbeSlot!), render.Parameter!.Name);
            return result;
        }
    }

    /// <summary>
    /// A wrapper whose baseline render produced a custom element of the expected surface.
    /// </summary>
    private sealed record SlotElement(string Tag, ComponentSurface Component, ComponentParityConfig Config, WrapperSlotRenders Wrapper)
    {
        public string Name => Wrapper.ComponentType.Name;
    }

    private static WrapperSlotRenders RenderWrapper(Type componentType)
    {
        var properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.IsDefined(typeof(ParameterAttribute), inherit: true) && p.CanWrite)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToList();

        var fragments = properties.Where(p => p.PropertyType == typeof(RenderFragment)).ToList();
        var iconNames = properties.Where(p => p.PropertyType == typeof(string) && p.Name.EndsWith(IconNameSuffix, StringComparison.Ordinal)).ToList();

        var samples = new List<(SampleKind Kind, PropertyInfo? Parameter, string Label, IReadOnlyList<(string Name, object? Value)> Values)>
        {
            (SampleKind.Baseline, null, "no parameter set", Array.Empty<(string, object?)>())
        };

        samples.AddRange(fragments.Select(p => (SampleKind.Fragment, (PropertyInfo?)p, $"{p.Name}=<{SlotProbe.Tag}>",
            (IReadOnlyList<(string, object?)>)new[] { (p.Name, (object?)SlotProbe.Fragment) })));
        samples.AddRange(iconNames.Select(p => (SampleKind.IconName, (PropertyInfo?)p, $"{p.Name}=\"{IconSample}\"",
            (IReadOnlyList<(string, object?)>)new[] { (p.Name, (object?)IconSample) })));
        samples.AddRange(RenderedAttributeParityTests.SampleValues(componentType)
            .Where(s => s.Value is bool or string && !iconNames.Contains(s.Property))
            .Select(s => (SampleKind.Value, (PropertyInfo?)s.Property, s.Label, (IReadOnlyList<(string, object?)>)new[] { (s.Property.Name, (object?)s.Value) })));

        var first = RenderedWrapperCatalog.RenderSlots(componentType, samples.Select(s => s.Values).ToList());
        var renders = samples.Select((s, i) => new SlotRender(s.Kind, s.Parameter, s.Label, first[i])).ToList();

        // the icon parameters again, each together with the fragment feeding the slot it renders into
        var fragmentSlots = new WrapperSlotRenders(componentType, first[0].Tag, renders).FragmentSlots();
        var pairs = new List<(PropertyInfo Icon, string Fragment)>();
        foreach (var render in renders.Where(r => r.Kind == SampleKind.IconName))
        {
            var icon = render.Slots.Children.FirstOrDefault(c => c.IconName == IconSample);
            if (icon != null && fragmentSlots.TryGetValue(CemSlotName(icon.Slot), out var fragment))
                pairs.Add((render.Parameter!, fragment));
        }

        var second = RenderedWrapperCatalog.RenderSlots(componentType, pairs
            .Select(p => (IReadOnlyList<(string, object?)>)new[] { (p.Fragment, (object?)SlotProbe.Fragment), (p.Icon.Name, (object?)IconSample) })
            .ToList());
        renders.AddRange(pairs.Select((p, i) => new SlotRender(SampleKind.IconWithFragment, p.Icon, $"{p.Icon.Name} with {p.Fragment}", second[i])));

        return new WrapperSlotRenders(componentType, first[0].Tag, renders);
    }

    private static IEnumerable<SlotElement> RenderedElements()
    {
        foreach (var wrapper in AllRenders.Value)
        {
            var tag = wrapper.Tag;
            if (tag == null || Config.IgnoredComponents.Contains(tag)) continue;
            if (!Surface.Components.TryGetValue(tag, out var component)) continue;

            yield return new SlotElement(tag, component, GetComponentConfig(tag), wrapper);
        }
    }

    // the CEM key of a rendered slot name (empty for the default slot)
    private static string CemSlotName(string slot) => slot.Length == 0 ? DefaultSlotKey : slot;

    // the rendered slot name of a CEM key
    private static string WrapperSlotName(string cemSlot) => cemSlot == DefaultSlotKey ? string.Empty : cemSlot;

    private static string DescribeSlot(string slot) => slot.Length == 0 ? "default slot" : $"slot \"{slot}\"";

    #endregion
}
