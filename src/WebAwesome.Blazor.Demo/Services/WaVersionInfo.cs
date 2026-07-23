using System;
using System.Reflection;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Demo.Services;

/// <summary>
/// Exposes the Web Awesome version the demo runs against, resolved from the loaded
/// WebAwesome.Blazor assembly's informational version metadata (the library version tracks
/// the bound Web Awesome release one-to-one).
/// </summary>
public static class WaVersionInfo
{
    /// <summary>
    /// The bound Web Awesome version (e.g. "3.2.0"), without build metadata.
    /// </summary>
    public static string Version { get; } = ResolveVersion();

    #region ------ Internals ------

    private static string ResolveVersion()
    {
        var informational = typeof(WebAwesomeOptions).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrEmpty(informational)) return string.Empty;

        // strip build metadata appended by CI builds (e.g. "3.2.0+abc123")
        var plusIndex = informational.IndexOf('+');
        return plusIndex < 0 ? informational : informational[..plusIndex];
    }

    #endregion
}
