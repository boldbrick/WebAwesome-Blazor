using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Services;

/// <summary>
/// Service for managing Web Awesome icon library registrations with high-level helpers. The registrations are
/// held by the Web Awesome instance loaded in the page, so call these methods once the app is interactive
/// (e.g. from OnAfterRenderAsync), not during prerendering.
/// </summary>
public class WaIconLibraryService
{
    /// <summary>
    /// Unlocks the Font Awesome Pro icons of the default icon library with the specified kit code (Web Awesome's
    /// <c>setKitCode</c>); icons without a <c>Library</c> then resolve from Font Awesome Pro. Setting
    /// <see cref="WebAwesomeOptions.FontAwesomeKitCode"/> does the same at startup.
    /// </summary>
    /// <param name="kitCode">Your Font Awesome Pro kit code; supply it from configuration, never hard-code it</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when kitCode is null or empty</exception>
    public async Task RegisterFontAwesomeProAsync(string kitCode)
    {
        if (string.IsNullOrEmpty(kitCode))
            throw new ArgumentNullException(nameof(kitCode));

        await jsInterop.SetKitCodeAsync(kitCode);
    }

    /// <summary>
    /// Registers the outline style of the Heroicons icon library (24 px) from the jsDelivr CDN as library
    /// "heroicons". For the solid style, register a custom library with a ".../24/solid/{name}.svg" resolver.
    /// </summary>
    /// <param name="version">Heroicons version (default: "2.0.18")</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task RegisterHeroiconsAsync(string version = "2.0.18")
    {
        var options = new IconLibraryOptions
        {
            Resolver = HeroiconsResolverTemplate.Replace(VersionPlaceholder, version, StringComparison.Ordinal)
        };

        await jsInterop.RegisterIconLibraryAsync(HeroiconsLibraryName, options);
    }

    /// <summary>
    /// Registers the Lucide icon library (the lucide-static package) from the jsDelivr CDN as library "lucide".
    /// </summary>
    /// <param name="version">Lucide version (default: "latest")</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task RegisterLucideAsync(string version = "latest")
    {
        var options = new IconLibraryOptions
        {
            Resolver = LucideResolverTemplate.Replace(VersionPlaceholder, version, StringComparison.Ordinal)
        };

        await jsInterop.RegisterIconLibraryAsync(LucideLibraryName, options);
    }

    /// <summary>
    /// Registers a custom icon library
    /// </summary>
    /// <param name="name">Name of the icon library</param>
    /// <param name="options">Configuration options for the library</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task RegisterIconLibraryAsync(string name, IconLibraryOptions options)
    {
        await jsInterop.RegisterIconLibraryAsync(name, options);
    }

    /// <summary>
    /// Unregisters an icon library
    /// </summary>
    /// <param name="name">Name of the icon library to remove</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task UnregisterIconLibraryAsync(string name)
    {
        await jsInterop.UnregisterIconLibraryAsync(name);
    }

    /// <summary>
    /// Sets the default icon family
    /// </summary>
    /// <param name="family">The icon family name</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task SetDefaultIconFamilyAsync(string family)
    {
        await jsInterop.SetDefaultIconFamilyAsync(family);
    }

    /// <summary>
    /// Gets the current default icon family
    /// </summary>
    /// <returns>The current default icon family name</returns>
    public async Task<string> GetDefaultIconFamilyAsync()
    {
        return await jsInterop.GetDefaultIconFamilyAsync();
    }

    #region ------ Constructors ------

    /// <summary>
    /// Initializes a new instance of the <see cref="WaIconLibraryService"/>.
    /// </summary>
    /// <param name="jsInterop">JavaScript interop service used to register and manage icon libraries</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsInterop"/> is null</exception>
    public WaIconLibraryService(WebAwesomeJSInterop jsInterop)
    {
        this.jsInterop = jsInterop ?? throw new ArgumentNullException(nameof(jsInterop));
    }

    #endregion

    #region ------ Internals ------

    private const string VersionPlaceholder = "{version}";
    private const string HeroiconsLibraryName = "heroicons";
    private const string HeroiconsResolverTemplate = "https://cdn.jsdelivr.net/npm/heroicons@{version}/24/outline/{name}.svg";
    private const string LucideLibraryName = "lucide";
    private const string LucideResolverTemplate = "https://cdn.jsdelivr.net/npm/lucide-static@{version}/icons/{name}.svg";

    private readonly WebAwesomeJSInterop jsInterop;

    #endregion
}
