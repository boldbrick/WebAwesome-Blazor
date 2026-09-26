using System;
using System.Collections.Generic;
using System.Linq;
using WebAwesome.Blazor.Base;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Ties the capability interfaces of the form control hierarchy (IWaPopupControl, IWaClearableControl,
/// IWaAffixedControl, IWaCalendarOptions) to the Custom Elements Manifest: a wrapper implements an interface exactly
/// when the element it renders (resolved by rendered tag, so WaRange counts as a wa-slider wrapper) declares every
/// attribute, slot, event and method of the cluster. The shared render helpers take the interface, so a wrapper
/// rendering a cluster through them implements it already; this check catches a wrapper that renders a cluster by
/// hand without it, and an interface on a wrapper whose element lacks part of the cluster. Clusters whose parts
/// also occur outside form controls (a dialog's open/show/hide, a button's start/end slots) are checked on the
/// WaInputBase descendants only. Skipped until parity-config.json sets "enabled": true, like the other parity tests.
/// </summary>
public class CapabilityInterfaceParityTests
{
    /// <summary>
    /// Every wrapper implements each capability interface exactly when its element declares the whole cluster.
    /// </summary>
    [Fact]
    public void CapabilityInterfaces_MatchTheElementClusters()
    {
        SkipUnlessParityEnabled();

        var misses = new List<string>();
        var implementations = 0;

        foreach (var (tag, component, wrapper) in WrappersByTag())
        {
            foreach (var capability in Capabilities)
            {
                if (capability.FormControlsOnly && !IsFormControl(wrapper)) continue;

                var missing = capability.MissingParts(component).ToList();
                var implements = capability.Interface.IsAssignableFrom(wrapper);
                if (implements) implementations++;

                if (missing.Count == 0 && !implements)
                    misses.Add($"{tag} ({wrapper.Name}): the element has the whole {capability.Interface.Name} cluster, but the wrapper does not implement it");
                else if (missing.Count > 0 && implements)
                    misses.Add($"{tag} ({wrapper.Name}): implements {capability.Interface.Name}, but the element lacks {string.Join(", ", missing)}");
            }
        }

        // guard the check itself: a cluster definition that matched no element would pass vacuously
        Assert.True(implementations > 0, "No wrapper implements any capability interface");

        AssertNoMisses(misses, "Capability interface mismatches");
    }

    #region ------ Internals ------

    /// <summary>
    /// A capability interface and the CEM parts of its cluster.
    /// </summary>
    /// <param name="Interface">The capability interface</param>
    /// <param name="FormControlsOnly">Whether only WaInputBase descendants are checked</param>
    /// <param name="Attributes">CEM attributes of the cluster</param>
    /// <param name="Slots">CEM slots of the cluster</param>
    /// <param name="Events">CEM events of the cluster</param>
    /// <param name="Methods">CEM methods of the cluster</param>
    private sealed record Capability(Type Interface, bool FormControlsOnly, string[] Attributes, string[] Slots, string[] Events, string[] Methods)
    {
        public IEnumerable<string> MissingParts(ComponentSurface component)
        {
            return Attributes.Where(a => !component.Attributes.ContainsKey(a)).Select(a => $"attribute '{a}'")
                .Concat(Slots.Where(s => !component.Slots.ContainsKey(s)).Select(s => $"slot '{s}'"))
                .Concat(Events.Where(e => !component.Events.ContainsKey(e)).Select(e => $"event '{e}'"))
                .Concat(Methods.Where(m => !component.Methods.ContainsKey(m)).Select(m => $"method '{m}'"));
        }
    }

    private static readonly Capability[] Capabilities =
    [
        new(typeof(IWaPopupControl), FormControlsOnly: true, ["open"], [], ["wa-show", "wa-hide", "wa-after-show", "wa-after-hide"], ["show", "hide"]),
        new(typeof(IWaClearableControl), FormControlsOnly: false, ["with-clear"], ["clear-icon"], ["wa-clear"], []),
        new(typeof(IWaAffixedControl), FormControlsOnly: true, [], ["start", "end"], [], []),
        new(typeof(IWaCalendarOptions), FormControlsOnly: false,
            ["min", "max", "today", "disabled-dates", "disabled-days-of-week", "disable-past", "disable-future", "first-day-of-week",
                "months", "page-by", "weekday-format", "with-outside-days", "with-week-numbers"], [], [], []),
    ];

    private static bool IsFormControl(Type wrapper)
    {
        for (var current = wrapper; current != null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(WaInputBase<>)) return true;
        }

        return false;
    }

    #endregion
}
