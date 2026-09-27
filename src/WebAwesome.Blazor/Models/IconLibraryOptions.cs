namespace WebAwesome.Blazor.Models;

/// <summary>
/// Configuration options for registering an icon library. Web Awesome takes JavaScript functions for the resolver
/// and the mutator; from .NET the resolver is a URL template and the mutator names a global JavaScript function.
/// </summary>
public class IconLibraryOptions
{
    /// <summary>
    /// URL template for resolving icon URLs (required). The placeholders {name}, {family} and {variant} are replaced
    /// with the icon's name, its family (or the default icon family) and its variant; an unset variant is replaced
    /// with an empty string.
    /// Example: "https://cdn.jsdelivr.net/npm/heroicons@2.0.18/24/outline/{name}.svg"
    /// </summary>
    public string? Resolver { get; set; }

    /// <summary>
    /// Dotted path of a global JavaScript function (e.g. "myApp.icons.mutate") that mutates each SVG before it is
    /// rendered. Web Awesome calls it with the SVG element and the wa-icon host element. The path is resolved when
    /// the library is registered, so the function must be defined by then.
    /// </summary>
    public string? Mutator { get; set; }

    /// <summary>
    /// Whether the resolver returns sprite sheet references (e.g. "/sprite.svg#{name}"), rendered through an SVG
    /// &lt;use&gt; element instead of being fetched
    /// </summary>
    public bool SpriteSheet { get; set; }
}
