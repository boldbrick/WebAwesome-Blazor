using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Checks the text and URL templates of the cell presets. A template is text with placeholders: <c>{field}</c> or
/// <c>{path.to.field}</c> reads a value of the row, <c>{field:format}</c> also formats it (text templates only), and
/// <c>{{</c> and <c>}}</c> stand for literal braces. The preset functions in <c>data-grid-presets.js</c> read the same
/// grammar.
/// </summary>
internal static class CellTemplate
{
    /// <summary>
    /// The formats that a placeholder of a text template can name. Each formats the value with the culture of the
    /// preset, as the preset of the same name does with its default options.
    /// </summary>
    public static readonly string[] Formats = ["number", "percent", "date", "datetime", "time"];

    /// <summary>
    /// The URL schemes that a link may have; a relative URL is allowed too.
    /// </summary>
    public static readonly string[] LinkSchemes = ["http", "https", "mailto", "tel"];

    /// <summary>
    /// The URL schemes that an image may have; a relative URL is allowed too.
    /// </summary>
    public static readonly string[] ImageSchemes = ["http", "https"];

    /// <summary>
    /// Checks a text template.
    /// </summary>
    /// <param name="template">The template</param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <exception cref="ArgumentException">Thrown when the template is empty, has an unmatched brace, a placeholder
    /// that is not a field path, or an unknown format</exception>
    public static void ValidateText(string template, string parameterName)
    {
        Parse(template, parameterName, allowFormats: true);
    }

    /// <summary>
    /// Checks a URL template. Each placeholder value is URL-encoded when the cell renders, so a value cannot add a
    /// scheme; the literal parts must not name a scheme other than http, https, mailto or tel.
    /// </summary>
    /// <param name="template">The template, for example <c>/orders/{id}</c></param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <exception cref="ArgumentException">Thrown when the template is empty, contains whitespace or a control
    /// character, has an unmatched brace, a placeholder that is not a field path, a format, or another scheme</exception>
    public static void ValidateUrl(string template, string parameterName)
    {
        ValidateUrl(template, parameterName, LinkSchemes);
    }

    /// <summary>
    /// Checks the URL template of an image: as <see cref="ValidateUrl(string, string)"/>, with the schemes http and
    /// https only.
    /// </summary>
    /// <param name="template">The template, for example <c>/photos/{id}.jpg</c></param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <exception cref="ArgumentException">Thrown when the template is not valid or names another scheme</exception>
    public static void ValidateImageUrl(string template, string parameterName)
    {
        ValidateUrl(template, parameterName, ImageSchemes);
    }

    /// <summary>
    /// Checks a field path, for example <c>currency</c> or <c>customer.name</c>.
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <exception cref="ArgumentException">Thrown when the path is not a dot-separated list of names</exception>
    public static void ValidateFieldPath(string path, string parameterName)
    {
        if (path is null || !PathPattern.IsMatch(path))
            throw new ArgumentException($"'{path}' is not a field path: use names of letters, digits, '_' and '$', separated by '.'.", parameterName);
    }

    #region ------ Internals ------

    private static void ValidateUrl(string template, string parameterName, string[] schemes)
    {
        if (!string.IsNullOrEmpty(template) && template.Any(c => char.IsWhiteSpace(c) || char.IsControl(c)))
            throw new ArgumentException($"'{template}' is not a valid URL template: it must not contain whitespace or control characters.", parameterName);

        var (prefix, literal) = Parse(template, parameterName, allowFormats: false);

        // the text before the first placeholder, and all literal text joined: an empty placeholder value joins the
        // literal parts, so "java{x}script:" must fail as "javascript:" does
        foreach (var text in new[] { prefix, literal })
        {
            var scheme = SchemePattern.Match(text);
            if (scheme.Success && !schemes.Contains(scheme.Groups[1].Value, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"'{template}' is not a valid URL template: only a relative URL or the schemes {string.Join(", ", schemes)} are allowed.", parameterName);
        }
    }

    private const char OpenBrace = '{';
    private const char CloseBrace = '}';
    private const char FormatSeparator = ':';

    // a field name of the row, as the JSON of a .NET object names it, and a dotted path of them
    private static readonly Regex PathPattern = new(@"^[A-Za-z_$][A-Za-z0-9_$]*(?:\.[A-Za-z_$][A-Za-z0-9_$]*)*$", RegexOptions.CultureInvariant);

    // a URL scheme as the URL standard reads it: a letter, then letters, digits, "+", "-" or ".", then ":"
    private static readonly Regex SchemePattern = new("^([A-Za-z][A-Za-z0-9+.-]*):", RegexOptions.CultureInvariant);

    // returns the literal text before the first placeholder and all literal text, with the escapes resolved
    private static (string Prefix, string Literal) Parse(string template, string parameterName, bool allowFormats)
    {
        if (string.IsNullOrEmpty(template))
            throw new ArgumentException("A template must not be empty.", parameterName);

        var literal = new StringBuilder();
        string? prefix = null;
        for (var i = 0; i < template.Length; i++)
        {
            var c = template[i];
            if (c == OpenBrace && i + 1 < template.Length && template[i + 1] == OpenBrace)
            {
                literal.Append(OpenBrace);
                i++;
            }
            else if (c == CloseBrace && i + 1 < template.Length && template[i + 1] == CloseBrace)
            {
                literal.Append(CloseBrace);
                i++;
            }
            else if (c == OpenBrace)
            {
                var end = template.IndexOf(CloseBrace, i + 1);
                if (end < 0)
                    throw new ArgumentException($"'{template}' is not a valid template: a '{{' has no matching '}}'. Write '{{{{' for a literal brace.", parameterName);

                ValidatePlaceholder(template, template.Substring(i + 1, end - i - 1), parameterName, allowFormats);
                prefix ??= literal.ToString();
                i = end;
            }
            else if (c == CloseBrace)
            {
                throw new ArgumentException($"'{template}' is not a valid template: a '}}' has no matching '{{'. Write '}}}}' for a literal brace.", parameterName);
            }
            else
            {
                literal.Append(c);
            }
        }

        var text = literal.ToString();
        return (prefix ?? text, text);
    }

    private static void ValidatePlaceholder(string template, string placeholder, string parameterName, bool allowFormats)
    {
        var separator = placeholder.IndexOf(FormatSeparator);
        var path = separator < 0 ? placeholder : placeholder[..separator];
        if (!PathPattern.IsMatch(path))
            throw new ArgumentException($"'{template}' is not a valid template: '{{{placeholder}}}' does not name a field. Use names of letters, digits, '_' and '$', separated by '.'.", parameterName);

        if (separator < 0) return;

        var format = placeholder[(separator + 1)..];
        if (!allowFormats)
            throw new ArgumentException($"'{template}' is not a valid template: a URL placeholder cannot have a format ('{{{placeholder}}}').", parameterName);

        if (!Formats.Contains(format, StringComparer.Ordinal))
            throw new ArgumentException($"'{template}' is not a valid template: the format '{format}' is unknown. Use one of {string.Join(", ", Formats)}.", parameterName);
    }

    #endregion
}
