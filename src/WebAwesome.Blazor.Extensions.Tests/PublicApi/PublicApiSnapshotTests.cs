using System;
using System.IO;
using PublicApiGenerator;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.PublicApi;

/// <summary>
/// Snapshots the public API surface of WebAwesome.Blazor.Extensions to catch accidental breaking changes
/// between our releases, as the main package's snapshot does for WebAwesome.Blazor. The Extensions package
/// does not follow the Web Awesome custom-elements manifest, so this snapshot is its only surface check. When
/// a change is intentional, promote the received file to the baseline as the failure message describes and
/// review the diff as part of the release notes.
/// </summary>
public class PublicApiSnapshotTests
{
    [Fact]
    public void PublicApi_MatchesApprovedBaseline()
    {
        var actual = Normalize(ExtensionsPackage.Assembly.GeneratePublicApi(GeneratorOptions));

        // write the current surface next to the test output so an intentional change can be
        // diffed and promoted to the new baseline without re-deriving it by hand
        var receivedPath = Path.Combine(AppContext.BaseDirectory, DataDirectory, ReceivedFileName);
        File.WriteAllText(receivedPath, actual);

        var approvedPath = Path.Combine(AppContext.BaseDirectory, DataDirectory, ApprovedFileName);
        Assert.True(File.Exists(approvedPath),
            $"Approved public API baseline not found at {approvedPath}. " +
            $"Copy '{receivedPath}' to 'src\\WebAwesome.Blazor.Extensions.Tests\\{DataDirectory}\\{ApprovedFileName}' to create it.");

        var approved = Normalize(File.ReadAllText(approvedPath));

        Assert.True(string.Equals(approved, actual, StringComparison.Ordinal),
            "The public API surface of WebAwesome.Blazor.Extensions differs from the approved baseline. " +
            $"Diff '{receivedPath}' against the baseline; if the change is intentional, copy it to " +
            $"'src\\WebAwesome.Blazor.Extensions.Tests\\{DataDirectory}\\{ApprovedFileName}' and mention the change in docs\\CHANGELOG.md.");
    }

    #region ------ Internals ------

    // the main test project builds into the same output folder, so the file names carry the package name
    private const string DataDirectory = "PublicApi";
    private const string ApprovedFileName = "approved-extensions-public-api.txt";
    private const string ReceivedFileName = "received-extensions-public-api.txt";

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
