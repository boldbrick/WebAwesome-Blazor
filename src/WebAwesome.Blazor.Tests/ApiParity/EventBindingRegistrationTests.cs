using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Guards the two invariants that make wa-* custom event callbacks actually fire:
/// (1) every event binding in the render trees uses the "onwa-" attribute prefix - Blazor
/// silently never dispatches events bound under a bare "wa-" attribute name; and
/// (2) every bound event name is registered with Blazor.registerCustomEventType in the
/// JS initializer (WebAwesome.Blazor.lib.module.js) - unregistered custom events are
/// likewise never delivered to .NET. Both failure modes are silent at build time and at
/// runtime, which is why this is enforced by a source scan.
/// </summary>
public class EventBindingRegistrationTests
{
    /// <summary>
    /// No render tree may bind an event under a bare "wa-" attribute name; such bindings
    /// compile and render but the handler never fires.
    /// </summary>
    [Fact]
    public void NoEventBinding_UsesBareWaAttributeName()
    {
        var misses = new List<string>();

        foreach (var file in WrapperSourceFiles())
        {
            foreach (Match match in BareEventBindingRegex.Matches(File.ReadAllText(file)))
                misses.Add($"{Path.GetFileName(file)}: event bound as \"{match.Groups[1].Value}\" - must use the \"on\" prefix (\"on{match.Groups[1].Value}\")");
        }

        Assert.True(misses.Count == 0,
            $"Dead event bindings without the \"on\" prefix ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// Every wa-* event bound anywhere in the wrappers must be registered in the JS
    /// initializer's event name list, or the browser event never reaches .NET.
    /// </summary>
    [Fact]
    public void AllBoundEvents_AreRegisteredInJsInitializer()
    {
        var registered = RegisteredEventNames();
        Assert.NotEmpty(registered);

        var misses = new List<string>();

        foreach (var file in WrapperSourceFiles())
        {
            foreach (Match match in PrefixedEventBindingRegex.Matches(File.ReadAllText(file)))
            {
                var eventName = match.Groups[1].Value;
                if (!registered.Contains(eventName))
                    misses.Add($"{Path.GetFileName(file)}: bound event '{eventName}' is not registered in {JsInitializerFileName}");
            }
        }

        Assert.True(misses.Count == 0,
            $"Bound events missing registerCustomEventType registration ({misses.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, misses));
    }

    #region ------ Internals ------

    private const string JsInitializerFileName = "WebAwesome.Blazor.lib.module.js";

    // any AddAttribute/AddAttributeIfHasDelegate with a bare "wa-..." attribute name; real
    // wa-* content attributes do not exist (WA attributes are plain kebab-case), so every
    // match is a dead event binding
    private static readonly Regex BareEventBindingRegex = new(
        "AddAttribute\\w*\\(\\d+, \"(wa-[a-z-]+)\"", RegexOptions.Compiled);

    private static readonly Regex PrefixedEventBindingRegex = new(
        "AddAttribute\\w*\\(\\d+, \"on(wa-[a-z-]+)\"", RegexOptions.Compiled);

    private static readonly Regex RegisteredNameRegex = new(
        "^\\s*'(wa-[a-z-]+)',\\s*$", RegexOptions.Compiled | RegexOptions.Multiline);

    private static IEnumerable<string> WrapperSourceFiles()
    {
        var componentsDir = Path.Combine(WrapperProjectDirectory(), "Components");
        Assert.True(Directory.Exists(componentsDir), $"Wrapper source directory not found: {componentsDir}");
        return Directory.EnumerateFiles(componentsDir, "*.cs");
    }

    private static HashSet<string> RegisteredEventNames()
    {
        var modulePath = Path.Combine(WrapperProjectDirectory(), "wwwroot", JsInitializerFileName);
        Assert.True(File.Exists(modulePath), $"JS initializer not found: {modulePath}");

        return RegisteredNameRegex.Matches(File.ReadAllText(modulePath))
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string WrapperProjectDirectory([CallerFilePath] string thisFile = "")
    {
        // this test file lives in src\WebAwesome.Blazor.Tests\ApiParity\
        var testProjectDir = Path.GetDirectoryName(Path.GetDirectoryName(thisFile))!;
        return Path.Combine(Path.GetDirectoryName(testProjectDir)!, "WebAwesome.Blazor");
    }

    #endregion
}
