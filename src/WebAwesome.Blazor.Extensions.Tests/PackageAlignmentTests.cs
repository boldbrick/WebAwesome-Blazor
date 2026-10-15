using System.Reflection;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests;

/// <summary>
/// The Extensions package ships with the same version as WebAwesome.Blazor and depends on it with a floor of
/// that version, so a consumer always pairs the two packages of one release. Both projects take their version
/// from src\Version.props; these tests fail when one of them overrides it or the assembly identity drifts.
/// </summary>
public class PackageAlignmentTests
{
    [Fact]
    public void InformationalVersion_EqualsTheMainPackageVersion()
    {
        var extensions = InformationalVersion(ExtensionsPackage.Assembly);
        var main = InformationalVersion(MainAssembly);

        Assert.False(string.IsNullOrEmpty(main), "WebAwesome.Blazor has no informational version");
        Assert.Equal(main, extensions);
    }

    [Fact]
    public void AssemblyVersion_EqualsTheMainAssemblyVersion()
    {
        Assert.Equal(MainAssembly.GetName().Version, ExtensionsPackage.Assembly.GetName().Version);
    }

    [Fact]
    public void AssemblyName_IsThePackageId()
    {
        Assert.Equal(ExtensionsPackage.AssemblyName, ExtensionsPackage.Assembly.GetName().Name);
    }

    #region ------ Internals ------

    private static readonly Assembly MainAssembly = typeof(WaButton).Assembly;

    // the informational version without a SourceLink "+<commit>" suffix, which CI builds append
    private static string? InformationalVersion(Assembly assembly)
    {
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (version is null) return null;

        var metadataStart = version.IndexOf('+');
        return metadataStart < 0 ? version : version[..metadataStart];
    }

    #endregion
}
