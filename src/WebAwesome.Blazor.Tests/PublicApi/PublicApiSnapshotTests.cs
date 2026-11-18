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
/// failure message and review the diff as part of the release notes. A target framework whose
/// surface legitimately differs (net11.0: the WaInputBound union of WaInput) has a baseline of
/// its own, approved-public-api.&lt;tfm&gt;.txt; every other target framework shares
/// approved-public-api.txt.
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

        // the baseline of this target framework when it has one of its own, else the shared one
        var frameworkFileName = string.Format(FrameworkApprovedFileNameFormat, TestTargetFramework.Moniker);
        var approvedFileName = File.Exists(Path.Combine(AppContext.BaseDirectory, DataDirectory, frameworkFileName))
            ? frameworkFileName
            : ApprovedFileName;
        var approvedPath = Path.Combine(AppContext.BaseDirectory, DataDirectory, approvedFileName);
        Assert.True(File.Exists(approvedPath),
            $"Approved public API baseline not found at {approvedPath}. " +
            $"Copy '{receivedPath}' to 'src\\WebAwesome.Blazor.Tests\\{DataDirectory}\\{ApprovedFileName}' to create it.");

        var approved = Normalize(File.ReadAllText(approvedPath));

        Assert.True(string.Equals(approved, actual, StringComparison.Ordinal),
            $"The public API surface of WebAwesome.Blazor ({TestTargetFramework.Moniker}) differs from the approved baseline. " +
            $"Diff '{receivedPath}' against the baseline; if the change is intentional, copy it to " +
            $"'src\\WebAwesome.Blazor.Tests\\{DataDirectory}\\{approvedFileName}' and mention the change in docs\\CHANGELOG.md.");
    }

    #region ------ Internals ------

    private const string DataDirectory = "PublicApi";
    private const string ApprovedFileName = "approved-public-api.txt";
    private const string FrameworkApprovedFileNameFormat = "approved-public-api.{0}.txt";
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
