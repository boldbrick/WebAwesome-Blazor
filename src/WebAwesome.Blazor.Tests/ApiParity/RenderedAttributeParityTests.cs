using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Render-based attribute parity: every wrapper component is rendered with bUnit (RenderedWrapperCatalog), once
/// plain and once per sample value of each [Parameter] - every member (and [Flags] combination) of an enum, true
/// and false for a bool, a marker string, a fractional and a negative number, a DateTime, a DateOnly, a TimeOnly with
/// and without seconds, a DateTimeOffset with an offset and in UTC, a complete and a half-filled WaDateRange, and a
/// non-empty (listed out of order) and an empty IReadOnlySet of DateOnly or DayOfWeek - set on its own, and the
/// attributes of the root element are compared with the Custom Elements Manifest of the tag the wrapper actually
/// renders (WaRange renders wa-slider). The renders run under a hostile culture (cs-CZ with its decimal comma, plus
/// the U+2212 minus sign and a '.' time separator some cultures use), because Blazor formats every non-string
/// attribute value with the current culture. Checked: (a) every rendered attribute is a CEM attribute of the tag,
/// an HTML global attribute, or allowlisted ("extraRenderedAttributes"); (b) every rendered value of an attribute
/// typed as a string-literal union (alias-resolved, or a "tokenListAttributes" token list) is in the union; (c) a
/// bool parameter set to false renders no attribute, or exactly "false" where the CEM default is true, true renders
/// the attribute, and no value is ever "True"/"False"; (d) every CEM attribute has a parameter that renders it when
/// set to a non-default value (allowlist "unrenderedAttributes"); (e) numbers and dates render in the invariant
/// culture; (f) with no parameter set, every rendered CEM attribute carries the element's own default (allowlist
/// "wrapperDefaultAttributes"), so a wrong wrapper default shows; (g) every parameter's C# default is the element's CEM default,
/// or null (allowlist "divergentParameterDefaults"); (h) every date, time, instant, range and set renders in the form the
/// element parses, checked with Web Awesome's own patterns (WaWirePatterns), and an empty set renders nothing. Parameters of
/// other types (other collections, objects, fragments, callbacks) are not sampled. Every allowlist
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
    /// (f) With no parameter set, a wrapper renders no CEM attribute other than with the element's own default: an
    /// attribute without a CEM default is not rendered, a boolean attribute defaulting to false is absent, and any
    /// other rendered value equals the CEM default (allowlist "wrapperDefaultAttributes"). This catches a wrong
    /// wrapper default (an initializer that turns on "open", flips a placement or changes a number) that the
    /// per-parameter samples of (c) and (d) cannot see, because they only compare against the baseline.
    /// </summary>
    [Fact]
    public void BaselineRender_EmitsOnlyElementDefaults()
    {
        SkipUnlessParityEnabled();

        var misses = RenderedElements()
            .SelectMany(e => BaselineDefaultMisses(e).Where(m => !e.Config.WrapperDefaultAttributes.Contains(m.Attribute)).Select(m => m.Miss))
            .ToList();

        AssertNoMisses(misses, "Attributes rendered without a parameter set whose value is not the element default");
    }

    /// <summary>
    /// (g) The C# default of every [Parameter] mapped to a CEM attribute is the element's own default, because the
    /// property's value is visible to consumers even where it is not rendered: a non-null default must equal the CEM
    /// default (a bool compares as true/false, an enum by its ToHtmlValue(), a number numerically, a string without
    /// its quotes), and where the element has no default only false, an empty string or null qualifies. A null
    /// default is always fine (nothing is rendered, see (f)). Allowlist "divergentParameterDefaults" for defaults
    /// the CEM cannot express.
    /// </summary>
    [Fact]
    public void ParameterDefaults_MatchTheElementDefaults()
    {
        SkipUnlessParityEnabled();

        var misses = RenderedElements()
            .SelectMany(e => ParameterDefaultMisses(e).Where(m => !e.Config.DivergentParameterDefaults.Contains(m.Attribute)).Select(m => m.Miss))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        AssertNoMisses(misses, "Parameter defaults that differ from the element default");
    }

    /// <summary>
    /// (h) Every date, time, instant, date-range and set parameter renders its attribute in the form Web Awesome
    /// parses, checked with the element's own patterns (WaWirePatterns, copied from the 3.12.0 sources): a date
    /// matches ISO_DATE and reads back as the sample, a time matches the time-input wire pattern, an instant is an
    /// ECMAScript date-time with its offset and reads back as the same instant, a range is one or two ISO dates split
    /// on '/', a date set is ascending ISO dates and a weekday set sun..sat tokens (split on whitespace) in DayOfWeek
    /// order, and an empty set renders no attribute at all (owner rule). Rendered under the hostile culture, like
    /// every sample, so a culture-formatted value fails here too.
    /// </summary>
    [Fact]
    public void DateAndTimeAttributes_MatchWebAwesomeParsePatterns()
    {
        SkipUnlessParityEnabled();

        var checkedKinds = new HashSet<string>(StringComparer.Ordinal);
        var misses = RenderedElements().SelectMany(e => WireFormatMisses(e, checkedKinds)).Distinct(StringComparer.Ordinal).ToList();

        // guard the check itself: every kind of value must have been rendered and checked at least once
        foreach (var kind in WireFormatKinds.Where(k => !checkedKinds.Contains(k)))
            misses.Add($"no {kind} sample was rendered into a mapped attribute, so its wire format went unchecked");

        // and every cited list separator must still belong to a list parameter the check reads
        foreach (var key in WaWirePatterns.ListSeparators.Keys.Where(k => !checkedKinds.Contains(ListKeyPrefix + k)))
            misses.Add($"WaWirePatterns.ListSeparators entry '{key}' belongs to no rendered list parameter and must be removed");

        AssertNoMisses(misses, "Date and time attributes not in the form Web Awesome parses");
    }

    /// <summary>
    /// (i) Sticky attributes (owner rule): once a wrapper has rendered an attribute that has an element default, a
    /// later render never removes it. Each parameter is rendered with a sample that renders its attribute, then the
    /// same instance is re-rendered with the parameter back at its C# default (null for a nullable parameter), and the
    /// attribute must still be there, holding the element default (the CEM literal default, which the library's
    /// WaElementDefaults table must match). Removing it would make Lit set the property to null, not to its default
    /// (a slider's max 0, a popup's placement null). Exempt by rule: bool parameters (a removed Lit boolean attribute
    /// reads false, the default), attributes without a literal CEM default (removal restores the unset state), and
    /// the bound value of an InputBase control (its live-value sync assigns the property).
    /// </summary>
    [Fact]
    public void RenderedAttributes_FallBackToTheElementDefault_InsteadOfBeingRemoved()
    {
        SkipUnlessParityEnabled();

        var checkedCount = 0;
        var misses = new List<string>();
        foreach (var element in RenderedElements())
            misses.AddRange(StickyMisses(element, ref checkedCount));

        // guard the check itself: it must have re-rendered parameters at all
        if (checkedCount == 0) misses.Add("no parameter was re-rendered at its default, so the sticky rule went unchecked");
        TestContext.Current.TestOutputHelper?.WriteLine($"{checkedCount} parameters re-rendered at their default");

        AssertNoMisses(misses.Distinct(StringComparer.Ordinal).ToList(), "Attributes removed (or not reset to the element default) after they were rendered");
    }

    /// <summary>
    /// Every "extraRenderedAttributes", "unrenderedAttributes", "attributePrerequisites", "trueFalseAttributes",
    /// "wrapperDefaultAttributes" and "divergentParameterDefaults" entry must carry a rationale of its own in ignoreReasons (or knownDefects), keyed
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

            foreach (var attribute in componentConfig.OnOffAttributes)
                AddMissingReason(misses, $"{OnOffAttributesKey}:{tag}:{attribute}");

            foreach (var attribute in componentConfig.WrapperDefaultAttributes)
                AddMissingReason(misses, $"{WrapperDefaultAttributesKey}:{tag}:{attribute}");

            foreach (var attribute in componentConfig.DivergentParameterDefaults)
                AddMissingReason(misses, $"{DivergentParameterDefaultsKey}:{tag}:{attribute}");

            foreach (var attribute in componentConfig.AdditionalAttributeParameters.Keys)
                AddMissingReason(misses, $"{AdditionalAttributeParametersKey}:{tag}:{attribute}");
        }

        AssertNoMisses(misses, "Rendered-attribute allowlist entries without a reason");
    }

    /// <summary>
    /// Every "extraRenderedAttributes" entry must still be rendered by a wrapper of the element and still be
    /// undeclared; every "unrenderedAttributes" entry must still be a CEM attribute some wrapper does not render;
    /// every "attributePrerequisites" entry must name a CEM attribute and parameters a wrapper of the element has;
    /// every "trueFalseAttributes" entry must name a boolean CEM attribute not defaulting to true whose parameter
    /// renders "false"; every "wrapperDefaultAttributes" entry must still be rendered without parameters with a
    /// value other than the element default.
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

            foreach (var attribute in componentConfig.OnOffAttributes)
            {
                var needed = ofTag.Any(e => e.Component.Attributes.TryGetValue(attribute, out var surface)
                    && UnionParts(surface.EffectiveType).Contains(BooleanType)
                    && e.Renders.Samples.Any(s => s.Sample.Value is true
                        && s.Sample.Property.Name == ExpectedParameterName(componentConfig, attribute)
                        && s.Root.Attributes.TryGetValue(attribute, out var value) && value == OnText));
                if (!needed)
                    misses.Add($"{tag}: onOffAttributes entry '{attribute}' is not a boolean attribute whose parameter renders \"{OnText}\" and must be removed");
            }

            foreach (var attribute in componentConfig.WrapperDefaultAttributes)
            {
                if (!ofTag.Any(e => BaselineDefaultMisses(e).Any(m => m.Attribute == attribute)))
                    misses.Add($"{tag}: wrapperDefaultAttributes entry '{attribute}' is not rendered with a non-default value without parameters and must be removed");
            }

            foreach (var (attribute, parameters) in componentConfig.AdditionalAttributeParameters)
            {
                var valid = parameters.Count > 0 && ofTag.Any(e => e.Component.Attributes.ContainsKey(attribute)
                    && parameters.All(p => FindParameter(e.Renders.ComponentType, p) != null));
                if (!valid)
                    misses.Add($"{tag}: additionalAttributeParameters entry '{attribute}' names no CEM attribute, or a parameter no wrapper of the element has, and must be removed");
            }

            foreach (var attribute in componentConfig.DivergentParameterDefaults)
            {
                if (!ofTag.Any(e => ParameterDefaultMisses(e).Any(m => m.Attribute == attribute)))
                    misses.Add($"{tag}: divergentParameterDefaults entry '{attribute}' names no parameter whose default differs from the element default and must be removed");
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
    private const string WrapperDefaultAttributesKey = "wrapperDefaultAttributes";
    private const string DivergentParameterDefaultsKey = "divergentParameterDefaults";
    private const string AdditionalAttributeParametersKey = "additionalAttributeParameters";
    private const string StringSample = "x-sample-value";
    private const string IsoDatePattern = "yyyy-MM-dd";
    private const string DateKind = "DateOnly";
    private const string TimeKind = "TimeOnly";
    private const string InstantKind = "DateTimeOffset";
    private const string RangeKind = "WaDateRange";
    private const string DateSetKind = "IReadOnlySet<DateOnly>";
    private const string WeekdaySetKind = "IReadOnlySet<DayOfWeek>";

    private static readonly string[] WireFormatKinds = [DateKind, TimeKind, InstantKind, RangeKind, DateSetKind, WeekdaySetKind, StepKind, LocalDateTimeKind, NumberListKind, StringListKind, StringSetKind, PlacementListKind];
    private const string BooleanType = "boolean";
    private const string NumberType = "number";
    private const string StringType = "string";
    private const string TrueDefault = "true";
    private const string TrueText = "true";
    private const string FalseText = "false";
    private const string OnText = "on";
    private const string OffText = "off";
    private const string OnOffAttributesKey = "onOffAttributes";
    private const string AnyStepText = "any";
    private const string AnyStepLiteral = "'any'";
    private const string StepKind = "WaStep";
    private const string LocalDateTimeKind = "DateTime";
    private const string NumberListKind = "IReadOnlyList<double>";
    private const string StringListKind = "IReadOnlyList<string>";
    private const string StringSetKind = "IReadOnlySet<string>";
    private const string PlacementListKind = "IReadOnlyList<WaPlacement>";
    private const string ListKeyPrefix = "list:";
    private const string DefaultConstantPrefix = "Default";
    private const string ListItemSampleA = "x-item-a";
    private const string ListItemSampleB = "x-item-b";
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

    // CEM defaults meaning the element leaves the attribute unset
    private static readonly HashSet<string> NoDefaultTexts = new(StringComparer.Ordinal) { "null", "undefined" };

    // quote characters around a CEM string default ('text')
    private static readonly char[] DefaultQuotes = { '\'', '"', '`' };

    private static readonly DateTime DateSample = new(2026, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc);
    private static readonly DateOnly DateOnlySample = new(2026, 1, 2);
    private static readonly DateOnly RangeEndSample = new(2026, 11, 30);
    private static readonly TimeOnly TimeWithSecondsSample = new(3, 4, 5);
    private static readonly TimeOnly TimeSample = new(13, 4);
    private static readonly DateTimeOffset InstantSample = new(2026, 1, 2, 3, 4, 5, 678, TimeSpan.FromHours(-5));
    private static readonly DateTimeOffset UtcInstantSample = new(2026, 1, 2, 3, 4, 5, 678, TimeSpan.Zero);

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
        else if (type == typeof(DateOnly))
        {
            yield return DateOnlySample;
        }
        else if (type == typeof(TimeOnly))
        {
            yield return TimeWithSecondsSample;
            yield return TimeSample;
        }
        else if (type == typeof(DateTimeOffset))
        {
            yield return InstantSample;
            yield return UtcInstantSample;
        }
        else if (type == typeof(WaDateRange))
        {
            yield return new WaDateRange(DateOnlySample, RangeEndSample);
            yield return new WaDateRange(DateOnlySample, null);
        }
        else if (type == typeof(IReadOnlySet<DateOnly>))
        {
            // listed out of order, so an unsorted rendering shows
            yield return new HashSet<DateOnly> { RangeEndSample, DateOnlySample };
            yield return new HashSet<DateOnly>();
        }
        else if (type == typeof(IReadOnlySet<DayOfWeek>))
        {
            yield return new HashSet<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };
            yield return new HashSet<DayOfWeek>();
        }
        else if (type == typeof(WaStep))
        {
            yield return (WaStep)(decimal)FractionalSample;
            yield return WaStep.Any;
        }
        else if (type == typeof(IReadOnlyList<double>))
        {
            // descending and fractional, so a sorted or culture-formatted rendering shows
            yield return new[] { 0.75, 0.25, 1 };
            yield return Array.Empty<double>();
        }
        else if (type == typeof(IReadOnlyList<string>))
        {
            yield return new[] { ListItemSampleB, ListItemSampleA };
            yield return Array.Empty<string>();
        }
        else if (type == typeof(IReadOnlySet<string>))
        {
            yield return new HashSet<string>(StringComparer.Ordinal) { ListItemSampleB, ListItemSampleA };
            yield return new HashSet<string>(StringComparer.Ordinal);
        }
        else if (type == typeof(IReadOnlyList<WaPlacement>))
        {
            yield return new[] { WaPlacement.RightEnd, WaPlacement.Bottom };
            yield return Array.Empty<WaPlacement>();
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
        DateOnly date => date.ToString(IsoDatePattern, CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        System.Collections.IEnumerable items => $"{{{string.Join(", ", items.Cast<object>().Select(Describe))}}}",
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

                // an empty token list holds no token outside the union (an empty sandbox is the strictest one)
                var values = EnumValueParityTests.ValuesOf(value, isTokenList);
                if ((values.Count > 0 || isTokenList) && values.All(v => union.Contains(v, StringComparer.Ordinal))) continue;
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
            var isOnOff = element.Config.OnOffAttributes.Contains(attribute);

            foreach (var render in element.Renders.Samples.Where(s => s.Sample.Property == property && s.Root.Error == null))
            {
                var present = render.Root.Attributes.TryGetValue(attribute, out var value);
                var label = $"{element.Tag} ({element.Name}.{render.Sample.Label})";

                if (isOnOff)
                {
                    // the converter reads only "off" (and absence or an empty value) as false, anything else as true
                    var expected = (bool)render.Sample.Value ? OnText : OffText;
                    if (value != expected)
                        yield return $"{label}: '{attribute}' reads \"{OnText}\"/\"{OffText}\" (onOffAttributes), so it must render {attribute}=\"{expected}\", but it renders {(present ? $"\"{value}\"" : "nothing")}";
                }
                else if ((bool)render.Sample.Value)
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

                // a string parameter passes its own value through, and a number | 'any' step may be "any"
                if (value.Contains(StringSample, StringComparison.Ordinal)) continue;
                if (value == AnyStepText && surface.EffectiveType.Contains(AnyStepLiteral, StringComparison.Ordinal)) continue;
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
                && !(DateTime.TryParse(renderedDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                    && parsedDate.Ticks == date.Ticks))
            {
                yield return $"{label}: renders {dateAttribute}=\"{renderedDate}\", which does not parse back to {date:O} with the invariant culture";
            }
        }
    }

    // the (h) misses of one element; records in checkedKinds which kinds of value it checked
    private static IEnumerable<string> WireFormatMisses(RenderedElement element, HashSet<string> checkedKinds)
    {
        var mapped = MappedAttributes(element).Where(m => m.Property != null).ToDictionary(m => m.Property!, m => m.Attribute);

        foreach (var render in element.Renders.Samples.Where(s => s.Root.Error == null))
        {
            var sample = render.Sample;
            if (!mapped.TryGetValue(sample.Property, out var attribute)) continue;

            var present = render.Root.Attributes.TryGetValue(attribute, out var value);
            var label = $"{element.Tag} ({element.Name}.{sample.Label})";
            var rendered = present ? $"{attribute}=\"{value}\"" : $"no '{attribute}'";

            string? miss;
            switch (sample.Value)
            {
                case DateOnly date:
                    checkedKinds.Add(DateKind);
                    miss = present && ReadsAsDate(value!) == date ? null : $"renders {rendered}, expected ISO_DATE reading as {Describe(date)}";
                    break;
                case TimeOnly time:
                    checkedKinds.Add(TimeKind);
                    miss = present && ReadsAsTime(value!, time) ? null : $"renders {rendered}, expected the time-input wire pattern reading as {time:HH:mm:ss}";
                    break;
                case DateTimeOffset instant:
                    checkedKinds.Add(InstantKind);
                    miss = present && WaWirePatterns.EcmaScriptInstant.IsMatch(value!)
                        && DateTimeOffset.Parse(value!, CultureInfo.InvariantCulture) is var parsed
                        && parsed == instant && parsed.Offset == instant.Offset
                        ? null
                        : $"renders {rendered}, expected an ECMAScript date-time with offset reading as {instant:O}";
                    break;
                case WaDateRange range:
                    checkedKinds.Add(RangeKind);
                    miss = present && ReadsAsRange(value!, range) ? null : $"renders {rendered}, expected one or two ISO dates split on '{WaWirePatterns.RangeSeparator}' reading as {range}";
                    break;
                case IReadOnlySet<DateOnly> dates:
                    checkedKinds.Add(DateSetKind);
                    miss = SetMiss(present, value, dates.Order().ToList(), ReadsAsDate, rendered);
                    break;
                case IReadOnlySet<DayOfWeek> days:
                    checkedKinds.Add(WeekdaySetKind);
                    miss = SetMiss(present, value, days.Order().ToList(), ReadsAsWeekday, rendered);
                    break;
                case IReadOnlyList<double> numbers:
                    checkedKinds.Add(NumberListKind);
                    checkedKinds.Add(ListKeyPrefix + EnumValueParityTests.AttributeKey(element.Tag, attribute));
                    miss = ListMiss(element.Tag, attribute, present, value, numbers.Select(n => n.ToString(CultureInfo.InvariantCulture)).ToList(),
                        (item, expected) => double.TryParse(item, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                            && parsed == double.Parse(expected, CultureInfo.InvariantCulture), rendered);
                    break;
                case IReadOnlySet<string> tokenSet:
                    checkedKinds.Add(StringSetKind);
                    checkedKinds.Add(ListKeyPrefix + EnumValueParityTests.AttributeKey(element.Tag, attribute));
                    miss = ListMiss(element.Tag, attribute, present, value, tokenSet.Order(StringComparer.Ordinal).ToList(), string.Equals, rendered);
                    break;
                case IReadOnlyList<string> tokens:
                    checkedKinds.Add(StringListKind);
                    checkedKinds.Add(ListKeyPrefix + EnumValueParityTests.AttributeKey(element.Tag, attribute));
                    miss = ListMiss(element.Tag, attribute, present, value, tokens.ToList(), string.Equals, rendered);
                    break;
                case IReadOnlyList<WaPlacement> placements:
                    checkedKinds.Add(PlacementListKind);
                    checkedKinds.Add(ListKeyPrefix + EnumValueParityTests.AttributeKey(element.Tag, attribute));
                    miss = ListMiss(element.Tag, attribute, present, value, placements.Select(p => EnumValueParityTests.HtmlValueOf(p) ?? string.Empty).ToList(),
                        string.Equals, rendered);
                    break;
                case DateTime dateTime:
                    checkedKinds.Add(LocalDateTimeKind);
                    miss = present && WaWirePatterns.LocalDateTime.IsMatch(value!)
                        && DateTime.Parse(value!, CultureInfo.InvariantCulture).Ticks == dateTime.Ticks
                        ? null
                        : $"renders {rendered}, expected a local date and time (no offset) reading as {dateTime:yyyy-MM-ddTHH:mm:ss.fff}";
                    break;
                case WaStep step:
                    checkedKinds.Add(StepKind);
                    miss = present && (step.IsAny
                            ? value == AnyStepText
                            : decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number) && number == step.Number)
                        ? null
                        : $"renders {rendered}, expected \"{AnyStepText}\" or the invariant number reading as {step}";
                    break;
                default:
                    continue;
            }

            if (miss != null) yield return $"{label}: {miss}";
        }
    }

    // null when an empty set renders nothing and a non-empty one renders exactly its items, in order, split on whitespace
    private static string? SetMiss<T>(bool present, string? value, List<T> expected, Func<string, T?> read, string rendered) where T : struct
    {
        if (expected.Count == 0)
            return present ? $"renders {rendered} for an empty set; an empty collection must render no attribute" : null;

        var items = present ? WaWirePatterns.ListSeparator.Split(value!) : [];
        var readItems = items.Select(read).ToList();
        return readItems.Count == expected.Count && readItems.Zip(expected).All(p => p.First.HasValue && EqualityComparer<T>.Default.Equals(p.First.Value, p.Second))
            ? null
            : $"renders {rendered}, expected {string.Join(" ", expected)} in that order, each in the form Web Awesome parses";
    }

    // null when an empty list renders nothing and a non-empty one renders exactly its items, in the expected order,
    // separated as the element splits the attribute (WaWirePatterns.ListSeparators)
    private static string? ListMiss(string tag, string attribute, bool present, string? value, IReadOnlyList<string> expected,
        Func<string, string, bool> itemEquals, string rendered)
    {
        if (expected.Count == 0)
            return present ? $"renders {rendered} for an empty list; an empty collection must render no attribute" : null;

        if (!WaWirePatterns.ListSeparators.TryGetValue(EnumValueParityTests.AttributeKey(tag, attribute), out var separator))
            return $"'{attribute}' is list-valued, but WaWirePatterns.ListSeparators cites no separator the element splits it on";

        var items = present ? separator.Split(value!) : [];
        return items.Length == expected.Count && items.Zip(expected).All(p => itemEquals(p.First, p.Second))
            ? null
            : $"renders {rendered}, expected the items {string.Join(", ", expected)} in that order, split on /{separator}/ as the element splits them";
    }

    // the date an ISO_DATE text stands for, or null when it does not match or is no calendar day
    private static DateOnly? ReadsAsDate(string text)
    {
        var match = WaWirePatterns.IsoDate.Match(text);
        if (!match.Success) return null;

        var (year, month, day) = (int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture), int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture));
        return month is >= 1 and <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year, month) ? new DateOnly(year, month, day) : null;
    }

    // whether a wire time reads as the sample: to the second, or to the minute when the seconds were left out
    private static bool ReadsAsTime(string text, TimeOnly expected)
    {
        var match = WaWirePatterns.WireTime.Match(text);
        if (!match.Success) return false;

        var hour = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var minute = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        if (hour != expected.Hour || minute != expected.Minute) return false;

        return !match.Groups[3].Success
            || (int)double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture) == expected.Second;
    }

    // whether a range text is its one or two ISO dates, the From first
    private static bool ReadsAsRange(string text, WaDateRange range)
    {
        var parts = text.Split(WaWirePatterns.RangeSeparator).Select(ReadsAsDate).ToList();
        var expected = new[] { range.From, range.To }.Where(d => d.HasValue).ToList();
        return parts.Count == expected.Count && parts.SequenceEqual(expected);
    }

    private static DayOfWeek? ReadsAsWeekday(string token)
    {
        var index = Array.IndexOf(WaWirePatterns.WeekdayTokens, token);
        return index >= 0 ? (DayOfWeek)index : null;
    }

    // the CEM attributes the baseline render (no parameter set) emits with a value other than the element default
    private static IEnumerable<(string Attribute, string Miss)> BaselineDefaultMisses(RenderedElement element)
    {
        var root = element.Renders.Baseline;
        if (root.Error != null || root.Tag != element.Tag) yield break;

        foreach (var (attribute, value) in root.Attributes.OrderBy(a => a.Key, StringComparer.Ordinal))
        {
            if (!element.Component.Attributes.TryGetValue(attribute, out var surface) || IsIgnoredAttribute(element.Config, attribute)) continue;

            // owner rule: an unset parameter renders nothing, not even the element default, so the attribute is
            // never rendered (and later removed) for a value the element holds anyway
            yield return (attribute, RendersElementDefault(value, surface)
                ? $"{element.Tag} ({element.Name}): renders {attribute}=\"{value}\" with no parameter set; it is the element default, which must not be rendered"
                : $"{element.Tag} ({element.Name}): renders {attribute}=\"{value}\" with no parameter set, but the element default is {surface.Default ?? "unset"}");
        }

        // a non-nullable parameter set to its own default on the first render is still unset
        var mapped = MappedAttributes(element).Where(m => m.Property != null).ToLookup(m => m.Property!, m => m.Attribute);
        var defaults = DefaultInstance(element.Renders.ComponentType);
        foreach (var render in element.Renders.Samples.Where(s => s.Root.Error == null && s.Sample.Value is not bool && s.Sample.Prerequisites.Count == 0))
        {
            var property = render.Sample.Property;
            if (defaults == null || Nullable.GetUnderlyingType(property.PropertyType) != null || !property.PropertyType.IsValueType) continue;
            if (!Equals(property.GetValue(defaults), render.Sample.Value)) continue;

            foreach (var attribute in mapped[property].Where(a => render.Root.Attributes.ContainsKey(a) && !element.Renders.Baseline.Attributes.ContainsKey(a)))
            {
                yield return (attribute, $"{element.Tag} ({element.Name}.{render.Sample.Label}): renders {attribute}=\"{render.Root.Attributes[attribute]}\" " +
                    "for the parameter's own default on the first render, which must render nothing");
            }
        }
    }

    // the (i) misses of one element; counts the parameters it re-rendered
    private static List<string> StickyMisses(RenderedElement element, ref int checkedCount)
    {
        var misses = new List<string>();
        var defaults = DefaultInstance(element.Renders.ComponentType);
        if (defaults == null || element.Renders.Baseline.Tag != element.Tag) return misses;

        foreach (var (attribute, _, property) in MappedAttributes(element))
        {
            if (property == null || EnumValueParityTests.IsBoolType(property.PropertyType) || IsBoundValue(element.Renders.ComponentType, property)) continue;

            var elementDefault = ElementDefaultsTableTests.CemDefaultOf(element.Tag, attribute);
            if (elementDefault == null) continue;

            // a sample that renders the attribute with another value than the element default
            var sample = element.Renders.Samples.FirstOrDefault(s => s.Sample.Property == property && s.Root.Error == null
                && s.Root.Attributes.TryGetValue(attribute, out var value) && !SameAttributeValue(value, elementDefault));
            if (sample == null) continue;

            var prerequisites = sample.Sample.Prerequisites.Select(p => (p.Name, (object?)p.Value)).ToList();
            var sets = new IReadOnlyList<(string Name, object? Value)>[]
            {
                prerequisites.Append((property.Name, sample.Sample.Value)).ToList(),
                prerequisites.Append((property.Name, property.GetValue(defaults))).ToList()
            };

            var after = RenderedWrapperCatalog.RenderSequence(element.Renders.ComponentType, sets, HostileCulture)[1];
            checkedCount++;
            var label = $"{element.Tag} ({element.Name}.{sample.Sample.Label}, then {property.Name} back to its default)";

            if (after.Error != null)
                misses.Add($"{label}: failed to re-render: {after.Error}");
            else if (!after.Attributes.TryGetValue(attribute, out var rendered))
                misses.Add($"{label}: removes '{attribute}', so the element property becomes null; it must render the element default \"{elementDefault}\"");
            else if (!SameAttributeValue(rendered, elementDefault))
                misses.Add($"{label}: renders {attribute}=\"{rendered}\", expected the element default \"{elementDefault}\"");
        }

        return misses;
    }

    // the bound Value of an InputBase control, which the live-value sync assigns to the element property
    private static bool IsBoundValue(Type componentType, PropertyInfo property)
        => property.Name == nameof(InputBase<object>.Value) && IsInputBase(componentType);

    private static bool IsInputBase(Type type)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(InputBase<>)) return true;
        }

        return false;
    }

    // whether two attribute texts stand for the same value: numerically for numbers, ordinally otherwise
    private static bool SameAttributeValue(string value, string other)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            && double.TryParse(other, NumberStyles.Float, CultureInfo.InvariantCulture, out var otherNumber))
        {
            return number.Equals(otherNumber);
        }

        return string.Equals(value, other, StringComparison.Ordinal);
    }

    // a component instance holding the parameters' C# defaults, or null when the type cannot be created bare
    private static object? DefaultInstance(Type componentType)
    {
        try
        {
            return Activator.CreateInstance(componentType);
        }
        catch (Exception ex) when (ex is MissingMethodException or TargetInvocationException)
        {
            return null;
        }
    }

    // the CEM attributes whose [Parameter] has a non-null C# default other than the element default
    private static IEnumerable<(string Attribute, string Miss)> ParameterDefaultMisses(RenderedElement element)
    {
        object instance;
        try
        {
            instance = Activator.CreateInstance(element.Renders.ComponentType)!;
        }
        catch (Exception ex) when (ex is MissingMethodException or TargetInvocationException)
        {
            yield break;
        }

        foreach (var (attribute, surface, property) in MappedAttributes(element).OrderBy(m => m.Attribute, StringComparer.Ordinal))
        {
            if (property == null || !property.CanRead) continue;

            var value = property.GetValue(instance);
            if (value == null) continue;

            // owner rule: a non-nullable default is declared as a named public constant and used as the default
            if (value is not bool && property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null && !IsBoundValue(element.Renders.ComponentType, property))
            {
                var constant = element.Renders.ComponentType.GetField(DefaultConstantPrefix + property.Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (constant == null || !(constant.IsLiteral || constant.IsDefined(typeof(DecimalConstantAttribute))))
                    yield return (attribute, $"{element.Tag} ({element.Name}): {property.Name} is non-nullable, but declares no public const {DefaultConstantPrefix}{property.Name} holding its default");
                else if (!Equals(constant.GetValue(null), value))
                    yield return (attribute, $"{element.Tag} ({element.Name}): {property.Name} defaults to {Describe(value)}, not to its constant {constant.Name} = {Describe(constant.GetValue(null)!)}");
            }

            var text = DefaultText(value);
            if (text == null || ParameterDefaultMatches(value, text, surface)) continue;

            yield return (attribute, $"{element.Tag} ({element.Name}): {property.Name} defaults to {Describe(value)} (\"{text}\"), " +
                $"but the element default of '{attribute}' is {surface.Default ?? "unset"}");
        }
    }

    // the attribute text a parameter value stands for, or null for a type the check cannot compare
    private static string? DefaultText(object value) => value switch
    {
        bool flag => flag ? TrueText : FalseText,
        string text => text,
        Enum member => EnumValueParityTests.HtmlValueOf(member),
        _ when IsNumber(value) => Convert.ToString(value, CultureInfo.InvariantCulture),
        _ => null
    };

    // whether a parameter's C# default equals the element default; where the element has none, only false and an
    // empty text (an empty string, or an enum member whose ToHtmlValue() is empty and so renders nothing) stand for "unset"
    private static bool ParameterDefaultMatches(object value, string text, AttributeSurface surface)
    {
        var defaultText = surface.Default?.Trim();
        if (string.IsNullOrEmpty(defaultText) || NoDefaultTexts.Contains(defaultText))
            return value is false || (value is not bool && text.Length == 0);

        if (value is bool) return text == defaultText;

        if (IsNumber(value))
            return decimal.TryParse(defaultText, NumberStyles.Float, CultureInfo.InvariantCulture, out var defaultNumber)
                && Convert.ToDecimal(value, CultureInfo.InvariantCulture) == defaultNumber;

        return text == defaultText.Trim(DefaultQuotes);
    }

    // whether a rendered value is what the element holds without the attribute: a present boolean reads true, a
    // number compares numerically, a string default compares without its quotes
    private static bool RendersElementDefault(string value, AttributeSurface surface)
    {
        var defaultText = surface.Default?.Trim();
        if (string.IsNullOrEmpty(defaultText) || NoDefaultTexts.Contains(defaultText)) return false;

        if (UnionParts(surface.EffectiveType).Contains(BooleanType))
            return defaultText == TrueDefault && (value == string.Empty || value == TrueText);

        if (decimal.TryParse(defaultText, NumberStyles.Float, CultureInfo.InvariantCulture, out var defaultNumber))
            return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) && number == defaultNumber;

        return value == defaultText.Trim(DefaultQuotes);
    }

    // the CEM attributes of the element that are not ignored, with the wrapper parameter mapped to each (if any)
    private static IEnumerable<(string Attribute, AttributeSurface Surface, PropertyInfo? Property)> MappedAttributes(RenderedElement element)
    {
        foreach (var (attribute, surface) in element.Component.Attributes)
        {
            if (IsIgnoredAttribute(element.Config, attribute)) continue;
            yield return (attribute, surface, FindParameter(element.Renders.ComponentType, ExpectedParameterName(element.Config, attribute)));

            // further parameters rendering the same attribute ("additionalAttributeParameters")
            if (!element.Config.AdditionalAttributeParameters.TryGetValue(attribute, out var others)) continue;
            foreach (var other in others)
            {
                var property = FindParameter(element.Renders.ComponentType, other);
                if (property != null) yield return (attribute, surface, property);
            }
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
