using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// The library's table of element defaults (src\WebAwesome.Blazor\Base\WaElementDefaults.cs), which the sticky
/// attribute rule renders when a rendered attribute returns to its default or to null, must be exactly what the
/// bound Web Awesome version's CEM says: one entry per attribute with a literal default (a quoted string, the empty
/// string included, a number or Infinity), for every non-boolean attribute and for boolean attributes defaulting
/// to true (read by a "true"/"false" converter, spellcheck); nothing for a computed default (new Date(), []) or for
/// none. The test generates the file from expected-api-surface.json and compares it with the checked-in one; on a
/// difference it writes received-element-defaults.cs next to the test binaries, to be copied over the source file
/// (the wa-upgrade process does this whenever the surface is re-exported). The sticky rule itself is checked on the
/// rendered output by RenderedAttributeParityTests (i).
/// </summary>
public class ElementDefaultsTableTests
{
    /// <summary>
    /// The checked-in table equals the one generated from the CEM.
    /// </summary>
    [Fact]
    public void ElementDefaultsTable_MatchesTheCem()
    {
        SkipUnlessParityEnabled();

        var expected = Generate();
        var receivedPath = Path.Combine(AppContext.BaseDirectory, ReceivedFileName);
        File.WriteAllText(receivedPath, expected);

        var sourcePath = Path.Combine(WrapperProjectDirectory(), BaseDirectory, SourceFileName);
        var actual = File.Exists(sourcePath) ? File.ReadAllText(sourcePath) : string.Empty;

        Assert.True(string.Equals(Normalize(actual), Normalize(expected), StringComparison.Ordinal),
            $"src\\WebAwesome.Blazor\\{BaseDirectory}\\{SourceFileName} differs from the CEM defaults of Web Awesome {Surface.Version}; " +
            $"if the surface changed intentionally, copy '{receivedPath}' over it.");
    }

    /// <summary>
    /// The element default the sticky rule renders for an attribute (the CEM literal default, as the table holds it), or
    /// null when the attribute has none, so the sticky rule removes it.
    /// </summary>
    /// <param name="tag">Element tag</param>
    /// <param name="attribute">Attribute name</param>
    /// <returns>The default text, or null</returns>
    internal static string? CemDefaultOf(string tag, string attribute)
        => CemDefaults.Value.TryGetValue(tag + KeySeparator + attribute, out var value) ? value : null;

    #region ------ Internals ------

    private static readonly Lazy<Dictionary<string, string>> CemDefaults = new(() => Entries().ToDictionary(e => e.Key, e => e.Default, StringComparer.Ordinal));

    private const string ReceivedFileName = "received-element-defaults.cs";
    private const string BaseDirectory = "Base";
    private const string SourceFileName = "WaElementDefaults.cs";
    private const string BooleanType = "boolean";
    private const string TrueDefault = "true";
    private const string KeySeparator = ":";

    // a quoted JS string literal without escapes, a number, or Infinity
    private static readonly Regex LiteralDefault = new(@"^(?:'(?<text>[^'\\]*)'|""(?<text>[^""\\]*)""|`(?<text>[^`\\$]*)`|(?<number>-?\d+(?:\.\d+)?|Infinity))$", RegexOptions.Compiled);

    // the tag:attribute keys and default texts of the table, sorted ordinally
    private static IEnumerable<(string Key, string Default)> Entries()
    {
        foreach (var (tag, component) in Surface.Components.OrderBy(c => c.Key, StringComparer.Ordinal))
        {
            foreach (var (attribute, surface) in component.Attributes.OrderBy(a => a.Key, StringComparer.Ordinal))
            {
                var defaultText = surface.Default?.Trim();
                if (string.IsNullOrEmpty(defaultText)) continue;

                var isBoolean = (surface.EffectiveType ?? string.Empty).Split('|').Any(p => p.Trim() == BooleanType);
                if (isBoolean)
                {
                    if (defaultText == TrueDefault) yield return (tag + KeySeparator + attribute, TrueDefault);
                    continue;
                }

                var match = LiteralDefault.Match(defaultText);
                if (!match.Success) continue;

                yield return (tag + KeySeparator + attribute, match.Groups["text"].Success ? match.Groups["text"].Value : match.Groups["number"].Value);
            }
        }
    }

    private static string Generate()
    {
        var text = new StringBuilder();
        text.AppendLine("using System;");
        text.AppendLine("using System.Collections.Generic;");
        text.AppendLine("using System.Diagnostics.CodeAnalysis;");
        text.AppendLine();
        text.AppendLine("namespace WebAwesome.Blazor.Base;");
        text.AppendLine();
        text.AppendLine("/// <summary>");
        text.AppendLine($"/// The element defaults of Web Awesome {Surface.Version} (the CEM default of every attribute that has a literal one),");
        text.AppendLine("/// which the sticky attribute rule renders in place of a rendered attribute's value when it returns to the default or");
        text.AppendLine("/// to null (<see cref=\"WaAttributeMemory\"/>). Generated by ElementDefaultsTableTests from expected-api-surface.json;");
        text.AppendLine("/// do not edit by hand, copy the test's received-element-defaults.cs over this file when the surface changes.");
        text.AppendLine("/// </summary>");
        text.AppendLine("internal static class WaElementDefaults");
        text.AppendLine("{");
        text.AppendLine("    /// <summary>");
        text.AppendLine("    /// Looks up the element default of an attribute.");
        text.AppendLine("    /// </summary>");
        text.AppendLine("    /// <param name=\"tag\">The element tag</param>");
        text.AppendLine("    /// <param name=\"attribute\">The attribute name</param>");
        text.AppendLine("    /// <param name=\"value\">The default as the attribute text, e.g. \"top\" or \"8\"</param>");
        text.AppendLine("    /// <returns>true when the attribute has a literal default</returns>");
        text.AppendLine("    public static bool TryGet(string tag, string attribute, [NotNullWhen(true)] out string? value)");
        text.AppendLine("        => Defaults.TryGetValue(tag + KeySeparator + attribute, out value);");
        text.AppendLine();
        text.AppendLine("    #region ------ Internals ------");
        text.AppendLine();
        text.AppendLine("    private const string KeySeparator = \":\";");
        text.AppendLine();
        text.AppendLine("    private static readonly Dictionary<string, string> Defaults = new(StringComparer.Ordinal)");
        text.AppendLine("    {");
        foreach (var (key, value) in Entries())
            text.AppendLine($"        [\"{key}\"] = \"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\",");
        text.AppendLine("    };");
        text.AppendLine();
        text.AppendLine("    #endregion");
        text.AppendLine("}");
        return text.ToString();
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimStart('﻿').TrimEnd();

    #endregion
}
