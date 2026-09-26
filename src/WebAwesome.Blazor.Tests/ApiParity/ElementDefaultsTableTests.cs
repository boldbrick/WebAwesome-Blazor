using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// The test-side oracle of element defaults (WaElementDefaults.cs next to this file), which the parity tests compare
/// the wrappers with: every wrapper's Default&lt;Name&gt; constant, every default a sticky call site passes, and every
/// default a sticky attribute falls back to on the rendered output (RenderedAttributeParityTests). The library itself
/// holds no such table: each call site passes its default explicitly. The oracle must be exactly what the bound Web
/// Awesome version's CEM says: one entry per attribute with a literal default (a quoted string, the empty string
/// included, a number or Infinity), for every non-boolean attribute and for boolean attributes defaulting to true (read
/// by a "true"/"false" converter, spellcheck); nothing for a computed default (new Date(), []) or for none. The test
/// generates the file from expected-api-surface.json and compares it with the checked-in one; on a difference it writes
/// received-element-defaults.cs next to the test binaries, to be copied over the oracle (the wa-upgrade process does
/// this whenever the surface is re-exported).
/// </summary>
public class ElementDefaultsTableTests
{
    /// <summary>
    /// The checked-in oracle equals the one generated from the CEM.
    /// </summary>
    [Fact]
    public void ElementDefaultsTable_MatchesTheCem()
    {
        SkipUnlessParityEnabled();

        var expected = Generate();
        var receivedPath = Path.Combine(AppContext.BaseDirectory, ReceivedFileName);
        File.WriteAllText(receivedPath, expected);

        var sourcePath = Path.Combine(ThisDirectory(), SourceFileName);
        var actual = File.Exists(sourcePath) ? File.ReadAllText(sourcePath) : string.Empty;

        Assert.True(string.Equals(Normalize(actual), Normalize(expected), StringComparison.Ordinal),
            $"src\\WebAwesome.Blazor.Tests\\ApiParity\\{SourceFileName} differs from the CEM defaults of Web Awesome {Surface.Version}; " +
            $"if the surface changed intentionally, copy '{receivedPath}' over it.");
    }

    /// <summary>
    /// The element default of an attribute (the CEM literal default, as the oracle holds it), or null when the attribute
    /// has none, so a sticky call site must pass none and removal restores the unset state.
    /// </summary>
    /// <param name="tag">Element tag</param>
    /// <param name="attribute">Attribute name</param>
    /// <returns>The default text, or null</returns>
    internal static string? CemDefaultOf(string tag, string attribute)
        => WaElementDefaults.TryGet(tag, attribute, out var value) ? value : null;

    #region ------ Internals ------

    private const string ReceivedFileName = "received-element-defaults.cs";
    private const string SourceFileName = "WaElementDefaults.cs";
    private const string BooleanType = "boolean";
    private const string TrueDefault = "true";
    private const string KeySeparator = ":";

    // a quoted JS string literal without escapes, a number, or Infinity
    private static readonly Regex LiteralDefault = new(@"^(?:'(?<text>[^'\\]*)'|""(?<text>[^""\\]*)""|`(?<text>[^`\\$]*)`|(?<number>-?\d+(?:\.\d+)?|Infinity))$", RegexOptions.Compiled);

    // the directory of this source file, where the oracle lives
    private static string ThisDirectory([CallerFilePath] string thisFile = "") => Path.GetDirectoryName(thisFile)!;

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
        text.AppendLine("namespace WebAwesome.Blazor.Tests.ApiParity;");
        text.AppendLine();
        text.AppendLine("/// <summary>");
        text.AppendLine($"/// The element defaults of Web Awesome {Surface.Version} (the CEM default of every attribute that has a literal one): the");
        text.AppendLine("/// test-side oracle the parity tests compare the wrappers' Default&lt;Name&gt; constants, the defaults their sticky");
        text.AppendLine("/// call sites pass and the defaults rendered attributes fall back to with. Generated by ElementDefaultsTableTests from");
        text.AppendLine("/// expected-api-surface.json; do not edit by hand, copy the test's received-element-defaults.cs over this file when");
        text.AppendLine("/// the surface changes.");
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
