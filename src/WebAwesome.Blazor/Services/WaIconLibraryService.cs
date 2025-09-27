using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Services;

/// <summary>
/// Service for managing Web Awesome icon library registrations with high-level helpers
/// </summary>
public class WaIconLibraryService
{
    private readonly WebAwesomeJSInterop jsInterop;

    public WaIconLibraryService(WebAwesomeJSInterop jsInterop)
    {
        this.jsInterop = jsInterop ?? throw new ArgumentNullException(nameof(jsInterop));
    }

    /// <summary>
    /// Registers Font Awesome Pro with the specified kit code
    /// </summary>
    /// <param name="kitCode">Your Font Awesome Pro kit code</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when kitCode is null or empty</exception>
    public async Task RegisterFontAwesomeProAsync(string kitCode)
    {
        if (string.IsNullOrEmpty(kitCode))
            throw new ArgumentNullException(nameof(kitCode));

        var options = new IconLibraryOptions
        {
            Resolver = $"https://kit.fontawesome.com/{kitCode}/{{family}}/{{variant}}/{{name}}.svg"
        };

        await jsInterop.RegisterIconLibraryAsync("fa-pro", options);
    }

    /// <summary>
    /// Registers Heroicons icon library
    /// </summary>
    /// <param name="version">Heroicons version (default: "2.0.18")</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task RegisterHeroiconsAsync(string version = "2.0.18")
    {
        var options = new IconLibraryOptions
        {
            Resolver = $"https://cdn.jsdelivr.net/npm/heroicons@{version}/24/{{variant}}/{{name}}.svg"
        };

        await jsInterop.RegisterIconLibraryAsync("heroicons", options);
    }

    /// <summary>
    /// Registers Lucide icon library
    /// </summary>
    /// <param name="version">Lucide version (default: "latest")</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task RegisterLucideAsync(string version = "latest")
    {
        var options = new IconLibraryOptions
        {
            Resolver = $"https://cdn.jsdelivr.net/npm/lucide@{version}/icons/{{name}}.svg"
        };

        await jsInterop.RegisterIconLibraryAsync("lucide", options);
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
}