using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAwesome.Blazor.Tests.ApiParity;

#nullable disable

/// <summary>
/// Deserialized form of expected-api-surface.json, produced by tools\upgrade\Export-WaApiSurface.ps1
/// from the Web Awesome Custom Elements Manifest.
/// </summary>
public class ApiSurface
{
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Event names that have an event class in the release's dist\events (the GlobalEventHandlersEventMap
    /// entries of the files events.d.ts re-exports); null when the export had no dist\events.
    /// </summary>
    [JsonPropertyName("declaredEventTypes")]
    public List<string> DeclaredEventTypes { get; set; }

    [JsonPropertyName("components")]
    public Dictionary<string, ComponentSurface> Components { get; set; }
}

/// <summary>
/// Expected API surface of a single Web Awesome custom element.
/// </summary>
public class ComponentSurface
{
    [JsonPropertyName("className")]
    public string ClassName { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, AttributeSurface> Attributes { get; set; }

    [JsonPropertyName("events")]
    public Dictionary<string, EventSurface> Events { get; set; }

    /// <summary>
    /// Event names the component's own @event JSDoc declares (its dist\components .d.ts); null when the export
    /// found no .d.ts for the component.
    /// </summary>
    [JsonPropertyName("jsDocEvents")]
    public List<string> JsDocEvents { get; set; }

    [JsonPropertyName("slots")]
    public Dictionary<string, string> Slots { get; set; }

    [JsonPropertyName("methods")]
    public Dictionary<string, MethodSurface> Methods { get; set; }
}

/// <summary>
/// Expected attribute of a Web Awesome custom element.
/// </summary>
public class AttributeSurface
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("default")]
    public string Default { get; set; }

    /// <summary>
    /// The type with its type aliases replaced by their unions (e.g. "IconCanvas | undefined" to
    /// "'fixed' | 'auto' | 'square' | 'roomy' | undefined"), resolved by the export from the release's .d.ts files
    /// and tools\upgrade\external-type-aliases.json; null when the type references no resolvable alias.
    /// </summary>
    [JsonPropertyName("resolvedType")]
    public string ResolvedType { get; set; }

    /// <summary>
    /// The alias-resolved type when there is one, otherwise the CEM type.
    /// </summary>
    [JsonIgnore]
    public string EffectiveType => ResolvedType ?? Type;
}

/// <summary>
/// Expected named event of a Web Awesome custom element.
/// </summary>
public class EventSurface
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}

/// <summary>
/// Expected documented public method of a Web Awesome custom element.
/// </summary>
public class MethodSurface
{
    [JsonPropertyName("signature")]
    public string Signature { get; set; }
}

/// <summary>
/// Deserialized form of parity-config.json: activation switch plus documented naming
/// deviations and intentional omissions of the Blazor wrappers.
/// </summary>
public class ParityConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("targetWaVersion")]
    public string TargetWaVersion { get; set; }

    [JsonPropertyName("globalIgnoredAttributes")]
    public List<string> GlobalIgnoredAttributes { get; set; } = new();

    [JsonPropertyName("ignoredComponents")]
    public List<string> IgnoredComponents { get; set; } = new();

    [JsonPropertyName("componentClassOverrides")]
    public Dictionary<string, string> ComponentClassOverrides { get; set; } = new();

    [JsonPropertyName("nativeElementMethods")]
    public List<string> NativeElementMethods { get; set; } = new();

    /// <summary>
    /// Native DOM events a wrapper may bind on any element although the element's CEM entry does not declare
    /// them, mapped to the name of the EventCallback that must carry them (e.g. "keydown" -> "OnKeyDown"). Every
    /// entry needs an ignoreReasons entry keyed "nativeDomEvents:&lt;event&gt;"; entries no wrapper needs fail.
    /// </summary>
    [JsonPropertyName("nativeDomEvents")]
    public Dictionary<string, string> NativeDomEvents { get; set; } = new();

    [JsonPropertyName("components")]
    public Dictionary<string, ComponentParityConfig> Components { get; set; } = new();

    /// <summary>
    /// Literals of CEM string-literal unions that deliberately no member of the named enum emits, keyed by enum
    /// type name (e.g. the long size spellings Web Awesome deprecates), applied only to the attributes the entry
    /// lists ("&lt;tag&gt;:&lt;attribute&gt;"), each of which must still need every literal. Every entry needs an
    /// ignoreReasons entry keyed "unreachableEnumUnionValues:&lt;enum&gt;".
    /// </summary>
    [JsonPropertyName("unreachableEnumUnionValues")]
    public Dictionary<string, EnumUnreachableValues> UnreachableEnumUnionValues { get; set; } = new();

    /// <summary>
    /// Rationale of each documented omission or deviation, keyed by the ignored name.
    /// </summary>
    [JsonPropertyName("ignoreReasons")]
    public Dictionary<string, string> IgnoreReasons { get; set; } = new();
}

/// <summary>
/// Union literals no member of one enum emits, and the attributes bound to that enum the exemption applies to.
/// </summary>
public class EnumUnreachableValues
{
    /// <summary>
    /// The deliberately unreachable union literals.
    /// </summary>
    [JsonPropertyName("literals")]
    public List<string> Literals { get; set; } = new();

    /// <summary>
    /// The exempted attributes, as "&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [JsonPropertyName("attributes")]
    public List<string> Attributes { get; set; } = new();
}

/// <summary>
/// Per-component parity overrides and suppressions.
/// </summary>
public class ComponentParityConfig
{
    [JsonPropertyName("attributeOverrides")]
    public Dictionary<string, string> AttributeOverrides { get; set; } = new();

    [JsonPropertyName("ignoredAttributes")]
    public List<string> IgnoredAttributes { get; set; } = new();

    [JsonPropertyName("eventOverrides")]
    public Dictionary<string, string> EventOverrides { get; set; } = new();

    [JsonPropertyName("ignoredEvents")]
    public List<string> IgnoredEvents { get; set; } = new();

    [JsonPropertyName("methodOverrides")]
    public Dictionary<string, string> MethodOverrides { get; set; } = new();

    [JsonPropertyName("ignoredMethods")]
    public List<string> IgnoredMethods { get; set; } = new();

    [JsonPropertyName("extraElementMethods")]
    public List<string> ExtraElementMethods { get; set; } = new();

    /// <summary>
    /// Enum members whose ToHtmlValue() output deliberately falls outside the attribute's
    /// CEM string-literal union, keyed by CEM attribute name; the member "*" exempts the whole
    /// attribute (e.g. an enum without a ToHtmlValue mapping). Every attribute entry needs an
    /// ignoreReasons entry keyed "ignoredEnumValues:&lt;tag&gt;:&lt;attribute&gt;" covering its members.
    /// </summary>
    [JsonPropertyName("ignoredEnumValues")]
    public Dictionary<string, List<string>> IgnoredEnumValues { get; set; } = new();

    /// <summary>
    /// Literals of a CEM string-literal union attribute that deliberately no member of the mapped enum parameter
    /// emits (e.g. the default, reached by leaving a nullable parameter unset), keyed by CEM attribute name. Every
    /// attribute entry needs an ignoreReasons entry keyed "unreachableUnionValues:&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [JsonPropertyName("unreachableUnionValues")]
    public Dictionary<string, List<string>> UnreachableUnionValues { get; set; } = new();

    /// <summary>
    /// CEM attributes typed as a string-literal union that are deliberately exposed as a bool/bool? parameter.
    /// Every entry needs an ignoreReasons entry keyed "ignoredBoolUnionAttributes:&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [JsonPropertyName("ignoredBoolUnionAttributes")]
    public List<string> IgnoredBoolUnionAttributes { get; set; } = new();

    /// <summary>
    /// CEM attributes typed as a plain string that Web Awesome reads as a space-separated list of tokens, mapped to
    /// the valid tokens (e.g. wa-tooltip "trigger" to click, hover, focus, manual); the enum checks treat the tokens
    /// as the attribute's union and split every emitted value. Every entry needs an ignoreReasons entry keyed
    /// "tokenListAttributes:&lt;tag&gt;:&lt;attribute&gt;" citing the source of the tokens.
    /// </summary>
    [JsonPropertyName("tokenListAttributes")]
    public Dictionary<string, List<string>> TokenListAttributes { get; set; } = new();

    /// <summary>
    /// CEM attributes bound to an enum parameter although their type resolves to no string-literal union (e.g. a
    /// free-form string for which the enum offers a subset of valid values), so the enum's values cannot be
    /// checked against the CEM. Every entry needs an ignoreReasons entry keyed
    /// "unresolvedEnumAttributes:&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [JsonPropertyName("unresolvedEnumAttributes")]
    public List<string> UnresolvedEnumAttributes { get; set; } = new();

    /// <summary>
    /// Attributes a wrapper deliberately renders on the element although neither the element's CEM entry nor the
    /// HTML global attributes declare them. Every entry needs an ignoreReasons entry keyed
    /// "extraRenderedAttributes:&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [JsonPropertyName("extraRenderedAttributes")]
    public List<string> ExtraRenderedAttributes { get; set; } = new();

    /// <summary>
    /// CEM attributes the render check cannot see rendered from their parameter (e.g. one emitted only together
    /// with another parameter, or a parameter type it does not sample). Every entry needs an ignoreReasons entry
    /// keyed "unrenderedAttributes:&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [JsonPropertyName("unrenderedAttributes")]
    public List<string> UnrenderedAttributes { get; set; } = new();

    /// <summary>
    /// CEM attributes the wrapper renders only while other parameters have given values (e.g. arrow-padding only
    /// with Arrow = true), mapped to those parameters and values; the render check sets them together with the
    /// attribute's own parameter. Every entry needs an ignoreReasons entry keyed
    /// "attributePrerequisites:&lt;tag&gt;:&lt;attribute&gt;".
    /// </summary>
    [JsonPropertyName("attributePrerequisites")]
    public Dictionary<string, Dictionary<string, JsonElement>> AttributePrerequisites { get; set; } = new();

    /// <summary>
    /// Boolean CEM attributes whose Web Awesome converter reads the literal "true"/"false" although the CEM default
    /// is not true (e.g. wa-combobox spellcheck), so false must render exactly "false". Every entry needs an
    /// ignoreReasons entry keyed "trueFalseAttributes:&lt;tag&gt;:&lt;attribute&gt;" citing the converter.
    /// </summary>
    [JsonPropertyName("trueFalseAttributes")]
    public List<string> TrueFalseAttributes { get; set; } = new();

    /// <summary>
    /// wa-* events the wrapper deliberately binds although the element's CEM entry does not declare them. Every
    /// entry needs an ignoreReasons entry keyed "undeclaredBoundEvents:&lt;tag&gt;:&lt;event&gt;".
    /// </summary>
    [JsonPropertyName("undeclaredBoundEvents")]
    public List<string> UndeclaredBoundEvents { get; set; } = new();

    /// <summary>
    /// EventCallbacks the wrapper raises from its handler of another event instead of binding an event of their
    /// own, mapped to that event (e.g. "OnCheckedChange" -> "change"). Every entry needs an ignoreReasons entry
    /// keyed "derivedEventCallbacks:&lt;tag&gt;:&lt;callback&gt;".
    /// </summary>
    [JsonPropertyName("derivedEventCallbacks")]
    public Dictionary<string, string> DerivedEventCallbacks { get; set; } = new();

    /// <summary>
    /// EventCallbacks the wrapper inherits from a shared base class but deliberately leaves unbound, because the
    /// element dispatches no matching event. Every entry needs an ignoreReasons entry keyed
    /// "unboundEventCallbacks:&lt;tag&gt;:&lt;callback&gt;".
    /// </summary>
    [JsonPropertyName("unboundEventCallbacks")]
    public List<string> UnboundEventCallbacks { get; set; } = new();

    /// <summary>
    /// CEM events that neither the component's @event JSDoc nor dist\events declare, i.e. manifest artifacts the
    /// element never dispatches; the event-binding checks do not rely on them. Every entry needs an ignoreReasons
    /// entry keyed "cemOnlyEvents:&lt;tag&gt;:&lt;event&gt;".
    /// </summary>
    [JsonPropertyName("cemOnlyEvents")]
    public List<string> CemOnlyEvents { get; set; } = new();

    /// <summary>
    /// CEM events the component's @event JSDoc omits but its compiled source was verified to dispatch; the
    /// event-binding checks rely on them, and they must be re-verified against the source on every upgrade.
    /// Every entry needs an ignoreReasons entry keyed "sourceVerifiedEvents:&lt;tag&gt;:&lt;event&gt;".
    /// </summary>
    [JsonPropertyName("sourceVerifiedEvents")]
    public List<string> SourceVerifiedEvents { get; set; } = new();
}

#nullable restore
