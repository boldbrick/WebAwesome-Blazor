using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Verifies that the Blazor wrappers cover the API surface of the bound Web Awesome version.
/// The expected surface (expected-api-surface.json) is generated from the Web Awesome
/// Custom Elements Manifest by tools\upgrade\Export-WaApiSurface.ps1; intentional naming
/// deviations and omissions are documented in parity-config.json. The tests are inert until
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
                if (Config.GlobalIgnoredAttributes.Contains(attributeName)) continue;
                if (componentConfig.IgnoredAttributes.Contains(attributeName)) continue;

                var expected = componentConfig.AttributeOverrides.TryGetValue(attributeName, out var over)
                    ? over
                    : ToPascalCase(attributeName);

                if (!HasParameter(wrapper, expected))
                    misses.Add($"{tag}: attribute '{attributeName}' has no [Parameter] property '{expected}' on {wrapper.Name}");
            }
        }

        AssertNoMisses(misses, "Attributes not covered by parameters");
    }

    /// <summary>
    /// Every named event of every custom element must be exposed as an EventCallback parameter.
    /// </summary>
    [Fact]
    public void AllEvents_AreExposedAsEventCallbacks()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();

        foreach (var (tag, component) in RelevantComponents())
        {
            var wrapper = FindWrapperType(tag, component);
            if (wrapper == null) continue;
            var componentConfig = GetComponentConfig(tag);

            foreach (var eventName in component.Events.Keys)
            {
                if (componentConfig.IgnoredEvents.Contains(eventName)) continue;

                var expected = componentConfig.EventOverrides.TryGetValue(eventName, out var over)
                    ? over
                    : ExpectedEventCallbackName(eventName);

                if (!HasEventCallback(wrapper, expected))
                    misses.Add($"{tag}: event '{eventName}' has no EventCallback parameter '{expected}' on {wrapper.Name}");
            }
        }

        AssertNoMisses(misses, "Events not covered by EventCallback parameters");
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
    private const string EventPrefix = "wa-";
    private const string DataDirectory = "ApiParity";
    private const string SurfaceFileName = "expected-api-surface.json";
    private const string ConfigFileName = "parity-config.json";

    private static readonly ApiSurface Surface = LoadDataFile<ApiSurface>(SurfaceFileName);
    private static readonly ParityConfig Config = LoadDataFile<ParityConfig>(ConfigFileName);
    private static readonly Assembly WrapperAssembly = typeof(WaButton).Assembly;

    private static T LoadDataFile<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, DataDirectory, fileName);
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<T>(stream)
            ?? throw new InvalidOperationException($"Failed to deserialize {path}");
    }

    private static IEnumerable<(string Tag, ComponentSurface Component)> RelevantComponents()
    {
        foreach (var (tag, component) in Surface.Components)
        {
            if (Config.IgnoredComponents.Contains(tag)) continue;
            yield return (tag, component);
        }
    }

    private static ComponentParityConfig GetComponentConfig(string tag)
    {
        return Config.Components.TryGetValue(tag, out var config) ? config : EmptyComponentConfig;
    }

    private static readonly ComponentParityConfig EmptyComponentConfig = new();

    private static string ExpectedWrapperName(string tag, ComponentSurface component)
    {
        if (Config.ComponentClassOverrides.TryGetValue(tag, out var over)) return over;
        return string.IsNullOrEmpty(component.ClassName) ? ToPascalCase(tag) : component.ClassName;
    }

    private static Type? FindWrapperType(string tag, ComponentSurface component)
    {
        var expectedName = ExpectedWrapperName(tag, component);

        // match on simple name with generic arity stripped, in any namespace of the wrapper assembly
        return WrapperAssembly.GetTypes()
            .FirstOrDefault(t => t.IsClass && !t.IsAbstract && StripGenericArity(t.Name) == expectedName);
    }

    private static string StripGenericArity(string typeName)
    {
        var index = typeName.IndexOf('`');
        return index < 0 ? typeName : typeName[..index];
    }

    private static string ToPascalCase(string kebabName)
    {
        var parts = kebabName.Split('-', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }

    private static string ExpectedEventCallbackName(string eventName)
    {
        // "wa-invalid" -> OnInvalid, "blur" -> OnBlur
        var baseName = eventName.StartsWith(EventPrefix, StringComparison.Ordinal)
            ? eventName[EventPrefix.Length..]
            : eventName;
        return "On" + ToPascalCase(baseName);
    }

    private static bool HasParameter(Type wrapper, string propertyName)
    {
        var property = wrapper.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property != null && property.IsDefined(typeof(ParameterAttribute), inherit: true);
    }

    private static bool HasEventCallback(Type wrapper, string propertyName)
    {
        var property = wrapper.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null || !property.IsDefined(typeof(ParameterAttribute), inherit: true)) return false;

        var type = property.PropertyType;
        return type == typeof(EventCallback)
            || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EventCallback<>));
    }

    private static bool HasMethod(Type wrapper, string methodName)
    {
        return wrapper.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Any(m => m.Name == methodName);
    }

    private static void AssertNoMisses(List<string> misses, string title)
    {
        Assert.True(misses.Count == 0,
            $"{title} ({misses.Count} gaps against Web Awesome {Surface.Version}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    #endregion
}
