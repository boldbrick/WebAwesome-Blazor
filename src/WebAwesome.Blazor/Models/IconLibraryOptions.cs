namespace WebAwesome.Blazor.Models;

/// <summary>
/// Configuration options for registering an icon library. Web Awesome takes JavaScript functions for the resolver
/// and the mutator. From .NET, the resolver is a URL template (<see cref="Resolver"/>) or a function reference
/// (<see cref="ResolverFunction"/>), and the mutator is a function reference (<see cref="MutatorFunction"/>).
/// </summary>
/// <remarks>
/// The library binds no icon library from configuration. A <see cref="WaJsFunction"/> has no public constructor, so
/// a configuration binder cannot create one: make the reference in code, for example from a configured name with
/// <see cref="WaJsFunction.Registered"/>, whose validation then checks the configured text.
/// </remarks>
public class IconLibraryOptions
{
    /// <summary>
    /// URL template for resolving icon URLs. The placeholders {name}, {family} and {variant} are replaced
    /// with the icon's name, its family (or the default icon family) and its variant; an unset variant is replaced
    /// with an empty string. Set this or <see cref="ResolverFunction"/>, not both.
    /// Example: "https://cdn.jsdelivr.net/npm/heroicons@2.0.18/24/outline/{name}.svg"
    /// </summary>
    public string? Resolver { get; set; }

    /// <summary>
    /// A JavaScript function that returns the URL of an icon, for a mapping that a URL template cannot express. Web
    /// Awesome calls it as <c>(name, family, variant, autoWidth, ctx)</c>: <c>ctx.args</c> holds the
    /// <see cref="WaJsFunction.WithArgs"/> value and <c>ctx.library</c> the library name. It returns a URL string, a
    /// Promise of one, or a sprite reference when <see cref="SpriteSheet"/> is set; a throw or an empty result draws no
    /// icon. Set this or <see cref="Resolver"/>, not both.
    /// </summary>
    /// <remarks>
    /// When the reference does not resolve in the browser, the registration fails. Declared tags
    /// (<see cref="WaJsFunction.Uses"/>) have no effect here.
    /// </remarks>
    public WaJsFunction? ResolverFunction { get; set; }

    /// <summary>
    /// A JavaScript function that changes each SVG of the library after it loads, for example its stroke width. Web
    /// Awesome calls it as <c>(svg, icon, ctx)</c> after each load and each update of a <c>wa-icon</c>: <c>svg</c> is
    /// the SVG element, <c>icon</c> the <c>wa-icon</c> element, <c>ctx.args</c> the <see cref="WaJsFunction.WithArgs"/>
    /// value and <c>ctx.library</c> the library name. The return value is not used.
    /// </summary>
    /// <remarks>
    /// When the reference does not resolve in the browser, it writes one <c>console.error</c>, and the library is
    /// registered without a mutator. Declared tags (<see cref="WaJsFunction.Uses"/>) have no effect here.
    /// </remarks>
    public WaJsFunction? MutatorFunction { get; set; }

    /// <summary>
    /// Whether the resolver returns sprite sheet references (e.g. "/sprite.svg#{name}"), rendered through an SVG
    /// &lt;use&gt; element instead of being fetched
    /// </summary>
    public bool SpriteSheet { get; set; }
}
