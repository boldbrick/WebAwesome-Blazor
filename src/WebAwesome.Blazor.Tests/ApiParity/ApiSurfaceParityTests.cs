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
/// not here by callback name. The tests are inert until
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
    /// Every attribute of every custom element must be exposed as a Blazor parameter.
    /// </summary>
    [Fact]
    public void AllAttributes_AreExposedAsParameters()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();

        foreach (var (tag, component) in RelevantComponents())
        {
            var wrapper = FindWrapperType(tag, component);
            if (wrapper == null) continue;
            var componentConfig = GetComponentConfig(tag);

            foreach (var attributeName in component.Attributes.Keys)
            {
                if (IsIgnoredAttribute(componentConfig, attributeName)) continue;

                var expected = ExpectedParameterName(componentConfig, attributeName);

                if (FindParameter(wrapper, expected) == null)
                    misses.Add($"{tag}: attribute '{attributeName}' has no [Parameter] property '{expected}' on {wrapper.Name}");
            }
        }

        AssertNoMisses(misses, "Attributes not covered by parameters");
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
