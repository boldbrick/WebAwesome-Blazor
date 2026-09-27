using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Exports the rendered event-binding data (RenderedWrapperCatalog) as the manifest the browser suite checks its
/// event coverage against: tools\e2e\data\event-callbacks.json lists every EventCallback parameter of every wrapper
/// with the tag it renders and the handler names the callback adds to the root element. The browser spec
/// tools\e2e\tests\event-coverage.spec.js requires each listed callback to be driven by a real-interaction
/// dispatch test or exempted with a reason, so a new or renamed callback fails here first (the manifest is stale)
/// and then in the browser suite until it is covered. The manifest is a snapshot: when this test fails, review the
/// received file written next to the test output and copy it over the checked-in manifest.
/// </summary>
public class EventCallbackManifestTests
{
    /// <summary>
    /// The checked-in manifest must equal the one computed from the rendered wrappers.
    /// </summary>
    [Fact]
    public void EventCallbackManifest_MatchesRenderedWrappers()
    {
        var actual = Serialize(BuildManifest());

        var receivedPath = Path.Combine(AppContext.BaseDirectory, ReceivedFileName);
        File.WriteAllText(receivedPath, actual);

        var manifestPath = ManifestPath();
        Assert.True(File.Exists(manifestPath),
            $"Event callback manifest not found at {manifestPath}. Copy '{receivedPath}' there to create it.");

        var approved = Normalize(File.ReadAllText(manifestPath));

        Assert.True(string.Equals(approved, Normalize(actual), StringComparison.Ordinal),
            "The wrappers' EventCallback parameters differ from the e2e event coverage manifest. " +
            $"Diff '{receivedPath}' against '{manifestPath}'; if the change is intentional, copy the received file over " +
            "the manifest, then cover every new callback in tools\\e2e (or exempt it with a reason in " +
            "tools\\e2e\\data\\event-coverage-exemptions.json) and drop removed ones from the coverage tables.");
    }

    #region ------ Internals ------

    private static readonly string[] ManifestDirectory = ["tools", "e2e", "data"];
    private const string ManifestFileName = "event-callbacks.json";
    private const string ReceivedFileName = "event-callbacks.received.json";
    private const string GeneratorPath = "src\\WebAwesome.Blazor.Tests\\ApiParity\\EventCallbackManifestTests.cs";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// The manifest document.
    /// </summary>
    /// <param name="GeneratedBy">Repository path of the generator (this test)</param>
    /// <param name="Callbacks">Every EventCallback parameter of every wrapper, sorted by id</param>
    private sealed record Manifest(string GeneratedBy, IReadOnlyList<ManifestCallback> Callbacks);

    /// <summary>
    /// One EventCallback parameter of one wrapper.
    /// </summary>
    /// <param name="Id">"Wrapper.Callback", e.g. "WaDialog.OnShow"</param>
    /// <param name="Tag">Tag the wrapper renders as its root element</param>
    /// <param name="Handlers">Event names (handler name without "on") the callback adds to the root element; empty
    /// when the callback is raised from a handler the wrapper always binds (e.g. a value-binding callback)</param>
    private sealed record ManifestCallback(string Id, string? Tag, IReadOnlyList<string> Handlers);

    private static Manifest BuildManifest()
    {
        var callbacks = RenderedWrapperCatalog.All
            .SelectMany(w => w.Callbacks.Select(c => new ManifestCallback(
                $"{w.ComponentType.Name}.{c.Name}",
                w.Tag,
                c.AddedHandlers
                    .Select(h => RenderedWrapperCatalog.EventNameOf(h) ?? h)
                    .OrderBy(e => e, StringComparer.Ordinal)
                    .ToList())))
            .OrderBy(c => c.Id, StringComparer.Ordinal)
            .ToList();

        Assert.NotEmpty(callbacks);
        return new Manifest(GeneratorPath, callbacks);
    }

    private static string Serialize(Manifest manifest)
    {
        // LF line ends on every platform, like the other files under tools\e2e
        return JsonSerializer.Serialize(manifest, SerializerOptions).Replace("\r\n", "\n") + "\n";
    }

    private static string ManifestPath()
    {
        // the wrapper project lives in src\WebAwesome.Blazor, two levels below the repository root
        var repoRoot = Path.GetDirectoryName(Path.GetDirectoryName(ApiParityData.WrapperProjectDirectory()))!;
        return Path.Combine([repoRoot, .. ManifestDirectory, ManifestFileName]);
    }

    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n").Trim();
    }

    #endregion
}
