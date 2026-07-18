using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Models;
using WebAwesome.Blazor.Services;

namespace WebAwesome.Blazor.Extensions;

/// <summary>
/// Extension methods for configuring Web Awesome Blazor services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configuration section name bound by <see cref="AddWebAwesome(IServiceCollection, IConfiguration)"/>.
    /// </summary>
    public const string ConfigurationSection = "WebAwesome";

    /// <summary>
    /// Adds Web Awesome Blazor services to the specified IServiceCollection with default options.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    /// <remarks>
    /// This method registers the WebAwesomeJSInterop service, WaIconLibraryService, and the
    /// WebAwesomeOptions consumed by the WebAwesomeAssets component.
    /// </remarks>
    public static IServiceCollection AddWebAwesome(this IServiceCollection services)
    {
        return services.AddWebAwesome(options => { });
    }

    /// <summary>
    /// Adds Web Awesome Blazor services with options configured by the supplied delegate.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <param name="configure">Delegate configuring the <see cref="WebAwesomeOptions"/></param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null</exception>
    public static IServiceCollection AddWebAwesome(this IServiceCollection services, Action<WebAwesomeOptions> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var options = new WebAwesomeOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddScoped<WebAwesomeJSInterop>();
        services.AddScoped<WaIconLibraryService>();
        return services;
    }

    /// <summary>
    /// Adds Web Awesome Blazor services with options bound from the "WebAwesome" configuration section.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <param name="configuration">Application configuration containing an optional "WebAwesome" section</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null</exception>
    public static IServiceCollection AddWebAwesome(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        return services.AddWebAwesome(options => BindOptions(configuration.GetSection(ConfigurationSection), options));
    }

    #region ------ Internals ------

    // bound manually to avoid a dependency on the configuration binder package
    private static void BindOptions(IConfiguration section, WebAwesomeOptions options)
    {
        var assetSource = section[nameof(WebAwesomeOptions.AssetSource)];
        if (!string.IsNullOrEmpty(assetSource))
            options.AssetSource = Enum.Parse<WaAssetSource>(assetSource, ignoreCase: true);

        options.Version = section[nameof(WebAwesomeOptions.Version)] ?? options.Version;
        options.CdnBaseUrl = section[nameof(WebAwesomeOptions.CdnBaseUrl)] ?? options.CdnBaseUrl;
        options.BasePath = section[nameof(WebAwesomeOptions.BasePath)] ?? options.BasePath;
        options.StylesheetUrl = section[nameof(WebAwesomeOptions.StylesheetUrl)] ?? options.StylesheetUrl;
        options.LoaderUrl = section[nameof(WebAwesomeOptions.LoaderUrl)] ?? options.LoaderUrl;
        options.FontAwesomeKitCode = section[nameof(WebAwesomeOptions.FontAwesomeKitCode)] ?? options.FontAwesomeKitCode;
    }

    #endregion
}
