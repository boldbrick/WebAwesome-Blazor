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
/// Shared data access and name-mapping helpers of the CEM-driven API parity tests
/// (ApiSurfaceParityTests, EnumValueParityTests): loads expected-api-surface.json and
/// parity-config.json once and resolves custom elements to wrapper classes and attributes to
/// [Parameter] properties, so every parity check applies the same mapping and overrides.
/// </summary>
internal static class ApiParityData
{
    /// <summary>
    /// Expected API surface of the bound Web Awesome version (expected-api-surface.json).
    /// </summary>
    public static readonly ApiSurface Surface = LoadDataFile<ApiSurface>(SurfaceFileName);

    /// <summary>
    /// Parity activation switch, overrides and documented omissions (parity-config.json).
    /// </summary>
    public static readonly ParityConfig Config = LoadDataFile<ParityConfig>(ConfigFileName);

    /// <summary>
    /// Assembly containing the Blazor wrapper components.
    /// </summary>
    public static readonly Assembly WrapperAssembly = typeof(WaButton).Assembly;

    /// <summary>
    /// Enumerates the custom elements of the expected surface that are not ignored as a whole.
    /// </summary>
    /// <returns>Tag name and expected surface of each relevant custom element</returns>
    public static IEnumerable<(string Tag, ComponentSurface Component)> RelevantComponents()
    {
        foreach (var (tag, component) in Surface.Components)
        {
            if (Config.IgnoredComponents.Contains(tag)) continue;
            yield return (tag, component);
        }
    }

    /// <summary>
    /// Returns the per-component parity configuration, or an empty configuration when the
    /// component has no entry.
    /// </summary>
    /// <param name="tag">Custom element tag name, e.g. "wa-button"</param>
    /// <returns>The component's parity configuration</returns>
    public static ComponentParityConfig GetComponentConfig(string tag)
    {
        return Config.Components.TryGetValue(tag, out var config) ? config : EmptyComponentConfig;
    }

    /// <summary>
    /// Returns the expected wrapper class name of a custom element.
    /// </summary>
    /// <param name="tag">Custom element tag name</param>
    /// <param name="component">Expected surface of the custom element</param>
    /// <returns>The configured override, the CEM class name, or the PascalCase tag name</returns>
    public static string ExpectedWrapperName(string tag, ComponentSurface component)
    {
        if (Config.ComponentClassOverrides.TryGetValue(tag, out var over)) return over;
        return string.IsNullOrEmpty(component.ClassName) ? ToPascalCase(tag) : component.ClassName;
    }

    /// <summary>
    /// Finds the wrapper class of a custom element in the wrapper assembly.
    /// </summary>
    /// <param name="tag">Custom element tag name</param>
    /// <param name="component">Expected surface of the custom element</param>
    /// <returns>The wrapper type, or null when no wrapper class exists</returns>
    public static Type? FindWrapperType(string tag, ComponentSurface component)
    {
        var expectedName = ExpectedWrapperName(tag, component);

        // match on simple name with generic arity stripped, in any namespace of the wrapper assembly
        return WrapperAssembly.GetTypes()
            .FirstOrDefault(t => t.IsClass && !t.IsAbstract && StripGenericArity(t.Name) == expectedName);
    }

    /// <summary>
    /// Removes the generic arity suffix from a CLR type name, e.g. "WaSelect`1" to "WaSelect".
    /// </summary>
    /// <param name="typeName">CLR type name</param>
    /// <returns>The type name without the arity suffix</returns>
    public static string StripGenericArity(string typeName)
    {
        var index = typeName.IndexOf('`');
        return index < 0 ? typeName : typeName[..index];
    }

    /// <summary>
    /// Converts a kebab-case name to PascalCase, e.g. "with-label" to "WithLabel".
    /// </summary>
    /// <param name="kebabName">Kebab-case name</param>
    /// <returns>The PascalCase name</returns>
    public static string ToPascalCase(string kebabName)
    {
        var parts = kebabName.Split('-', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }

    /// <summary>
    /// Determines whether an attribute is intentionally not exposed, either globally or for
    /// the component.
    /// </summary>
    /// <param name="componentConfig">Parity configuration of the component</param>
    /// <param name="attributeName">CEM attribute name</param>
    /// <returns>true when the attribute is ignored</returns>
    public static bool IsIgnoredAttribute(ComponentParityConfig componentConfig, string attributeName)
    {
        return Config.GlobalIgnoredAttributes.Contains(attributeName)
            || componentConfig.IgnoredAttributes.Contains(attributeName);
    }

    /// <summary>
    /// Returns the name of the [Parameter] property expected to expose an attribute.
    /// </summary>
    /// <param name="componentConfig">Parity configuration of the component</param>
    /// <param name="attributeName">CEM attribute name</param>
    /// <returns>The configured attribute override, or the PascalCase attribute name</returns>
    public static string ExpectedParameterName(ComponentParityConfig componentConfig, string attributeName)
    {
        return componentConfig.AttributeOverrides.TryGetValue(attributeName, out var over)
            ? over
            : ToPascalCase(attributeName);
    }

    /// <summary>
    /// Finds a public instance property marked with [Parameter] on a wrapper type.
    /// </summary>
    /// <param name="wrapper">Wrapper type</param>
    /// <param name="propertyName">Property name</param>
    /// <returns>The parameter property, or null when there is no such [Parameter] property</returns>
    public static PropertyInfo? FindParameter(Type wrapper, string propertyName)
    {
        var property = wrapper.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property != null && property.IsDefined(typeof(ParameterAttribute), inherit: true) ? property : null;
    }

    /// <summary>
    /// Fails with a single message listing every miss, or passes when there are none.
    /// </summary>
    /// <param name="misses">Collected gap descriptions</param>
    /// <param name="title">Headline of the failure message</param>
    public static void AssertNoMisses(List<string> misses, string title)
    {
        Assert.True(misses.Count == 0,
            $"{title} ({misses.Count} gaps against Web Awesome {Surface.Version}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    #region ------ Internals ------

    private const string DataDirectory = "ApiParity";
    private const string SurfaceFileName = "expected-api-surface.json";
    private const string ConfigFileName = "parity-config.json";

    private static readonly ComponentParityConfig EmptyComponentConfig = new();

    private static T LoadDataFile<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, DataDirectory, fileName);
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<T>(stream)
            ?? throw new InvalidOperationException($"Failed to deserialize {path}");
    }

    #endregion
}
