using System;
using System.IO;
using PublicApiGenerator;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.PublicApi;

/// <summary>
/// Snapshots the library's own public API surface to catch accidental breaking changes
/// between our releases. The API parity tests verify completeness against upstream Web
/// Awesome; this test verifies stability of what we ship to consumers. When a change is
/// intentional (e.g. a Web Awesome upgrade), regenerate the baseline as described in the
/// failure message and review the diff as part of the release notes.
/// </summary>
public class PublicApiSnapshotTests
{
    [Fact]
    public void PublicApi_MatchesApprovedBaseline()
    {
        var actual = Normalize(typeof(WaButton).Assembly.GeneratePublicApi(GeneratorOptions));

        // write the current surface next to the test output so an intentional change can be
        // diffed and promoted to the new baseline without re-deriving it by hand
        var receivedPath = Path.Combine(AppContext.BaseDirectory, DataDirectory, ReceivedFileName);
        File.WriteAllText(receivedPath, actual);

        var approvedPath = Path.Combine(AppContext.BaseDirectory, DataDirectory, ApprovedFileName);
        Assert.True(File.Exists(approvedPath),
            $"Approved public API baseline not found at {approvedPath}. " +
            $"Copy '{receivedPath}' to 'src\\WebAwesome.Blazor.Tests\\{DataDirectory}\\{ApprovedFileName}' to create it.");

        var approved = Normalize(File.ReadAllText(approvedPath));

        Assert.True(string.Equals(approved, actual, StringComparison.Ordinal),
            "The public API surface of WebAwesome.Blazor differs from the approved baseline. " +
            $"Diff '{receivedPath}' against the baseline; if the change is intentional, copy it to " +
            $"'src\\WebAwesome.Blazor.Tests\\{DataDirectory}\\{ApprovedFileName}' and mention the change in docs\\CHANGELOG.md.");
    }

    #region ------ Internals ------

    private const string DataDirectory = "PublicApi";
    private const string ApprovedFileName = "approved-public-api.txt";
    private const string ReceivedFileName = "received-public-api.txt";

    // assembly-level attributes vary per target framework (e.g. TargetFrameworkAttribute),
    // while the type surface is identical for net9.0/net10.0 - exclude them so a single
    // baseline serves both builds
    private static readonly ApiGeneratorOptions GeneratorOptions = new()
    {
        IncludeAssemblyAttributes = false,
    };

    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n").Trim();
    }

    #endregion
}
