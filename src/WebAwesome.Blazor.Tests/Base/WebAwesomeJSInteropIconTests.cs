using System;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Extensions;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Base;

/// <summary>
/// Integration tests for WebAwesomeJSInterop icon library methods: argument validation, and the exact interop
/// module invocation (identifier and arguments) each method makes. That the identifiers exist in
/// webawesome-interop.js is guarded by InteropModuleContractTests; that the JS side reaches Web Awesome's
/// registry is proven in a browser by tools\e2e\tests\icon-library.spec.js.
/// </summary>
public class WebAwesomeJSInteropIconTests : BunitContext
{
    [Fact]
    public async Task RegisterIconLibraryAsync_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());
        var options = new IconLibraryOptions();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.RegisterIconLibraryAsync(null!, options));
    }

    [Fact]
    public async Task RegisterIconLibraryAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.RegisterIconLibraryAsync("test", null!));
    }

    [Fact]
    public async Task RegisterIconLibraryAsync_WithoutResolver_ThrowsArgumentException()
    {
        // Arrange - Web Awesome cannot resolve any icon of a library without a resolver
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            jsInterop.RegisterIconLibraryAsync("test", new IconLibraryOptions { Mutator = "mutate" }));
    }

    [Fact]
    public async Task UnregisterIconLibraryAsync_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.UnregisterIconLibraryAsync(null!));
    }

    [Fact]
    public async Task SetDefaultIconFamilyAsync_WithNullFamily_ThrowsArgumentNullException()
    {
        // Arrange
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.SetDefaultIconFamilyAsync(null!));
    }

    [Fact]
    public async Task SetKitCodeAsync_WithNullKitCode_ThrowsArgumentNullException()
    {
        // Arrange
        var jsInterop = new WebAwesomeJSInterop(new MockJSRuntime());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            jsInterop.SetKitCodeAsync(null!));
    }

    [Fact]
    public async Task RegisterIconLibraryAsync_InvokesModule_WithConfiguredLoaderUrlNameAndOptions()
    {
        // Arrange
        var module = SetupInteropModule();
        var jsInterop = CreateConfiguredInterop();
        var options = new IconLibraryOptions { Resolver = SampleResolver, Mutator = SampleMutator, SpriteSheet = true };

        // Act
        await jsInterop.RegisterIconLibraryAsync(SampleLibrary, options);

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "registerIconLibrary");
        Assert.Equal(new object?[] { SampleLoaderUrl, SampleLibrary, options }, invocation.Arguments);
    }

    [Fact]
    public async Task UnregisterIconLibraryAsync_InvokesModule_WithConfiguredLoaderUrlAndName()
    {
        // Arrange
        var module = SetupInteropModule();

        // Act
        await CreateConfiguredInterop().UnregisterIconLibraryAsync(SampleLibrary);

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "unregisterIconLibrary");
        Assert.Equal(new object?[] { SampleLoaderUrl, SampleLibrary }, invocation.Arguments);
    }

    [Fact]
    public async Task SetDefaultIconFamilyAsync_InvokesModule_WithConfiguredLoaderUrlAndFamily()
    {
        // Arrange
        var module = SetupInteropModule();

        // Act
        await CreateConfiguredInterop().SetDefaultIconFamilyAsync(SampleFamily);

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setDefaultIconFamily");
        Assert.Equal(new object?[] { SampleLoaderUrl, SampleFamily }, invocation.Arguments);
    }

    [Fact]
    public async Task GetDefaultIconFamilyAsync_InvokesModule_AndReturnsItsResult()
    {
        // Arrange
        var module = SetupInteropModule();
        module.Setup<string>("getDefaultIconFamily", _ => true).SetResult(SampleFamily);

        // Act
        var family = await CreateConfiguredInterop().GetDefaultIconFamilyAsync();

        // Assert
        Assert.Equal(SampleFamily, family);
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "getDefaultIconFamily");
        Assert.Equal(new object?[] { SampleLoaderUrl }, invocation.Arguments);
    }

    [Fact]
    public async Task SetKitCodeAsync_InvokesModule_WithConfiguredLoaderUrlAndKitCode()
    {
        // Arrange
        var module = SetupInteropModule();

        // Act
        await CreateConfiguredInterop().SetKitCodeAsync(SampleKitCode);

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setKitCode");
        Assert.Equal(new object?[] { SampleLoaderUrl, SampleKitCode }, invocation.Arguments);
    }

    [Fact]
    public async Task IconLibraryMethods_WithoutOptions_PassNullLoaderUrl()
    {
        // Arrange - the JS side then relies on the page's own Web Awesome script tag alone
        var module = SetupInteropModule();
        var jsInterop = new WebAwesomeJSInterop(JSInterop.JSRuntime);

        // Act
        await jsInterop.SetDefaultIconFamilyAsync(SampleFamily);

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setDefaultIconFamily");
        Assert.Equal(new object?[] { null, SampleFamily }, invocation.Arguments);
    }

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
    private const string SampleLoaderUrl = "https://cdn.example.test/webawesome/webawesome.loader.js";
    private const string SampleLibrary = "sample-icons";
    private const string SampleResolver = "https://icons.example.test/{family}/{name}.svg";
    private const string SampleMutator = "sampleIcons.mutate";
    private const string SampleFamily = "sharp";
    private const string SampleKitCode = "your_kit_code_here";

    private BunitJSModuleInterop SetupInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.Mode = JSRuntimeMode.Loose;
        return module;
    }

    // resolved from DI as AddWebAwesome registers it, so the options-aware constructor is the one exercised
    private WebAwesomeJSInterop CreateConfiguredInterop()
    {
        Services.AddWebAwesome(options => options.LoaderUrl = SampleLoaderUrl);
        return Services.GetRequiredService<WebAwesomeJSInterop>();
    }

    private class MockJSRuntime : Microsoft.JSInterop.IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            throw new NotImplementedException();
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
