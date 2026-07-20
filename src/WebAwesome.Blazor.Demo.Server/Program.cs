using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Extensions;
using WebAwesome.Blazor.Demo.Services;

namespace WebAwesome.Blazor.Demo.Server;

/// <summary>
/// Entry point of the server-hosted (Blazor Web App, interactive server) variant of the demo.
/// The demo UI itself lives in WebAwesome.Blazor.Demo and is shared with the standalone
/// WebAssembly host, so both Blazor hosting models can be exercised against the same pages.
/// </summary>
public static class Program
{
    /// <summary>
    /// Configures and runs the server host.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // same service surface the WebAssembly host wires up in WebAwesome.Blazor.Demo.Program;
        // the HttpClient points back at this server so ApiSurfaceService can fetch the
        // static api-surface.json exactly like it does in the browser
        builder.Services.AddScoped(sp =>
        {
            var navigation = sp.GetRequiredService<NavigationManager>();
            return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
        });
        builder.Services.AddScoped<ApiSurfaceService>();
        builder.Services.AddWebAwesome(builder.Configuration);

        var app = builder.Build();

        app.UseAntiforgery();

        app.MapStaticAssets();

        // the routable pages live in the shared demo assembly, not in this host
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(typeof(WebAwesome.Blazor.Demo.App).Assembly);

        app.Run();
    }
}
