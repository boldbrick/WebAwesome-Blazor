using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Extensions;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for WebAwesomeOptions URL resolution and the AddWebAwesome registration overloads
/// backing the WebAwesomeAssets component.
/// </summary>
public class WebAwesomeAssetsTests
{
    #region ------ WebAwesomeOptions URL resolution ------

    [Fact]
    public void ResolveVersion_Default_UsesLibraryVersion()
    {
        // Arrange
        var options = new WebAwesomeOptions();

        // Act
        var version = options.ResolveVersion();

        // Assert - the default is the bound Web Awesome version, without the bindings revision of the package
        // version (package 3.14.0.1 binds Web Awesome 3.14.0)
        Assert.Equal(BoundWaVersion.Value, version);
    }

    [Fact]
    public void ResolveStylesheetUrl_CdnDefault_UsesVersionedJsdelivrUrl()
    {
        // Arrange
        var options = new WebAwesomeOptions();

        // Act
        var url = options.ResolveStylesheetUrl();

        // Assert
        Assert.Equal($"https://cdn.jsdelivr.net/npm/@awesome.me/webawesome@{BoundWaVersion.Value}/dist-cdn/styles/webawesome.css", url);
    }

    [Fact]
    public void ResolveLoaderUrl_ExplicitVersion_OverridesLibraryVersion()
    {
        // Arrange - a deliberately fictitious version proves the explicit value wins over the library version
        var options = new WebAwesomeOptions { Version = "99.0.0-explicit" };

        // Act
        var url = options.ResolveLoaderUrl();

        // Assert
        Assert.Equal("https://cdn.jsdelivr.net/npm/@awesome.me/webawesome@99.0.0-explicit/dist-cdn/webawesome.loader.js", url);
    }

    [Fact]
    public void ResolveStylesheetUrl_SelfHosted_UsesBasePath()
    {
        // Arrange
        var options = new WebAwesomeOptions
        {
            AssetSource = WaAssetSource.SelfHosted,
            BasePath = "/lib/webawesome/"
        };

        // Act
        var stylesheet = options.ResolveStylesheetUrl();
        var loader = options.ResolveLoaderUrl();

        // Assert
        Assert.Equal("/lib/webawesome/styles/webawesome.css", stylesheet);
        Assert.Equal("/lib/webawesome/webawesome.loader.js", loader);
    }

    [Fact]
    public void ResolveStylesheetUrl_SelfHostedWithoutBasePath_Throws()
    {
        // Arrange
        var options = new WebAwesomeOptions { AssetSource = WaAssetSource.SelfHosted };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ResolveStylesheetUrl());
    }

    [Fact]
    public void ResolveUrls_ExplicitOverrides_WinOverDerivation()
    {
        // Arrange
        var options = new WebAwesomeOptions
        {
            StylesheetUrl = "/custom/wa.css",
            LoaderUrl = "/custom/wa.loader.js"
        };

        // Act & Assert
        Assert.Equal("/custom/wa.css", options.ResolveStylesheetUrl());
        Assert.Equal("/custom/wa.loader.js", options.ResolveLoaderUrl());
    }

    #endregion

    #region ------ AddWebAwesome registration ------

    [Fact]
    public void AddWebAwesome_Default_RegistersDefaultOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWebAwesome();
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<WebAwesomeOptions>();
        Assert.Equal(WaAssetSource.Cdn, options.AssetSource);
        Assert.Null(options.FontAwesomeKitCode);
        Assert.Null(options.IconPath);
    }

    [Fact]
    public void AddWebAwesome_WithDelegate_AppliesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWebAwesome(options => options.Version = "99.0.0-explicit");
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.Equal("99.0.0-explicit", provider.GetRequiredService<WebAwesomeOptions>().ResolveVersion());
    }

    [Fact]
    public void AddWebAwesome_WithConfiguration_BindsWebAwesomeSection()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WebAwesome:AssetSource"] = "SelfHosted",
                ["WebAwesome:BasePath"] = "/assets/wa",
                ["WebAwesome:FontAwesomeKitCode"] = "test-kit-code"
            })
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddWebAwesome(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<WebAwesomeOptions>();
        Assert.Equal(WaAssetSource.SelfHosted, options.AssetSource);
        Assert.Equal("/assets/wa", options.BasePath);
        Assert.Equal("test-kit-code", options.FontAwesomeKitCode);
    }

    [Fact]
    public void AddWebAwesome_WithConfiguration_BindsIconPath()
    {
        // Arrange - the icon path is independent of the asset source
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WebAwesome:IconPath"] = "/lib/fontawesome/svgs"
            })
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddWebAwesome(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<WebAwesomeOptions>();
        Assert.Equal("/lib/fontawesome/svgs", options.IconPath);
        Assert.Equal(WaAssetSource.Cdn, options.AssetSource);
    }

    [Fact]
    public void AddWebAwesome_WithEmptyConfiguration_KeepsDefaults()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        // Act
        services.AddWebAwesome(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.Equal(WaAssetSource.Cdn, provider.GetRequiredService<WebAwesomeOptions>().AssetSource);
    }

    #endregion

    #region ------ Bound Web Awesome version ------

    [Theory]
    // a fourth numeric part is a bindings revision
    [InlineData("3.14.0.1", "3.14.0")]
    [InlineData("3.14.0.12", "3.14.0")]
    [InlineData("3.14.0.1+abc123", "3.14.0")]
    // the prerelease suffix of a four-part version belongs to the bindings revision
    [InlineData("3.14.0.1-beta.1", "3.14.0")]
    [InlineData("3.14.0.1-beta.1+abc123", "3.14.0")]
    // a three-part version is the Web Awesome version, with its own prerelease suffix
    [InlineData("3.14.0", "3.14.0")]
    [InlineData("3.14.0+abc123", "3.14.0")]
    [InlineData("3.0.0-beta.6", "3.0.0-beta.6")]
    [InlineData("3.0.0-beta.6+abc123", "3.0.0-beta.6")]
    [InlineData("3.0.0-rc.1.2", "3.0.0-rc.1.2")]
    public void ToWebAwesomeVersion_KeepsOnlyTheWebAwesomePart(string packageVersion, string expected)
    {
        // Act
        var version = ToWebAwesomeVersion(packageVersion);

        // Assert
        Assert.Equal(expected, version);
    }

    #endregion

    #region ------ Internals ------

    // the parser is internal, and InternalsVisibleTo applies to Debug builds only, hence the lookup by name
    private const string ToWebAwesomeVersionMethod = "ToWebAwesomeVersion";

    private static readonly Func<string, string> ToWebAwesomeVersion = typeof(WebAwesomeOptions)
        .GetMethod(ToWebAwesomeVersionMethod, BindingFlags.NonPublic | BindingFlags.Static)!
        .CreateDelegate<Func<string, string>>();

    #endregion
}
