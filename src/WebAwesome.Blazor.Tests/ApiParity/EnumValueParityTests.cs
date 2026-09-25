using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Verifies the values, not just the names, of wrapper parameters bound to CEM attributes whose type is
/// a pure string-literal union (e.g. 'long' | 'short' | 'narrow'; null and undefined members are
/// ignored). Types written with a type alias ('IconCanvas | undefined') are checked through the union the
/// export resolved into 'resolvedType'; an enum parameter bound to an attribute whose type still resolves to
/// no literal union fails unless allowlisted ("unresolvedEnumAttributes"). A plain-string attribute Web
/// Awesome reads as a space-separated token list ("tokenListAttributes", e.g. wa-tooltip trigger) is checked
/// token by token, and a [Flags] enum bound to it with every combination of its flags. For an enum or
/// nullable enum parameter, every enum member's ToHtmlValue() output must be one of the union's literals
/// (forward direction), and every union literal must be emitted by some member (reverse direction, so no
/// valid value is unreachable from C#). A bool/bool? parameter bound to such an attribute is always a
/// defect: Blazor renders true as an empty attribute and drops false, so no literal of the union can ever be
/// sent (the WaRelativeTime Numeric=false class of bug). Wrappers are resolved by the tag they render
/// (WaRange renders wa-slider), and attributes map to properties exactly as in ApiSurfaceParityTests
/// (kebab-case to PascalCase, attributeOverrides, global and per-component ignores). Deliberate deviations
/// are allowlisted in parity-config.json - per component in "ignoredEnumValues", "unreachableUnionValues",
/// "ignoredBoolUnionAttributes", "tokenListAttributes" and "unresolvedEnumAttributes", per enum type and
/// listed attribute in the top-level "unreachableEnumUnionValues" - each with an "ignoreReasons" entry;
/// stale allowlist entries fail. This class checks the ToHtmlValue() function; RenderedAttributeParityTests
/// checks what the wrappers actually render. The value checks are inert until parity-config.json sets
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
    /// Every enum parameter mapped to a CEM attribute must be checkable: the attribute's (alias-resolved) type must
    /// be a string-literal union or a declared token list, unless the attribute is allowlisted in
    /// "unresolvedEnumAttributes".
    /// </summary>
    [Fact]
    public void AllEnumParameters_AreBoundToResolvedUnion()
    {
        if (!Config.Enabled) return;

        var misses = UnresolvedEnumParameters()
            .Where(p => !GetComponentConfig(p.Tag).UnresolvedEnumAttributes.Contains(p.Attribute))
            .Select(p => $"{p.Tag}: attribute '{p.Attribute}' ({StripGenericArity(p.Wrapper.Name)}.{p.Property.Name} : {EnumTypeOf(p.Property).Name}) " +
                $"has type '{p.CemType}', which resolves to no string-literal union, so the enum's values cannot be checked " +
                "(resolve the alias in the export, e.g. via tools\\upgrade\\external-type-aliases.json, or allowlist it with a reason)")
            .ToList();

        AssertNoMisses(misses, "Enum parameters bound to attributes whose type resolves to no string-literal union");
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

        var bindings = EnumAttributeBindings().ToLookup(b => (b.Tag, b.Attribute));
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var (attributeName, members) in componentConfig.IgnoredEnumValues)
            {
                var attributeBindings = bindings[(tag, attributeName)].ToList();
                if (attributeBindings.Count == 0)
                {
                    misses.Add($"{tag}: ignoredEnumValues attribute '{attributeName}' is not an enum parameter bound to a CEM string-literal union attribute");
                    continue;
                }

                foreach (var member in members)
                {
                    var suppressesMiss = attributeBindings.Any(binding =>
                    {
                        var outOfUnion = binding.ToHtmlValue == null
                            ? new HashSet<string>(StringComparer.Ordinal)
                            : OutOfUnionValues(binding).Select(v => v.Member).ToHashSet(StringComparer.Ordinal);
                        return member == WildcardMember
                            ? binding.ToHtmlValue == null || outOfUnion.Count > 0
                            : outOfUnion.Contains(member);
                    });

                    if (!suppressesMiss)
                        misses.Add($"{tag}: attribute '{attributeName}': ignoredEnumValues entry '{member}' suppresses no miss and must be removed");
                }
            }
        }

        AssertNoMisses(misses, "Stale ignoredEnumValues entries");
    }

    /// <summary>
    /// Every unreachable-literal allowlist entry, per component ("unreachableUnionValues") and
    /// per enum type ("unreachableEnumUnionValues"), must list at least one literal (and, per enum
    /// type, at least one attribute) and carry a rationale in ignoreReasons.
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

        foreach (var (enumName, entry) in Config.UnreachableEnumUnionValues)
        {
            if (entry.Literals.Count == 0)
                misses.Add($"unreachableEnumUnionValues entry '{enumName}' lists no union literals");
            if (entry.Attributes.Count == 0)
                misses.Add($"unreachableEnumUnionValues entry '{enumName}' lists no attributes");

            AddMissingReason(misses, enumName, $"{UnreachableEnumUnionValuesKey}:{enumName}", $"unreachableEnumUnionValues entry '{enumName}'");
        }

        Assert.True(misses.Count == 0,
            $"Allowlisted unreachable union values without a reason ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Every unreachable-literal allowlist entry must still suppress a real miss: the literal must
    /// be in the bound attribute's union and emitted by no enum member. An entry of
    /// "unreachableEnumUnionValues" is checked per listed attribute: each must be bound to that enum
    /// and still need every literal of the entry.
    /// </summary>
    [Fact]
    public void UnreachableUnionValues_AreNotStale()
    {
        if (!Config.Enabled) return;

        var bindings = EnumAttributeBindings().Where(b => b.ToHtmlValue != null).ToList();
        var bindingsByAttribute = bindings.ToLookup(b => (b.Tag, b.Attribute));
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var (attributeName, literals) in componentConfig.UnreachableUnionValues)
            {
                var attributeBindings = bindingsByAttribute[(tag, attributeName)].ToList();
                if (attributeBindings.Count == 0)
                {
                    misses.Add($"{tag}: unreachableUnionValues attribute '{attributeName}' is not a mapped enum parameter bound to a CEM string-literal union attribute");
                    continue;
                }

                var unreachable = attributeBindings.SelectMany(UnreachableUnionValues).ToHashSet(StringComparer.Ordinal);
                foreach (var literal in literals.Where(l => !unreachable.Contains(l)))
                    misses.Add($"{tag}: attribute '{attributeName}': unreachableUnionValues entry '{literal}' suppresses no miss and must be removed");
            }
        }

        foreach (var (enumName, entry) in Config.UnreachableEnumUnionValues)
        {
            foreach (var key in entry.Attributes)
            {
                var attributeBindings = bindings.Where(b => AttributeKey(b.Tag, b.Attribute) == key && b.EnumType.Name == enumName).ToList();
                if (attributeBindings.Count == 0)
                {
                    misses.Add($"unreachableEnumUnionValues:{enumName}: attribute '{key}' is not bound to {enumName} with a CEM string-literal union and must be removed");
                    continue;
                }

                var unreachable = attributeBindings.SelectMany(UnreachableUnionValues).ToHashSet(StringComparer.Ordinal);
                foreach (var literal in entry.Literals.Where(l => !unreachable.Contains(l)))
                    misses.Add($"unreachableEnumUnionValues:{enumName}: literal '{literal}' is reachable or not in the union of '{key}', so the entry suppresses no miss there and the attribute must be removed from it");
            }
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

    /// <summary>
    /// Every "tokenListAttributes" and "unresolvedEnumAttributes" entry must carry a rationale in ignoreReasons,
    /// and a token list must list at least one token.
    /// </summary>
    [Fact]
    public void TokenListAndUnresolvedEnumAttributes_HaveReasons()
    {
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var (attributeName, tokens) in componentConfig.TokenListAttributes)
            {
                if (tokens == null || tokens.Count == 0)
                    misses.Add($"{tag}: tokenListAttributes entry '{attributeName}' lists no tokens");

                AddMissingReason(misses, tag, ReasonKey(TokenListAttributesKey, tag, attributeName), $"tokenListAttributes entry '{attributeName}'");
            }

            foreach (var attributeName in componentConfig.UnresolvedEnumAttributes)
                AddMissingReason(misses, tag, ReasonKey(UnresolvedEnumAttributesKey, tag, attributeName), $"unresolvedEnumAttributes entry '{attributeName}'");
        }

        AssertNoMisses(misses, "tokenListAttributes/unresolvedEnumAttributes entries without a reason");
    }

    /// <summary>
    /// A "tokenListAttributes" entry must name a CEM attribute of the element typed as a plain string (a literal
    /// union needs no token list), and an "unresolvedEnumAttributes" entry must still name an enum parameter bound
    /// to an attribute whose type resolves to no string-literal union.
    /// </summary>
    [Fact]
    public void TokenListAndUnresolvedEnumAttributes_AreNotStale()
    {
        if (!Config.Enabled) return;

        var unresolved = UnresolvedEnumParameters().Select(p => (p.Tag, p.Attribute)).ToHashSet();
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var attributeName in componentConfig.TokenListAttributes.Keys)
            {
                if (!Surface.Components.TryGetValue(tag, out var component) || !component.Attributes.TryGetValue(attributeName, out var attribute))
                    misses.Add($"{tag}: tokenListAttributes entry '{attributeName}' is not a CEM attribute of the element and must be removed");
                else if (attribute.EffectiveType != PlainStringType)
                    misses.Add($"{tag}: tokenListAttributes entry '{attributeName}' has CEM type '{attribute.EffectiveType}', not '{PlainStringType}'; check it as a union instead");
            }

            foreach (var attributeName in componentConfig.UnresolvedEnumAttributes.Where(a => !unresolved.Contains((tag, a))))
                misses.Add($"{tag}: unresolvedEnumAttributes entry '{attributeName}' is not an enum parameter bound to an unresolved attribute type and must be removed");
        }

        AssertNoMisses(misses, "Stale tokenListAttributes/unresolvedEnumAttributes entries");
    }

    #region ------ Internals ------

    private const string ToHtmlValueMethodName = "ToHtmlValue";
    private const string WildcardMember = "*";
    private const string IgnoredEnumValuesKey = "ignoredEnumValues";
    private const string UnreachableUnionValuesKey = "unreachableUnionValues";
    private const string UnreachableEnumUnionValuesKey = "unreachableEnumUnionValues";
    private const string IgnoredBoolUnionAttributesKey = "ignoredBoolUnionAttributes";
    private const string TokenListAttributesKey = "tokenListAttributes";
    private const string UnresolvedEnumAttributesKey = "unresolvedEnumAttributes";
    private const string UnionSeparator = " | ";
    private const string FlagSeparator = "|";
    private const string PlainStringType = "string";

    // a [Flags] enum with more single-bit members than this is not expanded into every combination
    private const int MaxExpandedFlags = 10;

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
    /// A [Parameter] property mapped to a CEM attribute whose type is a pure string-literal union (or a declared
    /// token list, whose tokens stand in for the union).
    /// </summary>
    internal sealed record LiteralUnionParameter(
        string Tag,
        string Attribute,
        Type Wrapper,
        PropertyInfo Property,
        IReadOnlyList<string> Union,
        bool IsTokenList = false);

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
        MethodInfo? ToHtmlValue,
        bool IsTokenList = false);

    /// <summary>
    /// An enum [Parameter] property mapped to a CEM attribute whose type resolves to no string-literal union.
    /// </summary>
    private sealed record UnresolvedEnumParameter(string Tag, string Attribute, Type Wrapper, PropertyInfo Property, string? CemType);

    private static IEnumerable<LiteralUnionParameter> AllLiteralUnionParameters()
    {
        foreach (var (tag, component, wrapper) in WrappersByTag())
        {
            foreach (var parameter in LiteralUnionParameters(tag, component, wrapper))
                yield return parameter;
        }
    }

    private static IEnumerable<EnumAttributeBinding> EnumAttributeBindings()
    {
        foreach (var parameter in AllLiteralUnionParameters())
        {
            var enumType = EnumTypeOf(parameter.Property);
            if (!enumType.IsEnum) continue;

            ToHtmlValueMethods.TryGetValue(enumType, out var toHtmlValue);
            yield return new EnumAttributeBinding(parameter.Tag, parameter.Attribute, parameter.Wrapper, parameter.Property,
                enumType, parameter.Union, toHtmlValue, parameter.IsTokenList);
        }
    }

    private static IEnumerable<UnresolvedEnumParameter> UnresolvedEnumParameters()
    {
        foreach (var (tag, component, wrapper) in WrappersByTag())
        {
            var componentConfig = GetComponentConfig(tag);

            foreach (var (attributeName, attribute) in component.Attributes)
            {
                if (IsIgnoredAttribute(componentConfig, attributeName)) continue;
                if (componentConfig.TokenListAttributes.ContainsKey(attributeName)) continue;
                if (ParseStringLiteralUnion(attribute.EffectiveType) != null) continue;

                var property = FindParameter(wrapper, ExpectedParameterName(componentConfig, attributeName));
                if (property == null || !EnumTypeOf(property).IsEnum) continue;

                yield return new UnresolvedEnumParameter(tag, attributeName, wrapper, property, attribute.EffectiveType);
            }
        }
    }

    private static Type EnumTypeOf(PropertyInfo property)
    {
        return Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
    }

    /// <summary>
    /// Maps the string-literal union attributes (and declared token-list attributes) of one custom element to the
    /// wrapper's [Parameter] properties.
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

            var isTokenList = componentConfig.TokenListAttributes.TryGetValue(attributeName, out var tokens);
            var union = isTokenList ? tokens : ParseStringLiteralUnion(attribute.EffectiveType);
            if (union == null) continue;

            // a missing parameter is reported by RenderedAttributeParityTests.AllCemAttributes_AreRenderedByTheirParameter
            var property = FindParameter(wrapper, ExpectedParameterName(componentConfig, attributeName));
            if (property == null) continue;

            yield return new LiteralUnionParameter(tag, attributeName, wrapper, property, union, isTokenList);
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
    /// Returns the values of an enum type to check: every member and, for a [Flags] enum, also every combination
    /// of its single-bit members no member names (a flag enum parameter can be set to any combination).
    /// </summary>
    /// <param name="enumType">Enum type</param>
    /// <returns>Description ("Member" or "A|B") and boxed value of each value</returns>
    internal static IEnumerable<(string Name, object Value)> EnumValuesToCheck(Type enumType)
    {
        foreach (var name in Enum.GetNames(enumType))
            yield return (name, Enum.Parse(enumType, name));

        if (!enumType.IsDefined(typeof(FlagsAttribute), inherit: false)) yield break;

        var flags = Enum.GetNames(enumType)
            .Select(name => (Name: name, Bits: Convert.ToUInt64(Enum.Parse(enumType, name))))
            .Where(f => BitOperations.PopCount(f.Bits) == 1)
            .ToList();
        if (flags.Count > MaxExpandedFlags) throw new InvalidOperationException($"{enumType.Name} has too many flags to combine");

        // the combinations no named member already stands for
        for (var mask = 1; mask < 1 << flags.Count; mask++)
        {
            var members = flags.Where((_, i) => (mask & (1 << i)) != 0).ToList();
            var value = Enum.ToObject(enumType, members.Aggregate(0UL, (acc, f) => acc | f.Bits));
            if (Enum.IsDefined(enumType, value)) continue;

            yield return (string.Join(FlagSeparator, members.Select(f => f.Name)), value);
        }
    }

    /// <summary>
    /// Splits an emitted value into the values to look up in the union: the whole value, or its whitespace-separated
    /// tokens for a token-list attribute.
    /// </summary>
    /// <param name="value">Emitted attribute value</param>
    /// <param name="isTokenList">Whether the attribute is a token list</param>
    /// <returns>The values to check</returns>
    internal static IReadOnlyList<string> ValuesOf(string value, bool isTokenList)
    {
        return isTokenList ? value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) : new[] { value };
    }

    /// <summary>
    /// Returns the enum members whose ToHtmlValue() output is not a literal of the binding's union (or that throw
    /// or yield null); for a token-list attribute every token must be in the union.
    /// </summary>
    /// <param name="binding">Enum binding with a ToHtmlValue method</param>
    /// <returns>Member name and a description of the offending output</returns>
    internal static IEnumerable<(string Member, string Detail)> OutOfUnionValues(EnumAttributeBinding binding)
    {
        foreach (var (memberName, value) in EnumValuesToCheck(binding.EnumType))
        {
            var call = $"{ToHtmlValueMethodName}({binding.EnumType.Name}.{memberName})";
            var (htmlValue, failure) = InvokeToHtmlValue(binding.ToHtmlValue!, value);

            if (failure != null)
                yield return (memberName, $"{call} = <throws {failure}>");
            else if (htmlValue == null)
                yield return (memberName, $"{call} = null");
            else if (ValuesOf(htmlValue, binding.IsTokenList) is var values && (values.Count == 0 || values.Any(v => !binding.Union.Contains(v, StringComparer.Ordinal))))
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
        var emitted = EnumValuesToCheck(binding.EnumType)
            .Select(v => InvokeToHtmlValue(binding.ToHtmlValue!, v.Value).Value)
            .Where(value => value != null)
            .SelectMany(value => ValuesOf(value!, binding.IsTokenList))
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

    /// <summary>
    /// Formats a union for a miss description.
    /// </summary>
    /// <param name="union">Union literals</param>
    /// <returns>The literals, quoted and separated by " | "</returns>
    internal static string FormatUnion(IReadOnlyList<string> union)
    {
        return string.Join(UnionSeparator, union.Select(v => $"'{v}'"));
    }

    /// <summary>
    /// Returns the allowlist key of an attribute of an element, "&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    /// <param name="tag">Custom element tag name</param>
    /// <param name="attribute">CEM attribute name</param>
    /// <returns>The key</returns>
    internal static string AttributeKey(string tag, string attribute) => $"{tag}:{attribute}";

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
            || (Config.UnreachableEnumUnionValues.TryGetValue(binding.EnumType.Name, out var entry)
                && entry.Literals.Contains(literal)
                && entry.Attributes.Contains(AttributeKey(binding.Tag, binding.Attribute)));
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

    private static string ReasonKey(string prefix, string tag, string attributeName)
    {
        return $"{prefix}:{tag}:{attributeName}";
    }

    #endregion
}
