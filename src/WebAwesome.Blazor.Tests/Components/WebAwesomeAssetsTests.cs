using System;
using System.Collections.Generic;
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

        // Assert - the library version tracks the bound Web Awesome release
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
}
