using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Rendering tests of the WebAwesomeAssets component: the module script that sets up the default icon library (the
/// Font Awesome kit code and the self-hosted icon path) imports from the loader URL, escapes the embedded values, and
/// is left out when neither setting is configured, in interactive and in static (prerendered) rendering. That the module reaches the page's Web Awesome instance and keeps
/// every icon request off the Font Awesome CDN is proven in a browser by tools\e2e\tests\icon-path.spec.js.
/// </summary>
public class WebAwesomeAssetsRenderTests : BunitContext
{
    [Fact]
    public void IconPath_Set_RendersSetIconPathModuleAfterTheLoader()
    {
        // Arrange
        Services.AddWebAwesome(options =>
        {
            options.LoaderUrl = SampleLoaderUrl;
            options.IconPath = SampleIconPath;
        });

        // Act
        var cut = Render<WebAwesomeAssets>();

        // Assert - the module follows the loader script and imports from the same URL, so it reaches the same instance
        var scripts = cut.FindAll("script");
        Assert.Equal(2, scripts.Count);
        Assert.Equal(SampleLoaderUrl, scripts[0].GetAttribute("src"));
        Assert.Equal("module", scripts[1].GetAttribute("type"));
        Assert.Null(scripts[1].GetAttribute("src"));
        Assert.Equal(
            $"import {{ setIconPath }} from '{SampleLoaderUrl}'; setIconPath('{SampleIconPath}');",
            scripts[1].TextContent);
    }

    [Fact]
    public void IconPath_Relative_IsResolvedAgainstTheAppBase()
    {
        // Arrange - a relative path means relative to the app base, like the loader and stylesheet URLs
        Services.AddWebAwesome(options =>
        {
            options.LoaderUrl = SampleLoaderUrl;
            options.IconPath = RelativeIconPath;
        });

        // Act
        var cut = Render<WebAwesomeAssets>();

        // Assert
        var module = cut.FindAll("script").Last();
        Assert.Equal(
            $"import {{ setIconPath }} from '{SampleLoaderUrl}'; setIconPath('{BunitBaseUri}{RelativeIconPath}');",
            module.TextContent);
    }

    [Fact]
    public void IconPath_WithQuoteBackslashAndScriptEnd_IsEscaped()
    {
        // Arrange - the values are embedded in JS string literals inside a script element
        Services.AddWebAwesome(options =>
        {
            options.LoaderUrl = "/it's/webawesome.loader.js";
            options.IconPath = "/icons/a'b\\c&amp;</script><script>alert(1)";
        });

        // Act
        var cut = Render<WebAwesomeAssets>();

        // Assert - no value ends its string literal or the script element, or starts a character reference
        var module = cut.FindAll("script").Last();
        Assert.Equal(
            "import { setIconPath } from '/it\\'s/webawesome.loader.js'; setIconPath('/icons/a\\'b\\\\c\\u0026amp;\\u003C/script>\\u003Cscript>alert(1)');",
            module.TextContent);
    }

    [Fact]
    public async Task StaticRendering_WritesTheModuleAsItIs()
    {
        // Arrange - a prerendered page gets the head from the static HTML renderer, which JS-encodes the text of a
        // script element (a quote became \u0027 and the module a syntax error); the module is markup, written as it is
        var services = new ServiceCollection();
        services.AddWebAwesome(options =>
        {
            options.LoaderUrl = SampleLoaderUrl;
            options.FontAwesomeKitCode = SampleKitCode;
            options.IconPath = SampleIconPath;
        });
        services.AddSingleton<NavigationManager>(new StaticNavigationManager());
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);

        // Act
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<WebAwesomeAssets>()).ToHtmlString());

        // Assert
        Assert.Contains(
            $"<script type=\"module\">import {{ setKitCode, setIconPath }} from '{SampleLoaderUrl}'; setKitCode('{SampleKitCode}'); setIconPath('{SampleIconPath}');</script>",
            html);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IconPath_NotSet_RendersNoModule(string? iconPath)
    {
        // Arrange - an empty value is what an unset appsettings entry ("IconPath": "") binds to
        Services.AddWebAwesome(options =>
        {
            options.LoaderUrl = SampleLoaderUrl;
            options.IconPath = iconPath;
        });

        // Act
        var cut = Render<WebAwesomeAssets>();

        // Assert - only the stylesheet and the loader
        Assert.Single(cut.FindAll("link"));
        var script = Assert.Single(cut.FindAll("script"));
        Assert.Equal(SampleLoaderUrl, script.GetAttribute("src"));
    }

    [Fact]
    public void KitCodeAndIconPath_RenderOneModuleSettingBoth()
    {
        // Arrange
        Services.AddWebAwesome(options =>
        {
            options.LoaderUrl = SampleLoaderUrl;
            options.FontAwesomeKitCode = SampleKitCode;
            options.IconPath = SampleIconPath;
        });

        // Act
        var cut = Render<WebAwesomeAssets>();

        // Assert - one import of both functions from the loader URL; Web Awesome then resolves from the path
        var scripts = cut.FindAll("script");
        Assert.Equal(2, scripts.Count);
        Assert.Equal(
            $"import {{ setKitCode, setIconPath }} from '{SampleLoaderUrl}'; setKitCode('{SampleKitCode}'); setIconPath('{SampleIconPath}');",
            scripts[1].TextContent);
    }

    [Fact]
    public void KitCodeOnly_RendersTheKitCodeModuleUnchanged()
    {
        // Arrange
        Services.AddWebAwesome(options =>
        {
            options.LoaderUrl = SampleLoaderUrl;
            options.FontAwesomeKitCode = SampleKitCode;
        });

        // Act
        var cut = Render<WebAwesomeAssets>();

        // Assert - the output of the kit code alone is the one before the icon path existed
        var module = cut.FindAll("script").Last();
        Assert.Equal(
            $"import {{ setKitCode }} from '{SampleLoaderUrl}'; setKitCode('{SampleKitCode}');",
            module.TextContent);
    }

    [Fact]
    public void OptionsParameter_IconPath_OverridesTheRegisteredOptions()
    {
        // Arrange - the registered options have no icon path
        Services.AddWebAwesome(options => options.LoaderUrl = SampleLoaderUrl);
        var options = new WebAwesomeOptions { LoaderUrl = SampleLoaderUrl, IconPath = SampleIconPath };

        // Act
        var cut = Render<WebAwesomeAssets>(parameters => parameters.Add(p => p.Options, options));

        // Assert
        var module = cut.FindAll("script").Last();
        Assert.Equal(
            $"import {{ setIconPath }} from '{SampleLoaderUrl}'; setIconPath('{SampleIconPath}');",
            module.TextContent);
    }

    #region ------ Internals ------

    private const string SampleLoaderUrl = "https://cdn.example.test/webawesome/webawesome.loader.js";
    private const string SampleIconPath = "/lib/fontawesome/svgs";
    private const string RelativeIconPath = "lib/fontawesome/svgs";
    private const string SampleKitCode = "your_kit_code_here";

    // the base URI of bUnit's NavigationManager
    private const string BunitBaseUri = "http://localhost/";

    // the NavigationManager of a static render, which the host provides in a real prerendering
    private sealed class StaticNavigationManager : NavigationManager
    {
        public StaticNavigationManager() => Initialize(BunitBaseUri, BunitBaseUri);
    }

    #endregion
}
