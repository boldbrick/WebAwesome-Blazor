using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Renders every wrapper component (RenderedWrapperCatalog) with each of the parameters all wrappers share -
/// Class, Style and the unmatched-attributes capture - set on its own, and requires the value on the wrapper's
/// root element. These parameters map to no CEM attribute, so the CEM-driven RenderedAttributeParityTests do not
/// see them; this replaces the per-wrapper tests that only read them back from an unrendered instance.
/// </summary>
public class CommonParameterRenderingTests
{
    [Fact]
    public void AllWrappers_RenderClassStyleAndUnmatchedAttributesOnTheirRoot()
    {
        var misses = new List<string>();
        var checkedCount = 0;

        foreach (var componentType in RenderedWrapperCatalog.WrapperTypes)
        {
            var samples = CommonSamples(componentType).ToList();
            if (samples.Count == 0) continue;

            var roots = RenderedWrapperCatalog.RenderRoots(componentType,
                samples.Select(s => (IReadOnlyList<(string Name, object? Value)>)new (string Name, object? Value)[] { (s.Parameter, s.Value) }).ToList(),
                CultureInfo.InvariantCulture);

            for (var i = 0; i < samples.Count; i++)
            {
                checkedCount++;
                var (parameter, _, attribute, expected) = samples[i];
                var root = roots[i];

                if (root.Error != null)
                    misses.Add($"{componentType.Name}.{parameter}: failed to render: {root.Error}");
                else if (!root.Attributes.TryGetValue(attribute, out var value) || !value.Contains(expected, StringComparison.Ordinal))
                    misses.Add($"{componentType.Name}.{parameter}: the root element <{root.Tag}> carries no {attribute}=\"{expected}\"");
            }
        }

        // guard the scan itself: a catalog that found no wrapper would pass vacuously
        Assert.True(checkedCount > 0, "No wrapper with Class, Style or an unmatched-attributes capture was found");
        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    #region ------ Internals ------

    private const string ClassParameter = "Class";
    private const string StyleParameter = "Style";
    private const string ClassAttribute = "class";
    private const string StyleAttribute = "style";
    private const string ClassSample = "common-class-probe";
    private const string StyleSample = "outline-offset: 7px";
    private const string DataAttribute = "data-common-probe";
    private const string DataSample = "unmatched";

    // the shared parameters a wrapper declares, each with a sample value, the root attribute it must render and
    // the text that attribute must contain
    private static IEnumerable<(string Parameter, object Value, string Attribute, string Expected)> CommonSamples(Type componentType)
    {
        var parameters = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.IsDefined(typeof(ParameterAttribute), inherit: true))
            .ToList();

        if (parameters.Any(p => p.Name == ClassParameter))
            yield return (ClassParameter, ClassSample, ClassAttribute, ClassSample);

        if (parameters.Any(p => p.Name == StyleParameter))
            yield return (StyleParameter, StyleSample, StyleAttribute, StyleSample);

        var capture = parameters.FirstOrDefault(p => p.GetCustomAttribute<ParameterAttribute>(inherit: true)!.CaptureUnmatchedValues);
        if (capture != null)
            yield return (capture.Name, new Dictionary<string, object> { [DataAttribute] = DataSample }, DataAttribute, DataSample);
    }

    #endregion
}
