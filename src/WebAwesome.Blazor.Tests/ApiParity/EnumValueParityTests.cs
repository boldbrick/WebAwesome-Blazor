using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Verifies the values, not just the names, of wrapper parameters bound to CEM attributes whose type is
/// a pure string-literal union (e.g. 'long' | 'short' | 'narrow'; null and undefined members are
/// ignored). For an enum or nullable enum parameter, every enum member's ToHtmlValue() output must be
/// one of the union's literals (forward direction), and every union literal must be emitted by some
/// member (reverse direction, so no valid value is unreachable from C#). A bool/bool? parameter bound
/// to such an attribute is always a defect: Blazor renders true as an empty attribute and drops false,
/// so no literal of the union can ever be sent (the WaRelativeTime Numeric=false class of bug).
/// Attributes map to properties exactly as in ApiSurfaceParityTests (kebab-case to PascalCase,
/// attributeOverrides, global and per-component ignores). Deliberate deviations are allowlisted in
/// parity-config.json - per component in "ignoredEnumValues", "unreachableUnionValues" and
/// "ignoredBoolUnionAttributes", per enum type in the top-level "unreachableEnumUnionValues" - each
/// with an "ignoreReasons" entry; stale allowlist entries fail. Not covered: unions with non-literal
/// members (string, number, boolean, type aliases) and wrappers that serialize an enum by other means
/// than its ToHtmlValue() extension. The value checks are inert until parity-config.json sets
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
    /// Every literal of a string-literal union attribute must be emitted by the ToHtmlValue() of
    /// some member of the mapped enum parameter, so every value Web Awesome accepts can be set.
    /// </summary>
    [Fact]
    public void AllAttributeUnionValues_AreReachableFromEnumParameter()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();

        foreach (var binding in EnumAttributeBindings())
        {
            if (binding.ToHtmlValue == null) continue;
            var componentConfig = GetComponentConfig(binding.Tag);

            foreach (var literal in UnreachableUnionValues(binding))
            {
                if (IsAllowedUnreachableValue(componentConfig, binding, literal)) continue;

                misses.Add($"{Describe(binding)}: union literal '{literal}' of {FormatUnion(binding.Union)} " +
                    $"is emitted by no {binding.EnumType.Name} member");
            }
        }

        AssertNoMisses(misses, "CEM string-literal union values unreachable from the mapped enum parameter");
    }

    /// <summary>
    /// No bool/bool? parameter may be bound to a string-literal union attribute: Blazor renders
    /// true as an empty attribute and omits false, so none of the union's literals is ever sent.
    /// </summary>
    [Fact]
    public void NoBoolParameter_IsBoundToStringLiteralUnion()
    {
        if (!Config.Enabled) return;

        var misses = new List<string>();

        foreach (var parameter in AllLiteralUnionParameters())
        {
            if (!IsBoolType(parameter.Property.PropertyType)) continue;
            if (GetComponentConfig(parameter.Tag).IgnoredBoolUnionAttributes.Contains(parameter.Attribute)) continue;

            misses.Add(DescribeBoolBinding(parameter));
        }

        AssertNoMisses(misses, "Bool parameters bound to a CEM string-literal union attribute");
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

                AddMissingReason(misses, tag, ReasonKey(IgnoredEnumValuesKey, tag, attributeName), $"ignoredEnumValues entry '{attributeName}'");
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

    /// <summary>
    /// Every unreachable-literal allowlist entry, per component ("unreachableUnionValues") and
    /// per enum type ("unreachableEnumUnionValues"), must list at least one literal and carry a
    /// rationale in ignoreReasons.
    /// </summary>
    [Fact]
    public void UnreachableUnionValues_HaveReasons()
    {
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var (attributeName, literals) in componentConfig.UnreachableUnionValues)
            {
                if (literals == null || literals.Count == 0)
                    misses.Add($"{tag}: unreachableUnionValues entry '{attributeName}' lists no union literals");

                AddMissingReason(misses, tag, ReasonKey(UnreachableUnionValuesKey, tag, attributeName), $"unreachableUnionValues entry '{attributeName}'");
            }
        }

        foreach (var (enumName, literals) in Config.UnreachableEnumUnionValues)
        {
            if (literals == null || literals.Count == 0)
                misses.Add($"unreachableEnumUnionValues entry '{enumName}' lists no union literals");

            AddMissingReason(misses, enumName, $"{UnreachableEnumUnionValuesKey}:{enumName}", $"unreachableEnumUnionValues entry '{enumName}'");
        }

        Assert.True(misses.Count == 0,
            $"Allowlisted unreachable union values without a reason ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Every unreachable-literal allowlist entry must still suppress a real miss: the literal must
    /// be in the bound attribute's union and emitted by no enum member.
    /// </summary>
    [Fact]
    public void UnreachableUnionValues_AreNotStale()
    {
        if (!Config.Enabled) return;

        var bindings = EnumAttributeBindings().Where(b => b.ToHtmlValue != null).ToList();
        var bindingsByAttribute = bindings.ToDictionary(b => (b.Tag, b.Attribute));
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var (attributeName, literals) in componentConfig.UnreachableUnionValues)
            {
                if (!bindingsByAttribute.TryGetValue((tag, attributeName), out var binding))
                {
                    misses.Add($"{tag}: unreachableUnionValues attribute '{attributeName}' is not a mapped enum parameter bound to a CEM string-literal union attribute");
                    continue;
                }

                var unreachable = UnreachableUnionValues(binding).ToHashSet(StringComparer.Ordinal);
                foreach (var literal in literals.Where(l => !unreachable.Contains(l)))
                    misses.Add($"{Describe(binding)}: unreachableUnionValues entry '{literal}' suppresses no miss and must be removed");
            }
        }

        foreach (var (enumName, literals) in Config.UnreachableEnumUnionValues)
        {
            var enumBindings = bindings.Where(b => b.EnumType.Name == enumName).ToList();
            if (enumBindings.Count == 0)
            {
                misses.Add($"unreachableEnumUnionValues enum '{enumName}' is bound to no CEM string-literal union attribute");
                continue;
            }

            var unreachable = enumBindings.SelectMany(UnreachableUnionValues).ToHashSet(StringComparer.Ordinal);
            foreach (var literal in literals.Where(l => !unreachable.Contains(l)))
                misses.Add($"unreachableEnumUnionValues:{enumName}: entry '{literal}' suppresses no miss and must be removed");
        }

        AssertNoMisses(misses, "Stale unreachableUnionValues/unreachableEnumUnionValues entries");
    }

    /// <summary>
    /// Every allowlisted bool-for-literal-union attribute must carry a rationale in ignoreReasons,
    /// keyed "ignoredBoolUnionAttributes:&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [Fact]
    public void IgnoredBoolUnionAttributes_HaveReasons()
    {
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var attributeName in componentConfig.IgnoredBoolUnionAttributes)
                AddMissingReason(misses, tag, ReasonKey(IgnoredBoolUnionAttributesKey, tag, attributeName), $"ignoredBoolUnionAttributes entry '{attributeName}'");
        }

        Assert.True(misses.Count == 0,
            $"Allowlisted bool-for-union attributes without a reason ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Every allowlisted bool-for-literal-union attribute must still suppress a real miss.
    /// </summary>
    [Fact]
    public void IgnoredBoolUnionAttributes_AreNotStale()
    {
        if (!Config.Enabled) return;

        var boolBindings = AllLiteralUnionParameters()
            .Where(p => IsBoolType(p.Property.PropertyType))
            .Select(p => (p.Tag, p.Attribute))
            .ToHashSet();
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var attributeName in componentConfig.IgnoredBoolUnionAttributes)
            {
                if (!boolBindings.Contains((tag, attributeName)))
                    misses.Add($"{tag}: ignoredBoolUnionAttributes entry '{attributeName}' is not a bool parameter bound to a CEM string-literal union attribute and must be removed");
            }
        }

        AssertNoMisses(misses, "Stale ignoredBoolUnionAttributes entries");
    }

    #region ------ Internals ------

    private const string ToHtmlValueMethodName = "ToHtmlValue";
    private const string WildcardMember = "*";
    private const string IgnoredEnumValuesKey = "ignoredEnumValues";
    private const string UnreachableUnionValuesKey = "unreachableUnionValues";
    private const string UnreachableEnumUnionValuesKey = "unreachableEnumUnionValues";
    private const string IgnoredBoolUnionAttributesKey = "ignoredBoolUnionAttributes";
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

    /// <summary>
    /// A [Parameter] property mapped to a CEM attribute whose type is a pure string-literal union.
    /// </summary>
    internal sealed record LiteralUnionParameter(
        string Tag,
        string Attribute,
        Type Wrapper,
        PropertyInfo Property,
        IReadOnlyList<string> Union);

    /// <summary>
    /// An enum-typed <see cref="LiteralUnionParameter"/> with the ToHtmlValue() method serializing its enum, if any.
    /// </summary>
    internal sealed record EnumAttributeBinding(
        string Tag,
        string Attribute,
        Type Wrapper,
        PropertyInfo Property,
        Type EnumType,
        IReadOnlyList<string> Union,
        MethodInfo? ToHtmlValue);

    private static IEnumerable<LiteralUnionParameter> AllLiteralUnionParameters()
    {
        foreach (var (tag, component) in RelevantComponents())
        {
            var wrapper = FindWrapperType(tag, component);
            if (wrapper == null) continue;

            foreach (var parameter in LiteralUnionParameters(tag, component, wrapper))
                yield return parameter;
        }
    }

    private static IEnumerable<EnumAttributeBinding> EnumAttributeBindings()
    {
        foreach (var parameter in AllLiteralUnionParameters())
        {
            var enumType = Nullable.GetUnderlyingType(parameter.Property.PropertyType) ?? parameter.Property.PropertyType;
            if (!enumType.IsEnum) continue;

            ToHtmlValueMethods.TryGetValue(enumType, out var toHtmlValue);
            yield return new EnumAttributeBinding(parameter.Tag, parameter.Attribute, parameter.Wrapper, parameter.Property,
                enumType, parameter.Union, toHtmlValue);
        }
    }

    /// <summary>
    /// Maps the string-literal union attributes of one custom element to the wrapper's [Parameter] properties.
    /// </summary>
    /// <param name="tag">Custom element tag name</param>
    /// <param name="component">Expected surface of the custom element</param>
    /// <param name="wrapper">Wrapper type exposing the parameters</param>
    /// <returns>The mapped parameters; attributes without a parameter are skipped</returns>
    internal static IEnumerable<LiteralUnionParameter> LiteralUnionParameters(string tag, ComponentSurface component, Type wrapper)
    {
        var componentConfig = GetComponentConfig(tag);

        foreach (var (attributeName, attribute) in component.Attributes)
        {
            if (IsIgnoredAttribute(componentConfig, attributeName)) continue;

            var union = ParseStringLiteralUnion(attribute.Type);
            if (union == null) continue;

            // a missing parameter is reported by ApiSurfaceParityTests.AllAttributes_AreExposedAsParameters
            var property = FindParameter(wrapper, ExpectedParameterName(componentConfig, attributeName));
            if (property == null) continue;

            yield return new LiteralUnionParameter(tag, attributeName, wrapper, property, union);
        }
    }

    /// <summary>
    /// Parses a CEM attribute type that is a pure string-literal union.
    /// </summary>
    /// <param name="type">CEM attribute type, e.g. "'always' | 'auto'"</param>
    /// <returns>The literals, or null when the type has a non-literal member (e.g. boolean, string, a type alias)</returns>
    internal static IReadOnlyList<string>? ParseStringLiteralUnion(string? type)
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

    /// <summary>
    /// Returns the enum members whose ToHtmlValue() output is not a literal of the binding's union (or that throw
    /// or yield null).
    /// </summary>
    /// <param name="binding">Enum binding with a ToHtmlValue method</param>
    /// <returns>Member name and a description of the offending output</returns>
    internal static IEnumerable<(string Member, string Detail)> OutOfUnionValues(EnumAttributeBinding binding)
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

    /// <summary>
    /// Returns the union literals that no enum member's ToHtmlValue() emits, before any allowlisting.
    /// </summary>
    /// <param name="binding">Enum binding with a ToHtmlValue method</param>
    /// <returns>The unreachable literals, in union order</returns>
    internal static IEnumerable<string> UnreachableUnionValues(EnumAttributeBinding binding)
    {
        var emitted = Enum.GetNames(binding.EnumType)
            .Select(name => InvokeToHtmlValue(binding.ToHtmlValue!, Enum.Parse(binding.EnumType, name)).Value)
            .Where(value => value != null)
            .ToHashSet(StringComparer.Ordinal);

        return binding.Union.Where(literal => !emitted.Contains(literal));
    }

    /// <summary>
    /// Determines whether a parameter type is bool or bool?.
    /// </summary>
    /// <param name="type">Parameter property type</param>
    /// <returns>true for bool and bool?</returns>
    internal static bool IsBoolType(Type type)
    {
        return (Nullable.GetUnderlyingType(type) ?? type) == typeof(bool);
    }

    /// <summary>
    /// Describes a bool parameter bound to a string-literal union attribute.
    /// </summary>
    /// <param name="parameter">The offending parameter</param>
    /// <returns>The miss description</returns>
    internal static string DescribeBoolBinding(LiteralUnionParameter parameter)
    {
        return $"{parameter.Tag}: attribute '{parameter.Attribute}' ({StripGenericArity(parameter.Wrapper.Name)}.{parameter.Property.Name} : " +
            $"{parameter.Property.PropertyType.Name}) is a string-literal union {FormatUnion(parameter.Union)}; a bool parameter can emit none of its literals";
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

    private static bool IsAllowedUnreachableValue(ComponentParityConfig componentConfig, EnumAttributeBinding binding, string literal)
    {
        return (componentConfig.UnreachableUnionValues.TryGetValue(binding.Attribute, out var componentLiterals) && componentLiterals.Contains(literal))
            || (Config.UnreachableEnumUnionValues.TryGetValue(binding.EnumType.Name, out var enumLiterals) && enumLiterals.Contains(literal));
    }

    private static void AddMissingReason(List<string> misses, string owner, string key, string entry)
    {
        if (!Config.IgnoreReasons.TryGetValue(key, out var reason) || string.IsNullOrWhiteSpace(reason))
            misses.Add($"{owner}: {entry} has no ignoreReasons entry '{key}'");
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

    private static string ReasonKey(string prefix, string tag, string attributeName)
    {
        return $"{prefix}:{tag}:{attributeName}";
    }

    #endregion
}
