using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests;

/// <summary>
/// Access to the library under test. The library has no public type yet, so the tests load its assembly by name
/// (the project reference copies it next to the test binaries) and read its sources from the source tree.
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
    /// <param name="thisFile">Path of this source file, supplied by the compiler</param>
    /// <returns>Absolute path of src\WebAwesome.Blazor.Extensions</returns>
    public static string ProjectDirectory([CallerFilePath] string thisFile = "")
    {
        // this file lives in src\WebAwesome.Blazor.Extensions.Tests\
        var testProjectDir = Path.GetDirectoryName(thisFile)!;
        var projectDir = Path.Combine(Path.GetDirectoryName(testProjectDir)!, AssemblyName);
        Assert.True(Directory.Exists(projectDir), $"Library source directory not found: {projectDir}");
        return projectDir;
    }
}
