using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;

namespace WebAwesome.Blazor.Tests;

/// <summary>
/// The target framework the tests run on, for the checks whose oracle legitimately differs per framework (the public
/// API baseline and the parity allowlists; net11.0 has the WaInputBound union, docs\technical\technical.md,
/// Multi-targeting).
/// </summary>
public static class TestTargetFramework
{
    /// <summary>
    /// The moniker of the running target framework, e.g. "net11.0".
    /// </summary>
    public static string Moniker { get; } = Load();

    #region ------ Internals ------

    // the moniker of a .NET 5+ framework: net<major>.<minor>
    private const string MonikerFormat = "net{0}.{1}";

    private static string Load()
    {
        var frameworkName = typeof(TestTargetFramework).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName
            ?? throw new InvalidOperationException("The test assembly has no TargetFrameworkAttribute.");
        var version = new FrameworkName(frameworkName).Version;
        return string.Format(CultureInfo.InvariantCulture, MonikerFormat, version.Major, version.Minor);
    }

    #endregion
}
