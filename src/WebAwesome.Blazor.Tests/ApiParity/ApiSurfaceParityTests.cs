using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Verifies that the Blazor wrappers cover the API surface of the bound Web Awesome version.
/// The expected surface (expected-api-surface.json) is generated from the Web Awesome
/// Custom Elements Manifest by tools\upgrade\Export-WaApiSurface.ps1; intentional naming
/// deviations and omissions are documented in parity-config.json. Events are checked by
/// EventCallbackBindingParityTests on the rendered output (each callback must bind its CEM event),
/// and attributes by RenderedAttributeParityTests (each CEM attribute must have a parameter that
/// renders it, on every wrapper of the element), not here by name. The tests are inert until
/// parity-config.json sets "enabled": true, which the upgrade process does once the expected
/// surface matches the version being implemented.
/// </summary>
public class ApiSurfaceParityTests
{
    /// <summary>
    /// Every custom element in the expected surface must have a corresponding wrapper class.
    /// </summary>
    [Fact]
    public void AllComponents_HaveWrapperClasses()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();

        foreach (var (tag, component) in RelevantComponents())
        {
            if (FindWrapperType(tag, component) == null)
                misses.Add($"{tag}: no wrapper class '{ExpectedWrapperName(tag, component)}' found");
        }

        AssertNoMisses(misses, "Missing wrapper classes");
    }

    /// <summary>
    /// Every documented public method of every custom element must be exposed as a wrapper
    /// method (typically an Async JS-interop method).
    /// </summary>
    [Fact]
    public void AllDocumentedMethods_AreExposedAsWrapperMethods()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();

        foreach (var (tag, component) in RelevantComponents())
        {
            var wrapper = FindWrapperType(tag, component);
            if (wrapper == null) continue;
            var componentConfig = GetComponentConfig(tag);

            foreach (var methodName in component.Methods.Keys)
            {
                if (componentConfig.IgnoredMethods.Contains(methodName)) continue;

                var expected = componentConfig.MethodOverrides.TryGetValue(methodName, out var over)
                    ? over
                    : ToPascalCase(methodName) + AsyncSuffix;

                if (!HasMethod(wrapper, expected))
                    misses.Add($"{tag}: method '{methodName}' has no wrapper method '{expected}' on {wrapper.Name}");
            }
        }

        AssertNoMisses(misses, "Documented methods not covered by wrapper methods");
    }

    /// <summary>
    /// Every "ignoredComponents", "componentClassOverrides", "ignoredMethods" and "methodOverrides" entry must still
    /// hide a miss: an ignored component must be a CEM element without a wrapper class, a class override must name
    /// an existing wrapper other than the CEM class name, an ignored method must be CEM-documented and not exposed by
    /// the wrapper, and a method override must name a CEM-documented method and differ from the convention.
    /// </summary>
    [Fact]
    public void ComponentAndMethodAllowlists_AreNotStale()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();

        foreach (var tag in Config.IgnoredComponents)
        {
            if (!Surface.Components.TryGetValue(tag, out var component))
                misses.Add($"ignoredComponents entry '{tag}' is no custom element of the surface and must be removed");
            else if (FindWrapperType(tag, component) is { } wrapper)
                misses.Add($"ignoredComponents entry '{tag}' has a wrapper class ({wrapper.Name}) and must be removed");
        }

        foreach (var (tag, className) in Config.ComponentClassOverrides)
        {
            if (!Surface.Components.TryGetValue(tag, out var component))
                misses.Add($"componentClassOverrides entry '{tag}' is no custom element of the surface and must be removed");
            else if (className == (string.IsNullOrEmpty(component.ClassName) ? ToPascalCase(tag) : component.ClassName))
                misses.Add($"componentClassOverrides entry '{tag}' -> '{className}' equals the CEM class name and must be removed");
            else if (FindWrapperType(tag, component) == null)
                misses.Add($"componentClassOverrides entry '{tag}' -> '{className}' names no wrapper class and must be removed");
        }

        foreach (var (tag, componentConfig) in Config.Components)
        {
            if (!Surface.Components.TryGetValue(tag, out var component)) continue;
            var wrapper = FindWrapperType(tag, component);

            foreach (var methodName in componentConfig.IgnoredMethods)
            {
                if (!component.Methods.ContainsKey(methodName))
                    misses.Add($"{tag}: ignoredMethods entry '{methodName}' is no CEM-documented method of the element and must be removed");
                else if (wrapper != null && HasMethod(wrapper, ToPascalCase(methodName) + AsyncSuffix))
                    misses.Add($"{tag}: ignoredMethods entry '{methodName}' is exposed by {wrapper.Name}.{ToPascalCase(methodName)}{AsyncSuffix} and must be removed");
            }

            foreach (var (methodName, wrapperMethod) in componentConfig.MethodOverrides)
            {
                if (!component.Methods.ContainsKey(methodName))
                    misses.Add($"{tag}: methodOverrides entry '{methodName}' is no CEM-documented method of the element and must be removed");
                else if (wrapperMethod == ToPascalCase(methodName) + AsyncSuffix)
                    misses.Add($"{tag}: methodOverrides entry '{methodName}' -> '{wrapperMethod}' equals the naming convention and must be removed");
            }
        }

        AssertNoMisses(misses, "Stale component and method allowlist entries");
    }

    /// <summary>
    /// The parity data files must be loadable and structurally sound whenever they exist,
    /// so a malformed regeneration is caught even while parity is disabled.
    /// </summary>
    [Fact]
    public void ParityDataFiles_AreWellFormed()
    {
        Assert.NotNull(Config);
        Assert.NotNull(Surface);
        Assert.False(string.IsNullOrEmpty(Surface.Version));
        Assert.NotEmpty(Surface.Components);
        if (Config.Enabled)
            Assert.Equal(Config.TargetWaVersion, Surface.Version);
    }

    #region ------ Internals ------

    private const string AsyncSuffix = "Async";

    private static bool HasMethod(Type wrapper, string methodName)
    {
        return wrapper.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Any(m => m.Name == methodName);
    }

    #endregion
}
