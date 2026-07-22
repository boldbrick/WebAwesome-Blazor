using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// Emits the Web Awesome asset tags — stylesheet link, autoloader script module, and (when configured)
/// the Font Awesome kit code registration — based on the registered <see cref="WebAwesomeOptions"/>.
/// Place once in the application's head content (e.g. in App.razor). In standalone WebAssembly apps
/// with a static index.html, prefer the equivalent static tags documented in the README.
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

        // register the Font Awesome kit code via the loader's setKitCode API when configured
        if (!string.IsNullOrEmpty(options.FontAwesomeKitCode))
        {
            builder.OpenElement(20, "script");
            builder.AddAttribute(21, "type", "module");
            builder.AddContent(22, BuildKitCodeModule(loaderUrl, options.FontAwesomeKitCode));
            builder.CloseElement();
        }
    }

    #endregion

    #region ------ Internals ------

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

    private static string BuildKitCodeModule(string loaderUrl, string kitCode)
    {
        // values are embedded in a JS module; escape to prevent breaking out of the string literals
        return $"import {{ setKitCode }} from '{EscapeJsString(loaderUrl)}'; setKitCode('{EscapeJsString(kitCode)}');";
    }

    private static string EscapeJsString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal)
            .Replace("<", "\\u003C", StringComparison.Ordinal);
    }

    #endregion
}
