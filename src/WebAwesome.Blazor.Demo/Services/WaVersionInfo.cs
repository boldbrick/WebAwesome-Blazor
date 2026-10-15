using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Demo.Services;

/// <summary>
/// Exposes the Web Awesome version the demo runs against: the version the loaded WebAwesome.Blazor
/// assembly binds, i.e. the first three parts of its package version (package 3.14.0.1 binds 3.14.0).
/// </summary>
public static class WaVersionInfo
{
    /// <summary>
    /// The bound Web Awesome version (e.g. "3.14.0"), without the bindings revision and build metadata.
    /// </summary>
    public static string Version { get; } = new WebAwesomeOptions().ResolveVersion();
}
