using System;
using System.Globalization;
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
    /// Web Awesome version used to resolve CDN URLs. Defaults to the Web Awesome version the library binds:
    /// the first three parts of the package version (package 3.14.0.1 binds Web Awesome 3.14.0).
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
    /// Base URL of a self-hosted Font Awesome <c>svgs</c> folder for the default icon library, e.g.
    /// "/lib/fontawesome/svgs". Null or empty (the default) keeps Web Awesome's own behavior: the default library
    /// loads every icon from the Font Awesome CDN. When set, the WebAwesomeAssets component calls Web Awesome's
    /// <c>setIconPath</c> before any icon renders, and every icon of the default library, including the icons that
    /// components such as wa-zoomable-frame render themselves, loads from <c>{IconPath}/{folder}/{name}.svg</c>, with
    /// no request to the Font Awesome CDN.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The folder is the <c>svgs</c> folder of a Font Awesome package, with one subfolder per style, named as Web
    /// Awesome maps the icon family and variant: <c>solid</c> (the default), <c>regular</c>, <c>brands</c>,
    /// <c>sharp-solid</c>, <c>duotone</c> and so on. The SVG files are not part of Web Awesome or of this package;
    /// copy them from the Font Awesome package that your license covers (Free or Pro) and serve them as static files.
    /// </para>
    /// <para>
    /// A relative path is relative to the app base, like the other asset URLs of these options: the component turns it
    /// into an absolute URL with the app's base URI, so it works under an app base path and on every route. A
    /// root-relative path ("/...") is used as it is.
    /// </para>
    /// <para>
    /// With a path set, Web Awesome resolves the default library from the path and no longer from the Font Awesome
    /// CDN, also when <see cref="FontAwesomeKitCode"/> is set: the path wins over the kit code, so a Pro folder must
    /// contain the Pro styles that the app uses. The setting is independent of <see cref="AssetSource"/>.
    /// </para>
    /// <para>
    /// A standalone WebAssembly app with static asset tags in index.html (no WebAwesomeAssets component) adds its own
    /// module script after the loader script, with the same loader URL:
    /// <c>&lt;script type="module"&gt;import { setIconPath } from '/lib/webawesome/webawesome.loader.js'; setIconPath('/lib/fontawesome/svgs');&lt;/script&gt;</c>.
    /// A runtime call (<see cref="Services.WaIconLibraryService.SetIconPathAsync(string)"/>) comes too late to
    /// prevent the first CDN requests.
    /// </para>
    /// </remarks>
    public string? IconPath { get; set; }

    /// <summary>
    /// Whether the wrappers write placement metadata (data-wablazor-component, data-wablazor-chain,
    /// data-wablazor-placement, data-wablazor-site) on their root elements, for the WA Inspector browser extension.
    /// Defaults to false. Turn it on in development and test environments only: the metadata shows the names of the
    /// application's components and costs some render time. The setting is process-wide.
    /// </summary>
    public bool EmitInspectorMetadata { get; set; }

    /// <summary>
    /// Resolves the effective Web Awesome version: the explicitly configured <see cref="Version"/>,
    /// or the Web Awesome version the library binds.
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

    /// <summary>
    /// Derives the bound Web Awesome version from a package version. The first three parts are the Web Awesome
    /// version; a fourth numeric part is a revision of the bindings and is dropped together with its prerelease
    /// suffix, which belongs to that revision ("3.14.0.1-beta.1" binds "3.14.0"). A three-part version keeps its
    /// prerelease suffix, which is the Web Awesome prerelease ("3.0.0-beta.6"). Build metadata is always dropped.
    /// </summary>
    /// <param name="packageVersion">Package version, e.g. the informational version "3.14.0.1+abc123"</param>
    /// <returns>The Web Awesome version used in CDN URLs, e.g. "3.14.0"</returns>
    internal static string ToWebAwesomeVersion(string packageVersion)
    {
        // strip build metadata appended by CI builds (e.g. "3.0.0-beta.6+abc123")
        var plusIndex = packageVersion.IndexOf(BuildMetadataSeparator);
        var version = plusIndex < 0 ? packageVersion : packageVersion[..plusIndex];

        // a fourth numeric part marks a bindings revision: keep only the Web Awesome part
        var dashIndex = version.IndexOf(PrereleaseSeparator);
        var parts = (dashIndex < 0 ? version : version[..dashIndex]).Split(VersionPartSeparator);
        var isRevision = parts.Length == WebAwesomeVersionPartCount + 1
            && uint.TryParse(parts[^1], NumberStyles.None, CultureInfo.InvariantCulture, out _);
        return isRevision ? string.Join(VersionPartSeparator, parts, 0, WebAwesomeVersionPartCount) : version;
    }

    private const string DefaultCdnBaseUrl = "https://cdn.jsdelivr.net/npm/@awesome.me/webawesome@{version}/dist-cdn";

    private const char BuildMetadataSeparator = '+';
    private const char PrereleaseSeparator = '-';
    private const char VersionPartSeparator = '.';

    // major.minor.patch of the bound Web Awesome release
    private const int WebAwesomeVersionPartCount = 3;

    // the Web Awesome part of the package version; the informational version carries the full package version
    private static readonly string LibraryVersion = ResolveLibraryVersion();

    private static string ResolveLibraryVersion()
    {
        var informational = typeof(WebAwesomeOptions).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrEmpty(informational)) throw new InvalidOperationException("Library version could not be determined");

        return ToWebAwesomeVersion(informational);
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
