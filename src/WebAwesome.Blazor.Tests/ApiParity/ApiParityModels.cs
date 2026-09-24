using System.Collections.Generic;
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

    [JsonPropertyName("components")]
    public Dictionary<string, ComponentParityConfig> Components { get; set; } = new();

    /// <summary>
    /// Literals of CEM string-literal unions that deliberately no member of the named enum emits, keyed by enum
    /// type name (e.g. the long size spellings Web Awesome deprecates), applied to every attribute bound to that
    /// enum. Every entry needs an ignoreReasons entry keyed "unreachableEnumUnionValues:&lt;enum&gt;".
    /// </summary>
    [JsonPropertyName("unreachableEnumUnionValues")]
    public Dictionary<string, List<string>> UnreachableEnumUnionValues { get; set; } = new();

    /// <summary>
    /// Rationale of each documented omission or deviation, keyed by the ignored name.
    /// </summary>
    [JsonPropertyName("ignoreReasons")]
    public Dictionary<string, string> IgnoreReasons { get; set; } = new();
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
    /// wa-* events the wrapper deliberately binds although the element's CEM entry does not declare them. Every
    /// entry needs an ignoreReasons entry keyed "undeclaredBoundEvents:&lt;tag&gt;:&lt;event&gt;".
    /// </summary>
    [JsonPropertyName("undeclaredBoundEvents")]
    public List<string> UndeclaredBoundEvents { get; set; } = new();
}

#nullable restore
