using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebAwesome.Blazor.Demo.Services;

#nullable disable

/// <summary>
/// API surface document generated from the Web Awesome Custom Elements Manifest
/// (tools\upgrade\Export-WaApiSurface.ps1), served as a static asset.
/// </summary>
public class ApiSurfaceDocument
{
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("components")]
    public Dictionary<string, ComponentSurface> Components { get; set; }
}

/// <summary>
/// API surface of a single Web Awesome custom element.
/// </summary>
public class ComponentSurface
{
    [JsonPropertyName("className")]
    public string ClassName { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, MemberDetail> Attributes { get; set; }

    [JsonPropertyName("events")]
    public Dictionary<string, MemberDetail> Events { get; set; }

    [JsonPropertyName("slots")]
    public Dictionary<string, string> Slots { get; set; }

    [JsonPropertyName("methods")]
    public Dictionary<string, MemberDetail> Methods { get; set; }

    [JsonPropertyName("cssParts")]
    public List<string> CssParts { get; set; }
}

/// <summary>
/// Detail of an attribute, event, or method in the API surface.
/// </summary>
public class MemberDetail
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("default")]
    public string Default { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }
}

#nullable restore

/// <summary>
/// Loads and caches the API surface document used by the ApiTable component and the navigation.
/// </summary>
public class ApiSurfaceService
{
    /// <summary>
    /// Returns the API surface document, loading it on first use.
    /// </summary>
    /// <returns>The cached API surface document</returns>
    public async Task<ApiSurfaceDocument> GetSurfaceAsync()
    {
        surface ??= await http.GetFromJsonAsync<ApiSurfaceDocument>("data/api-surface.json")
            ?? new ApiSurfaceDocument();
        return surface;
    }

    #region ------ Constructors ------

    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <param name="http">HTTP client with the application base address</param>
    public ApiSurfaceService(HttpClient http)
    {
        this.http = http;
    }

    #endregion

    #region ------ Internals ------

    private readonly HttpClient http;
    private ApiSurfaceDocument? surface;

    #endregion
}
