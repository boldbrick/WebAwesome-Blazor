using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Render-based attribute parity: every wrapper component is rendered with bUnit (RenderedWrapperCatalog), once
/// plain and once per sample value of each [Parameter] - every member (and [Flags] combination) of an enum, true
/// and false for a bool, a marker string, a fractional and a negative number, a date - set on its own, and the
/// attributes of the root element are compared with the Custom Elements Manifest of the tag the wrapper actually
/// renders (WaRange renders wa-slider). The renders run under a hostile culture (cs-CZ with its decimal comma, plus
/// the U+2212 minus sign and a '.' time separator some cultures use), because Blazor formats every non-string
/// attribute value with the current culture. Checked: (a) every rendered attribute is a CEM attribute of the tag,
/// an HTML global attribute, or allowlisted ("extraRenderedAttributes"); (b) every rendered value of an attribute
/// typed as a string-literal union (alias-resolved, or a "tokenListAttributes" token list) is in the union; (c) a
/// bool parameter set to false renders no attribute, or exactly "false" where the CEM default is true, true renders
/// the attribute, and no value is ever "True"/"False"; (d) every CEM attribute has a parameter that renders it when
/// set to a non-default value (allowlist "unrenderedAttributes"); (e) numbers and dates render in the invariant
/// culture. Parameters of other types (collections, objects, fragments, callbacks) are not sampled. Every allowlist
/// entry needs a reason, and stale entries fail. Skipped until parity-config.json sets "enabled": true.
/// </summary>
public class RenderedAttributeParityTests
{
    /// <summary>
    /// Every wrapper must render with each sample value of each of its parameters.
    /// </summary>
    [Fact]
    public void AllWrappers_RenderWithEverySampleValue()
    {
        SkipUnlessParityEnabled();

        var misses = new List<string>();

        foreach (var wrapper in AllRenders.Value)
        {
            if (wrapper.Baseline.Error != null)
                misses.Add($"{wrapper.ComponentType.Name}: failed to render: {wrapper.Baseline.Error}");

            foreach (var render in wrapper.Samples.Where(s => s.Root.Error != null))
                misses.Add($"{wrapper.ComponentType.Name}.{render.Sample.Label}: failed to render: {render.Root.Error}");
        }

        AssertNoMisses(misses, "Wrappers that fail to render with a sample parameter value");
    }

    /// <summary>
    /// (a) Every attribute a wrapper renders on its root element must be a CEM attribute of the rendered tag, an
    /// HTML global attribute, or allowlisted in "extraRenderedAttributes".
    /// </summary>
    [Fact]
    public void AllRenderedAttributes_AreDeclaredByTheElement()
    {
        SkipUnlessParityEnabled();

        var misses = RenderedElements().SelectMany(UndeclaredAttributes).Distinct(StringComparer.Ordinal).ToList();

        AssertNoMisses(misses, "Rendered attributes the element does not declare");
    }

    /// <summary>
    /// (b) Every rendered value of an attribute whose CEM type is a string-literal union (or a declared token list)
    /// must be in the union.
    /// </summary>
    [Fact]
    public void AllRenderedUnionValues_AreInTheAttributeUnion()
    {
        SkipUnlessParityEnabled();

        var misses = RenderedElements().SelectMany(OutOfUnionRenders).Distinct(StringComparer.Ordinal).ToList();

        AssertNoMisses(misses, "Rendered attribute values outside the CEM string-literal union");
    }

    /// <summary>
    /// (c) A bool parameter bound to a boolean CEM attribute renders the attribute when true; when false it renders
    /// nothing, or exactly "false" where the CEM default is true. No attribute value is ever "True" or "False".
    /// </summary>
    [Fact]
    public void BooleanParameters_RenderValidBooleanAttributes()
    {
        SkipUnlessParityEnabled();

        var misses = RenderedElements().SelectMany(BooleanMisses).Distinct(StringComparer.Ordinal).ToList();

        AssertNoMisses(misses, "Bool parameters rendering an invalid boolean attribute");
    }

    /// <summary>
    /// (d) Every CEM attribute of a rendered element must be exposed by a [Parameter] of each wrapper rendering the
    /// element, and setting that parameter to a non-default value must render (or change) the attribute; allowlist
    /// "unrenderedAttributes"; an element no wrapper renders fails. This replaces the retired name-only
    /// ApiSurfaceParityTests.AllAttributes_AreExposedAsParameters, which accepted a [Parameter] of any type that
    /// rendered nothing, and saw only the wrapper found by class name.
    /// </summary>
    [Fact]
    public void AllCemAttributes_AreRenderedByTheirParameter()
    {
        SkipUnlessParityEnabled();

        var elements = RenderedElements().ToList();
        var misses = elements.SelectMany(UnrenderedAttributes).ToList();

        // an element no wrapper renders without parameters would have its attributes checked by nothing
        var renderedTags = elements.Select(e => e.Tag).ToHashSet(StringComparer.Ordinal);
        misses.AddRange(RelevantComponents()
            .Where(c => !renderedTags.Contains(c.Tag))
            .Select(c => $"{c.Tag}: no wrapper renders the element without parameters, so its attributes cannot be checked"));

        AssertNoMisses(misses, "CEM attributes no parameter renders");
    }

    /// <summary>
    /// (e) Numbers and dates render in the invariant culture: a number parameter's attribute parses back to the
    /// value with the invariant culture, no attribute carries the culture-formatted value, and every value of a
    /// number-typed CEM attribute parses as an invariant-culture number.
    /// </summary>
    [Fact]
    public void NumberAndDateAttributes_RenderInTheInvariantCulture()
    {
        SkipUnlessParityEnabled();

        var misses = RenderedElements().SelectMany(CultureMisses).Distinct(StringComparer.Ordinal).ToList();

        AssertNoMisses(misses, $"Attribute values not in the invariant culture (rendered under {HostileCulture.Name})");
    }

    /// <summary>
    /// Every "extraRenderedAttributes", "unrenderedAttributes", "attributePrerequisites" and "trueFalseAttributes"
    /// entry must carry a rationale of its own in ignoreReasons (or knownDefects), keyed
    /// "&lt;list&gt;:&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [Fact]
    public void RenderedAttributeAllowlists_HaveReasons()
    {
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            foreach (var attribute in componentConfig.ExtraRenderedAttributes)
                AddMissingReason(misses, $"{ExtraRenderedAttributesKey}:{tag}:{attribute}");

            foreach (var attribute in componentConfig.UnrenderedAttributes)
                AddMissingReason(misses, $"{UnrenderedAttributesKey}:{tag}:{attribute}");

            foreach (var attribute in componentConfig.AttributePrerequisites.Keys)
                AddMissingReason(misses, $"{AttributePrerequisitesKey}:{tag}:{attribute}");

            foreach (var attribute in componentConfig.TrueFalseAttributes)
                AddMissingReason(misses, $"{TrueFalseAttributesKey}:{tag}:{attribute}");
        }

        AssertNoMisses(misses, "Rendered-attribute allowlist entries without a reason");
    }

    /// <summary>
    /// Every "extraRenderedAttributes" entry must still be rendered by a wrapper of the element and still be
    /// undeclared; every "unrenderedAttributes" entry must still be a CEM attribute some wrapper does not render;
    /// every "attributePrerequisites" entry must name a CEM attribute and parameters a wrapper of the element has;
    /// every "trueFalseAttributes" entry must name a boolean CEM attribute not defaulting to true whose parameter
    /// renders "false".
    /// </summary>
    [Fact]
    public void RenderedAttributeAllowlists_AreNotStale()
    {
        SkipUnlessParityEnabled();

        var elements = RenderedElements().ToList();
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            var ofTag = elements.Where(e => e.Tag == tag).ToList();

            foreach (var attribute in componentConfig.ExtraRenderedAttributes)
            {
                var needed = ofTag.Any(e => !e.Component.Attributes.ContainsKey(attribute)
                    && !IsHtmlGlobalAttribute(attribute)
                    && e.Renders.AllRoots.Any(r => r.Attributes.ContainsKey(attribute)));
                if (!needed)
                    misses.Add($"{tag}: extraRenderedAttributes entry '{attribute}' is not rendered, or is now declared, and must be removed");
            }

            foreach (var attribute in componentConfig.UnrenderedAttributes)
            {
                var needed = ofTag.Any(e => e.Component.Attributes.ContainsKey(attribute)
                    && UnrenderedAttributeMiss(e, attribute) != null);
                if (!needed)
                    misses.Add($"{tag}: unrenderedAttributes entry '{attribute}' is rendered by every wrapper of the element, or no CEM attribute, and must be removed");
            }

            foreach (var (attribute, prerequisites) in componentConfig.AttributePrerequisites)
            {
                var valid = ofTag.Any(e => e.Component.Attributes.ContainsKey(attribute)
                    && FindParameter(e.Renders.ComponentType, ExpectedParameterName(componentConfig, attribute)) != null
                    && prerequisites.Keys.All(p => FindParameter(e.Renders.ComponentType, p) != null));
                if (!valid)
                    misses.Add($"{tag}: attributePrerequisites entry '{attribute}' names no CEM attribute, or parameters no wrapper of the element has, and must be removed");
            }

            foreach (var attribute in componentConfig.TrueFalseAttributes)
            {
                var needed = ofTag.Any(e => e.Component.Attributes.TryGetValue(attribute, out var surface)
                    && surface.Default != TrueDefault
                    && e.Renders.Samples.Any(s => s.Sample.Value is false
                        && s.Sample.Property.Name == ExpectedParameterName(componentConfig, attribute)
                        && s.Root.Attributes.TryGetValue(attribute, out var value) && value == FalseText));
                if (!needed)
                    misses.Add($"{tag}: trueFalseAttributes entry '{attribute}' is not a non-true-default boolean attribute rendered as \"{FalseText}\" and must be removed");
            }
        }

        AssertNoMisses(misses, "Stale rendered-attribute allowlist entries");
    }

    /// <summary>
    /// Every "globalIgnoredAttributes" and "ignoredAttributes" entry must still hide a miss: the attribute must be a
    /// CEM attribute of the element (of some element, for the global list) that a wrapper of the element does not
    /// render from its parameter. Every "attributeOverrides" entry must differ from the naming convention and name a
    /// CEM attribute of the element and a [Parameter] a wrapper of the element has.
    /// </summary>
    [Fact]
    public void IgnoredAttributesAndAttributeOverrides_AreNotStale()
    {
        SkipUnlessParityEnabled();

        var elements = RenderedElements().ToList();
        var misses = new List<string>();

        foreach (var attribute in Config.GlobalIgnoredAttributes)
        {
            if (!elements.Any(e => IsUnrenderedCemAttribute(e, attribute)))
                misses.Add($"globalIgnoredAttributes entry '{attribute}' is declared by no rendered element, or every wrapper of the elements declaring it renders it from its parameter, and must be removed");
        }

        foreach (var (tag, componentConfig) in Config.Components)
        {
            var ofTag = elements.Where(e => e.Tag == tag).ToList();

            foreach (var attribute in componentConfig.IgnoredAttributes)
            {
                if (!ofTag.Any(e => IsUnrenderedCemAttribute(e, attribute)))
                    misses.Add($"{tag}: ignoredAttributes entry '{attribute}' is no CEM attribute of the element, or every wrapper of the element renders it from its parameter, and must be removed");
            }

            foreach (var (attribute, parameter) in componentConfig.AttributeOverrides)
            {
                if (parameter == ToPascalCase(attribute))
                    misses.Add($"{tag}: attributeOverrides entry '{attribute}' -> '{parameter}' equals the naming convention and must be removed");
                else if (!ofTag.Any(e => e.Component.Attributes.ContainsKey(attribute) && FindParameter(e.Renders.ComponentType, parameter) != null))
                    misses.Add($"{tag}: attributeOverrides entry '{attribute}' -> '{parameter}' names no CEM attribute of the element, or a parameter no wrapper of the element has, and must be removed");
            }
        }

        AssertNoMisses(misses, "Stale ignored-attribute and attribute-override entries");
    }

    #region ------ Internals ------

    private const string ExtraRenderedAttributesKey = "extraRenderedAttributes";
    private const string UnrenderedAttributesKey = "unrenderedAttributes";
    private const string AttributePrerequisitesKey = "attributePrerequisites";
    private const string TrueFalseAttributesKey = "trueFalseAttributes";
    private const string StringSample = "x-sample-value";
    private const string BooleanType = "boolean";
    private const string NumberType = "number";
    private const string StringType = "string";
    private const string TrueDefault = "true";
    private const string TrueText = "true";
    private const string FalseText = "false";
    private const string AriaPrefix = "aria-";
    private const string DataPrefix = "data-";
    private const string MinusSign = "−";
    private const string DotTimeSeparator = ".";
    private const string CzechCultureName = "cs-CZ";
    private const double FractionalSample = 2.5;
    private const double NegativeFractionalSample = -1.5;
    private const long IntegralSample = 7;
    private const long NegativeIntegralSample = -3;

    // non-number values Web Awesome's number attributes accept
    private static readonly HashSet<string> NumberKeywords = new(StringComparer.Ordinal) { "Infinity" };

    private static readonly DateTime DateSample = new(2026, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc);

    // capitalized .NET boolean text, which no Web Awesome attribute reads as intended
    private static readonly HashSet<string> DotNetBooleanTexts = new(StringComparer.Ordinal) { bool.TrueString, bool.FalseString };

    // the HTML Living Standard's global attributes (section 3.2.6, "Global attributes") plus ARIA's role; valid on
    // every element, so a wrapper may render them on any root element whatever the element's CEM lists
    private static readonly HashSet<string> HtmlGlobalAttributes = new(StringComparer.Ordinal)
    {
        "accesskey", "autocapitalize", "autocorrect", "autofocus", "class", "contenteditable", "dir", "draggable",
        "enterkeyhint", "hidden", "id", "inert", "inputmode", "is", "itemid", "itemprop", "itemref", "itemscope",
        "itemtype", "lang", "nonce", "popover", "role", "slot", "spellcheck", "style", "tabindex", "title",
        "translate", "writingsuggestions"
    };

    private static readonly HashSet<Type> FloatingTypes = new() { typeof(double), typeof(float), typeof(decimal) };

    private static readonly HashSet<Type> SignedIntegralTypes = new() { typeof(int), typeof(long), typeof(short), typeof(sbyte) };

    private static readonly HashSet<Type> UnsignedIntegralTypes = new() { typeof(uint), typeof(ulong), typeof(ushort), typeof(byte) };

    /// <summary>
    /// cs-CZ without user overrides (decimal comma), with the minus sign and time separator replaced by the U+2212
    /// minus and the '.' some cultures use, so every culture-sensitive number or date formatting shows.
    /// </summary>
    internal static readonly CultureInfo HostileCulture = CreateHostileCulture();

    private static readonly Lazy<IReadOnlyList<WrapperRenders>> AllRenders = new(() =>
        RenderedWrapperCatalog.WrapperTypes.Select(RenderWrapper).ToList());

    /// <summary>
    /// A sample value of one parameter.
    /// </summary>
    /// <param name="Property">The [Parameter] property</param>
    /// <param name="Label">"Property=value" for miss descriptions</param>
    /// <param name="Value">The boxed value</param>
    internal sealed record ParameterSample(PropertyInfo Property, string Label, object Value)
    {
        /// <summary>
        /// Other parameters set in the same render (the attribute's "attributePrerequisites").
        /// </summary>
        public IReadOnlyList<(string Name, object Value)> Prerequisites { get; init; } = Array.Empty<(string, object)>();
    }

    /// <summary>
    /// One render with a single parameter set to a sample value.
    /// </summary>
    internal sealed record SampleRender(ParameterSample Sample, RenderedRoot Root);

    /// <summary>
    /// The baseline render and all sample renders of one wrapper.
    /// </summary>
    internal sealed record WrapperRenders(Type ComponentType, RenderedRoot Baseline, IReadOnlyList<SampleRender> Samples)
    {
        public IEnumerable<RenderedRoot> AllRoots => Samples.Select(s => s.Root).Prepend(Baseline);
    }

    /// <summary>
    /// A wrapper whose baseline render produced a custom element of the expected surface.
    /// </summary>
    internal sealed record RenderedElement(string Tag, ComponentSurface Component, ComponentParityConfig Config, WrapperRenders Renders)
    {
        public string Name => Renders.ComponentType.Name;
    }

    private static CultureInfo CreateHostileCulture()
    {
        var culture = (CultureInfo)new CultureInfo(CzechCultureName, useUserOverride: false).Clone();
        culture.NumberFormat.NegativeSign = MinusSign;
        culture.DateTimeFormat.TimeSeparator = DotTimeSeparator;
        return culture;
    }

    private static WrapperRenders RenderWrapper(Type componentType)
    {
        var baseline = RenderedWrapperCatalog.RenderRoots(componentType, new[] { Array.Empty<(string, object?)>() }, HostileCulture)[0];
        var prerequisites = PrerequisitesByParameter(componentType, baseline.Tag);
        var samples = SampleValues(componentType)
            .Select(s => prerequisites.TryGetValue(s.Property.Name, out var with)
                ? s with { Label = $"{s.Label} (with {string.Join(", ", with.Select(p => $"{p.Name}={Describe(p.Value)}"))})", Prerequisites = with }
                : s)
            .ToList();

        var parameterSets = samples
            .Select(s => (IReadOnlyList<(string Name, object? Value)>)s.Prerequisites.Select(p => (p.Name, (object?)p.Value)).Append((s.Property.Name, s.Value)).ToList())
            .ToList();

        var roots = RenderedWrapperCatalog.RenderRoots(componentType, parameterSets, HostileCulture);
        return new WrapperRenders(componentType, baseline, samples.Select((s, i) => new SampleRender(s, roots[i])).ToList());
    }

    // the "attributePrerequisites" of the rendered element, keyed by the name of the attribute's parameter and
    // converted to the prerequisite parameters' types
    private static Dictionary<string, IReadOnlyList<(string Name, object Value)>> PrerequisitesByParameter(Type componentType, string? tag)
    {
        var result = new Dictionary<string, IReadOnlyList<(string Name, object Value)>>(StringComparer.Ordinal);
        if (tag == null) return result;

        var componentConfig = GetComponentConfig(tag);
        foreach (var (attribute, prerequisites) in componentConfig.AttributePrerequisites)
        {
            var converted = new List<(string Name, object Value)>();
            foreach (var (name, json) in prerequisites)
            {
                var property = FindParameter(componentType, name);
                if (property == null) continue;
                converted.Add((name, ConvertJson(json, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType)));
            }

            result[ExpectedParameterName(componentConfig, attribute)] = converted;
        }

        return result;
    }

    private static object ConvertJson(JsonElement json, Type type)
    {
        if (type.IsEnum) return Enum.Parse(type, json.GetString()!);
        if (type == typeof(bool)) return json.GetBoolean();
        if (type == typeof(string)) return json.GetString()!;
        return Convert.ChangeType(json.GetDecimal(), type, CultureInfo.InvariantCulture);
    }

    private static IEnumerable<RenderedElement> RenderedElements()
    {
        foreach (var renders in AllRenders.Value)
        {
            var tag = renders.Baseline.Tag;
            if (tag == null || Config.IgnoredComponents.Contains(tag)) continue;
            if (!Surface.Components.TryGetValue(tag, out var component)) continue;

            yield return new RenderedElement(tag, component, GetComponentConfig(tag), renders);
        }
    }

    /// <summary>
    /// Returns the sample values of every sampled [Parameter] of a component type.
    /// </summary>
    /// <param name="componentType">Component type</param>
    /// <returns>The samples, parameter by parameter</returns>
    internal static IEnumerable<ParameterSample> SampleValues(Type componentType)
    {
        foreach (var property in componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            var parameter = property.GetCustomAttribute<ParameterAttribute>(inherit: true);
            if (parameter == null || parameter.CaptureUnmatchedValues || !property.CanWrite) continue;

            foreach (var value in SampleValuesOf(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType))
                yield return new ParameterSample(property, $"{property.Name}={Describe(value)}", value);
        }
    }

    private static IEnumerable<object> SampleValuesOf(Type type)
    {
        if (type.IsEnum)
        {
            foreach (var (_, value) in EnumValueParityTests.EnumValuesToCheck(type))
                yield return value;
        }
        else if (type == typeof(bool))
        {
            yield return true;
            yield return false;
        }
        else if (type == typeof(string))
        {
            yield return StringSample;
        }
        else if (FloatingTypes.Contains(type))
        {
            yield return Convert.ChangeType(FractionalSample, type, CultureInfo.InvariantCulture);
            yield return Convert.ChangeType(NegativeFractionalSample, type, CultureInfo.InvariantCulture);
        }
        else if (SignedIntegralTypes.Contains(type))
        {
            yield return Convert.ChangeType(IntegralSample, type, CultureInfo.InvariantCulture);
            yield return Convert.ChangeType(NegativeIntegralSample, type, CultureInfo.InvariantCulture);
        }
        else if (UnsignedIntegralTypes.Contains(type))
        {
            yield return Convert.ChangeType(IntegralSample, type, CultureInfo.InvariantCulture);
        }
        else if (type == typeof(DateTime))
        {
            yield return DateSample;
        }
    }

    private static bool IsNumber(object value) => FloatingTypes.Contains(value.GetType())
        || SignedIntegralTypes.Contains(value.GetType())
        || UnsignedIntegralTypes.Contains(value.GetType());

    private static string Describe(object value) => value switch
    {
        string text => $"\"{text}\"",
        bool flag => flag ? TrueText : FalseText,
        Enum member => member.ToString(),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    /// <summary>
    /// Determines whether an attribute is valid on every HTML element (a global attribute, aria-* or data-*).
    /// </summary>
    /// <param name="attribute">Attribute name</param>
    /// <returns>true for a global attribute</returns>
    internal static bool IsHtmlGlobalAttribute(string attribute)
    {
        return HtmlGlobalAttributes.Contains(attribute)
            || attribute.StartsWith(AriaPrefix, StringComparison.Ordinal)
            || attribute.StartsWith(DataPrefix, StringComparison.Ordinal);
    }

    private static IEnumerable<string> UndeclaredAttributes(RenderedElement element)
    {
        var reported = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (root, label) in RootsWithLabels(element.Renders))
        {
            if (root.Tag != element.Tag) continue;

            foreach (var attribute in root.Attributes.Keys)
            {
                if (element.Component.Attributes.ContainsKey(attribute) || IsHtmlGlobalAttribute(attribute)) continue;
                if (element.Config.ExtraRenderedAttributes.Contains(attribute) || !reported.Add(attribute)) continue;

                yield return $"{element.Tag} ({element.Name}): renders '{attribute}' ({label}), which is neither a CEM attribute of the element nor an HTML global attribute";
            }
        }
    }

    private static IEnumerable<string> OutOfUnionRenders(RenderedElement element)
    {
        foreach (var render in element.Renders.Samples.Prepend(null))
        {
            var root = render?.Root ?? element.Renders.Baseline;
            if (root.Tag != element.Tag) continue;

            foreach (var (attribute, value) in root.Attributes)
            {
                if (!element.Component.Attributes.TryGetValue(attribute, out var surface)) continue;

                var isTokenList = element.Config.TokenListAttributes.TryGetValue(attribute, out var tokens);
                var union = isTokenList ? tokens : EnumValueParityTests.ParseStringLiteralUnion(surface.EffectiveType);
                if (union == null) continue;

                // a string parameter passes its own value through; it is not an enum defect
                if (render?.Sample.Value is string && value.Contains(StringSample, StringComparison.Ordinal)) continue;

                var values = EnumValueParityTests.ValuesOf(value, isTokenList);
                if (values.Count > 0 && values.All(v => union.Contains(v, StringComparer.Ordinal))) continue;
                if (!isTokenList && union.Contains(value, StringComparer.Ordinal)) continue;

                yield return $"{element.Tag} ({element.Name}): renders {attribute}=\"{value}\" ({render?.Sample.Label ?? "no parameter set"}), " +
                    $"not in {EnumValueParityTests.FormatUnion(union)}";
            }
        }
    }

    private static IEnumerable<string> BooleanMisses(RenderedElement element)
    {
        // a string-typed attribute (e.g. a checkbox's submitted value) may carry any text
        var reported = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (root, label) in RootsWithLabels(element.Renders))
        {
            foreach (var (attribute, value) in root.Attributes.Where(a => DotNetBooleanTexts.Contains(a.Value)))
            {
                if (element.Component.Attributes.TryGetValue(attribute, out var declared) && UnionParts(declared.EffectiveType).Contains(StringType)) continue;
                if (reported.Add(attribute))
                    yield return $"{element.Tag} ({element.Name}): renders {attribute}=\"{value}\" ({label}); .NET boolean text is no valid attribute value";
            }
        }

        foreach (var (attribute, surface, property) in MappedAttributes(element))
        {
            if (property == null || !EnumValueParityTests.IsBoolType(property.PropertyType)) continue;
            if (!UnionParts(surface.EffectiveType).Contains(BooleanType)) continue;

            var readsFalse = string.Equals(surface.Default, TrueDefault, StringComparison.Ordinal) || element.Config.TrueFalseAttributes.Contains(attribute);

            foreach (var render in element.Renders.Samples.Where(s => s.Sample.Property == property && s.Root.Error == null))
            {
                var present = render.Root.Attributes.TryGetValue(attribute, out var value);
                var label = $"{element.Tag} ({element.Name}.{render.Sample.Label})";

                if ((bool)render.Sample.Value)
                {
                    if (!present)
                        yield return $"{label}: renders no '{attribute}' attribute";
                    else if (value != string.Empty && value != TrueText)
                        yield return $"{label}: renders {attribute}=\"{value}\", expected a present attribute or \"{TrueText}\"";
                }
                else if (readsFalse)
                {
                    if (value != FalseText)
                        yield return $"{label}: '{attribute}' defaults to true or reads \"{FalseText}\" (trueFalseAttributes), so false must render {attribute}=\"{FalseText}\", but it renders {(present ? $"\"{value}\"" : "nothing")}";
                }
                else if (present)
                {
                    yield return $"{label}: renders {attribute}=\"{value}\" for false; a present boolean attribute reads as true";
                }
            }
        }
    }

    private static IEnumerable<string> UnrenderedAttributes(RenderedElement element)
    {
        foreach (var attribute in element.Component.Attributes.Keys.OrderBy(a => a, StringComparer.Ordinal))
        {
            if (IsIgnoredAttribute(element.Config, attribute) || element.Config.UnrenderedAttributes.Contains(attribute)) continue;

            var miss = UnrenderedAttributeMiss(element, attribute);
            if (miss != null) yield return miss;
        }
    }

    // whether the attribute is a CEM attribute of the element that its wrapper does not render from a parameter
    private static bool IsUnrenderedCemAttribute(RenderedElement element, string attribute)
    {
        return element.Component.Attributes.ContainsKey(attribute) && UnrenderedAttributeMiss(element, attribute) != null;
    }

    // why the element's wrapper does not render the attribute from its parameter, or null when it does
    private static string? UnrenderedAttributeMiss(RenderedElement element, string attribute)
    {
        var parameterName = ExpectedParameterName(element.Config, attribute);
        var property = FindParameter(element.Renders.ComponentType, parameterName);
        var label = $"{element.Tag} ({element.Name})";

        if (property == null)
            return $"{label}: attribute '{attribute}' has no [Parameter] property '{parameterName}'";

        var samples = element.Renders.Samples.Where(s => s.Sample.Property == property && s.Root.Error == null).ToList();
        if (samples.Count == 0)
            return $"{label}: attribute '{attribute}' is exposed as {property.Name} : {property.PropertyType.Name}, a type the render check cannot sample";

        element.Renders.Baseline.Attributes.TryGetValue(attribute, out var baseline);
        if (samples.Any(s => s.Root.Attributes.TryGetValue(attribute, out var value) ? value != baseline : baseline != null)) return null;

        return $"{label}: setting {property.Name} to {string.Join(", ", samples.Select(s => Describe(s.Sample.Value)))} never renders or changes '{attribute}'";
    }

    private static IEnumerable<string> CultureMisses(RenderedElement element)
    {
        // every value of a number-typed attribute must be an invariant-culture number
        foreach (var (root, label) in RootsWithLabels(element.Renders))
        {
            foreach (var (attribute, value) in root.Attributes)
            {
                if (!element.Component.Attributes.TryGetValue(attribute, out var surface)) continue;

                var parts = UnionParts(surface.EffectiveType);
                if (!parts.Contains(NumberType) || parts.Contains(StringType)) continue;

                // a string parameter (e.g. a step that also takes "any") passes its own value through
                if (value.Contains(StringSample, StringComparison.Ordinal)) continue;
                if (NumberKeywords.Contains(value) || decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _)) continue;

                yield return $"{element.Tag} ({element.Name}): renders number attribute {attribute}=\"{value}\" ({label}), which is no invariant-culture number";
            }
        }

        var mapped = MappedAttributes(element).Where(m => m.Property != null).ToDictionary(m => m.Property!, m => m.Attribute);

        foreach (var render in element.Renders.Samples.Where(s => s.Root.Error == null))
        {
            var sample = render.Sample;
            var label = $"{element.Tag} ({element.Name}.{sample.Label})";

            if (IsNumber(sample.Value))
            {
                var invariant = Convert.ToString(sample.Value, CultureInfo.InvariantCulture)!;
                var hostile = Convert.ToString(sample.Value, HostileCulture)!;

                foreach (var (attribute, value) in render.Root.Attributes)
                {
                    element.Renders.Baseline.Attributes.TryGetValue(attribute, out var baseline);
                    if (value != baseline && hostile != invariant && value.Contains(hostile, StringComparison.Ordinal))
                        yield return $"{label}: renders {attribute}=\"{value}\", formatted with the current culture (invariant: \"{invariant}\")";
                }

                if (mapped.TryGetValue(sample.Property, out var mappedAttribute)
                    && render.Root.Attributes.TryGetValue(mappedAttribute, out var rendered)
                    && !(decimal.TryParse(rendered, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                        && parsed == Convert.ToDecimal(sample.Value, CultureInfo.InvariantCulture)))
                {
                    yield return $"{label}: renders {mappedAttribute}=\"{rendered}\", which does not parse back to {invariant} with the invariant culture";
                }
            }
            else if (sample.Value is DateTime date && mapped.TryGetValue(sample.Property, out var dateAttribute)
                && render.Root.Attributes.TryGetValue(dateAttribute, out var renderedDate)
                && !(DateTime.TryParse(renderedDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedDate)
                    && parsedDate.ToUniversalTime() == date))
            {
                yield return $"{label}: renders {dateAttribute}=\"{renderedDate}\", which does not parse back to {date:O} with the invariant culture";
            }
        }
    }

    // the CEM attributes of the element that are not ignored, with the wrapper parameter mapped to each (if any)
    private static IEnumerable<(string Attribute, AttributeSurface Surface, PropertyInfo? Property)> MappedAttributes(RenderedElement element)
    {
        foreach (var (attribute, surface) in element.Component.Attributes)
        {
            if (IsIgnoredAttribute(element.Config, attribute)) continue;
            yield return (attribute, surface, FindParameter(element.Renders.ComponentType, ExpectedParameterName(element.Config, attribute)));
        }
    }

    private static IEnumerable<(RenderedRoot Root, string Label)> RootsWithLabels(WrapperRenders renders)
    {
        yield return (renders.Baseline, "no parameter set");
        foreach (var render in renders.Samples)
            yield return (render.Root, render.Sample.Label);
    }

    private static HashSet<string> UnionParts(string? type)
    {
        return (type ?? string.Empty).Split('|').Select(p => p.Trim()).Where(p => p.Length > 0).ToHashSet(StringComparer.Ordinal);
    }

    private static void AddMissingReason(List<string> misses, string key)
    {
        if (!HasReason(key))
            misses.Add($"allowlist entry has no ignoreReasons (or knownDefects) entry '{key}'");
    }

    #endregion
}
