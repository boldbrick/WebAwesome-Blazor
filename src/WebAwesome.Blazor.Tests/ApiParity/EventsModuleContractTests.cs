using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Guards the public module of the custom event types, wwwroot\webawesome-events.js: the JS initializers of
/// WebAwesome.Blazor, of the Extensions package and of sister wrapper sets import it by path and call its exports, so a
/// renamed export or a moved registry fails only in the browser.
/// </summary>
public class EventsModuleContractTests
{
    [Theory]
    [InlineData("registerEventTypes")]
    [InlineData("sanitize")]
    [InlineData("detailArgs")]
    [InlineData("isoDate")]
    public void EventsModule_ExportsTheContractFunction(string name)
    {
        Assert.Matches(new Regex($"^export\\s+function\\s+{name}\\s*\\(", RegexOptions.Multiline), Read(EventsModuleFileName));
    }

    [Fact]
    public void EventsModule_KeepsTheRegisteredNamesOnASharedSymbol()
    {
        var source = Read(EventsModuleFileName);

        // every copy of the module (another base path, another version) must see the same registrations
        Assert.Contains("Symbol.for('WebAwesome.Blazor.eventTypes')", source);
        Assert.Contains("globalThis[registryKey]", source);
    }

    [Fact]
    public void EventsModule_SkipsADuplicateWithAWarning()
    {
        var source = Read(EventsModuleFileName);

        Assert.Matches(new Regex(@"if \(names\.has\(name\)\) \{\s*console\.warn\([^\n]*\);\s*continue;\s*\}"), source);
        Assert.DoesNotMatch(new Regex(@"\bthrow\b"), source);
    }

    [Fact]
    public void Initializer_RegistersThroughTheEventsModule()
    {
        var source = Read(InitializerFileName);

        Assert.Matches(new Regex(@"^import \{[^}]*\bregisterEventTypes\b[^}]*\} from '\./webawesome-events\.js';", RegexOptions.Multiline), source);

        // the helpers moved to the events module; the initializer must not keep a second copy
        Assert.DoesNotMatch(new Regex(@"^function\s+(sanitize|detailArgs|isoDate)\s*\(", RegexOptions.Multiline), source);
        Assert.DoesNotContain("blazor.registerCustomEventType(", source);
    }

    [Fact]
    public void EventsModule_NeverEvaluatesText()
    {
        var source = Read(EventsModuleFileName);

        Assert.DoesNotMatch(new Regex(@"\beval\s*\("), source);
        Assert.DoesNotMatch(new Regex(@"\bnew\s+Function\b"), source);
        Assert.DoesNotMatch(new Regex(@"\bFunction\s*\("), source);
        Assert.DoesNotMatch(new Regex(@"\.innerHTML\b|\.outerHTML\b|insertAdjacentHTML"), source);
    }

    #region ------ Internals ------

    private const string EventsModuleFileName = "webawesome-events.js";
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
