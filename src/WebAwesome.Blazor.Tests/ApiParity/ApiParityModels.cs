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
}

#nullable restore
