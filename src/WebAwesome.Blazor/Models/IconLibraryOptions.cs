namespace WebAwesome.Blazor.Models;

/// <summary>
/// Configuration options for registering an icon library
/// </summary>
public class IconLibraryOptions
{
    /// <summary>
    /// URL template for resolving icon URLs. Use {name}, {family}, and {variant} as placeholders.
    /// Example: "https://cdn.jsdelivr.net/npm/heroicons@2.0.18/24/{variant}/{name}.svg"
    /// </summary>
    public string? Resolver { get; set; }

    /// <summary>
    /// JavaScript function name for mutating the SVG before rendering.
    /// The function should accept an SVGElement parameter.
    /// </summary>
    public string? Mutator { get; set; }

    /// <summary>
    /// Whether this library uses sprite sheets instead of individual SVG files
    /// </summary>
    public bool SpriteSheet { get; set; }
}