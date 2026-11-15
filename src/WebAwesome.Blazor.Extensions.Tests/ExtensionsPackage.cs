using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests;

/// <summary>
/// Access to the library under test: its assembly, loaded by name (the project reference copies it next to the test
/// binaries), and its sources in the source tree.
/// </summary>
internal static class ExtensionsPackage
{
    /// <summary>
    /// Name of the library assembly and of its project folder.
    /// </summary>
    public const string AssemblyName = "WebAwesome.Blazor.Extensions";

    /// <summary>
    /// The library assembly.
    /// </summary>
    public static Assembly Assembly { get; } = Assembly.Load(new AssemblyName(AssemblyName));

    /// <summary>
    /// Finds the source directory of the library project.
    /// </summary>
    /// <returns>Absolute path of src\WebAwesome.Blazor.Extensions</returns>
    public static string ProjectDirectory()
    {
        // this file lives in src\WebAwesome.Blazor.Extensions.Tests\ (the path of this file, not of the caller, which can
        // be in a subfolder)
        var testProjectDir = Path.GetDirectoryName(ThisFile())!;
        var projectDir = Path.Combine(Path.GetDirectoryName(testProjectDir)!, AssemblyName);
        Assert.True(Directory.Exists(projectDir), $"Library source directory not found: {projectDir}");
        return projectDir;
    }

    #region ------ Internals ------

    private static string ThisFile([CallerFilePath] string path = "") => path;

    #endregion
}
