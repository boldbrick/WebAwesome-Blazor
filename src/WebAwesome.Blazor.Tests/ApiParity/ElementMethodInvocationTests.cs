using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Verifies the inverse direction of the method parity check: every JavaScript element method a wrapper invokes
/// via <c>InvokeMethodAsync</c> must actually exist on the Web Awesome element it runs against. A method is
/// accepted when it is documented in the Custom Elements Manifest (expected-api-surface.json), is a native DOM
/// element method (parity-config.json "nativeElementMethods"), or is explicitly allowlisted for the component
/// ("extraElementMethods" — element methods verified against the Web Awesome source but absent from the CEM;
/// these must be re-verified manually on every upgrade). This guards against the class of bug where a wrapper
/// calls a renamed or nonexistent element method and fails only at runtime (e.g. the observers'
/// disconnect/reconnect → stopObserver/startObserver rename, and the dead "initialize" calls removed in 3.0.0).
/// Every C# file of the library is scanned, whatever the generic arguments of the call, and a method name passed
/// through a string parameter is traced to the literal call sites of the helper declaring it (WaVideo's
/// InvokeVoidElementMethodAsync). The element of a file is the tag its classes, or the concrete wrappers deriving
/// from them, actually render (RenderedWrapperCatalog), so a base class such as WaInputBase is checked against
/// every element its descendants render and the charts' OpenElement(0, TagName) resolves; a file with
/// invocations whose element cannot be resolved fails.
/// </summary>
public class ElementMethodInvocationTests
{
    /// <summary>
    /// Every element method name invoked from wrapper source must be known for every element the invoking
    /// class renders: CEM-documented, native DOM, or explicitly allowlisted with a per-component reason.
    /// </summary>
    [Fact]
    public void AllInvokedElementMethods_AreKnownElementMethods()
    {
        var misses = new List<string>();
        var invocationCount = 0;

        foreach (var file in WrapperSourceFiles())
        {
            var source = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);
            var invocations = InvokedMethods(source).ToList();
            if (invocations.Count == 0) continue;

            invocationCount += invocations.Count;
            misses.AddRange(invocations.Where(i => i.MethodName == null).Select(i =>
                $"{fileName}: invokes an element method named by '{i.Expression}', which traces to no string literal"));

            var tags = RenderedTagsOf(DeclaredTypeNames(source));
            if (tags.Count == 0)
            {
                misses.Add($"{fileName}: invokes element methods, but none of its classes (or their descendants) renders an element, so the methods cannot be checked");
                continue;
            }

            foreach (var tag in tags)
            {
                var known = KnownMethodsFor(tag);
                foreach (var methodName in invocations.Select(i => i.MethodName).OfType<string>().Distinct(StringComparer.Ordinal))
                {
                    if (!known.Contains(methodName))
                        misses.Add($"{fileName} ({tag}): invokes element method '{methodName}' " +
                            "which is neither CEM-documented, a native DOM method (nativeElementMethods), " +
                            "nor allowlisted in parity-config.json (extraElementMethods)");
                }
            }
        }

        // guard the scan itself: a pattern that silently stopped matching would pass vacuously
        Assert.True(invocationCount > 0, "The source scan found no element method invocations at all");

        AssertNoMisses(misses, "Unknown element method invocations");
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

        AssertNoMisses(misses, "Redundant extraElementMethods entries");
    }

    /// <summary>
    /// Allowlisted extra element methods must still be invoked by a wrapper of the element; an entry nothing
    /// invokes would silently admit a future misspelling.
    /// </summary>
    [Fact]
    public void ExtraElementMethods_AreInvoked()
    {
        var invoked = new HashSet<(string Tag, string Method)>();

        foreach (var file in WrapperSourceFiles())
        {
            var source = File.ReadAllText(file);
            var methods = InvokedMethods(source).Select(i => i.MethodName).OfType<string>().ToList();
            foreach (var tag in RenderedTagsOf(DeclaredTypeNames(source)))
                invoked.UnionWith(methods.Select(m => (tag, m)));
        }

        var misses = Config.Components
            .SelectMany(c => c.Value.ExtraElementMethods.Select(m => (Tag: c.Key, Method: m)))
            .Where(e => !invoked.Contains(e))
            .Select(e => $"{e.Tag}: extraElementMethods entry '{e.Method}' is invoked by no wrapper of the element and must be removed")
            .ToList();

        AssertNoMisses(misses, "Stale extraElementMethods entries");
    }

    /// <summary>
    /// Every "nativeElementMethods" entry must still be invoked by some wrapper; an entry nothing invokes would
    /// silently admit a misspelled call on any element.
    /// </summary>
    [Fact]
    public void NativeElementMethods_AreInvoked()
    {
        var invoked = WrapperSourceFiles()
            .SelectMany(file => InvokedMethods(File.ReadAllText(file)).Select(i => i.MethodName).OfType<string>())
            .ToHashSet(StringComparer.Ordinal);

        var misses = Config.NativeElementMethods
            .Where(m => !invoked.Contains(m))
            .Select(m => $"nativeElementMethods entry '{m}' is invoked by no wrapper and must be removed")
            .ToList();

        AssertNoMisses(misses, "Stale nativeElementMethods entries");
    }

    #region ------ Internals ------

    // a call of WebAwesomeJSInterop.InvokeMethodAsync (member access, so the declarations do not match) with any
    // generic arguments, however nested, and any element expression; the method name is a literal or an identifier
    private static readonly Regex InvocationRegex = new(
        "\\.InvokeMethodAsync\\b[^(]*\\(\\s*[^,()]+,\\s*(?:\"([^\"]+)\"|([A-Za-z_]\\w*))", RegexOptions.Compiled);

    private static readonly Regex DeclaredTypeRegex = new(
        "\\b(?:class|record)\\s+([A-Za-z_]\\w*)", RegexOptions.Compiled);

    /// <summary>
    /// An element method invocation found in source.
    /// </summary>
    /// <param name="MethodName">The invoked method name, or null when it cannot be traced to a literal</param>
    /// <param name="Expression">The method-name expression as written</param>
    internal sealed record ElementMethodInvocation(string? MethodName, string Expression);

    /// <summary>
    /// Finds the element method invocations in a source file. A method name passed as a string parameter of the
    /// enclosing helper method is resolved to the string literals the helper is called with.
    /// </summary>
    /// <param name="source">C# source text</param>
    /// <returns>The invocations; untraceable names have a null MethodName</returns>
    internal static IEnumerable<ElementMethodInvocation> InvokedMethods(string source)
    {
        foreach (Match match in InvocationRegex.Matches(source))
        {
            if (match.Groups[1].Success)
            {
                yield return new ElementMethodInvocation(match.Groups[1].Value, match.Groups[1].Value);
                continue;
            }

            var parameterName = match.Groups[2].Value;
            var helper = Regex.Match(source, $"\\b([A-Za-z_]\\w*)\\s*\\(\\s*string\\s+{Regex.Escape(parameterName)}\\b");
            var literals = helper.Success
                ? Regex.Matches(source, $"\\b{Regex.Escape(helper.Groups[1].Value)}\\s*\\(\\s*\"([^\"]+)\"").Select(m => m.Groups[1].Value).ToList()
                : new List<string>();

            if (literals.Count == 0)
            {
                yield return new ElementMethodInvocation(null, parameterName);
                continue;
            }

            foreach (var literal in literals)
                yield return new ElementMethodInvocation(literal, parameterName);
        }
    }

    /// <summary>
    /// Returns the names of the classes and records a source file declares.
    /// </summary>
    /// <param name="source">C# source text</param>
    /// <returns>The declared type names</returns>
    internal static IReadOnlySet<string> DeclaredTypeNames(string source)
    {
        return DeclaredTypeRegex.Matches(source).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Returns the tags rendered by the wrappers that are, or derive from, one of the named types.
    /// </summary>
    /// <param name="typeNames">Type names (generic arity stripped)</param>
    /// <returns>The rendered tags</returns>
    internal static IReadOnlySet<string> RenderedTagsOf(IReadOnlySet<string> typeNames)
    {
        return RenderedWrapperCatalog.All
            .Where(w => w.Tag != null && IsOrDerivesFromAny(w.ComponentType, typeNames))
            .Select(w => w.Tag!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool IsOrDerivesFromAny(Type type, IReadOnlySet<string> typeNames)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            if (current.Assembly == WrapperAssembly && typeNames.Contains(StripGenericArity(current.Name))) return true;
        }

        return false;
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
