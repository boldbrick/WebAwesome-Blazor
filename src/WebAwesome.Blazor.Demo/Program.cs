using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Extensions;
using WebAwesome.Blazor.Demo.Services;

namespace WebAwesome.Blazor.Demo;

/// <summary>
/// Entry point of the Web Awesome Blazor demo application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Configures and runs the WebAssembly host.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddScoped<ApiSurfaceService>();

        // the getting-started path consumers follow: services + configuration binding
        builder.Services.AddWebAwesome(builder.Configuration);

        await builder.Build().RunAsync();
    }
}
