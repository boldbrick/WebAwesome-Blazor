using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Verifies the inverse direction of the method parity check: every JavaScript element
/// method a wrapper invokes via <c>InvokeMethodAsync</c> must actually exist on the bound
/// Web Awesome element. A method is accepted when it is documented in the Custom Elements
/// Manifest (expected-api-surface.json), is a native DOM element method
/// (parity-config.json "nativeElementMethods"), or is explicitly allowlisted for the
/// component ("extraElementMethods" — element methods verified against the Web Awesome
/// source but absent from the CEM; these must be re-verified manually on every upgrade).
/// This guards against the class of bug where a wrapper calls a renamed or nonexistent
/// element method and fails only at runtime (e.g. the observers' disconnect/reconnect →
/// stopObserver/startObserver rename, and the dead "initialize" calls removed in 3.0.0).
/// </summary>
public class ElementMethodInvocationTests
{
    /// <summary>
    /// Every element method name invoked from wrapper source must be known: CEM-documented,
    /// native DOM, or explicitly allowlisted with a per-component reason.
    /// </summary>
    [Fact]
    public void AllInvokedElementMethods_AreKnownElementMethods()
    {
        var misses = new List<string>();

        foreach (var file in WrapperSourceFiles())
        {
            var source = File.ReadAllText(file);

            var tagMatch = TagRegex.Match(source);
            if (!tagMatch.Success) continue;
            var tag = tagMatch.Groups[1].Value;

            var known = KnownMethodsFor(tag);

            foreach (Match invocation in InvocationRegex.Matches(source))
            {
                var methodName = invocation.Groups[1].Value;
                if (!known.Contains(methodName))
                    misses.Add($"{Path.GetFileName(file)} ({tag}): invokes element method '{methodName}' " +
                        "which is neither CEM-documented, a native DOM method (nativeElementMethods), " +
                        "nor allowlisted in parity-config.json (extraElementMethods)");
            }
        }

        Assert.True(misses.Count == 0,
            $"Unknown element method invocations ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Allowlisted extra element methods must not shadow CEM-documented ones; when the CEM
    /// starts documenting a method, the allowlist entry has to be removed so the regular
    /// parity checks own it again.
    /// </summary>
    [Fact]
    public void ExtraElementMethods_AreNotCemDocumented()
    {
        var misses = new List<string>();

        foreach (var (tag, componentConfig) in Config.Components)
        {
            if (!Surface.Components.TryGetValue(tag, out var component)) continue;

            foreach (var methodName in componentConfig.ExtraElementMethods)
            {
                if (component.Methods.ContainsKey(methodName))
                    misses.Add($"{tag}: '{methodName}' is CEM-documented and must not be listed in extraElementMethods");
            }
        }

        Assert.True(misses.Count == 0,
            $"Redundant extraElementMethods entries ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    #region ------ Internals ------

    private const string DataDirectory = "ApiParity";
    private const string SurfaceFileName = "expected-api-surface.json";
    private const string ConfigFileName = "parity-config.json";

    // the component's own element is always opened at sequence 0; nested helper elements use
    // later sequence numbers, so the first wa-* OpenElement identifies the wrapped tag
    private static readonly Regex TagRegex = new(
        "OpenElement\\(0,\\s*\"(wa-[a-z0-9-]+)\"\\)", RegexOptions.Compiled);

    private static readonly Regex InvocationRegex = new(
        "InvokeMethodAsync(?:<[^>(]+>)?\\(\\s*Element\\.Value,\\s*\"([^\"]+)\"", RegexOptions.Compiled);

    private static readonly ApiSurface Surface = LoadDataFile<ApiSurface>(SurfaceFileName);
    private static readonly ParityConfig Config = LoadDataFile<ParityConfig>(ConfigFileName);

    private static T LoadDataFile<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, DataDirectory, fileName);
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<T>(stream)
            ?? throw new InvalidOperationException($"Failed to deserialize {path}");
    }

    private static IEnumerable<string> WrapperSourceFiles()
    {
        var componentsDir = Path.Combine(WrapperProjectDirectory(), "Components");
        Assert.True(Directory.Exists(componentsDir), $"Wrapper source directory not found: {componentsDir}");
        return Directory.EnumerateFiles(componentsDir, "*.cs");
    }

    private static string WrapperProjectDirectory([CallerFilePath] string thisFile = "")
    {
        // this test file lives in src\WebAwesome.Blazor.Tests\ApiParity\
        var testProjectDir = Path.GetDirectoryName(Path.GetDirectoryName(thisFile))!;
        return Path.Combine(Path.GetDirectoryName(testProjectDir)!, "WebAwesome.Blazor");
    }

    private static HashSet<string> KnownMethodsFor(string tag)
    {
        var known = new HashSet<string>(Config.NativeElementMethods, StringComparer.Ordinal);

        if (Surface.Components.TryGetValue(tag, out var component))
            known.UnionWith(component.Methods.Keys);

        if (Config.Components.TryGetValue(tag, out var componentConfig))
            known.UnionWith(componentConfig.ExtraElementMethods);

        return known;
    }

    #endregion
}
