using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// The C# factories refer to the preset functions by their registered names (WaJsFunction.Registered), and the browser
/// finds a function only when the Extensions initializer registered it under exactly that name; a missing name makes
/// the column show raw values after one console error. The presets also build their DOM without parsing HTML (design
/// section 11.6), and the package never evaluates text as code.
/// </summary>
public class PresetScriptContractTests
{
    [Fact]
    public void EveryPresetName_IsInThePresetTable_AndNothingElse()
    {
        var presets = PresetScripts.Read(PresetScripts.PresetsFileName);
        var table = Regex.Match(presets, @"export const presets = Object\.freeze\(\{(?<entries>.*?)\}\);", RegexOptions.Singleline);
        Assert.True(table.Success, "data-grid-presets.js does not export the presets table");

        var registered = Regex.Matches(table.Groups["entries"].Value, @"'([^']+)':").Select(match => match.Groups[1].Value).Order().ToArray();
        var names = PresetNamesOfTheFactories().Order().ToArray();

        Assert.NotEmpty(names);
        Assert.Equal(names, registered);
    }

    [Fact]
    public void Initializer_RegistersEveryPresetBeforeBlazorStarts()
    {
        var initializer = PresetScripts.Read(PresetScripts.InitializerFileName);

        Assert.Matches(@"import \{ registerFunction, registerContextDecorator \} from '\.\./WebAwesome\.Blazor/webawesome-functions\.js';", initializer);
        Assert.Matches(@"for \(const \[name, preset\] of Object\.entries\(presets\)\) \{\s*registerFunction\(name, preset\);", initializer);
        // the ctx helpers (ctx.emit, ctx.h, ...) are a context decorator, registered in the same step as the presets
        Assert.Matches(@"import \{ decorateContext \} from '\./data-grid-context\.js';", initializer);
        Assert.Matches(@"registerFunction\(name, preset\);\s*\}\s*registerContextDecorator\(decorateContext\);", initializer);
        foreach (var callback in new[] { "beforeWebStart", "beforeStart" })
        {
            Assert.Matches($@"export function {callback}\(\) \{{\s*registerPresets\(\);", initializer);
        }
    }

    [Theory]
    [InlineData("innerHTML")]
    [InlineData("outerHTML")]
    [InlineData("insertAdjacentHTML")]
    [InlineData("document.write")]
    [InlineData("eval(")]
    [InlineData("new Function")]
    [InlineData("createContextualFragment")]
    public void PackageScripts_NeverParseHtmlOrEvaluateText(string member)
    {
        foreach (var path in PresetScripts.All())
        {
            // the one HTML parse of the package, the Razor template HTML in a <template> element (checked below)
            var code = string.Join('\n', File.ReadAllLines(path)
                .Where(line => !line.TrimStart().StartsWith("//") && !line.TrimStart().StartsWith('*'))
                .Where(line => !(Path.GetFileName(path) == PresetScripts.TemplatesFileName && line.Trim() == TemplateParseLine)));
            Assert.False(code.Contains(member), $"{Path.GetFileName(path)} uses {member}");
        }
    }

    [Fact]
    public void RazorTemplates_AreParsedOnceInAnInertTemplateElement()
    {
        // the HTML comes from the Razor renderer of the app (text encoded); a <template> parses it into an inert
        // document, so no script runs and no element upgrades while it is parsed
        var templates = PresetScripts.Read(PresetScripts.TemplatesFileName);

        Assert.Single(Regex.Matches(templates, Regex.Escape(TemplateParseLine)));
        Assert.Matches(@"template = document\.createElement\('template'\);\s*template\.innerHTML = html;", templates);
        Assert.Contains("document.importNode(parsedTemplate(html).content, true)", templates);
        Assert.Matches(@"import \{ templateNode \} from '\./data-grid-templates\.js';", PresetScripts.Read(PresetScripts.PresetsFileName));
    }

    [Fact]
    public void RazorTemplateProperty_IsTheSameInCSharpAndInJavaScript()
    {
        var presets = PresetScripts.Read(PresetScripts.PresetsFileName);
        var name = Regex.Match(presets, @"export const templateHtmlProperty = '([^']+)';");
        var type = ExtensionsPackage.Assembly.GetType(RazorTemplateRowTypeName, throwOnError: true)!;

        Assert.True(name.Success, "data-grid-presets.js does not export templateHtmlProperty");
        Assert.Equal((string)type.GetField("HtmlProperty")!.GetRawConstantValue()!, name.Groups[1].Value);
        // the sanitized row of a cell action or a request leaves the HTML out
        Assert.Contains("if (row) delete row[templateHtmlProperty];", presets);
    }

    #region ------ Internals ------

    private const string PresetNamesTypeName = "WebAwesome.Blazor.Extensions.DataGrid.PresetNames";
    private const string PrefixFieldName = "Prefix";
    private const string RazorTemplateRowTypeName = "WebAwesome.Blazor.Extensions.DataGrid.RazorTemplateRow";
    private const string TemplateParseLine = "template.innerHTML = html;";

    // the constants of PresetNames (internal; InternalsVisibleTo applies to Debug builds only, so read by reflection)
    private static string[] PresetNamesOfTheFactories()
    {
        var type = ExtensionsPackage.Assembly.GetType(PresetNamesTypeName, throwOnError: true)!;
        return type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.Name != PrefixFieldName)
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToArray();
    }

    #endregion
}
