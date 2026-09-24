using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Verifies the values, not just the names, of enum-typed wrapper parameters: for every CEM
/// attribute whose type is a pure string-literal union (e.g. 'long' | 'short' | 'narrow';
/// null and undefined members are ignored) and whose mapped [Parameter] property is an enum
/// or nullable enum, every enum member's ToHtmlValue() output must be one of the union's
/// literals. Attributes map to properties exactly as in ApiSurfaceParityTests (kebab-case to
/// PascalCase, attributeOverrides, global and per-component ignores). Deliberate deviations
/// are allowlisted per component in parity-config.json "ignoredEnumValues", each with an
/// "ignoreReasons" entry. Not covered: unions with non-literal members (string, number, type
/// aliases), parameters of a non-enum type bound to a literal union (e.g. a bool parameter for
/// an 'always' | 'auto' attribute), and wrappers that serialize an enum by other means than
/// its ToHtmlValue() extension. The value checks are inert until parity-config.json sets
/// "enabled": true, like the other parity tests.
/// </summary>
public class EnumValueParityTests
{
    /// <summary>
    /// Every ToHtmlValue() output of an enum parameter must be a literal of the mapped
    /// attribute's CEM string-literal union.
    /// </summary>
    [Fact]
    public void AllEnumParameterValues_AreInAttributeUnion()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();

        foreach (var binding in EnumAttributeBindings())
        {
            if (binding.ToHtmlValue == null) continue;
            var componentConfig = GetComponentConfig(binding.Tag);

            foreach (var (member, detail) in OutOfUnionValues(binding))
            {
                if (IsIgnoredEnumValue(componentConfig, binding.Attribute, member)) continue;

                misses.Add($"{Describe(binding)}: {detail} is not one of {FormatUnion(binding.Union)}");
            }
        }

        AssertNoMisses(misses, "Enum parameter values outside the attribute's CEM string-literal union");
    }

    /// <summary>
    /// Every enum parameter mapped to a string-literal union attribute must have a
    /// ToHtmlValue() extension method, otherwise its emitted values cannot be verified.
    /// </summary>
    [Fact]
    public void AllEnumParameters_HaveToHtmlValueMapping()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();

        foreach (var binding in EnumAttributeBindings())
        {
            if (binding.ToHtmlValue != null) continue;
            if (IsIgnoredEnumValue(GetComponentConfig(binding.Tag), binding.Attribute, WildcardMember)) continue;

            misses.Add($"{Describe(binding)}: no public static {ToHtmlValueMethodName}({binding.EnumType.Name}) " +
                $"method in the wrapper assembly, so its values cannot be verified against {FormatUnion(binding.Union)}");
        }

        AssertNoMisses(misses, "Enum parameters without a ToHtmlValue mapping");
    }

    /// <summary>
    /// Every allowlisted attribute entry must list at least one enum member and carry a
    /// rationale covering its members in ignoreReasons, keyed "ignoredEnumValues:&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [Fact]
    public void IgnoredEnumValues_HaveReasons()
    {
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var (attributeName, members) in componentConfig.IgnoredEnumValues)
            {
                if (members == null || members.Count == 0)
                    misses.Add($"{tag}: ignoredEnumValues entry '{attributeName}' lists no enum members");

                var key = ReasonKey(tag, attributeName);
                if (!Config.IgnoreReasons.TryGetValue(key, out var reason) || string.IsNullOrWhiteSpace(reason))
                    misses.Add($"{tag}: ignoredEnumValues entry '{attributeName}' has no ignoreReasons entry '{key}'");
            }
        }

        Assert.True(misses.Count == 0,
            $"Allowlisted enum values without a reason ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Every allowlisted enum value must still suppress a real miss; entries whose attribute,
    /// parameter or enum member no longer exists, or whose value is now in the union, are stale
    /// and must be removed so they cannot hide a future regression.
    /// </summary>
    [Fact]
    public void IgnoredEnumValues_AreNotStale()
    {
        if (!Config.Enabled) return;

        var bindings = EnumAttributeBindings().ToDictionary(b => (b.Tag, b.Attribute));
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var (attributeName, members) in componentConfig.IgnoredEnumValues)
            {
                if (!bindings.TryGetValue((tag, attributeName), out var binding))
                {
                    misses.Add($"{tag}: ignoredEnumValues attribute '{attributeName}' is not an enum parameter bound to a CEM string-literal union attribute");
                    continue;
                }

                var outOfUnion = binding.ToHtmlValue == null
                    ? new HashSet<string>(StringComparer.Ordinal)
                    : OutOfUnionValues(binding).Select(v => v.Member).ToHashSet(StringComparer.Ordinal);

                foreach (var member in members)
                {
                    var suppressesMiss = member == WildcardMember
                        ? binding.ToHtmlValue == null || outOfUnion.Count > 0
                        : outOfUnion.Contains(member);

                    if (!suppressesMiss)
                        misses.Add($"{Describe(binding)}: ignoredEnumValues entry '{member}' suppresses no miss and must be removed");
                }
            }
        }

        AssertNoMisses(misses, "Stale ignoredEnumValues entries");
    }

    #region ------ Internals ------

    private const string ToHtmlValueMethodName = "ToHtmlValue";
    private const string WildcardMember = "*";
    private const string ReasonKeyPrefix = "ignoredEnumValues";
    private const string UnionSeparator = " | ";

    // union members that only express optionality, not an emitted value
    private static readonly HashSet<string> OptionalUnionMembers = new(StringComparer.Ordinal) { "null", "undefined" };

    // a single- or double-quoted string literal with no embedded quote of the same kind
    private static readonly Regex StringLiteralRegex = new(
        "^(?:'(?<value>[^']*)'|\"(?<value>[^\"]*)\")$", RegexOptions.Compiled);

    // enum type -> public static ToHtmlValue(enum) method anywhere in the wrapper assembly
    private static readonly Dictionary<Type, MethodInfo> ToHtmlValueMethods = WrapperAssembly.GetTypes()
        .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        .Where(m => m.Name == ToHtmlValueMethodName)
        .Select(m => (Method: m, Parameters: m.GetParameters()))
        .Where(x => x.Parameters.Length == 1 && x.Parameters[0].ParameterType.IsEnum)
        .GroupBy(x => x.Parameters[0].ParameterType)
        .ToDictionary(g => g.Key, g => g.First().Method);

    private sealed record EnumAttributeBinding(
        string Tag,
        string Attribute,
        Type Wrapper,
        PropertyInfo Property,
        Type EnumType,
        IReadOnlyList<string> Union,
        MethodInfo? ToHtmlValue);

    private static IEnumerable<EnumAttributeBinding> EnumAttributeBindings()
    {
        foreach (var (tag, component) in RelevantComponents())
        {
            var wrapper = FindWrapperType(tag, component);
            if (wrapper == null) continue;
            var componentConfig = GetComponentConfig(tag);

            foreach (var (attributeName, attribute) in component.Attributes)
            {
                if (IsIgnoredAttribute(componentConfig, attributeName)) continue;

                var union = ParseStringLiteralUnion(attribute.Type);
                if (union == null) continue;

                // a missing parameter is reported by ApiSurfaceParityTests.AllAttributes_AreExposedAsParameters
                var property = FindParameter(wrapper, ExpectedParameterName(componentConfig, attributeName));
                if (property == null) continue;

                var enumType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (!enumType.IsEnum) continue;

                ToHtmlValueMethods.TryGetValue(enumType, out var toHtmlValue);
                yield return new EnumAttributeBinding(tag, attributeName, wrapper, property, enumType, union, toHtmlValue);
            }
        }
    }

    private static IReadOnlyList<string>? ParseStringLiteralUnion(string? type)
    {
        if (string.IsNullOrWhiteSpace(type)) return null;

        var literals = new List<string>();

        foreach (var part in type.Split('|'))
        {
            // multi-line CEM unions start with a leading '|', which yields an empty first part
            var member = part.Trim();
            if (member.Length == 0 || OptionalUnionMembers.Contains(member)) continue;

            var match = StringLiteralRegex.Match(member);
            if (!match.Success) return null;
            literals.Add(match.Groups["value"].Value);
        }

        return literals.Count == 0 ? null : literals;
    }

    private static IEnumerable<(string Member, string Detail)> OutOfUnionValues(EnumAttributeBinding binding)
    {
        foreach (var memberName in Enum.GetNames(binding.EnumType))
        {
            var call = $"{ToHtmlValueMethodName}({binding.EnumType.Name}.{memberName})";
            var (htmlValue, failure) = InvokeToHtmlValue(binding.ToHtmlValue!, Enum.Parse(binding.EnumType, memberName));

            if (failure != null)
                yield return (memberName, $"{call} = <throws {failure}>");
            else if (htmlValue == null)
                yield return (memberName, $"{call} = null");
            else if (!binding.Union.Contains(htmlValue, StringComparer.Ordinal))
                yield return (memberName, $"{call} = '{htmlValue}'");
        }
    }

    private static (string? Value, string? Failure) InvokeToHtmlValue(MethodInfo toHtmlValue, object enumValue)
    {
        try
        {
            return (toHtmlValue.Invoke(null, new[] { enumValue }) as string, null);
        }
        catch (TargetInvocationException ex)
        {
            return (null, (ex.InnerException ?? ex).GetType().Name);
        }
    }

    private static bool IsIgnoredEnumValue(ComponentParityConfig componentConfig, string attributeName, string member)
    {
        return componentConfig.IgnoredEnumValues.TryGetValue(attributeName, out var members)
            && (members.Contains(member) || members.Contains(WildcardMember));
    }

    private static string Describe(EnumAttributeBinding binding)
    {
        return $"{binding.Tag}: attribute '{binding.Attribute}' " +
            $"({StripGenericArity(binding.Wrapper.Name)}.{binding.Property.Name} : {binding.EnumType.Name})";
    }

    private static string FormatUnion(IReadOnlyList<string> union)
    {
        return string.Join(UnionSeparator, union.Select(v => $"'{v}'"));
    }

    private static string ReasonKey(string tag, string attributeName)
    {
        return $"{ReasonKeyPrefix}:{tag}:{attributeName}";
    }

    #endregion
}
