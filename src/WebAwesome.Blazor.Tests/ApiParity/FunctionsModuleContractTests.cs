using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Guards the public module of the JavaScript function references, wwwroot\webawesome-functions.js: apps and the
/// Extensions package import it by path and call its exports, so a renamed export or a moved registry fails only in
/// the browser. The checks also keep the rule that no library JavaScript evaluates text as code.
/// </summary>
public class FunctionsModuleContractTests
{
    [Theory]
    [InlineData("registerFunction")]
    [InlineData("registerContextDecorator")]
    [InlineData("resolveFunction")]
    [InlineData("isFunctionDescriptor")]
    [InlineData("findGlobalFunction")]
    public void FunctionsModule_ExportsTheContractFunction(string name)
    {
        Assert.Matches(new Regex($"^export\\s+(?:async\\s+)?function\\s+{name}\\s*\\(", RegexOptions.Multiline), Read(FunctionsModuleFileName));
    }

    [Fact]
    public void FunctionsModule_KeepsTheRegistryAndTheCallArgumentsOnSharedSymbols()
    {
        var source = Read(FunctionsModuleFileName);

        // every copy of the module (another base path, another version) must find the same registry
        Assert.Contains("Symbol.for('WebAwesome.Blazor.functions')", source);
        Assert.Matches(new Regex(@"^export\s+const\s+callArguments\s*=\s*Symbol\.for\('WebAwesome\.Blazor\.callArguments'\)", RegexOptions.Multiline), source);
    }

    [Fact]
    public void InteropModule_ResolvesThroughTheFunctionsModule()
    {
        Assert.Contains("from './webawesome-functions.js'", Read(InteropModuleFileName));
    }

    [Theory]
    [InlineData(FunctionsModuleFileName)]
    [InlineData(InteropModuleFileName)]
    [InlineData(InitializerFileName)]
    public void LibraryJavaScript_NeverEvaluatesText(string fileName)
    {
        var source = Read(fileName);

        Assert.DoesNotMatch(new Regex(@"\beval\s*\("), source);
        Assert.DoesNotMatch(new Regex(@"\bnew\s+Function\b"), source);
        Assert.DoesNotMatch(new Regex(@"\bFunction\s*\("), source);
        Assert.DoesNotMatch(new Regex(@"\.innerHTML\b|\.outerHTML\b|insertAdjacentHTML"), source);
    }

    #region ------ Internals ------

    private const string FunctionsModuleFileName = "webawesome-functions.js";
    private const string InteropModuleFileName = "webawesome-interop.js";
    private const string InitializerFileName = "WebAwesome.Blazor.lib.module.js";

    private static string Read(string fileName)
    {
        var path = Path.Combine(WrapperProjectDirectory(), "wwwroot", fileName);
        Assert.True(File.Exists(path), $"JavaScript module not found: {path}");
        return File.ReadAllText(path);
    }

    private static string WrapperProjectDirectory([CallerFilePath] string thisFile = "")
    {
        // this test file lives in src\WebAwesome.Blazor.Tests\ApiParity\
        var testProjectDir = Path.GetDirectoryName(Path.GetDirectoryName(thisFile))!;
        return Path.Combine(Path.GetDirectoryName(testProjectDir)!, "WebAwesome.Blazor");
    }

    #endregion
}
