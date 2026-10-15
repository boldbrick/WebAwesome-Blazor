using System;
using System.Reflection;
using System.Text.Json;
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

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            jsInterop.RegisterIconLibraryAsync("test", new IconLibraryOptions { MutatorFunction = SampleMutatorFunction }));

        // Assert - the message names both ways to give one
        Assert.Contains(nameof(IconLibraryOptions.Resolver), exception.Message);
        Assert.Contains(nameof(IconLibraryOptions.ResolverFunction), exception.Message);
    }

    [Fact]
    public async Task RegisterIconLibraryAsync_WithResolverAndResolverFunction_ThrowsAndInvokesNothing()
    {
        // Arrange - both would write the one resolver of the library
        var module = SetupInteropModule();
        var options = new IconLibraryOptions { Resolver = SampleResolver, ResolverFunction = SampleResolverFunction };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateConfiguredInterop().RegisterIconLibraryAsync(SampleLibrary, options));

        // Assert
        Assert.Contains($"both {nameof(IconLibraryOptions.Resolver)} and {nameof(IconLibraryOptions.ResolverFunction)}", exception.Message);
        Assert.Empty(module.Invocations);
    }

    [Fact]
    public async Task RegisterIconLibraryAsync_WithMutatorAndMutatorFunction_ThrowsAndInvokesNothing()
    {
        // Arrange - the obsolete path and its replacement would write the one mutator of the library
        var module = SetupInteropModule();
#pragma warning disable CS0618 // the obsolete mutator path is under test
        var options = new IconLibraryOptions { Resolver = SampleResolver, Mutator = SampleMutator, MutatorFunction = SampleMutatorFunction };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateConfiguredInterop().RegisterIconLibraryAsync(SampleLibrary, options));

        // Assert
        Assert.Contains($"both {nameof(IconLibraryOptions.Mutator)} and {nameof(IconLibraryOptions.MutatorFunction)}", exception.Message);
#pragma warning restore CS0618
        Assert.Empty(module.Invocations);
    }

    [Fact]
    public async Task RegisterIconLibraryAsync_WithResolverFunctionOnly_InvokesModule()
    {
        // Arrange - a resolver function replaces the URL template
        var module = SetupInteropModule();
        var options = new IconLibraryOptions { ResolverFunction = SampleResolverFunction, MutatorFunction = SampleMutatorFunction };

        // Act
        await CreateConfiguredInterop().RegisterIconLibraryAsync(SampleLibrary, options);

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "registerIconLibrary");
        Assert.Equal(new object?[] { SampleLoaderUrl, SampleLibrary, options }, invocation.Arguments);
    }

    [Fact]
    public void IconLibraryOptions_Json_HoldsTheFunctionDescriptors()
    {
        // Arrange - the JS interop writes camelCase names, and WaJsFunction writes its descriptor with any options
        var options = new IconLibraryOptions
        {
            ResolverFunction = SampleResolverFunction,
            MutatorFunction = SampleMutatorFunction,
            SpriteSheet = true
        };

        // Act
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(options, JsonSerializerOptions.Web));
        var root = json.RootElement;

        // Assert - registerIconLibrary reads these keys
        Assert.Equal(JsonValueKind.Null, root.GetProperty("resolver").ValueKind);
        Assert.True(root.GetProperty("spriteSheet").GetBoolean());

        var resolver = root.GetProperty("resolverFunction");
        Assert.Equal("fn", resolver.GetProperty("$wab").GetString());
        Assert.Equal("module", resolver.GetProperty("ref").GetProperty("kind").GetString());
        Assert.Equal(SampleModule, resolver.GetProperty("ref").GetProperty("url").GetString());
        Assert.Equal(SampleResolverExport, resolver.GetProperty("ref").GetProperty("export").GetString());
        Assert.Equal(SampleIconSet, resolver.GetProperty("args").GetProperty("set").GetString());

        var mutator = root.GetProperty("mutatorFunction");
        Assert.Equal("fn", mutator.GetProperty("$wab").GetString());
        Assert.Equal("global", mutator.GetProperty("ref").GetProperty("kind").GetString());
        Assert.Equal(SampleMutator, mutator.GetProperty("ref").GetProperty("path").GetString());
    }

    [Fact]
    public void IconLibraryOptions_Mutator_IsObsoleteForTheNextTrain()
    {
        // Arrange - the removal is due in the 3.15.0 train (CHANGELOG "Next-release check")
        var property = typeof(IconLibraryOptions).GetProperty(nameof(IconLibraryOptions.MutatorFunction))!;
#pragma warning disable CS0618 // the obsolete mutator path is under test
        var obsolete = typeof(IconLibraryOptions).GetProperty(nameof(IconLibraryOptions.Mutator))!;
#pragma warning restore CS0618

        // Act
        var attribute = obsolete.GetCustomAttribute<ObsoleteAttribute>();

        // Assert - the message names the replacement and the train, and the replacement is not obsolete
        Assert.NotNull(attribute);
        Assert.False(attribute.IsError);
        Assert.Contains(nameof(IconLibraryOptions.MutatorFunction), attribute.Message);
        Assert.Contains("3.15.0 train", attribute.Message);
        Assert.Null(property.GetCustomAttribute<ObsoleteAttribute>());
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
#pragma warning disable CS0618 // the obsolete mutator path still registers until its removal
        var options = new IconLibraryOptions { Resolver = SampleResolver, Mutator = SampleMutator, SpriteSheet = true };
#pragma warning restore CS0618

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
    private const string SampleModule = "./Pages/Icons.razor.js";
    private const string SampleResolverExport = "iconUrl";
    private const string SampleIconSet = "outline";

    private static readonly WaJsFunction SampleResolverFunction =
        WaJsFunction.Module(SampleModule, SampleResolverExport).WithArgs(new { set = SampleIconSet });
    private static readonly WaJsFunction SampleMutatorFunction = WaJsFunction.Global(SampleMutator);
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
