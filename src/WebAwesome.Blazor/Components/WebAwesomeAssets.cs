using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// Emits the Web Awesome asset tags — stylesheet link, autoloader script module, and (when configured) a module
/// script that sets up the default icon library: the Font Awesome kit code and the self-hosted icon path — based on
/// the registered <see cref="WebAwesomeOptions"/>. Place once in the application's head content (e.g. in App.razor).
/// In standalone WebAssembly apps with a static index.html, prefer the equivalent static tags documented in the README.
/// </summary>
public class WebAwesomeAssets : ComponentBase
{
    /// <summary>
    /// Options overriding the registered <see cref="WebAwesomeOptions"/> for this instance.
    /// </summary>
    [Parameter] public WebAwesomeOptions? Options { get; set; }

    #region ------ Dependency Injection ------

    [Inject] private WebAwesomeOptions RegisteredOptions { get; set; } = default!;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var options = Options ?? RegisteredOptions;
        var stylesheetUrl = Absolutize(options.ResolveStylesheetUrl());
        var loaderUrl = Absolutize(options.ResolveLoaderUrl());

        builder.OpenElement(0, "link");
        builder.AddAttribute(1, "rel", "stylesheet");
        builder.AddAttribute(2, "href", stylesheetUrl);
        builder.CloseElement();

        builder.OpenElement(10, "script");
        builder.AddAttribute(11, "type", "module");
        builder.AddAttribute(12, "src", loaderUrl);
        builder.CloseElement();

        // set up the default icon library via the loader's setKitCode and setIconPath API when configured
        var iconPath = string.IsNullOrEmpty(options.IconPath) ? null : Absolutize(options.IconPath);
        var iconLibraryModule = BuildIconLibraryModule(loaderUrl, options.FontAwesomeKitCode, iconPath);
        if (iconLibraryModule != null)
        {
            builder.OpenElement(20, "script");
            builder.AddAttribute(21, "type", "module");

            // markup, not text: prerendering JS-encodes the text of a script element, quotes included, which breaks the
            // module; the escaped module holds no '<' or '&', so the markup is the same text in both render paths
            builder.AddMarkupContent(22, iconLibraryModule);
            builder.CloseElement();
        }
    }

    #endregion

    #region ------ Internals ------

    private const string SetKitCodeFunction = "setKitCode";
    private const string SetIconPathFunction = "setIconPath";

    /// <summary>
    /// Resolves an app-base-relative asset URL to an absolute one. Web Awesome's autoloader derives
    /// its component base path from the loader script's raw src attribute; a relative value would
    /// yield a bare dynamic-import specifier (components silently never upgrade) and would also
    /// resolve against the current route instead of the app base. Absolute and root-relative URLs
    /// pass through unchanged.
    /// </summary>
    private string Absolutize(string url)
    {
        if (url.StartsWith('/') || Uri.TryCreate(url, UriKind.Absolute, out _)) return url;

        // NavigationManager.BaseUri always ends with a slash
        return Navigation.BaseUri + url;
    }

    /// <summary>
    /// Builds the module that sets the kit code and the icon path of the default icon library, or null when neither
    /// is configured. The module imports from the loader URL itself: another URL would load a second Web Awesome
    /// instance, whose settings the page's components never read.
    /// </summary>
    private static string? BuildIconLibraryModule(string loaderUrl, string? kitCode, string? iconPath)
    {
        var imports = new List<string>();
        var calls = new StringBuilder();

        // values are embedded in a JS module; escape to prevent breaking out of the string literals
        if (!string.IsNullOrEmpty(kitCode))
        {
            imports.Add(SetKitCodeFunction);
            calls.Append($" {SetKitCodeFunction}('{EscapeJsString(kitCode)}');");
        }

        if (!string.IsNullOrEmpty(iconPath))
        {
            imports.Add(SetIconPathFunction);
            calls.Append($" {SetIconPathFunction}('{EscapeJsString(iconPath)}');");
        }

        if (imports.Count == 0) return null;

        return $"import {{ {string.Join(", ", imports)} }} from '{EscapeJsString(loaderUrl)}';{calls}";
    }

    /// <summary>
    /// Escapes a value for a single-quoted JS string literal in the markup of a script element: the value cannot end
    /// the literal ('), the script element ('&lt;') or start a character reference ('&amp;').
    /// </summary>
    private static string EscapeJsString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal)
            .Replace("<", "\\u003C", StringComparison.Ordinal)
            .Replace("&", "\\u0026", StringComparison.Ordinal);
    }

    #endregion
}
