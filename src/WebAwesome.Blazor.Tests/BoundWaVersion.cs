using System;
using System.IO;
using System.Text.Json;

namespace WebAwesome.Blazor.Tests;

/// <summary>
/// Central source of the bound Web Awesome version for tests. Reads targetWaVersion from
/// ApiParity\parity-config.json (copied to the test output), which the release preflight gates
/// against src\Version.props — so no test hard-codes the version and upgrades bump it in exactly
/// one place.
/// </summary>
public static class BoundWaVersion
{
    /// <summary>
    /// The Web Awesome version the library is bound to, e.g. "3.1.0".
    /// </summary>
    public static string Value { get; } = Load();

    #region ------ Internals ------

    private static string Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "ApiParity", "parity-config.json");
        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream);
        var version = document.RootElement.GetProperty("targetWaVersion").GetString();
        if (string.IsNullOrEmpty(version))
            throw new InvalidOperationException($"targetWaVersion missing in {path}");
        return version;
    }

    #endregion
}
