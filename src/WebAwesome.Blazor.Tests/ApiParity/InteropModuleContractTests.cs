using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Guards the contract between the C# interop layer and wwwroot\webawesome-interop.js: every identifier the C#
/// side invokes on the imported interop module must be exported by the module, and every export must be invoked
/// (or allowlisted with a reason). A missing export fails only at runtime, in the browser, and every bUnit or Moq
/// test that mocks the module or WebAwesomeJSInterop passes regardless - the icon library service called four
/// functions that did not exist while its tests were green. The check is therefore a source scan, which also
/// keeps all JS calls behind the module, where they can be checked.
/// </summary>
public class InteropModuleContractTests
{
    /// <summary>
    /// Every module.Invoke*Async("identifier", ...) in the wrapper library must name an export of the interop module.
    /// </summary>
    [Fact]
    public void AllInvokedModuleIdentifiers_AreExportedByInteropModule()
    {
        var exported = ExportedFunctions();
        var invoked = InvokedIdentifiers();
        Assert.NotEmpty(invoked);

        var misses = invoked
            .Where(i => !exported.Contains(i.Identifier))
            .Select(i => $"{i.File}: invokes '{i.Identifier}', which {InteropModuleFileName} does not export")
            .ToList();

        Assert.True(misses.Count == 0,
            $"Interop identifiers without a JS export ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Every export of the interop module must be invoked from C#, unless allowlisted with a reason; an unused
    /// export usually means the C# side calls it under a different (misspelt) name.
    /// </summary>
    [Fact]
    public void AllInteropModuleExports_AreInvokedOrAllowlisted()
    {
        var invoked = InvokedIdentifiers().Select(i => i.Identifier).ToHashSet(StringComparer.Ordinal);

        var misses = ExportedFunctions()
            .Where(e => !invoked.Contains(e) && !UnusedExportAllowlist.ContainsKey(e))
            .Select(e => $"{InteropModuleFileName}: export '{e}' is never invoked from C#")
            .ToList();

        Assert.True(misses.Count == 0,
            $"Unused interop exports ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Outside the interop module the wrapper library may invoke no global JS identifier except the module
    /// "import": a global (e.g. "eval" with an element argument, which WaPopup.RepositionAsync used and which
    /// cannot see its arguments) is invisible to this contract check and to every test that mocks JS.
    /// </summary>
    [Fact]
    public void NoGlobalJsInvocations_OtherThanModuleImport()
    {
        var misses = new List<string>();
        foreach (var file in WrapperSourceFiles())
        {
            foreach (Match match in GlobalInvocationRegex.Matches(File.ReadAllText(file)))
            {
                if (match.Groups[1].Value != "module" && match.Groups[2].Value != ModuleImportIdentifier)
                    misses.Add($"{Path.GetFileName(file)}: invokes global JS '{match.Groups[2].Value}' via {match.Groups[1].Value}");
            }
        }

        Assert.True(misses.Count == 0,
            $"Global JS invocations - route them through webawesome-interop.js ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Allowlist entries must still be exports, so a stale entry cannot silently widen the allowlist.
    /// </summary>
    [Fact]
    public void UnusedExportAllowlist_OnlyNamesExistingExports()
    {
        var exported = ExportedFunctions();
        Assert.All(UnusedExportAllowlist.Keys, name => Assert.Contains(name, exported));
    }

    /// <summary>
    /// The module path the C# side imports must be the interop module file this contract is checked against.
    /// </summary>
    [Fact]
    public void ImportedModulePath_PointsAtInteropModule()
    {
        var source = File.ReadAllText(Path.Combine(WrapperProjectDirectory(), "Base", "WebAwesomeJSInterop.cs"));
        Assert.Contains($"\"./_content/WebAwesome.Blazor/{InteropModuleFileName}\"", source);
        Assert.True(File.Exists(InteropModulePath()), $"Interop module not found: {InteropModulePath()}");
    }

    #region ------ Internals ------

    private const string InteropModuleFileName = "webawesome-interop.js";

    // exports deliberately not invoked from C#, each with the reason
    private static readonly Dictionary<string, string> UnusedExportAllowlist = new(StringComparer.Ordinal);

    // module.InvokeAsync<T>("id", ...), module.InvokeVoidAsync("id", ...); the variable holding the imported
    // interop module is named "module" throughout the wrapper library
    private static readonly Regex ModuleInvocationRegex = new(
        "\\bmodule\\.Invoke(?:Void)?Async(?:<[^>(]+>)?\\(\\s*\"([A-Za-z_$][\\w$]*)\"", RegexOptions.Compiled);

    private const string ModuleImportIdentifier = "import";

    // any receiver.InvokeAsync<T>("id", ...) / receiver.InvokeVoidAsync("id", ...)
    private static readonly Regex GlobalInvocationRegex = new(
        "\\b(\\w+)\\.Invoke(?:Void)?Async(?:<[^>(]+>)?\\(\\s*\"([^\"]+)\"", RegexOptions.Compiled);

    private static readonly Regex ExportedFunctionRegex = new(
        "^export\\s+(?:async\\s+)?function\\s+([A-Za-z_$][\\w$]*)\\s*\\(", RegexOptions.Compiled | RegexOptions.Multiline);

    private static HashSet<string> ExportedFunctions()
    {
        var modulePath = InteropModulePath();
        Assert.True(File.Exists(modulePath), $"Interop module not found: {modulePath}");

        var exported = ExportedFunctionRegex.Matches(File.ReadAllText(modulePath))
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);
        Assert.NotEmpty(exported);
        return exported;
    }

    private static IEnumerable<string> WrapperSourceFiles()
    {
        return Directory.EnumerateFiles(WrapperProjectDirectory(), "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsBuildOutput(f));
    }

    private static List<(string File, string Identifier)> InvokedIdentifiers()
    {
        var invoked = new List<(string File, string Identifier)>();
        foreach (var file in WrapperSourceFiles())
        {
            foreach (Match match in ModuleInvocationRegex.Matches(File.ReadAllText(file)))
                invoked.Add((Path.GetFileName(file), match.Groups[1].Value));
        }

        return invoked;
    }

    private static bool IsBuildOutput(string path)
    {
        var separator = Path.DirectorySeparatorChar;
        return path.Contains($"{separator}bin{separator}", StringComparison.OrdinalIgnoreCase)
            || path.Contains($"{separator}obj{separator}", StringComparison.OrdinalIgnoreCase);
    }

    private static string InteropModulePath() => Path.Combine(WrapperProjectDirectory(), "wwwroot", InteropModuleFileName);

    private static string WrapperProjectDirectory([CallerFilePath] string thisFile = "")
    {
        // this test file lives in src\WebAwesome.Blazor.Tests\ApiParity\
        var testProjectDir = Path.GetDirectoryName(Path.GetDirectoryName(thisFile))!;
        return Path.Combine(Path.GetDirectoryName(testProjectDir)!, "WebAwesome.Blazor");
    }

    #endregion
}
