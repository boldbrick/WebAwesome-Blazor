using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests;

/// <summary>
/// The Blazor runtime loads a package's JS initializer only when the file is wwwroot\&lt;PackageId&gt;.lib.module.js,
/// and calls only the exported functions with the names it knows: beforeWebStart and afterWebStarted for Blazor Web
/// (blazor.web.js), beforeStart and afterStarted for the classic hosts. A wrong file name or a missing export fails
/// silently in the browser: the preset functions would not be registered, or the cell action event would not reach
/// .NET.
/// </summary>
public class JsInitializerContractTests
{
    [Fact]
    public void Initializer_IsNamedAfterThePackageId()
    {
        Assert.True(File.Exists(InitializerPath()), $"JS initializer not found at {InitializerPath()}");
    }

    [Theory]
    [InlineData("beforeWebStart")]
    [InlineData("afterWebStarted")]
    [InlineData("beforeStart")]
    [InlineData("afterStarted")]
    public void Initializer_ExportsTheStartCallback(string name)
    {
        var source = File.ReadAllText(InitializerPath());
        var export = new Regex($@"^export\s+(async\s+)?function\s+{name}\s*\(", RegexOptions.Multiline);

        Assert.True(export.IsMatch(source), $"{FileName} does not export the function '{name}'");
    }

    #region ------ Internals ------

    private const string WebRootDirectory = "wwwroot";
    private const string FileName = ExtensionsPackage.AssemblyName + ".lib.module.js";

    private static string InitializerPath()
    {
        return Path.Combine(ExtensionsPackage.ProjectDirectory(), WebRootDirectory, FileName);
    }

    #endregion
}
