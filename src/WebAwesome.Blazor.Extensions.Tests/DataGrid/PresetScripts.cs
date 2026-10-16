using System.IO;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// Access to the JavaScript files of the package in the source tree.
/// </summary>
internal static class PresetScripts
{
    /// <summary>
    /// The preset functions and the cell action bridge.
    /// </summary>
    public const string PresetsFileName = "data-grid-presets.js";

    /// <summary>
    /// The HTML of the static Razor templates: the one HTML parse of the package.
    /// </summary>
    public const string TemplatesFileName = "data-grid-templates.js";

    /// <summary>
    /// The request handlers of ctx.invoke.
    /// </summary>
    public const string RequestsFileName = "data-grid-requests.js";

    /// <summary>
    /// The ctx helpers of data grid functions.
    /// </summary>
    public const string ContextFileName = "data-grid-context.js";

    /// <summary>
    /// The JS initializer of the package.
    /// </summary>
    public const string InitializerFileName = ExtensionsPackage.AssemblyName + ".lib.module.js";

    /// <summary>
    /// Reads a file of the package's wwwroot folder.
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The text with Unix line ends</returns>
    public static string Read(string fileName)
    {
        var path = Path.Combine(ExtensionsPackage.ProjectDirectory(), WebRootDirectory, fileName);
        Assert.True(File.Exists(path), $"{fileName} not found at {path}");
        return File.ReadAllText(path).Replace("\r\n", "\n");
    }

    /// <summary>
    /// The JavaScript files of the package's wwwroot folder.
    /// </summary>
    /// <returns>The full paths</returns>
    public static string[] All()
    {
        return Directory.GetFiles(Path.Combine(ExtensionsPackage.ProjectDirectory(), WebRootDirectory), "*.js", SearchOption.AllDirectories);
    }

    #region ------ Internals ------

    private const string WebRootDirectory = "wwwroot";

    #endregion
}
