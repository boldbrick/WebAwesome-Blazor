using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Services;

namespace WebAwesome.Blazor.Extensions;

/// <summary>
/// Extension methods for configuring Web Awesome Blazor services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Web Awesome Blazor services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    /// <remarks>
    /// This method registers the WebAwesomeJSInterop service and WaIconLibraryService
    /// which are required for JavaScript interop functionality and icon library management.
    /// </remarks>
    public static IServiceCollection AddWebAwesome(this IServiceCollection services)
    {
        services.AddScoped<WebAwesomeJSInterop>();
        services.AddScoped<WaIconLibraryService>();
        return services;
    }
}