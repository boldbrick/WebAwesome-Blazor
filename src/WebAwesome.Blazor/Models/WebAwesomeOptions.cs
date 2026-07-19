using System;
using System.Reflection;

namespace WebAwesome.Blazor.Models;

/// <summary>
/// Source from which the Web Awesome assets (stylesheet and loader script) are served.
/// </summary>
public enum WaAssetSource
{
    /// <summary>Assets are loaded from a public CDN (default).</summary>
    Cdn,

    /// <summary>Assets are served by the application itself from <see cref="WebAwesomeOptions.BasePath"/>.</summary>
    SelfHosted
}

/// <summary>
/// Configuration options for Web Awesome asset delivery, consumed by the WebAwesomeAssets component.
/// Bind from configuration section "WebAwesome" or configure via the AddWebAwesome overloads.
/// </summary>
public class WebAwesomeOptions
{
    /// <summary>
    /// Where the Web Awesome assets are served from. Defaults to <see cref="WaAssetSource.Cdn"/>.
    /// </summary>
    public WaAssetSource AssetSource { get; set; } = WaAssetSource.Cdn;

    /// <summary>
    /// Web Awesome version used to resolve CDN URLs. Defaults to the library's own version,
    /// which tracks the bound Web Awesome release one-to-one.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// CDN base URL template; the {version} placeholder is replaced with <see cref="Version"/>.
    /// </summary>
    public string CdnBaseUrl { get; set; } = DefaultCdnBaseUrl;

    /// <summary>
    /// Base path for self-hosted assets (used when <see cref="AssetSource"/> is <see cref="WaAssetSource.SelfHosted"/>),
    /// e.g. "/lib/webawesome". The standard dist layout (styles\webawesome.css, webawesome.loader.js) is assumed beneath it.
    /// </summary>
    public string? BasePath { get; set; }

    /// <summary>
    /// Explicit stylesheet URL; overrides the URL derived from the asset source when set.
    /// </summary>
    public string? StylesheetUrl { get; set; }

    /// <summary>
    /// Explicit loader script URL; overrides the URL derived from the asset source when set.
    /// </summary>
    public string? LoaderUrl { get; set; }

    /// <summary>
    /// Font Awesome kit code unlocking premium icon packs. Never hard-code a real kit code;
    /// supply it via configuration (e.g. appsettings "WebAwesome:FontAwesomeKitCode") or user secrets.
    /// </summary>
    public string? FontAwesomeKitCode { get; set; }

    /// <summary>
    /// Resolves the effective Web Awesome version: the explicitly configured <see cref="Version"/>,
    /// or the library's own version.
    /// </summary>
    /// <returns>The version string used in CDN URLs</returns>
    public string ResolveVersion() => Version ?? LibraryVersion;

    /// <summary>
    /// Resolves the effective stylesheet URL from the configured asset source and overrides.
    /// </summary>
    /// <returns>Absolute or app-relative URL of the Web Awesome stylesheet</returns>
    public string ResolveStylesheetUrl() => StylesheetUrl ?? $"{ResolveBaseUrl()}/styles/webawesome.css";

    /// <summary>
    /// Resolves the effective loader script URL from the configured asset source and overrides.
    /// </summary>
    /// <returns>Absolute or app-relative URL of the Web Awesome autoloader module</returns>
    public string ResolveLoaderUrl() => LoaderUrl ?? $"{ResolveBaseUrl()}/webawesome.loader.js";

    #region ------ Internals ------

    private const string DefaultCdnBaseUrl = "https://cdn.jsdelivr.net/npm/@awesome.me/webawesome@{version}/dist-cdn";

    // the library version tracks the bound Web Awesome release; informational version carries the full semver
    private static readonly string LibraryVersion = ResolveLibraryVersion();

    private static string ResolveLibraryVersion()
    {
        var informational = typeof(WebAwesomeOptions).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrEmpty(informational)) throw new InvalidOperationException("Library version could not be determined");

        // strip build metadata appended by CI builds (e.g. "3.0.0-beta.6+abc123")
        var plusIndex = informational.IndexOf('+');
        return plusIndex < 0 ? informational : informational[..plusIndex];
    }

    private string ResolveBaseUrl()
    {
        if (AssetSource == WaAssetSource.SelfHosted)
        {
            if (string.IsNullOrEmpty(BasePath))
                throw new InvalidOperationException($"{nameof(BasePath)} must be configured when {nameof(AssetSource)} is {nameof(WaAssetSource.SelfHosted)}");
            return BasePath.TrimEnd('/');
        }

        return CdnBaseUrl.Replace("{version}", ResolveVersion()).TrimEnd('/');
    }

    #endregion
}
