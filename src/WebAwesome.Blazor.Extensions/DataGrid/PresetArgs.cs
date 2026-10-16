using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Collects the options of one cell preset and turns them into the <see cref="WaJsFunction"/> reference of the
/// registered preset function. Only the options that are set are written, so the preset applies its own default for the
/// others. Map keys are written verbatim: the serializer of <see cref="WaJsFunction.WithArgs"/> has no dictionary key
/// policy.
/// </summary>
internal sealed class PresetArgs
{
    /// <summary>
    /// The locale that stands for the invariant culture, whose number and date formats follow American English. The
    /// invariant culture has no BCP 47 name that the browser could read.
    /// </summary>
    public const string InvariantCultureLocale = "en-US";

    /// <summary>
    /// Sets an option when the value is not null.
    /// </summary>
    /// <param name="key">The option name that the preset function reads</param>
    /// <param name="value">The value</param>
    /// <returns>This instance</returns>
    public PresetArgs Set(string key, object? value)
    {
        if (value is not null) values[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the <c>culture</c> option: the name of the given culture, or of the current culture when none is given.
    /// </summary>
    /// <param name="culture">The culture, or null for <see cref="CultureInfo.CurrentCulture"/></param>
    /// <returns>This instance</returns>
    public PresetArgs SetCulture(CultureInfo? culture)
    {
        return Set(CultureKey, LocaleOf(culture ?? CultureInfo.CurrentCulture));
    }

    /// <summary>
    /// Sets the <c>nullText</c> option, the text of a null, undefined or empty value.
    /// </summary>
    /// <param name="nullText">The text, or null for an empty cell</param>
    /// <returns>This instance</returns>
    public PresetArgs SetNullText(string? nullText) => Set(NullTextKey, nullText);

    /// <summary>
    /// Creates the reference to the registered preset function with these options.
    /// </summary>
    /// <param name="presetName">The registered name of the preset function</param>
    /// <param name="tags">The Web Awesome elements the preset renders</param>
    /// <returns>The function reference</returns>
    public WaJsFunction ToFunction(string presetName, params string[] tags)
    {
        var function = WaJsFunction.Registered(presetName).WithArgs(values);
        return tags.Length == 0 ? function : function.Uses(tags);
    }

    /// <summary>
    /// The BCP 47 name the browser reads for a culture.
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>The culture name, or <see cref="InvariantCultureLocale"/> for the invariant culture</returns>
    public static string LocaleOf(CultureInfo culture)
    {
        return string.IsNullOrEmpty(culture.Name) ? InvariantCultureLocale : culture.Name;
    }

    /// <summary>
    /// Copies a map of display texts and checks it.
    /// </summary>
    /// <param name="map">The map, value key to text</param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <returns>A copy in the order of the map, or null when the map is null</returns>
    /// <exception cref="ArgumentException">Thrown when a text is null</exception>
    public static Dictionary<string, string>? CopyTexts(IReadOnlyDictionary<string, string>? map, string parameterName)
    {
        if (map is null) return null;
        if (map.Any(entry => entry.Value is null))
            throw new ArgumentException("A text of the map must not be null.", parameterName);

        return map.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
    }

    /// <summary>
    /// Copies a map of variants as the strings that the elements read.
    /// </summary>
    /// <param name="map">The map, value key to variant</param>
    /// <returns>A copy in the order of the map, or null when the map is null</returns>
    public static Dictionary<string, string>? CopyVariants(IReadOnlyDictionary<string, WaVariant>? map)
    {
        return map?.ToDictionary(entry => entry.Key, entry => entry.Value.ToHtmlValue(), StringComparer.Ordinal);
    }

    /// <summary>
    /// Copies a map of icons as the objects that the preset functions read.
    /// </summary>
    /// <param name="map">The map, value key to icon</param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <returns>A copy in the order of the map, or null when the map is null</returns>
    /// <exception cref="ArgumentException">Thrown when an icon is null</exception>
    public static Dictionary<string, object>? CopyIcons(IReadOnlyDictionary<string, WaDataGridIcon>? map, string parameterName)
    {
        if (map is null) return null;
        if (map.Any(entry => entry.Value is null))
            throw new ArgumentException("An icon of the map must not be null.", parameterName);

        return map.ToDictionary(entry => entry.Key, entry => (object)IconOf(entry.Value), StringComparer.Ordinal);
    }

    /// <summary>
    /// The object that the preset functions read for an icon: the name, and the family, variant and library when set.
    /// </summary>
    /// <param name="icon">The icon</param>
    /// <returns>The icon object, or null when the icon is null</returns>
    public static object? IconOrNull(WaDataGridIcon? icon) => icon is null ? null : IconOf(icon);

    /// <summary>
    /// The object that a composite preset reads for the text format of a value: the name of a text preset and its
    /// arguments.
    /// </summary>
    /// <param name="format">A text preset of <see cref="WaDataGridFormatters"/>, or null</param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <returns>The format object (<c>{ name, args }</c>), or null when the format is null</returns>
    /// <exception cref="ArgumentException">Thrown when the reference is not a text preset that formats a value without
    /// its row</exception>
    public static object? TextFormatOrNull(WaJsFunction? format, string parameterName)
    {
        if (format is null) return null;
        if (format.Kind != WaJsFunctionKind.Registered || !PresetNames.TextFormats.Contains(format.Reference, StringComparer.Ordinal))
            throw new ArgumentException($"The format must be a text preset of {nameof(WaDataGridFormatters)} such as Number, Currency, Percent, Date or Lookup, not {format}.", parameterName);

        var result = new Dictionary<string, object> { [FormatNameKey] = format.Reference };
        if (format.Args is { } args) result[FormatArgsKey] = args;
        return result;
    }

    /// <summary>
    /// Checks an action id: non-empty, without leading or trailing whitespace and without control characters.
    /// </summary>
    /// <param name="action">The action id</param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <exception cref="ArgumentException">Thrown when the action id is not valid</exception>
    public static void ValidateActionId(string action, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(action) || action.Trim().Length != action.Length || action.Any(char.IsControl))
            throw new ArgumentException($"'{action}' is not a valid action id: it must be non-empty, without leading or trailing whitespace and without control characters.", parameterName);
    }

    /// <summary>
    /// Checks a text that the browser shows or reads: not null, with no control characters.
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="parameterName">The parameter name for exceptions</param>
    /// <exception cref="ArgumentException">Thrown when the text is null, empty or has a control character</exception>
    public static void ValidateLabel(string text, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Any(char.IsControl))
            throw new ArgumentException("A label must be a non-empty text without control characters.", parameterName);
    }

    /// <summary>
    /// Reads the display texts (<c>labels</c>) and the lookup fallback text from the arguments of a preset that has
    /// them (<see cref="PresetNames.Labeled"/>).
    /// </summary>
    /// <param name="formatter">The formatter reference</param>
    /// <returns>The labels and the fallback text, or null when the reference is not such a preset or has no labels</returns>
    public static (Dictionary<string, string> Labels, string? Fallback)? LabelsOf(WaJsFunction? formatter)
    {
        if (formatter is null || formatter.Kind != WaJsFunctionKind.Registered
            || !PresetNames.Labeled.Contains(formatter.Reference, StringComparer.Ordinal)
            || formatter.Args is not { ValueKind: JsonValueKind.Object } args
            || !args.TryGetProperty(LabelsKey, out var labels) || labels.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var label in labels.EnumerateObject())
        {
            if (label.Value.ValueKind == JsonValueKind.String) map[label.Name] = label.Value.GetString()!;
        }

        // a lookup shows its fallback text for a value without a label; the fallback of a badge is a variant
        string? fallback = null;
        if (formatter.Reference == PresetNames.Lookup && args.TryGetProperty(FallbackKey, out var fallbackText) && fallbackText.ValueKind == JsonValueKind.String)
            fallback = fallbackText.GetString();

        return (map, fallback);
    }

    #region ------ Internals ------

    private const string CultureKey = "culture";
    private const string NullTextKey = "nullText";

    // the text format object of the composite presets, and the option names of the labeled presets
    private const string FormatNameKey = "name";
    private const string FormatArgsKey = "args";
    private const string LabelsKey = "labels";
    private const string FallbackKey = "fallback";

    // the icon object of design section 6.3: name, family, variant, library
    private const string IconNameKey = "name";
    private const string IconFamilyKey = "family";
    private const string IconVariantKey = "variant";
    private const string IconLibraryKey = "library";

    private static Dictionary<string, object> IconOf(WaDataGridIcon icon)
    {
        var result = new Dictionary<string, object> { [IconNameKey] = icon.Name };
        if (icon.Family is not null) result[IconFamilyKey] = icon.Family;
        if (icon.Variant is not null) result[IconVariantKey] = icon.Variant;
        if (icon.Library is not null) result[IconLibraryKey] = icon.Library;
        return result;
    }

    // insertion order is the JSON order, so the descriptor of the same options is always the same text
    private readonly Dictionary<string, object> values = new(StringComparer.Ordinal);

    #endregion
}
